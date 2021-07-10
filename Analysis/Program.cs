using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine;
using Chessington.GameEngine.Pieces;

namespace Analysis
{


    class Program
    {
        static void Main(string[] args)
        {
            var watch = new System.Diagnostics.Stopwatch();
            watch.Start();

            //  Console.WriteLine(Magic.FindMagics.VerifyBestMagics());
            // Magic.FindMagics.FindAllMagics(false, 1);
            //   Console.WriteLine(BitUtils.Count1s(Magic.FindMagics.rookMask(7, 7)));
            //   Console.WriteLine(Magic.FindMagics.TryRookMagic(0x7645FFFECBFEA79EUL, 11, 7, 7));
            //     Console.WriteLine(Magic.FindMagics.FindMagic(false, 4, 1, 1));
      //      Chessington.GameEngine.Bitboard.BestMagics.InitialiseTables();
            Console.WriteLine(Chessington.GameEngine.Bitboard.BestMagics.rookAttacks.Length);
            Console.WriteLine(Chessington.GameEngine.Bitboard.BestMagics.bishopAttacks.Length);

            watch.Stop();
            Console.WriteLine($"Execution Time: {watch.ElapsedMilliseconds} ms");

            Console.WriteLine("Done! Press enter to close");
            Console.ReadLine();
            // ComputeEndgameTable()
        }

        private static void ComputeEndgameTable()
        {
            ThreePiecesPawn threePieces = new ThreePiecesPawn();
            //  threePieces.SolveForQueen();
            threePieces.SolveForPawn();

            Console.WriteLine("Longest chain: {0}", threePieces.whiteTable.Select(entry => entry == null ? 0 : entry.DTM).Max());

            Console.WriteLine("Writing to: {0}", Path.Combine(Directory.GetCurrentDirectory(), "KPK.tbs"));
            threePieces.WriteTables(Path.Combine(Directory.GetCurrentDirectory(), "KPK.tbs"));

            Console.WriteLine("Writing new versions of other tables");
            threePieces.queenTable.WriteTables(Path.Combine(Directory.GetCurrentDirectory(), "KQK.tbs"));
            threePieces.rookTable.WriteTables(Path.Combine(Directory.GetCurrentDirectory(), "KRK.tbs"));

            Console.WriteLine("Writing full versions of all tables");
            threePieces.WriteFullTables(Path.Combine(Directory.GetCurrentDirectory(), "KPK_DTM.tbs"));
            threePieces.queenTable.WriteFullTables(Path.Combine(Directory.GetCurrentDirectory(), "KQK_DTM.tbs"));
            threePieces.rookTable.WriteFullTables(Path.Combine(Directory.GetCurrentDirectory(), "KRK_DTM.tbs"));

            Console.WriteLine("Done! Press enter to close");
            Console.ReadLine();
        }
    }
}
