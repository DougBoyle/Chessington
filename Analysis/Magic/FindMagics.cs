using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Chessington.GameEngine.BitUtils;
using static Chessington.GameEngine.Bitboard.BestMagics;

namespace Analysis.Magic
{
    // goal is to find magics for calculating bitboard rook/bishop attacks
    // always possible with table of same number of bits as occupancy mask, but goal is to minimise
    // e.g. a1 rook has 12 bit mask i.e. 4096 table, but only 7*7 = 49 distinct outputs (in theory only 6 bits)

    // suggestion (https://www.chessprogramming.org/Looking_for_Magics) is to & together three random bitstrings
    // (so about 12.5% of bits are ones)
    public class FindMagics
    {
        public static bool VerifyBestMagics()
        {
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    int idx = row * 8 + col;
                    if (!TryMagic(true, rookMagics[idx], rookShifts[idx], row, col))
                    {
                        Console.WriteLine($"Rook magic {idx} failed. Row {row} Col {col}");
                        return false;
                    }
                    if (!TryMagic(false, bishopMagics[idx], bishopShifts[idx], row, col))
                    {
                        Console.WriteLine($"Bishop magic {idx} failed. Row {row} Col {col}");
                        return false;
                    }
                }
            }
            return true;
        }        

        public static bool TryMagic(bool rook, ulong magic, int indexSize, int row, int col)
        {
            ulong[] attacks = new ulong[1 << indexSize]; // attempted hash table size for this magic
            ulong mask = rook ? RookMask(row, col) : BishopMask(row, col);
            int permutations = 1 << Count1s(mask); // number of combinations that need to fit into table

            for (int i = 0; i < permutations; i++)
            {
                ulong occupancy = MakeOccupancy(mask, i);
                int index = (int)((occupancy * magic) >> (64 - indexSize));
                ulong result = rook ? RookAttacks(row, col, occupancy) : BishopAttacks(row, col, occupancy);

                if (attacks[index] == 0) attacks[index] = result; // cell unused
                else if (attacks[index] != result) return false; // destructive collision, magic doesn't work
            }
            return true;
        }

        public static void FindAllMagics(bool rook, int reduction) // reduction forces smaller table, harder to find
        {
            ulong[] magics = new ulong[64];
            int[] sizes = new int[64];
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    // count 1s in mask to determine size of hashTbl to try
                    int size = Count1s(rook ? RookMask(row, col) : BishopMask(row, col));
                    ulong magic = FindMagic(rook, size - reduction, row, col);
                    if (magic != 0UL)
                    {
                        sizes[row * 8 + col] = size - reduction;
                        magics[row * 8 + col] = magic;
                    } else
                    {
                        // fill in with already known best value
                        sizes[row * 8 + col] = (rook ? rookShifts : bishopShifts)[row * 8 + col];
                        magics[row * 8 + col] = (rook ? rookMagics : bishopMagics)[row * 8 + col];
                    }
                }
            }
            Console.WriteLine(String.Join(", ", sizes));
            Console.WriteLine(String.Join(", ", magics.Select(magic => $"0x{magic:x}UL")));
        }

        public static ulong FindMagic(bool rook, int indexSize, int row, int col)
        {
            ulong[] attacks = new ulong[1 << indexSize];
            ulong mask = rook ? RookMask(row, col) : BishopMask(row, col);
            int permutations = 1 << Count1s(mask);

            // rather than recomputing for every new magic to try 
            ulong[] occupancies = new ulong[permutations];
            ulong[] correctAttacks = new ulong[permutations];
            for (int i = 0; i < permutations; i++)
            {
                occupancies[i] = MakeOccupancy(mask, i);
                correctAttacks[i] = rook ? RookAttacks(row, col, occupancies[i]) : BishopAttacks(row, col, occupancies[i]);
            }

            // now try magic values
            for (int n = 0; n < 10000000; n++)
            {
                ulong magic = GetRandomMagic();
                bool success = true;

                for (int i = 0; i < (1 << indexSize); i++) attacks[i] = 0UL;

                for (int i = 0; i < permutations; i++)
                {
                    int index = (int)((occupancies[i] * magic) >> (64 - indexSize));
                    if (attacks[index] == 0) attacks[index] = correctAttacks[i];
                    else if (attacks[index] != correctAttacks[i])
                    {
                        success = false;
                        break;
                    }
                }

                if (success)
                {
                    Console.WriteLine($"Found {row * 8 + col} {magic:x} {indexSize}");
                    return magic;
                }
            }
            Console.WriteLine($"None found {row * 8 + col}");
            return 0UL; // gave up after 1M attempts
        }

        private static Random r = new Random();
        private static ulong GetRandom()
        {
            byte[] bytes = new byte[8];
            r.NextBytes(bytes);
            return BitConverter.ToUInt64(bytes, 0);
        }
        // can adjust to change number of 1s e.g. low for finding any magic, but high seems to help with dense magic.
        private static ulong GetRandomMagic()
        {
            // seems that few 1s helps find an initial magic, but most magics for smaller tables have more 1s
            // return GetRandom() & GetRandom() & GetRandom();
            return GetRandom() | GetRandom();
        }

    }
}
