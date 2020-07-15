using System;
using System.Collections.Generic;
using System.Linq;
using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine
{
    public class Board
    {
        public Square? EnPassantSquare { get; set; }
        private readonly Piece[,] board;
        public Player CurrentPlayer { get; set; }
        public IList<Piece> CapturedPieces { get; private set; }

        public bool LeftWhiteCastling { get; set; } = true;
        public bool RightWhiteCastling { get; set; } = true;
        public bool LeftBlackCastling { get; set; } = true;
        public bool RightBlackCastling { get; set; } = true;
        

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
            board[square.Row, square.Col] = pawn;
        }
    
        public Piece GetPiece(Square square)
        {
            return board[square.Row, square.Col];
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

            throw new ArgumentException("The supplied piece is not on the board.", "piece");
        }

        public void MovePiece(Square from, Square to)
        {
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
            }

            //Move the piece and set the 'from' square to be empty.
            board[to.Row, to.Col] = board[from.Row, from.Col];
            board[from.Row, from.Col] = null;

            CurrentPlayer = movingPiece.Player == Player.White ? Player.Black : Player.White;
            OnCurrentPlayerChanged(CurrentPlayer);
        }

        public Dictionary<Square, List<Square>> GetAllAvailableMoves()
        {
            Dictionary<Square, List<Square>> availableMoves = new Dictionary<Square, List<Square>>();
            for (int i = 0; i < GameSettings.BoardSize; i++) {
                for (int j = 0; j < GameSettings.BoardSize; j++) {
                    var piece = GetPiece(Square.At(i, j));
                    if (piece == null || piece.Player != CurrentPlayer) continue;
                    var pieceMoves = piece.GetAvailableMoves(this).ToList();
                    if (pieceMoves.Count != 0)
                    {
                        availableMoves[Square.At(i,j)] = pieceMoves;
                    }
                }
            }

            return availableMoves;
        }

        public Square FindKing(Player player) {
            for (int i = 0; i < GameSettings.BoardSize; i++) {
                for (int j = 0; j < GameSettings.BoardSize; j++) {
                    var piece = GetPiece(Square.At(i, j));
                    if (piece != null && piece.Player == player && piece is King) {
                        return Square.At(i, j);
                    }
                }
            }
            return Square.At(-1, -1); // allows tests without kings on the board to work
        }

        public bool InCheck(Player player) {
            Square kingSquare = FindKing(player);
            for (int i = 0; i < GameSettings.BoardSize; i++) {
                for (int j = 0; j < GameSettings.BoardSize; j++) {
                    var piece = GetPiece(Square.At(i, j));
                    if (piece == null || piece.Player == player) continue;
                    if (piece.GetRelaxedAvailableMoves(this).Contains(kingSquare)) {
                        return true;
                    }
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
