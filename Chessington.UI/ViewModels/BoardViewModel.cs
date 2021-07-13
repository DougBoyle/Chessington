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

            var moves = new ReadOnlyCollection<Square>(
                Board.GetAllAvailableMoves()
                .Where(move => BitUtils.IndexToSquare(move.FromIdx) == currentSquare)
                .Select(move => BitUtils.IndexToSquare(move.ToIdx)).ToList());
            ChessingtonServices.EventAggregator.Publish(new ValidMovesUpdated(moves));
        }

        public void Handle(SelectionCleared message)
        {
            currentPiece = null;
        }

        // TODO: Modify to handle promotion, display something to player
        public void Handle(SquareSelected message)
        {
            if (promoting)
            {
                promoting = false; // clicking anywhere else undos choice
                NotifyPromotionChanged(null);
            }

            var piece = Board.GetPiece(message.Square);
            if (piece != null && piece.Player == Board.CurrentPlayer)
            {
                ChessingtonServices.EventAggregator.Publish(new PieceSelected(message.Square));
                return;
            }

            if (currentPiece == null)
                return;

            var moves = Board.GetAllAvailableMoves()
                .Where(move => move.FromIdx == BitUtils.SquareToIndex(currentSquare) &&
                               move.ToIdx == BitUtils.SquareToIndex(message.Square));

            if (moves.Any())
            {
                if (currentPiece.PieceType == PieceType.Pawn && (message.Square.Row == 0 || message.Square.Row == 7))
                {
                    // prompt about promotion
                    promoting = true;
                    promotionSquare = message.Square;
                    NotifyPromotionChanged(currentPiece.Player);
                    return;
                }

                Board.MakeMove(moves.First()); // should only be 1 possibility if not a promotion

                PiecesMoved();
              //  ChessingtonServices.EventAggregator.Publish(new PiecesMoved(Board));
                ChessingtonServices.EventAggregator.Publish(new SelectionCleared());
            }
        }

        private bool promoting;
        private Square promotionSquare; // remember for when move made

        public EventHandler<Player?> PromotionChanged; // promotion has become selected/deselected

        private void NotifyPromotionChanged(Player? player)
        {
            // TODO: Why are events raised in this way?
            var handler = PromotionChanged;
            handler?.Invoke(this, player);
        }

        // handler for button press for promotion
        public void PromotionChoiceMade(PieceType pieceType)
        {
            if (!promoting) return;

            
            promoting = false; // clicking anywhere else undos choice
            NotifyPromotionChanged(null);

            // Allow choice of how to promote
            // Same as completing a regular move
            var move = Board.GetAllAvailableMoves().First(m => m.PromotionPiece % 6 == (int)pieceType &&
                m.FromIdx == BitUtils.SquareToIndex(currentSquare) && m.ToIdx == BitUtils.SquareToIndex(promotionSquare));

            Board.MakeMove(move);

            PiecesMoved();
            //  ChessingtonServices.EventAggregator.Publish(new PiecesMoved(Board));
            ChessingtonServices.EventAggregator.Publish(new SelectionCleared());
        }

        private static void BoardOnPieceCaptured(int piece)
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
                Board.MakeMove(move);
                PiecesMoved();
            //    ChessingtonServices.EventAggregator.Publish(new PiecesMoved(Board));
            }
        }
    }
}