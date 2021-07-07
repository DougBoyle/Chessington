using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using Chessington.GameEngine.Pieces;
using Chessington.GameEngine;

namespace Chessington.UI.Factories
{
    /// <summary>
    /// Given a piece, returns in image for that piece. Change things here if you want new icons.
    /// </summary>
    public static class PieceImageFactory
    {
        private static readonly Dictionary<Type, string> PieceSuffixes = new Dictionary<Type, string>
        {
            { typeof(Pawn), "P" },
            { typeof(Rook), "R" },
            { typeof(Knight), "N" },
            { typeof(Bishop), "B" },
            { typeof(King), "K" },
            { typeof(Queen), "Q" },
        };

        // these two used for promotion 'popup'
        private static readonly Dictionary<PieceType, string> PieceTypeSuffixes = new Dictionary<PieceType, string>
        {
            { PieceType.Pawn, "P" },
            { PieceType.Rook, "R" },
            { PieceType.Knight, "N" },
            { PieceType.Bishop, "B" },
            { PieceType.King, "K" },
            { PieceType.Queen, "Q" },
        };

        public static BitmapImage GetImage(PieceType piece, Player player)
        {
            return new BitmapImage(new Uri(string.Format("{0}{1} {2}.ico", InterfaceSettings.IconRoot, player, PieceTypeSuffixes[piece])));
        }


        public static BitmapImage GetImage(Piece piece)
        {
            return new BitmapImage(new Uri(string.Format("{0}{1} {2}.ico", InterfaceSettings.IconRoot, piece.Player, PieceSuffixes[piece.GetType()])));
        }
        
        public static BitmapImage GetRedImage(Piece piece)
        {
            return new BitmapImage(new Uri(string.Format("{0}{1} K Red.ico", InterfaceSettings.IconRoot, piece.Player)));
        }
    }
}