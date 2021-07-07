using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine.AI.Endgame
{
    public class ComputeIndices
    {
        // KXK where X is not a pawn
        public static int SimpleThreePieceBoardToIndex(Board b)
        {
            int numPieces = 0;
            int index = 0;
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = b.GetPiece(row, col);
                    if (piece == null) continue;
                    numPieces++;
                    if (piece.PieceType == PieceType.King)
                    {
                        if (piece.Player == Player.White) // more complex as only 10 positions
                        {
                            switch (row)
                            {
                                case 7: index += 64 * 64 * col; break;
                                case 6: index += 64 * 64 * (3 + col); break;
                                case 5: index += 64 * 64 * (5 + col); break;
                                case 4: index += 64 * 64 * 9; break;
                            }
                        }
                        else
                        {
                            index += (8 * row + col) * 64;
                        }
                    }
                    else
                    {
                        index += 8 * row + col;
                    }
                }
            }
            if (numPieces != 3) throw new ArgumentException("Board does not contain 3 pieces");
            return index;
        }
    }
}
