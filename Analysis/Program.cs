using System;
using System.Collections.Generic;
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
            Board b = new Board();

            b.AddPiece(Square.At(2, 7), new King(Player.White));

            NormalForm.NormaliseBoard(b);

            Console.WriteLine(b.FindKing(Player.White));
        }
    }
}
