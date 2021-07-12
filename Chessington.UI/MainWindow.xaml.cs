﻿using System.Windows;
using Chessington.UI.Factories;
using Chessington.UI.ViewModels;

using System.Windows.Controls;

namespace Chessington.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new GameViewModel();
            
            var boardViewModel = new BoardViewModel();
            GameEngine.StartingPositionFactory.Setup(boardViewModel.Board);
            BoardViewFactory.CreateBoardView(boardViewModel, GridContainer);

            boardViewModel.PiecesMoved();
        }
    }
}
