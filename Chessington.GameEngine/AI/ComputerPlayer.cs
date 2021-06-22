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
                    int score = -AlphaBeta(newBoard, MAX_DEPTH - 1, -bestScore); // still initialise the upper bound of first recursive call
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
 
        // Doesn't matter that the top level only tracks one score (not both alpha/beta) as the other is fixed at infinity at the top level
        // Alpha is the lower bound on guaranteed score i.e. best path found from this node.
        // Beta is the upper bound on guaranteed score i.e. best path on current prediction of move opponent takes before. If an option is found
        //      which exceeds beta, can stop searching this node as the opponent will never allow us to reach here.
        // Alpha/Beta keep swapping, so technically NegaMax.

        // score is for current player, so upperBound gets negated each recursive call

        // From the alpha-beta pruning algorithm given here: https://en.wikipedia.org/wiki/Negamax
        // only the upper bound (beta) is necessary, as alpha can effectively be initialised as the value of the first choice
        // *** alpha/beta swap sides each recursive call, so is this still valid? ***
        // if alpha > beta holds from initial value of alpha, recursive call shouldn't have happened anyway?
        // TODO: Test exactly how many nodes explored by each method
        public static int AlphaBeta(Board Board, int depth, int upperBound) 
        {
            int sign = Board.CurrentPlayer == Player.White ? 1 : -1;
            if (depth == 0)
            {
                return Evaluator.GetBoardValue(Board) * sign;
            }
            else
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

                        // as soon as recursive call finds a path worse than bestScore for us, we know we won't go down that route
                        // hence -bestScore is the new recursive upperBound
                        bestScore = Math.Max(bestScore, -AlphaBeta(newBoard, depth - 1, -bestScore));
                        if (bestScore > upperBound) return bestScore;
                    }
                }
                return bestScore;
            }
        }
    }
}
