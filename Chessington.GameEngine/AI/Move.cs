using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine.AI
{
    // For ordering all moves when doing alpha-beta pruning (rather than one piece at a time)
    // and to allow making/undoing moves
    // TODO: Produce these rather than Lists of Squares when generating moves
    public class Move
    {
        // TODO: Make format of everything more efficient e.g. byte vs 4 bools, 2 ints for Square rather than 1 byte 0-63
        // Important: Original move was From->To, so now need to move in reverse direction
        public readonly Square From;
        public readonly Square To;

        public Piece Captured; // is null for en-passant
        public bool Promotion; // if true, replace piece with a pawn rather than whatever promoted piece it is now

        // information about 50-move counter/castling status/en-passant can be remembered once from the board, not stored per move

        public Move(Square from, Square to, Piece captured, bool promotion)
        {
            From = from;
            To = to;
            Captured = captured;
            Promotion = promotion;
        }

        // convert a pair of squares to a 'Move' - TODO: Produce Move instances directly
        public Move(Square from, Square to, Board before)
        {
            From = from;
            To = to;
            Captured = before.GetPiece(to);
            // detect promotions
            Promotion = (before.GetPiece(from).PieceType == PieceType.Pawn) && (to.Row == 0 || to.Row == 7);
        }

        public static char[] columns = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
        
        // 0 is top of the board, 7 is bottom (white)
        public override string ToString()
        {
            return columns[From.Col] + (8 - From.Row).ToString() + "-" + columns[To.Col] + (8 - To.Row).ToString();
        }
    }
}
