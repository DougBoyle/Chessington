using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine.AI
{
    public class ComputerPlayer
    {
        private static Random r = new Random();

        // Currently 4 is too high. Move only displays after computer has moved too (need to change order/do async), and takes ~15s per move
        private const int MAX_DEPTH = 2; // 2 ply, mine then yours

        // TODO: Can put 'this' in type definition of list to make it an extension method i.e. lst.Shuffle()
        private static void Shuffle<T>(IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = r.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static void MakeMove(Board Board)
        {
        //    int sign = Board.CurrentPlayer == Player.White ? 1 : -1;


            var allAvailableMoves = Board.GetAllAvailableMoves().ToList();
            // Issue: Now doing deterministically, so need to randomise somehow
            Shuffle(allAvailableMoves);

            // choose the move with the best immediate score (stupidly aggressive for now, need to make recursive)
            Piece PieceToMove = null;
            Square BestMove = Square.At(-1, -1); // placeholder
            int bestScore = -1000000;

            foreach (KeyValuePair<Square, List<Square>> piece in allAvailableMoves)
            {
                // also randomise each selection of moves
                Shuffle(piece.Value);
                Piece actualPiece = Board.GetPiece(piece.Key);
                foreach (Square MoveTo in piece.Value)
                {
                    var newBoard = new Board(Board);
                    actualPiece.MoveTo(newBoard, MoveTo);
                    //  int score = Evaluator.GetBoardValue(newBoard) * sign; // Have to take negative if playing as black
                    int score = -MiniMax(newBoard, MAX_DEPTH - 1);
                    if (score > bestScore)
                    {
                        PieceToMove = actualPiece;
                        BestMove = MoveTo;
                        bestScore = score;
                    }
                }
            }

            // indicates stalemate/checkmate if no valid moves
            if (PieceToMove != null)
            {
                PieceToMove.MoveTo(Board, BestMove);
            }
        }

        // TODO: Optimise to not pass board around, represent moves more efficiently, and do alpha-beta pruning
        // Top level calls this for each possible move. This method just returns the score of the best choice (for the current player).
        // As such, also no need for shuffling order
        public static int MiniMax(Board Board, int depth)
        {
            int sign = Board.CurrentPlayer == Player.White ? 1 : -1;
            if (depth == 0)
            {
                return Evaluator.GetBoardValue(Board) * sign;
            } else
            {
                var allAvailableMoves = Board.GetAllAvailableMoves().ToList();
                int bestScore = -1000000;

                foreach (KeyValuePair<Square, List<Square>> piece in allAvailableMoves)
                {
                    Piece actualPiece = Board.GetPiece(piece.Key);
                    foreach (Square MoveTo in piece.Value)
                    {
                        var newBoard = new Board(Board);
                        actualPiece.MoveTo(newBoard, MoveTo);

                        bestScore = Math.Max(bestScore, -MiniMax(newBoard, depth - 1));
                    }
                }
                return bestScore;
            }
        }
    }
}
