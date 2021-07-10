using System;
using System.Collections.Generic;
using System.Linq;
using Chessington.GameEngine.AI;
using Chessington.GameEngine.Pieces;

using static Chessington.GameEngine.BitUtils;

namespace Chessington.GameEngine
{
    public class Board
    {
        // TODO: This can just be a single column
        public Square? EnPassantSquare { get; set; }
        // TODO: Would be more efficient to allow accessing directly?
        private readonly Piece[,] board;
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

        public Board(Player currentPlayer, Piece[,] boardState = null)
        {
            board = boardState ?? new Piece[GameSettings.BoardSize, GameSettings.BoardSize]; 
            CurrentPlayer = currentPlayer;
            CapturedPieces = new List<Piece>();
        }

        public Board(Board board) {
            this.board = new Piece[GameSettings.BoardSize, GameSettings.BoardSize];
            for (int i = 0; i < GameSettings.BoardSize; i++) {
                for (int j = 0; j < GameSettings.BoardSize; j++) {
                    this.board[i, j] = board.GetPiece( Square.At(i, j));
                }
            }

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

        public void AddPiece(Square square, Piece pawn)
        {
            Piece previous = board[square.Row, square.Col];
            board[square.Row, square.Col] = pawn;

            // must remove existing piece if present
            ulong bit = SquareToBit(square);
            if (previous != null) Bitboards[PieceToBoardIndex(previous)] &= ~bit;
            if (pawn != null) Bitboards[PieceToBoardIndex(pawn)] |= bit;
        }

        public Piece GetPiece(Square square)
        {
            return board[square.Row, square.Col];
        }
        // TODO: Creating Square objects is very heavyweight, add voerrides to pass ints directly
        public Piece GetPiece(int row, int col)
        {
            return board[row, col];
        }

        public bool IsSquareEmpty(Square square)
        {
            return GetPiece(square) == null;
        }

        public bool IsOpponent(Square square, Player player)
        {
            return GetPiece(square) != null && GetPiece(square).Player != player;
        }

        public bool IsEmptyOrOpponent(Square square, Player player) {
            return GetPiece(square) == null || GetPiece(square).Player != player;
        }

        public Square FindPiece(Piece piece)
        {
            for (var row = 0; row < GameSettings.BoardSize; row++)
                for (var col = 0; col < GameSettings.BoardSize; col++)
                    if (board[row, col] == piece)
                        return Square.At(row, col);

            // TODO: Debug print board to figure out what is happening?
            throw new ArgumentException("The supplied piece is not on the board.", "piece");
        }

        // TODO: Need a quiet version of this that doesn't generate events? Or allows reversing?
        //       Currently, create a copy of the board (which doesn't copy across handlers) to avoid anything being triggered
        public void MovePiece(Square from, Square to)
        {
            ulong bitFrom = SquareToBit(from);
            ulong bitTo = SquareToBit(to);

            var movingPiece = board[from.Row, from.Col];
            if (movingPiece == null) { return; }
            
            if (to.Equals(Square.At(7,0)))
            { 
                LeftWhiteCastling = false;
            }
            else if (to.Equals(Square.At(7,7)))
            {
                RightWhiteCastling = false;
            }
            else if (to.Equals(Square.At(0,0)))
            {
                LeftBlackCastling = false;
            }
            else if (to.Equals(Square.At(0,7)))
            {
                RightBlackCastling = false;
            }

            if (movingPiece.Player != CurrentPlayer)
            {
                throw new ArgumentException("The supplied piece does not belong to the current player.");
            }

            //If the space we're moving to is occupied, we need to mark it as captured.
            if (board[to.Row, to.Col] != null)
            {
                OnPieceCaptured(board[to.Row, to.Col]);
                // also remove from bitboard. Bit bitTo should be set on that bitboard so could just do ^=
                Bitboards[PieceToBoardIndex(board[to.Row, to.Col])] &= ~bitTo;
            }

            //Move the piece and set the 'from' square to be empty.
            board[to.Row, to.Col] = board[from.Row, from.Col];
            Bitboards[PieceToBoardIndex(movingPiece)] |= bitTo;
            board[from.Row, from.Col] = null;
            Bitboards[PieceToBoardIndex(movingPiece)] &= ~bitFrom;

            CurrentPlayer = movingPiece.Player == Player.White ? Player.Black : Player.White;
            OnCurrentPlayerChanged(CurrentPlayer);
        }

        // MovePiece without all the side-effects, to allow *undoing* moves
        public void QuietMovePiece(Square from, Square to, Piece captured)
        {
            ulong bitFrom = SquareToBit(from);
            ulong bitTo = SquareToBit(to);

            Piece movingPiece = board[from.Row, from.Col];

            // for undoing moves, so 'to' square should be unoccupied
            board[to.Row, to.Col] = board[from.Row, from.Col];
            Bitboards[PieceToBoardIndex(movingPiece)] |= bitTo;
            Bitboards[PieceToBoardIndex(movingPiece)] &= ~bitFrom;
            board[from.Row, from.Col] = captured; // Used for undoing moves, hence replace square with captured piece
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

        // Slightly faster than using 'GetAllRelaxedMoves' as it stops as soon as 1 found
        public bool InCheck(Player player) {
            Square kingSquare = FindKing(player);
            for (int i = 6 * (1 - (int)player); i < 6 * (1 - (int)player) + 6; i++)
            {
                ulong bitboard = Bitboards[i];
                while (bitboard != 0UL)
                {
                    ulong bit = GetLSB(bitboard);
                    Square square = IndexToSquare(BitToIndex(bit));
                    bitboard = DropLSB(bitboard);
                    if (GetPiece(square).GetRelaxedAvailableMoves(this, square)
                        .Select(move => move.To).Contains(kingSquare)) return true;
                }
            }
            return false;
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
