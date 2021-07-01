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

        // TODO: Test reversible board rather than copying, need to time it
        public static Move MakeMove(Board Board)
        {
            //  TODO: Even with Undo-move possible, want to operate on copy of board so don't have to worry about events etc.
            var tempBoard = new Board(Board);
            var gameInfo = new GameExtraInfo(Board);

            var allAvailableMoves = Board.GetAllAvailableMoves().ToList();
            // Issue: Now doing deterministically, so need to randomise somehow
            Shuffle(allAvailableMoves);

            // choose the move with the best immediate score (stupidly aggressive for now, need to make recursive)

            // TODO: Getting "can't find piece" error. PieceToMove getting removed then replaced as "new Pawn(...)"?
            // Instead remember just the square, and lookup actual Piece at very end
            //      Piece PieceToMove = null;
            Square PieceToMove = Square.At(-1, -1);


            Square BestMove = Square.At(-1, -1); // placeholder
            int bestScore = -1000000;

            Move TheBestMove = null;

            foreach (KeyValuePair<Square, List<Square>> piece in allAvailableMoves)
            {
                // also randomise each selection of moves
                Shuffle(piece.Value);
             //   Piece actualPiece = Board.GetPiece(piece.Key);
                foreach (Square MoveTo in piece.Value)
                {
                    // Note: Done inside loop in case piece removed then replaced during search (e.g. en-passant)
                    Piece actualPiece = tempBoard.GetPiece(piece.Key);

                    var move = new Move(piece.Key, MoveTo, tempBoard);

                    // TODO: Test without making new board
                    //  var newBoard = new Board(Board);
                    //  actualPiece.MoveTo(newBoard, MoveTo);
                    actualPiece.MoveTo(tempBoard, MoveTo);

                    //  int score = Evaluator.GetBoardValue(newBoard) * sign; // Have to take negative if playing as black
                    //   int score = -AlphaBeta(newBoard, MAX_DEPTH - 1, -bestScore); // still initialise the upper bound of first recursive call
                    int score = -AlphaBeta(tempBoard, MAX_DEPTH - 1, -bestScore);
                    if (score > bestScore)
                    {
                        //       PieceToMove = actualPiece;
                        PieceToMove = piece.Key;
                        BestMove = MoveTo;
                        bestScore = score;

                        TheBestMove = move;
                    }

                    actualPiece.UndoMove(tempBoard, move, gameInfo); // undo the move
                }
            }

            // indicates stalemate/checkmate if no valid moves
            /*     if (PieceToMove != null)
                 {
                     PieceToMove.MoveTo(Board, BestMove);
                 }*/
            // placeholder value for null
            //   if (PieceToMove.Col != -1)
            //    {
            //        Board.GetPiece(PieceToMove).MoveTo(Board, BestMove);
            //    }
            return TheBestMove;
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
                var gameInfo = new GameExtraInfo(Board);

                foreach (KeyValuePair<Square, List<Square>> piece in allAvailableMoves)
                {
                    
                    foreach (Square MoveTo in piece.Value)
                    {
                        // Note: Done inside loop in case piece removed then replaced during search (e.g. en-passant)
                        Piece actualPiece = Board.GetPiece(piece.Key);

                        //   var newBoard = new Board(Board);
                        //   actualPiece.MoveTo(newBoard, MoveTo);
                        var move = new Move(piece.Key, MoveTo, Board);
                        actualPiece.MoveTo(Board, MoveTo);

                        // as soon as recursive call finds a path worse than bestScore for us, we know we won't go down that route
                        // hence -bestScore is the new recursive upperBound
                        //    bestScore = Math.Max(bestScore, -AlphaBeta(newBoard, depth - 1, -bestScore));
                        bestScore = Math.Max(bestScore, -AlphaBeta(Board, depth - 1, -bestScore));
                        actualPiece.UndoMove(Board, move, gameInfo); // undo the move
                        if (bestScore > upperBound) return bestScore;
                    }
                }
                return bestScore;
            }
        }
    }
}
