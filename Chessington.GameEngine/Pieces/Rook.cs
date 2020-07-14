using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.Pieces
{
    public class Rook : Piece
    {
        public Rook(Player player)
            : base(player) { }

        public override IEnumerable<Square> GetRelaxedAvailableMoves(Board board)
        {
            return Moves.GetLateralMoves(board, board.FindPiece(this), Player);
        }
    }
}