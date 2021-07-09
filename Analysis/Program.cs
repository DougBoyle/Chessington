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
            /*Board b = new Board();

            b.AddPiece(Square.At(4, 3), new King(Player.White));

            NormalForm.SquareMapper mapper = NormalForm.NormaliseBoard(b);

            Square k = b.FindKing(Player.White);
            Console.WriteLine(k);
            Console.WriteLine(mapper(k));*/

            /*
            ThreePieces threePieces = new ThreePieces();
           //  threePieces.SolveForQueen();
            threePieces.SolveForRook();

            Console.WriteLine("Longest chain: {0}", threePieces.whiteTable.Select(entry => entry == null ? 0 : entry.DTM).Max());

            Console.WriteLine("Writing to: {0}", Path.Combine(Directory.GetCurrentDirectory(), "KRK.tbs"));
            threePieces.WriteTables(Path.Combine(Directory.GetCurrentDirectory(), "KRK.tbs"));*/

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
