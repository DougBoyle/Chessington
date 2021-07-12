using Chessington.GameEngine;
using Chessington.GameEngine.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analysis.Perft
{
    // left out of the Tests project as it is very intensive, and can be used as a performance measure
    public class PerftTest
    {

        /*  Starting position results: 
         *  Depth:  1       2       3       4       5           6       7
         *  Count:  20      400     8902    197281  4865609     
         *  Time:                                   6s
         */ 
        // assumes depth >= 1
        public static long CountLeaves(Board board, int depth)
        {
            var moves = board.GetAllAvailableMoves();
            if (depth <= 1) return moves.Count();

            long total = 0;
            var gameInfo = new GameExtraInfo(board);

            foreach (Move move in moves)
            {
                board.MakeMove(move);
                total += CountLeaves(board, depth - 1);
                board.UndoMove(move, gameInfo);
            }
            return total;
        }
    }
}