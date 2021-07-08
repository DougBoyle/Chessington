using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

namespace Chessington.GameEngine.Pieces
{
    public class Bishop : Piece
    {
        

        public Bishop(Player player)
            : base(player) { PieceType = PieceType.Bishop; }

        public override IEnumerable<Square> GetRelaxedAvailableMoves(Board board, Square position) {
            return Moves.GetDiagonalMoves(board, position, Player);
        }

        public override IEnumerable<Move> GetRelaxedAvailableMoves2(Board board, Square here)
        {
            return GetRelaxedAvailableMoves(board, here).Select(to => new Move(here, to, board));
        }
    }
}