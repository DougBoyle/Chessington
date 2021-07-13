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
        public int DTZ = -1; // depth to zero of 50-move counter (actually 100 to allow both players moving)
        public int DTM = -1; // depth to mate (in ply rather than standard "mate in 3" etc)
        public Outcome Outcome = Outcome.Unknown; // for current player
        public Move BestMove = null;

        // initially, all other metrics unknown
        public TableEntry(Board board)
        {
            Board = board;
        }

        public static Outcome Opposite(Outcome o)
        {
            switch (o)
            {
                case Outcome.Win: return Outcome.Lose;
                case Outcome.Lose: return Outcome.Win;
                case Outcome.Draw: return Outcome.Draw;
                default: return Outcome.Unknown;
            }
        }

        public static bool ResetsFiftyMoveCounter(Board board, Move move)
        {
            return move.CapturedPiece >= 0 || board.GetPieceIndex(move.FromIdx) % 6 == BitUtils.PAWN_BOARD;
        }
    }
}
