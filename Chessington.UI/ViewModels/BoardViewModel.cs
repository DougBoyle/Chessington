using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows;
using Chessington.GameEngine;
using Chessington.GameEngine.Pieces;
using Chessington.UI.Caliburn.Micro;
using Chessington.UI.Notifications;

using Chessington.GameEngine.AI;

namespace Chessington.UI.ViewModels
{
    // BoardViewModel seems to be the right place to put all logic/control refering to actual chess game.
    // GameViewModel is the slightly higher elements such as displaying the pieces that have been captured
    public class BoardViewModel : IHandle<PieceSelected>, IHandle<SquareSelected>, IHandle<SelectionCleared>
    {
        // Allows calling back to UI thread to make updates after a computer move
        private System.Windows.Threading.Dispatcher Dispatcher { get; set; }

        private Piece currentPiece;
        private Square currentSquare;

        public BoardViewModel()
        {
            Dispatcher = System.Windows.Threading.Dispatcher.CurrentDispatcher;
            Board = new Board();
            Board.PieceCaptured += BoardOnPieceCaptured;
            Board.CurrentPlayerChanged += BoardOnCurrentPlayerChanged;
            // TODO: Only do this after other UI updates made e.g. PieceMoved
        //    Board.CurrentPlayerChanged += BoardOnCurrentPlayerChangedComputer;
            ChessingtonServices.EventAggregator.Subscribe(this);
        }
        
        public Board Board { get; private set; }

        // must call this rather than publishing event directly, so computer can play move
        public void PiecesMoved()
        {
            ChessingtonServices.EventAggregator.Publish(new PiecesMoved(Board));
            if (Board.CurrentPlayer == Player.White && GameSettings.WhiteAsComputer || Board.CurrentPlayer == Player.Black && GameSettings.BlackAsComputer)
            {

                BackgroundWork worker = new BackgroundWork(ComputerTask);
                worker.BeginInvoke(null, null);
            }
        }

        public void Handle(PieceSelected message)
        {
            currentPiece = Board.GetPiece(message.Square);
            if (currentPiece == null) return;
            currentSquare = message.Square;

            var moves = new ReadOnlyCollection<Square>(currentPiece.GetAvailableMoves(Board, currentSquare).ToList());
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

            var moves = currentPiece.GetAvailableMoves(Board, currentSquare);

            if (moves.Contains(message.Square))
            {
                currentPiece.MoveTo(Board, message.Square);

                PiecesMoved();
              //  ChessingtonServices.EventAggregator.Publish(new PiecesMoved(Board));
                ChessingtonServices.EventAggregator.Publish(new SelectionCleared());
            }
        }

        private static void BoardOnPieceCaptured(Piece piece)
        {
            ChessingtonServices.EventAggregator.Publish(new PieceTaken(piece));
        }

        private void BoardOnCurrentPlayerChanged(Player player)
        {
            ChessingtonServices.EventAggregator.Publish(new CurrentPlayerChanged(player));
           
        }

        public delegate void BackgroundWork();
        public delegate void UpdateUI(Move move);

        private void ComputerTask()
        {
            var move = ComputerPlayer.MakeMove(Board);
            Dispatcher.BeginInvoke(new UpdateUI(CompleteComputerMove), move);
        }

        private void CompleteComputerMove(Move move)
        {
            if (move != null)
            {
                Console.WriteLine("Move played: {0}", move);
                Board.GetPiece(move.From).MoveTo(Board, move.To);
                PiecesMoved();
            //    ChessingtonServices.EventAggregator.Publish(new PiecesMoved(Board));
            }
        }
    }
}