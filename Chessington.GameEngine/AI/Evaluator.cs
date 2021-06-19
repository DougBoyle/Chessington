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
        // map pieces to values
        public static Dictionary<PieceType, int> PieceValues = new Dictionary<PieceType, int>
        {
            {PieceType.Pawn, 100}, {PieceType.Knight, 300}, {PieceType.Bishop, 320}, {PieceType.Rook, 500}, {PieceType.Queen, 900}, {PieceType.King, 10000}
        };


        // calculates the score from white's perspective, position not taken into account
        public static int GetBoardValue(Board b)
        {
            int total = 0;
            for (int i = 0; i < GameSettings.BoardSize; i++)
            {
                for (int j = 0; j < GameSettings.BoardSize; j++)
                {
                    Piece p = b.GetPiece(i, j);
                    if (p != null)
                    {
                        if (p.Player == Player.White)
                        {
                            total += PieceValues[p.PieceType];
                        } else
                        {
                            total -= PieceValues[p.PieceType];
                        }
                    }
                }
            }
            return total;
        }
    }
}
