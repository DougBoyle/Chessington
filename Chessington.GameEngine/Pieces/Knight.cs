using System;
using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

namespace Chessington.GameEngine.Pieces
{
    public class Knight : Piece
    {
        public Knight(Player player)
            : base(player) { PieceType = PieceType.Knight; }

        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition)
        {
            List<Square> availableSquares = new List<Square>();
            var dir = new int[] { -1, 1 };
            foreach (var twoStep in dir)
            {
                foreach (var oneStep in dir)
                {
                    availableSquares.Add(new Square(currentPosition.Row + 2 * twoStep,
                        currentPosition.Col + oneStep));
                    availableSquares.Add(new Square(currentPosition.Row + oneStep,
                        currentPosition.Col + 2 * twoStep));
                }
            }
            return availableSquares.Where(square => square.IsValid() && board.IsEmptyOrOpponent(square, Player))
                .Select(to => new Move(currentPosition, to, board));
        }
    }
}