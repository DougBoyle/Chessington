using System;
using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.AI
{
    public class ComputerPlayer
    {
        private static Random r = new Random();

        private const int MAX_DEPTH = 6; // 2 ply, mine then yours

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

        // TODO: Test reversible board rather than copying, need to time it
        public static Move MakeMove(Board Board)
        {
            /*********************************************************************************/
            // TODO: Should probably double-check the validity of these moves

            // use opening book if possible
            var openingMove = Polyglot.GetMove(Board);
            if (openingMove != null) return openingMove;

            // use endgame table if possible
            var endgameMove = Endgame.Endgame.MakeMove(Board);
            if (endgameMove != null) return endgameMove;

            /*********************************************************************************/

            //  TODO: Even with Undo-move possible, want to operate on copy of board so don't have to worry about events etc.
            var tempBoard = new Board(Board);
            var gameInfo = new GameExtraInfo(Board);

            // require an actually valid move at top level, not relaxed moves
            var allAvailableMoves = Board.GetAllAvailableMoves().ToList();
            // Issue: Now doing deterministically, so need to randomise somehow
            Shuffle(allAvailableMoves);


            // Note: This has to be worse than the worst possible evaluation, so that the computer
            // still moves even if it knows it is being checkmated
            int bestScore = -10000000;

            Move bestMove = null;

            foreach (Move move in allAvailableMoves)
            {
                tempBoard.MakeMove(move);

                int score = -AlphaBeta(tempBoard, MAX_DEPTH - 1, -bestScore);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }

                tempBoard.UndoMove(move, gameInfo); // undo the move
            }

            Console.WriteLine("Best score found: {0}", bestScore);
            return bestMove;
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
                // use relaxed moves to avoid searching all moves at next level for check, just to evaluate them all again
                // when that level is actually searched (TODO: Can't detect stalemate, looks like losing)
                // TODO: Sorting??
                var allAvailableMoves = Board.GetAllRelaxedMoves();
                allAvailableMoves.Sort();

                int bestScore = -1000000 - 1000*depth; // the sooner in the search a checkmate is, the worse it is valued
                var gameInfo = new GameExtraInfo(Board);


                foreach (Move move in allAvailableMoves)
                {
                    if (move.CapturedPiece == BitUtils.KING_BOARD 
                        || move.CapturedPiece == BitUtils.KING_BOARD + 6) return 1000000 + 1000 * depth;

                    Board.MakeMove(move);

                    // as soon as recursive call finds a path worse than bestScore for us, we know we won't go down that route
                    // hence -bestScore is the new recursive upperBound
                    //    bestScore = Math.Max(bestScore, -AlphaBeta(newBoard, depth - 1, -bestScore));
                    bestScore = Math.Max(bestScore, -AlphaBeta(Board, depth - 1, -bestScore));
                    Board.UndoMove(move, gameInfo);  // undo the move
                    if (bestScore > upperBound) return bestScore;
                }

                return bestScore;
            }
        }
    }
}
