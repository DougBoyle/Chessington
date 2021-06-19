using System;
using System.Collections.Generic;
using System.Linq;

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

        // TODO: Currently very inefficient copying the board every time. Need to make reversible
        public virtual IEnumerable<Square> GetAvailableMoves(Board board) {
            IEnumerable<Square> possible = GetRelaxedAvailableMoves(board);
            List<Square> actual = new List<Square>();
            foreach (var square in possible) {
                var newBoard = new Board(board);
                MoveTo(newBoard, square);
                if (!newBoard.InCheck(Player)) {
                    actual.Add(square);
                }
            }
            return actual;
        }
        
        public abstract IEnumerable<Square> GetRelaxedAvailableMoves(Board board);

        public virtual void MoveTo(Board board, Square newSquare)
        {
            var currentSquare = board.FindPiece(this);
            board.MovePiece(currentSquare, newSquare);
            board.EnPassantSquare = null;
        }
    }
}