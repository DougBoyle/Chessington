using System;
using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

namespace Chessington.GameEngine.Pieces
{
    // ultimately, don't want to be dealing with class instances, use efficient ints etc. instead
    public enum PieceType { Pawn, Knight, Bishop, Rook, Queen, King };

    public abstract class Piece
    {
        public PieceType PieceType;

        protected Piece(Player player)
        {
            Player = player;
        }

        public Player Player { get; private set; }

        public virtual IEnumerable<Square> GetAvailableMoves(Board board, Square position) {
            IEnumerable<Square> possible = GetRelaxedAvailableMoves(board, position);
            List<Square> actual = new List<Square>();

            var tempBoard = new Board(board);
            var gameInfo = new GameExtraInfo(board);

            foreach (var square in possible) {
                //   var newBoard = new Board(board);
                var move = new Move(position, square, board);

                MoveTo(tempBoard, square);
                if (!tempBoard.InCheck(Player)) {
                    actual.Add(square);
                }
                UndoMove(tempBoard, move, gameInfo);
            }
            return actual;
        }

        public virtual IEnumerable<Move> GetAvailableMoves2(Board board, Square position)
        {
            IEnumerable<Move> possible = GetRelaxedAvailableMoves2(board, position);
            List<Move> actual = new List<Move>();

            // could also just be done at the whole board level
            var tempBoard = new Board(board);
            var gameInfo = new GameExtraInfo(board);

            foreach (var move in possible)
            {
                MoveTo(tempBoard, move);
                if (!tempBoard.InCheck(Player))
                {
                    actual.Add(move);
                }
                UndoMove(tempBoard, move, gameInfo);
            }
            return actual;
        }

        // Takes the position of the piece as an input, rather than having to find it twice
        public abstract IEnumerable<Square> GetRelaxedAvailableMoves(Board board, Square position);
        public abstract IEnumerable<Move> GetRelaxedAvailableMoves2(Board board, Square position);

        // TODO: use Move object to allow handling promotions
        public virtual void MoveTo(Board board, Square newSquare)
        {
            var currentSquare = board.FindPiece(this);
            board.MovePiece(currentSquare, newSquare);
            board.EnPassantSquare = null;
        }

        public virtual void MoveTo(Board board, Move move)
        {
            board.MovePiece(move.From, move.To);
            board.EnPassantSquare = null;
        }

        // for computer evaluation
        // GameExtraInfo restores all information about castling/en passant (and also current player) that can't be reversed otherwise
        public virtual void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            if (move.Promotion != null)
            {
                board.AddPiece(move.To, new Pawn(info.CurrentPlayer));
            }
            board.QuietMovePiece(move.To, move.From, move.Captured);
            info.RestoreInfo(board);
        }
    }
}