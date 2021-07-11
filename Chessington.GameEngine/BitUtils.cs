using Chessington.GameEngine.Pieces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chessington.GameEngine
{
    // lots of functions for processing bitboards, or converting between Squares and bitboard positions etc.
    public class BitUtils
    {
        public const int PAWN_BOARD = 0, KNIGHT_BOARD = 1, BISHOP_BOARD = 2,
            ROOK_BOARD = 3, QUEEN_BOARD = 4, KING_BOARD = 5;

        // haven't decided if these should be part of bitboards[] or not yet
        public static ulong BoardOccupancy(Board board, Player player)
        {
            ulong result = 0UL;
            for (int i = (int)player * 6; i < (int)player * 6 + 6; i++)
            {
                result |= board.Bitboards[i];
            }
            return result;
        }


        /* Multiplier gotten from https://www.chessprogramming.org/De_Bruijn_Sequence
           Table checked with Python:

        mult = 0x022fdd63cc95386d
        mp = {}
        for i in range(64):
	        n = (((2**i)*mult) >> 58) % 64
	        if n in mp:
		        print("Clash!")
	        else:
		        mp[n] = i
        */
        const ulong deBruijn64 = 0x022fdd63cc95386dUL;
        private static readonly int[] deBruijnHashtbl = 
            {0, 1, 2, 53, 3, 7, 54, 27, 4, 38, 41, 8, 34, 55,
            48, 28, 62, 5, 39, 46, 44, 42, 22, 9, 24, 35, 59,
            56, 49, 18, 29, 11, 63, 52, 6, 26, 37, 40, 33, 47,
            61, 45, 43, 21, 23, 58, 17, 10, 51, 25, 36, 32, 60,
            20, 57, 16, 50, 31, 19, 15, 30, 14, 13, 12};

        // assumes exactly 1 bit set in string. (returns position 0 if no bits set, same as if last bit set)
        public static int BitToIndex(ulong bit) 
        {
            return deBruijnHashtbl[(deBruijn64 * bit) >> 58];
        }

        public static ulong GetLSB(ulong board)
        {
            return board & (~board + 1); // (~board + 1) is the same as -board, but -board not valid for ulong
        }

        public static ulong DropLSB(ulong board)
        {
            return board & (board - 1);
        }

        public static int Count1s(ulong board)
        {
            int r;
            for (r = 0; board != 0; r++) board = DropLSB(board);
            return r;
        }

        public static int PieceToBoardIndex(Piece piece)
        {
            return (int)piece.PieceType + 6 * (int)piece.Player;
        }

        public static ulong SquareToBit(Square square)
        {
            return 1UL << ((7 - square.Row) * 8 + square.Col);
        }

        public static int SquareToIndex(Square square)
        {
            return (7 - square.Row) * 8 + square.Col;
        }

        public static Square IndexToSquare(int index)
        {
            return Square.At(7 - index / 8, index % 8);
        }
    }
}
