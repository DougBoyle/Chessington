using System;
using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.Pieces
{
    public class Knight : Piece
    {
        public Knight(Player player)
            : base(player) { }

        public override IEnumerable<Square> GetAvailableMoves(Board board)
        {
            List<Square> availableMoves = new List<Square>();
            var currentPosition = board.FindPiece(this);
            var dir = new int[] {-1, 1};
            foreach (var twoStep in dir)
            {
                foreach (var oneStep in dir) {
<<<<<<< HEAD
                    var target = new Square(currentPosition.Row + 2*twoStep, currentPosition.Col + oneStep);
                    if (target.IsValid()) {
                        availableMoves.Add(target);
                    }
                    target = new Square(currentPosition.Row + oneStep, currentPosition.Col + 2*twoStep);
                    if (target.IsValid()) {
                        availableMoves.Add(target);
                    }
=======
                    availableMoves.Add(new Square(currentPosition.Row + 2*twoStep,
                        currentPosition.Col + oneStep));
                    availableMoves.Add(new Square(currentPosition.Row + oneStep, 
                        currentPosition.Col + 2*twoStep));
>>>>>>> Kings and knights can't take friendly pieces
                }
            }
            return availableMoves.Where(square => square.isValid() && board.IsEmptyOrOpponent(square, Player));
        }
    }
}