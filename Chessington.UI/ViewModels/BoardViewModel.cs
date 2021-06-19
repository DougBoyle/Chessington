using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Chessington.GameEngine;
using Chessington.GameEngine.Pieces;
using Chessington.UI.Caliburn.Micro;
using Chessington.UI.Notifications;

namespace Chessington.UI.ViewModels
{
    public class BoardViewModel : IHandle<PieceSelected>, IHandle<SquareSelected>, IHandle<SelectionCleared>
    {
        private Piece currentPiece;

        public BoardViewModel()
        {
            Board = new Board();
            Board.PieceCaptured += BoardOnPieceCaptured;
            Board.CurrentPlayerChanged += BoardOnCurrentPlayerChanged;
            Board.CurrentPlayerChanged += BoardOnCurrentPlayerChangedComputer;
            ChessingtonServices.EventAggregator.Subscribe(this);
        }
        
        public Board Board { get; private set; }

        public void PiecesMoved()
        {
            ChessingtonServices.EventAggregator.Publish(new PiecesMoved(Board));
        }

        public void Handle(PieceSelected message)
        {
            currentPiece = Board.GetPiece(message.Square);
            if (currentPiece == null) return;

            var moves = new ReadOnlyCollection<Square>(currentPiece.GetAvailableMoves(Board).ToList());
            ChessingtonServices.EventAggregator.Publish(new ValidMovesUpdated(moves));
        }

        public void Handle(SelectionCleared message)
        {
            currentPiece = null;
        }

        public void Handle(SquareSelected message)
        {
            var piece = Board.GetPiece(message.Square);
            if (piece != null && piece.Player == Board.CurrentPlayer)
            {
                ChessingtonServices.EventAggregator.Publish(new PieceSelected(message.Square));
                return;
            }

            if (currentPiece == null)
                return;

            var moves = currentPiece.GetAvailableMoves(Board);

            if (moves.Contains(message.Square))
            {
                currentPiece.MoveTo(Board, message.Square);
                
                ChessingtonServices.EventAggregator.Publish(new PiecesMoved(Board));
                ChessingtonServices.EventAggregator.Publish(new SelectionCleared());
            }
        }

        private static void BoardOnPieceCaptured(Piece piece)
        {
            ChessingtonServices.EventAggregator.Publish(new PieceTaken(piece));
        }

        private static void BoardOnCurrentPlayerChanged(Player player)
        {
            ChessingtonServices.EventAggregator.Publish(new CurrentPlayerChanged(player));
        }

        private static Random r = new Random();

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

        // TODO: Move all the logic to the AI namespace
        private void BoardOnCurrentPlayerChangedComputer(Player player)
        {
            if (player == Player.Black)
            {
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
                    foreach (Square MoveTo in piece.Value) {
                        var newBoard = new Board(Board);
                        actualPiece.MoveTo(newBoard, MoveTo);
                        int score = -GameEngine.AI.Evaluator.GetBoardValue(newBoard); // Have to take negative as we are playing as black
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


                // var randomPiece = allAvailableMoves[r.Next(allAvailableMoves.Count)];
                // var moveTo = randomPiece.Value[r.Next(randomPiece.Value.Count)];
                // Board.GetPiece(randomPiece.Key).MoveTo(Board, moveTo);
            }
        }
    }
}