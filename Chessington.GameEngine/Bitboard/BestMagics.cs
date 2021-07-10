using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Chessington.GameEngine.BitUtils;

namespace Chessington.GameEngine.Bitboard
{
    // actual tables to lookup stored in a file (could probably also compute on loading too - need to time this)
    // can trivially initialise the masks needed at startup, rather than storing or computing each time
    public class BestMagics
    {
        // some taken from online sources e.g:
        // https://www.chessprogramming.org/Best_Magics_so_far or http://vicki-chess.blogspot.com/2013/04/magics.html
        public static int[] rookShifts = { 
            12, 11, 11, 11, 11, 11, 11, 12,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            11, 10, 10, 10, 10, 10, 10, 11,
            12, 11, 11, 11, 11, 11, 11, 12 };

        public static ulong[] rookMagics= { 
            0x80004004e41080UL, 0x4000a000100040UL, 0x808020001000800aUL, 0x300091000850020UL, // 0
            0x8080040088000280UL, 0x80240080110200UL, 0x8400028410060801UL, 0x100010000482082UL,
            0x200800021400280UL, 0x680400c822000UL, 0x2180100080a000UL, 0x4000801000800800UL, // 1
            0x308800800840280UL, 0x60ce000884108200UL, 0x4002004488160005UL, 0x8101000283000842UL,
            0x4802208000804016UL, 0x541401000a008UL, 0x4420808020009004UL, 0x8002210028100101UL, // 2
            0x800301004c080010UL, 0xc00400a010040UL, 0x1340030082512UL, 0x704020030410084UL,
            0x8020800080204004UL, 0x2000500040002000UL, 0x2010040020006800UL, 0x8340204900100500UL, // 3
            0x5004002808004080UL, 0x2812000200080410UL, 0xc002000200030824UL, 0x4090200208c44UL, 
            0x63804004800c68UL, 0xc8102000c4401000UL, 0x8401a00443001100UL, 0x8000881801001UL, // 4
            0x721080c401800800UL, 0x2808400802a00UL, 0x3400063004008809UL, 0x120018406000065UL,
            0x408824000208004UL, 0xc010004820044000UL, 0x8003200041010050UL, 0x2463002410010019UL, // 5
            0xc2008810620004UL, 0x46000408020010UL, 0xa000480610140029UL, 0x81002840810006UL, 
            0x8480104008600240UL, 0x20b008020c00100UL, 0x1010821004200480UL, 0x4106681001002100UL, // 6
            0xa228018004008880UL, 0xa806200040080UL, 0x4080210011400UL, 0x8401007200UL, 
            0x1210080014019UL, 0x200248208200d102UL, 0x2085240802202UL, 0x220c80430002101UL,  // 7
            0x830004100a0801UL, 0x4002002810440b02UL, 0xa01821001180cUL, 0xa02c0021028042UL 
        };

        public static int[] bishopShifts = { 
            5, 4, 5, 5, 5, 5, 4, 5,
            4, 4, 5, 5, 5, 5, 4, 4,
            5, 5, 7, 7, 7, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 9, 9, 7, 5, 5,
            5, 5, 7, 7, 7, 7, 5, 5,
            4, 4, 5, 5, 5, 5, 4, 4,
            5, 4, 5, 5, 5, 5, 4, 5 
        };

        public static ulong[] bishopMagics = {
            0xf7e5faf4fbebfbfeUL, 0xf9c6e6d9b477ffd7UL, 0x10208200405408UL, 0x8488484500000025UL, // 0
            0x504102800008040UL, 0x4842011098001000UL, 0x36e5ba6edaff55fbUL, 0x3dff4ad7ddfcffffUL,
            0xd3fe72f54cf5dff9UL, 0xfffaf97b7add8fffUL, 0x300441400920500UL, 0x404040502000000UL, // 1
            0x800020210000032UL, 0x800484110100004UL, 0xfefbddfecd9c7f69UL, 0x7d79f7bb86b7ffefUL,  
            0x830a040100a248cUL, 0x1108003090058880UL, 0x8200c08440008UL, 0x22ca280802024014UL, // 2
            0xd04001080a04200UL, 0x800100414000UL, 0x9000044502420UL, 0xc9019604a61100UL,
            0x102288010101000UL, 0x841204210040104UL, 0x434010082081101UL, 0x4240800006200c0UL, // 3
            0x40840040802002UL, 0x1281012a005600UL, 0xc403810c022988UL, 0x402082010a804400UL, 
            0x2030882008080218UL, 0x180a101480420800UL, 0x4004040410020020UL, 0x10428080480200UL, // 4
            0x80041040100c0100UL, 0x4202018200410041UL, 0x1004848413328404UL, 0x4090200422484UL, 
            0x8404420610104180UL, 0x9081044220000200UL, 0x1462001044020804UL, 0x6021822011084800UL, // 5
            0x254080104000440UL, 0x40900c1000200UL, 0x490204042400c1a0UL, 0x10488101400102UL,
            0xffbffd59e4bf6797UL, 0x9ffffbf579e59eefUL, 0x20504444100200UL, 0xc80c00446080000UL, // 6
            0x404080502e020000UL, 0x88050810470200UL, 0xd7fdf3bab99bffefUL, 0x79fffbfe3d32cde7UL,
            0x7eaffefede54ae7dUL, 0x77553ffcfcfaf98fUL, 0x106e000240445002UL, 0x82000186840400UL, // 7
            0x4000e11820200UL, 0x400400444080210UL, 0x7f7b7ff5f9f15bf3UL, 0x9ffff6eff45f7c6eUL
        };


        // compute required tables when initalised
        public static ulong[] rookMasks = new ulong[64];
        public static ulong[] bishopMasks = new ulong[64];
        public static ulong[] rookAttacks;
        public static ulong[] bishopAttacks;
        // lookup table size depends on square, so store offsets per square into rook/bishopAttacks
        public static int[] rookOffsets = new int[64];
        public static int[] bishopOffsets = new int[64];

        static BestMagics()
        {
            InitialiseTables();
        }

        // compute all of these at startup, rather than storing in file/array or computing each time needed.
        // takes about 50ms to compute. rookAttacks has 102,400 entries, bishopAttacks has 4928 entries.
        private static void InitialiseTables()
        {
            int rookEntries = 0;
            int bishopEntries = 0;
            for (int i = 0; i < 64; i++)
            {
                int row = i / 8; int col = i % 8;
                rookMasks[i] = RookMask(row, col);
                bishopMasks[i] = BishopMask(row, col);

                rookOffsets[i] = rookEntries;
                bishopOffsets[i] = bishopEntries;

                rookEntries += 1 << rookShifts[i];
                bishopEntries += 1 << bishopShifts[i];
            }

            // can only construct/fill in attack tables once total number of entries known
            rookAttacks = new ulong[rookEntries];
            bishopAttacks = new ulong[bishopEntries];

            for (int i = 0; i < 64; i++)
            {
                int row = i / 8; int col = i % 8;
                // rooks
                ulong mask = rookMasks[i];
                int perms = 1 << Count1s(mask);
                for (int j = 0; j < perms; j++)
                {
                    // will have some repetition, but not large amounts
                    ulong occupancy = MakeOccupancy(mask, j);
                    int index = (int)((occupancy * rookMagics[i]) >> (64 - rookShifts[i]));
                    rookAttacks[rookOffsets[i] + index] = RookAttacks(row, col, occupancy);
                }
                // bishops
                mask = bishopMasks[i];
                perms = 1 << Count1s(mask);
                for (int j = 0; j < perms; j++)
                {
                    // will have some repetition, but not large amounts
                    ulong occupancy = MakeOccupancy(mask, j);
                    int index = (int)((occupancy * bishopMagics[i]) >> (64 - bishopShifts[i]));
                    bishopAttacks[bishopOffsets[i] + index] = BishopAttacks(row, col, occupancy);
                }
            }
        }


        // all the squares relevant to determining where rook can move to (ignoring capturing own pieces)
        public static ulong RookMask(int rowPos, int colPos)
        {
            ulong result = 0UL;
            for (int row = rowPos + 1; row < 7; row++) result |= 1UL << (row * 8 + colPos);
            for (int row = rowPos - 1; row > 0; row--) result |= 1UL << (row * 8 + colPos);
            for (int col = colPos + 1; col < 7; col++) result |= 1UL << (rowPos * 8 + col);
            for (int col = colPos - 1; col > 0; col--) result |= 1UL << (rowPos * 8 + col);
            return result;
        }

        public static ulong BishopMask(int rowPos, int colPos)
        {
            ulong result = 0UL;
            for (int row = rowPos + 1, col = colPos + 1; row < 7 && col < 7; row++, col++) result |= 1UL << (row * 8 + col);
            for (int row = rowPos + 1, col = colPos - 1; row < 7 && col > 0; row++, col--) result |= 1UL << (row * 8 + col);
            for (int row = rowPos - 1, col = colPos + 1; row > 0 && col < 7; row--, col++) result |= 1UL << (row * 8 + col);
            for (int row = rowPos - 1, col = colPos - 1; row > 0 && col > 0; row--, col--) result |= 1UL << (row * 8 + col);
            return result;
        }

        // includes/leaves out bits of mask based on index (0 <= index < 2^(num_bits_in_mask))
        public static ulong MakeOccupancy(ulong mask, int index)
        {
            ulong result = 0UL;
            for (int bit = 0; index != 0; bit++)
            {
                ulong maskBit = GetLSB(mask);
                mask = DropLSB(mask);
                // count in binary, but using the set bits of the mask
                // can do instead by shifting index right and checking if odd
                if ((index & 1) != 0)
                {
                    result |= maskBit;
                }
                index >>= 1;
            }
            return result;
        }

        
        // all the squares the piece actually attacks, given relevant occupances
        // filling in arrays with corresponding attacks while testing a magic allows detecting allowed/invalid clashes
        public static ulong RookAttacks(int rowPos, int colPos, ulong occupied)
        {
            ulong result = 0UL;
            for (int row = rowPos + 1; row < 8; row++)
            {
                ulong bit = 1UL << (row * 8 + colPos);
                result |= bit;
                if ((occupied & bit) != 0) break;
            }

            for (int row = rowPos - 1; row >= 0; row--)
            {
                ulong bit = 1UL << (row * 8 + colPos);
                result |= bit;
                if ((occupied & bit) != 0) break;
            }

            for (int col = colPos + 1; col < 8; col++)
            {
                ulong bit = 1UL << (rowPos * 8 + col);
                result |= bit;
                if ((occupied & bit) != 0) break;
            }

            for (int col = colPos - 1; col >= 0; col--)
            {
                ulong bit = 1UL << (rowPos * 8 + col);
                result |= bit;
                if ((occupied & bit) != 0) break;
            }
            return result;
        }

        public static ulong BishopAttacks(int rowPos, int colPos, ulong occupied)
        {
            ulong result = 0UL;
            for (int row = rowPos + 1, col = colPos + 1; row < 8 && col < 8; row++, col++)
            {
                ulong bit = 1UL << (row * 8 + col);
                result |= bit;
                if ((occupied & bit) != 0) break;
            }

            for (int row = rowPos + 1, col = colPos - 1; row < 8 && col >= 0; row++, col--)
            {
                ulong bit = 1UL << (row * 8 + col);
                result |= bit;
                if ((occupied & bit) != 0) break;
            }

            for (int row = rowPos - 1, col = colPos + 1; row >= 0 && col < 8; row--, col++)
            {
                ulong bit = 1UL << (row * 8 + col);
                result |= bit;
                if ((occupied & bit) != 0) break;
            }

            for (int row = rowPos - 1, col = colPos - 1; row >= 0 && col >= 0; row--, col--)
            {
                ulong bit = 1UL << (row * 8 + col);
                result |= bit;
                if ((occupied & bit) != 0) break;
            }
            return result;
        }
    }
}
