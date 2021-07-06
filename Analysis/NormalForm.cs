using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine;
using Chessington.GameEngine.Pieces;
using Chessington.GameEngine.AI;

namespace Analysis
{
    // only positions with white king on the 10 squares at angle 180 to 225 degrees from center considered
    // i.e a1, b1-2, c1-3, d1-4
    // all other positions mapped to this by reflection/rotation
    // only 10 squares rather than 64, so reduces table size by about 6x

    // assumes castling not possible
    // TODO: Possibility of en-passant if one pawn on either side
    public class NormalForm
    {
        // flips along / diagonal as (0,0) is top left
        private static void TransposeBoard(Board b)
        {
            for (int i = 1; i < GameSettings.BoardSize; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    Piece tmp = b.board[GameSettings.BoardSize - 1 - i, j];
                    b.board[GameSettings.BoardSize - 1 - i, j] = b.board[GameSettings.BoardSize - 1 - j, i];
                    b.board[GameSettings.BoardSize - 1 - j, i] = tmp;
                }
            }
        }

        private static void ReverseRows(Board b)
        {
            for (int row = 0; row < GameSettings.BoardSize; row++)
            {
                for (int col = 0; col < GameSettings.BoardSize / 2; col++)
                {
                    Piece tmp = b.board[row, col];
                    b.board[row, col] = b.board[row, GameSettings.BoardSize - 1 - col];
                    b.board[row, GameSettings.BoardSize - 1 - col] = tmp;
                }
            }
        }

        private static void ReverseCols(Board b)
        {
            for (int col = 0; col < GameSettings.BoardSize; col++)
            {
                for (int row = 0; row < GameSettings.BoardSize / 2; row++)
                {
                    Piece tmp = b.board[row, col];
                    b.board[row, col] = b.board[GameSettings.BoardSize - 1 - row, col];
                    b.board[GameSettings.BoardSize - 1 - row, col] = tmp;
                }
            }
        }

        public static void NormaliseBoard(Board b)
        {
            Square king = b.FindKing(Player.White);

            // remember row 0 = top of board
            if (king.Col >= GameSettings.BoardSize / 2)
            {
                ReverseRows(b);
                king = Square.At(king.Row, GameSettings.BoardSize - 1 - king.Col);
            }
            if (king.Row < GameSettings.BoardSize / 2)
            {
                ReverseCols(b);
                king = Square.At(GameSettings.BoardSize - 1 - king.Row, king.Col);
            }

            if (king.Col + king.Row + 1 < GameSettings.BoardSize)
            {
                TransposeBoard(b);
            }


        }

    }
}
