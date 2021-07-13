using System;
using System.Collections.Generic;
using System.Linq;
using Chessington.GameEngine.AI;
using Chessington.GameEngine.Pieces;

using static Chessington.GameEngine.BitUtils;
using static Chessington.GameEngine.Bitboard.OtherMasks;
using static Chessington.GameEngine.Bitboard.BitMoves;

namespace Chessington.GameEngine
{
    public class Board
    {
        // TODO: This can just be a single column
        public Square? EnPassantSquare { get; set; }
        public Player CurrentPlayer { get; set; }
        // TODO: Where is this actually used? Logic on BoardViewModel that wraps around MakeMove?
        public IList<Piece> CapturedPieces { get; private set; }

        public bool LeftWhiteCastling { get; set; } = true;
        public bool RightWhiteCastling { get; set; } = true;
        public bool LeftBlackCastling { get; set; } = true;
        public bool RightBlackCastling { get; set; } = true;

        // TODO: Count to 50 for stalemate

        // shift to using bitboards rather than Piece[,] board
        // add functions to operate on bitboards in parallel for now
        // White then Black, P-N-B-R-Q-K. So get index as:
        //      (int)PieceType + 6*(int)Player

        // TODO: Want ulong[14] and make the last two white/black occupancies? Or computer at start of AllMoves?
        public ulong[] Bitboards = new ulong[12];

        public Board()
            : this(Player.White) { }

        public Board(Player currentPlayer)
        {
            CurrentPlayer = currentPlayer;
            CapturedPieces = new List<Piece>();
        }

        public Board(Board board) {
            for (int i = 0; i < 12; i++)
            {
                Bitboards[i] = board.Bitboards[i];
            }

            CurrentPlayer = board.CurrentPlayer;
            EnPassantSquare = board.EnPassantSquare;
            CapturedPieces = new List<Piece>();
            LeftBlackCastling = board.LeftBlackCastling;
            RightBlackCastling = board.RightBlackCastling;
            LeftWhiteCastling = board.LeftWhiteCastling;
            RightWhiteCastling = board.RightWhiteCastling;
        }

        // relatively expensive to scan through for possibly replaced piece.
        // hence AddPiece should only be used for setup/tests, not finding/performing moves etc.
        public void AddPiece(Square square, Piece pawn)
        {
            // must remove existing piece if present
            ulong bit = SquareToBit(square);
            int pieceIndex = pawn == null ? -1 : PieceToBoardIndex(pawn);
            for (int i = 0; i < 12; i++)
            {
                if (i == pieceIndex) Bitboards[i] |= bit;
                else Bitboards[i] &= ~bit;
            }
        }

        // expensive to scan through bitboards and construct correct Piece object,
        // should only be used by UI
        public Piece GetPiece(Square square)
        {
            ulong bit = SquareToBit(square);
            for (int i = 0; i < 12; i++)
            {
                if ((Bitboards[i] & bit) != 0)
                {
                    Player player = (Player)(i / 6);
                    switch (i % 6)
                    {
                        case PAWN_BOARD: return new Pawn(player);
                        case KNIGHT_BOARD: return new Knight(player);
                        case BISHOP_BOARD: return new Bishop(player);
                        case ROOK_BOARD: return new Rook(player);
                        case QUEEN_BOARD: return new Queen(player);
                        default: return new King(player);
                    }
                }
            }
            return null;
        }

        public Piece GetPiece(byte squareIdx)
        {
            return GetPiece(IndexToSquare(squareIdx));
        }

        public int GetPieceIndex(Square square)
        {
            ulong bit = SquareToBit(square);
            for (int i = 0; i < 12; i++)
            {
                if ((Bitboards[i] & bit) != 0) return i;
               
            }
            return NO_PIECE;
        }

        public int GetPieceIndex(byte index)
        {
            ulong bit = 1UL << index;
            for (int i = 0; i < 12; i++)
            {
                if ((Bitboards[i] & bit) != 0) return i;

            }
            return NO_PIECE;
        }

        // TODO: Replace with passing in index rather than row/col (here 0,0 = a8, not a1)
        public Piece GetPiece(int row, int col)
        {
            return GetPiece(Square.At(row, col));
        }

        public bool IsSquareEmpty(Square square)
        {
            ulong bit = SquareToBit(square);
            return Bitboards.All(board => (board & bit) == 0UL);
        }


        // rather than calling Piece.MoveTo, as bitboards don't have piece instances so can't use overriden function
        public void MakeMove(Move move)
        {
            switch ((PieceType)(move.MovingPiece % 6))
            {
                // pawns need to be handled specially (en-passant/promotion)
                case PieceType.Pawn: Pawn.MakeMove(this, move); break;
                // king needs to be handled specially (castling)
                case PieceType.King: King.MakeMove(this, move); break;
                default: Piece.MakeMove(this, move); break;
            }
        }

        // as above
        public void UndoMove(Move move, GameExtraInfo info)
        {
            switch ((PieceType)(move.MovingPiece % 6))
            {
                case PieceType.Pawn: Pawn.UndoMove(this, move, info); break;
                case PieceType.King: King.UndoMove(this, move, info); break;
                default: Piece.UndoMove(this, move, info); break;
            }
        }

       
        // MovePiece without all the side-effects, to allow *undoing* moves
        // 'from' here was 'to' when move was made
        public void QuietMovePiece(int fromIdx, int toIdx, int captured, int movingPiece)
        {
            ulong bitFrom = 1UL << fromIdx;
            ulong bitTo = 1UL << toIdx;

            // for undoing moves, so 'to' square should be unoccupied and 'from' should be piece that was captured
            Bitboards[movingPiece] ^= bitTo | bitFrom;
            if (captured >= 0) Bitboards[captured] |= bitFrom;
        }


        // TODO: May be worth just doing GetRelaxedAvailableMoves and checking them all at top level?
        public List<Move> GetAllAvailableMoves()
        {
            IEnumerable<Move> possible = GetAllRelaxedMoves();
            List<Move> actual = new List<Move>();

            var tempBoard = new Board(this); // make a copy due to possible event handlers for moves/captures
            var gameInfo = new GameExtraInfo(this);

            foreach (var move in possible)
            {
                tempBoard.MakeMove(move);
                if (!tempBoard.InCheck(CurrentPlayer))
                {
                    actual.Add(move);
                }
                tempBoard.UndoMove(move, gameInfo);
            }
            return actual;
        }

        // TODO: Probably a better way of doing this/better place to put this.
        // downside of not having class instances, effectively have to make pointers explicit
        public delegate IEnumerable<Move> MoveGetter(Board board, Square here, Player player, ulong mine, ulong yours);
        private readonly MoveGetter[] moveGetters =
            {Pawn.GetRelaxedAvailableMoves, Knight.GetRelaxedAvailableMoves, Bishop.GetRelaxedAvailableMoves,
             Rook.GetRelaxedAvailableMoves, Queen.GetRelaxedAvailableMoves, King.GetRelaxedAvailableMoves};


        // avoid repeating effort for computer search - can just use relaxed moves, very negative score will indicate check
        public List<Move> GetAllRelaxedMoves()
        {
            ulong myPieces = BoardOccupancy(this, CurrentPlayer);
            ulong yourPieces = BoardOccupancy(this, CurrentPlayer == Player.White ? Player.Black : Player.White);

            List<Move> availableMoves = new List<Move>();

            // faster to iterate through max 16 pieces on 6 bitboards than all 64 squares
            for (int i = 0; i < 6; i++)
            {
                ulong bitboard = Bitboards[i + 6 * (int)CurrentPlayer];
                while (bitboard != 0UL)
                {
                    ulong bit = GetLSB(bitboard);
                    Square square = IndexToSquare(BitToIndex(bit));
                    bitboard = DropLSB(bitboard);
                    availableMoves.AddRange(moveGetters[i](this, square, CurrentPlayer, myPieces, yourPieces));
                }
            }

            return availableMoves;
        }

        public Square FindKing(Player player) {
            ulong kingBoard = Bitboards[(int)player * 6 + 5];
            if (kingBoard != 0UL) return IndexToSquare(BitToIndex(kingBoard));

            return Square.At(-1, -1); // allows tests without kings on the board to work
        }

        public bool InCheck(Player player)
        {
            return InCheck(player, FindKing(player));
        }

        // Make InCheck more efficient to compute, don't need to look at all pieces
        // determine if king under attack by REVERSE lookup (as if queen/knight/pawn was on king's square)
        // avoids doing GetAvailable/RelaxedMoves so no recursive loop (doesn't consider castling, can't capture king that way)
        // (able to pass in the square of the king to make computing if castling possible easier)
        public bool InCheck(Player player, Square kingSquare) {
            ulong myPieces = BoardOccupancy(this, player);
            ulong yourPieces = BoardOccupancy(this, player == Player.White ? Player.Black : Player.White);
            Player otherPlayer = player == Player.White ? Player.Black : Player.White;

            ulong attackMap = BishopAttacks(kingSquare, this, myPieces, myPieces | yourPieces)
                & (Bitboards[(int)otherPlayer * 6 + BISHOP_BOARD] | Bitboards[(int)otherPlayer * 6 + QUEEN_BOARD]);
            attackMap |= RookAttacks(kingSquare, this, myPieces, myPieces | yourPieces)
                & (Bitboards[(int)otherPlayer * 6 + ROOK_BOARD] | Bitboards[(int)otherPlayer * 6 + QUEEN_BOARD]);
            int kingIndex = SquareToIndex(kingSquare);
            attackMap |= knightMasks[kingIndex] & Bitboards[(int)otherPlayer * 6 + KNIGHT_BOARD];
            // can never capture opponent by castling
            // kings can never actually get adjacent, but need this check to prevent that happening
            attackMap |= kingMasks[kingIndex] & Bitboards[(int)otherPlayer * 6 + KING_BOARD];
            // pawn
            ulong bit = 1UL << kingIndex;
            attackMap |= (player == Player.White ? bit << 9 : bit >> 7) & Not_A_File & Bitboards[(int)otherPlayer * 6 + PAWN_BOARD];
            attackMap |= (player == Player.White ? bit << 7 : bit >> 9) & Not_H_File & Bitboards[(int)otherPlayer * 6 + PAWN_BOARD];

            return attackMap != 0UL;
        }
        
        public delegate void PieceCapturedEventHandler(int piece);
        
        public event PieceCapturedEventHandler PieceCaptured;

        public virtual void OnPieceCaptured(int piece)
        {
            var handler = PieceCaptured;
            if (handler != null) handler(piece);
        }

        public delegate void CurrentPlayerChangedEventHandler(Player player);

        public event CurrentPlayerChangedEventHandler CurrentPlayerChanged;

        public virtual void OnCurrentPlayerChanged(Player player)
        {
            var handler = CurrentPlayerChanged;
            if (handler != null) handler(player);
        }
    }
}
