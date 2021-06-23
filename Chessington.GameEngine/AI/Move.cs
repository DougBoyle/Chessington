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
    }
}
