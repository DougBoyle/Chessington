using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chessington.GameEngine.AI
{
    public class Move
    {
        public readonly Square From;
        public readonly Square To;

        public Move(Square from, Square to)
        {
            From = from;
            To = to;
        }
    }
}
