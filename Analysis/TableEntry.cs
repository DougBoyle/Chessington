using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine;
using Chessington.GameEngine.AI;

namespace Analysis
{
    public enum Outcome { Win, Lose, Draw, Unknown }

    public class TableEntry
    {
        public Board Board;
        public int DTZ; // depth to zero of 50-move counter (actually 100 to allow both players moving)
        public int DTM; // depth to mate (for the winning side, decrements when that player makes their move)
        public Outcome Outcome; // for current player
        public Move BestMove = null;

        // initially, all other metrics unknown
        public TableEntry(Board board)
        {
            Board = board;
            Outcome = Outcome.Unknown;
        }
    }
}
