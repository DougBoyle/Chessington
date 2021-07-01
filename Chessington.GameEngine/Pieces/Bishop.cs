using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.Pieces
{
    public class Bishop : Piece
    {
        

        public Bishop(Player player)
            : base(player) { PieceType = PieceType.Bishop; }

        public override IEnumerable<Square> GetRelaxedAvailableMoves(Board board, Square position) {
            return Moves.GetDiagonalMoves(board, position, Player);
        }
    }
}