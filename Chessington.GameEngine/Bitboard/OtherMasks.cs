using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chessington.GameEngine.Bitboard
{
    // relevant masks/tables for use with knights/kings/pawns
    public class OtherMasks
    {
        // also initialise tables for knights and kings
        public static ulong[] knightMasks = new ulong[64];
        public static ulong[] kingMasks = new ulong[64];

        // can exploit over/under-flows to avoid issues at top/bottom
        // remember lowest bit is 'a', highest bit is 'h' i.e. little endian when drawn on board
        public const ulong Not_H_File = 0x7f7f7f7f7f7f7f7fUL;
        public const ulong Not_GH_File = 0x3f3f3f3f3f3f3f3fUL;
        public const ulong Not_A_File = 0xfefefefefefefefeUL;
        public const ulong Not_AB_File = 0xfcfcfcfcfcfcfcfcUL;

        static OtherMasks() {
            InitialiseTables();
        }

        private static void InitialiseTables()
        {
            
            for (int i = 0; i < 64; i++)
            {
                ulong bit = 1UL << i;
                ulong result = 0UL;

                // knight table
                // in order of compass bearings
                // remember a left-shift moves a1 -> h1 so moves piece right-then-up
                result |= (bit & Not_H_File) << 17;
                result |= (bit & Not_GH_File) << 10;
                result |= (bit & Not_GH_File) >> 6;
                result |= (bit & Not_H_File) >> 15;

                result |= (bit & Not_A_File) >> 17;
                result |= (bit & Not_AB_File) >> 10;
                result |= (bit & Not_AB_File) << 6;
                result |= (bit & Not_A_File) << 15;

                knightMasks[i] = result;

                // king table
                result = (bit & Not_H_File) << 1;
                result |= (bit & Not_A_File) >> 1;
                bit |= result;
                result |= (bit << 8) | (bit >> 8);
                kingMasks[i] = result;
            }
        }
    }
}
