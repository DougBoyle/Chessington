using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using Chessington.GameEngine;
using Chessington.GameEngine.Pieces;
using Chessington.UI.Caliburn.Micro;
using Chessington.UI.Factories;
using Chessington.UI.Notifications;
using Chessington.UI.Properties;

namespace Chessington.UI.ViewModels
{
    // different view model, as this needs to respond to much less.
    // effectively just needs to become visible or not - no highlighting/changing which piece displayed?
    public class PromotionSquareViewModel : INotifyPropertyChanged  //, 
     //   IHandle<PiecesMoved>, 
     //   IHandle<PieceSelected>, 
     //   IHandle<SelectionCleared>
    {
        private BitmapImage image;

        public PromotionSquareViewModel(PieceType pieceType, BoardViewModel parent)
        {
            this.pieceType = pieceType;

            //    ChessingtonServices.EventAggregator.Subscribe(this);
            // as a change - subscribes/handles things directly, rather than a heavyweight aggregator/new messages
            // Compilcation: Some functions/passing around is made harder, since BoardViewModel is pass to Factory
            //               which is what actually constructs this PromotionSquareViewModel
            parent.PromotionChanged += SetImage;
            
        }

        private PieceType pieceType;
        public PieceType PieceType
        {
            get { return pieceType; }
            set
            {
                if (value.Equals(pieceType)) return;
                pieceType = value;
                OnPropertyChanged();
                OnPropertyChanged("Self");
            }
        }

        public PromotionSquareViewModel Self { get { return this; } }


        public BitmapImage Image
        {
            get { return image; }
            set
            {
                if (Equals(value, image)) return;
                image = value;
                OnPropertyChanged();
                OnPropertyChanged("Self");
            }
        }

        // 'sender' is just there to match delegate type needed for event handler
        public void SetImage(object sender, Player? playerNullable)
        {
            if (playerNullable is Player player) Image = PieceImageFactory.GetImage(pieceType, player);
            else Image = null;
        }

        // replaces event below
        public void SetImage(Piece piece)
        {
            if (piece == null) Image = null;
            Image = PieceImageFactory.GetImage(piece);
        }

        // TODO: Need to change this as square isn't actually part of board
        /*
        public void Handle(PiecesMoved notification)
        {
            var currentPiece = notification.Board.GetPiece(square);

            if (currentPiece == null)
            {
                Image = null;
                return;
            }

            if (notification.Board.CurrentPlayer == currentPiece.Player && currentPiece is King
                && notification.Board.InCheck(currentPiece.Player)) {
                Image = PieceImageFactory.GetRedImage(currentPiece);
            }
            else {
                Image = PieceImageFactory.GetImage(currentPiece);
            }
        }*/
        

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}