using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

namespace Chessington.GameEngine.Pieces {
    public class Queen : Piece {
        public Queen(Player player)
            : base(player) { PieceType = PieceType.Queen; }

        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition)
        {
            return Moves.GetDiagonalMoves(board, currentPosition, Player)
                .Concat(Moves.GetLateralMoves(board, currentPosition, Player));
        }
    }
}