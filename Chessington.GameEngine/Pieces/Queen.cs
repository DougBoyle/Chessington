using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.Pieces {
    public class Queen : Piece {
        public Queen(Player player)
            : base(player) { }

        public override IEnumerable<Square> GetRelaxedAvailableMoves(Board board) {
            return Moves.GetDiagonalMoves(board, board.FindPiece(this), Player)
                .Concat(Moves.GetLateralMoves(board, board.FindPiece(this), Player));
        }
    }
}