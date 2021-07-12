using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine.AI
{
    public class Evaluator
    {
        // TODO: Take account of position
        private static readonly int[] pieceScores = { 100, 300, 320, 500, 900, 10000 };

        // calculates the score from white's perspective, position not taken into account
        public static int GetBoardValue(Board b)
        {
            int[] playerTotals = { 0, 0 };

            for (int player = 0; player < 2; player++)
            {
                for (int i = 0; i < 6; i++)
                {
                    ulong bitboard = b.Bitboards[i + 6 * player];
                    while (bitboard != 0UL)
                    {
                        playerTotals[player] += pieceScores[i];
                        bitboard = BitUtils.DropLSB(bitboard);
                    }
                }
            }
            return playerTotals[0] - playerTotals[1];
        }
    }
}
