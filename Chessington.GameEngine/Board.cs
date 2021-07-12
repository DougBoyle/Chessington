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
                case PieceType.Pawn: PawnMakeMove(this, move); break;
                // king needs to be handled specially (castling)
                case PieceType.King: KingMakeMove(this, move); break; // TODO!
                default: PieceMakeMove(this, move); break;
            }
        }

        // just like Piece.MoveTo, but uses extra fields of move to access bitboards more efficiently than Board.MovePiece
        // TODO: Move to Piece class once board[,] no longer used
        public static void PieceMakeMove(Board board, Move move)
        {
            int fromIdx = SquareToIndex(move.From);
            int toIdx = SquareToIndex(move.To);
            ulong bitFrom = 1UL << fromIdx;
            ulong bitTo = 1UL << toIdx;

            // Board.MovePiece allowed possibility that moving piece does not exist.
            // no longer allowed. Also don't check that piece belongs to correct player

            // var movingPiece = board[from.Row, from.Col];
            //  if (movingPiece == null) { return; }

            // also testing fromIdx handles moving rooks initially too
            if (toIdx == 0 || fromIdx == 0) board.LeftWhiteCastling = false;
            if (toIdx == 7 || fromIdx == 7) board.RightWhiteCastling = false;
            if (toIdx == 56 || fromIdx == 56) board.LeftBlackCastling = false;
            if (toIdx == 63 || fromIdx == 63) board.RightBlackCastling = false;


            // if (movingPiece.Player != CurrentPlayer)
            // {
            //     throw new ArgumentException("The supplied piece does not belong to the current player.");
            // }

            // TODO: Switch to using ints rather than Piece objects
            if (move.Captured != null)
            {
                board.OnPieceCaptured(move.Captured);
                board.Bitboards[PieceToBoardIndex(move.Captured)] ^= bitTo; // should be same as &= (~bitTo)
            }

            //Move the piece and set the 'from' square to be empty.
            board.Bitboards[move.MovingPiece] |= bitTo;
            board.Bitboards[move.MovingPiece] ^= bitFrom; // &= ~bitFrom;

            board.CurrentPlayer = (Player)(move.MovingPiece / 6) == Player.White ? Player.Black : Player.White;
            board.OnCurrentPlayerChanged(board.CurrentPlayer);

            board.EnPassantSquare = null;
        }

        public static void PawnMakeMove(Board board, Move move)
        {
            int fromIdx = SquareToIndex(move.From);
            int toIdx = SquareToIndex(move.To);
            ulong bitTo = 1UL << toIdx;


            if (board.EnPassantSquare is Square square && toIdx == SquareToIndex(square))
            {
                // TODO: Change OnPieceCaptured to not use Piece class
                // square is just bitTo shifted left/right 8
                board.OnPieceCaptured(board.GetPiece(Square.At(move.From.Row, move.To.Col)));
                // as pawns are either 0 or 6
                board.Bitboards[6 - move.MovingPiece] ^= SquareToBit(Square.At(move.From.Row, move.To.Col));
            }

            PieceMakeMove(board, move);

            // set up en-passant
            if (fromIdx - toIdx == 16 || toIdx - fromIdx == 16)
            {
                board.EnPassantSquare = IndexToSquare((toIdx + fromIdx) / 2);
            }

            if (move.Promotion != null)
            {
                // piece has now moved, so is on move.To square
                board.OnPieceCaptured(board.GetPiece(move.To));

                board.Bitboards[move.MovingPiece] ^= bitTo;
                board.Bitboards[PieceToBoardIndex(move.Promotion)] ^= bitTo; // &= ~bitFrom;
            }
        }

        public static void KingMakeMove(Board board, Move move)
        {
            int fromIdx = SquareToIndex(move.From);
            int toIdx = SquareToIndex(move.To);
            ulong bitFrom = 1UL << fromIdx;
            ulong bitTo = 1UL << toIdx;

            var currentPosition = move.From;
            var newSquare = move.To;

            // should be able to use either board.CurrentPlayer or move.MovingPiece/6
            if (board.CurrentPlayer == Player.White)
            {
                if (fromIdx == 4)
                {
                    board.RightWhiteCastling = false;
                    board.LeftWhiteCastling = false;
                    // short castling
                    if (toIdx == 6)
                    {
                        board.Bitboards[ROOK_BOARD] ^= 0xa0UL; // 0000_0101
                    } else if (toIdx == 2)
                    {
                        board.Bitboards[ROOK_BOARD] ^= 0x9UL; // 1001_0000
                    }
                }
            }
            else
            {
                if (fromIdx == 60)
                {
                    board.RightBlackCastling = false;
                    board.LeftBlackCastling = false;
                    // short castling
                    if (toIdx == 62)
                    {
                        board.Bitboards[6 + ROOK_BOARD] ^= 0xa000000000000000UL; // 0000_0101
                    }
                    else if (toIdx == 58)
                    {
                        board.Bitboards[6 + ROOK_BOARD] ^= 0x900000000000000UL; // 1001_0000
                    }
                }
            }

            PieceMakeMove(board, move);
        }


        // MovePiece without all the side-effects, to allow *undoing* moves
        public void QuietMovePiece(Square from, Square to, Piece captured, int movingPiece)
        {
            ulong bitFrom = SquareToBit(from);
            ulong bitTo = SquareToBit(to);

            // for undoing moves, so 'to' square should be unoccupied
            Bitboards[movingPiece] ^= bitTo|bitFrom;
            if (captured != null) Bitboards[PieceToBoardIndex(captured)] |= bitFrom;
        }


        // TODO: May be worth just doing GetRelaxedAvailableMoves and checking them all at top level?
        public List<Move> GetAllAvailableMoves()
        {
            List<Move> availableMoves = new List<Move>();
            for (int i = 0; i < GameSettings.BoardSize; i++)
            {
                for (int j = 0; j < GameSettings.BoardSize; j++)
                {
                    var square = Square.At(i, j);
                    var piece = GetPiece(square);
                    if (piece == null || piece.Player != CurrentPlayer) continue;
                    availableMoves.AddRange(piece.GetAvailableMoves(this, square));
                }
            }

            return availableMoves;
        }

        // avoid repeating effort for computer search - can just use relaxed moves, very negative score will indicate check
        public List<Move> GetAllRelaxedMoves()
        {
            List<Move> availableMoves = new List<Move>();

            // faster to iterate through max 16 pieces on 6 bitboards than all 64 squares
            for (int i = 6*(int)CurrentPlayer; i < 6*(int)CurrentPlayer + 6; i++)
            {
                ulong bitboard = Bitboards[i];
                while (bitboard != 0UL)
                {
                    ulong bit = GetLSB(bitboard);
                    Square square = IndexToSquare(BitToIndex(bit));
                    bitboard = DropLSB(bitboard);
                    availableMoves.AddRange(GetPiece(square).GetRelaxedAvailableMoves(this, square));
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
        
        public delegate void PieceCapturedEventHandler(Piece piece);
        
        public event PieceCapturedEventHandler PieceCaptured;

        public virtual void OnPieceCaptured(Piece piece)
        {
            var handler = PieceCaptured;
            if (handler != null) handler(piece);
        }

        public delegate void CurrentPlayerChangedEventHandler(Player player);

        public event CurrentPlayerChangedEventHandler CurrentPlayerChanged;

        protected virtual void OnCurrentPlayerChanged(Player player)
        {
            var handler = CurrentPlayerChanged;
            if (handler != null) handler(player);
        }
    }
}
