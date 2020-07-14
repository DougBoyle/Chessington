using System;
using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.Pieces {
    public class Pawn : Piece {
        public Pawn(Player player)
            : base(player) { }

        public override IEnumerable<Square> GetAvailableMoves(Board board) {
            List<Square> available = new List<Square>();
            Square here = board.FindPiece(this);

            var direction = Player == Player.White ? -1 : 1;
            var homeRow = Player == Player.White ? GameSettings.BoardSize - 2 : 1;

            Square targetMove = new Square(here.Row + direction, here.Col);
            if (targetMove.IsValid() && board.IsSquareEmpty(targetMove)) {
                available.Add(targetMove);
                Square targetMove2 = new Square(here.Row + 2 * direction, here.Col);
                if (here.Row == homeRow && board.IsSquareEmpty(targetMove2)) {
                    available.Add(targetMove2);
                }
            }

            return available;
        }
    }
}