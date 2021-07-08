using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

namespace Chessington.GameEngine.Pieces
{
    public class Bishop : Piece
    {
        public Bishop(Player player)
            : base(player) { PieceType = PieceType.Bishop; }

        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square position)
        {
            return Moves.GetDiagonalMoves(board, position, Player);
        }
    }
}