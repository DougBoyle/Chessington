using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Chessington.GameEngine;
using Chessington.UI.Converters;
using Chessington.UI.Notifications;
using Chessington.UI.ViewModels;

using Chessington.GameEngine.Pieces;

namespace Chessington.UI.Factories
{
    /// <summary>
    /// Creates a view which can be used to render a board.
    /// 
    /// We do this in code because the XAML would be too verbose.
    /// </summary>
    public static class BoardViewFactory
    {
        public static void CreateBoardView(BoardViewModel boardViewModel, Panel parent)
        {
            var grid = new Grid();
            grid.MaxWidth = grid.MaxHeight = grid.Width = grid.Height = GameSettings.BoardSize * InterfaceSettings.SquareSize;
            grid.DataContext = boardViewModel;
            parent.Children.Add(grid);

            for (var i = 0; i < GameSettings.BoardSize; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            for (var row = 0; row < GameSettings.BoardSize; row++)
                for (var col = 0; col < GameSettings.BoardSize; col++)
                    CreateSquare(row, col, grid);

            // popup for promotions - mostly following pattern above
            var promotions = new Grid();
            promotions.MaxWidth = promotions.Width = 4 * InterfaceSettings.SquareSize; ;
            promotions.MaxHeight = promotions.Height = InterfaceSettings.SquareSize; ;
            promotions.RowDefinitions.Add(new RowDefinition());
            for (int i = 0; i < 4; i++) promotions.ColumnDefinitions.Add(new ColumnDefinition());
            for (int i = 0; i < 4; i++) CreatePromotionSquare(i, promotions, boardViewModel);


           // var textBlock = new TextBlock();

          //  textBlock.Text = "For promotions";
          //  textBlock.HorizontalAlignment = HorizontalAlignment.Center;
            parent.Children.Add(promotions);
        }

        private static void CreateSquare(int row, int col, Panel grid)
        {
            var squareColor = (row + col) % 2 == 0 ? Colors.White : Colors.Black;
            var square = new Canvas { Width = InterfaceSettings.SquareSize, Height = InterfaceSettings.SquareSize, Background = new SolidColorBrush(squareColor) };

            grid.Children.Add(square);
            Grid.SetRow(square, row);
            Grid.SetColumn(square, col);

            var squareViewModel = new SquareViewModel(Square.At(row, col));
            square.DataContext = squareViewModel;

            var pieceImage = new Image();
            BindingOperations.SetBinding(pieceImage, Image.SourceProperty, new Binding("Image"));
            pieceImage.Height = pieceImage.Width = (InterfaceSettings.SquareSize - 4); //Allow for border space...
            
            var pieceBorder = new Border { BorderThickness = new Thickness(2) };
            BindingOperations.SetBinding(pieceBorder, Border.BorderBrushProperty, new Binding("Self") { Converter = new SquareToBorderBrushConverter() });
            pieceBorder.Child = pieceImage;
            
            square.MouseDown += SquareOnMouseDown;
            
            square.Children.Add(pieceBorder);
        }

        private static void SquareOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            var square = (sender as Canvas).DataContext as SquareViewModel;
            ChessingtonServices.EventAggregator.Publish(new SquareSelected(square.Location)); 
            mouseButtonEventArgs.Handled = true;
        }


        // TODO: A way to construct pieces by piecetype would be useful
        private static readonly PieceType[] promotionOptions = { PieceType.Knight, PieceType.Bishop, PieceType.Rook, PieceType.Queen };

        private static void CreatePromotionSquare(int col, Panel grid, BoardViewModel boardViewModel)
        {
            var squareColor = Colors.White;
            var square = new Canvas { Width = InterfaceSettings.SquareSize, Height = InterfaceSettings.SquareSize, Background = new SolidColorBrush(squareColor) };

            grid.Children.Add(square);
            Grid.SetRow(square, 0);
            Grid.SetColumn(square, col);

            var pieceType = promotionOptions[col];

            // TODO: Needed or not? (Or need new viewModel for this? Need to know which piece clicked somehow)
            var squareViewModel = new PromotionSquareViewModel(pieceType, boardViewModel);
            square.DataContext = squareViewModel; // allows "Image" property below to be found


            Piece piece;
            switch (promotionOptions[col])
            {
                case PieceType.Knight: piece = new Knight(Player.White); break;
                case PieceType.Bishop: piece = new Bishop(Player.White); break;
                case PieceType.Rook: piece = new Rook(Player.White); break;
                default: piece = new Queen(Player.White); break;
            }

            // TODO: Change this
            //  squareViewModel.SetImage(piece); // set directly here, won't change
            squareViewModel.SetImage(null, null);

            var pieceImage = new Image();

          //  pieceImage.Source = PieceImageFactory.GetImage(piece); // TODO: GetImage should really take 'pieceType' not actual Piece class
            BindingOperations.SetBinding(pieceImage, Image.SourceProperty, new Binding("Image"));
            pieceImage.Height = pieceImage.Width = (InterfaceSettings.SquareSize - 4); //Allow for border space...

            var pieceBorder = new Border { BorderThickness = new Thickness(2) };
        //    BindingOperations.SetBinding(pieceBorder, Border.BorderBrushProperty, new Binding("Self") { Converter = new SquareToBorderBrushConverter() });
            pieceBorder.Child = pieceImage;

            square.MouseDown += (sender, args) => boardViewModel.PromotionChoiceMade(pieceType);

            square.Children.Add(pieceBorder);
        }
    }
}