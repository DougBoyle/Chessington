using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chessington.GameEngine.AI
{
    public struct GameExtraInfo
    {
        public bool LeftWhiteCastling;
        public bool RightWhiteCastling;
        public bool LeftBlackCastling;
        public bool RightBlackCastling;
        public Square? EnPassantSquare { get; set; }

        // Not actually necessary, but easier for now
        public Player CurrentPlayer { get; set; }

        // TODO: 50 count to stalemate

        public GameExtraInfo(Board b)
        {
            LeftWhiteCastling = b.LeftWhiteCastling;
            RightWhiteCastling = b.RightWhiteCastling;
            LeftBlackCastling = b.LeftBlackCastling;
            RightBlackCastling = b.RightBlackCastling;
            EnPassantSquare = b.EnPassantSquare;
            CurrentPlayer = b.CurrentPlayer;
        }

        public void RestoreInfo(Board b)
        {
            b.LeftWhiteCastling = LeftWhiteCastling;
            b.RightWhiteCastling = RightWhiteCastling;
            b.LeftBlackCastling = LeftBlackCastling;
            b.RightBlackCastling = RightBlackCastling;
            b.EnPassantSquare = EnPassantSquare;
            b.CurrentPlayer = CurrentPlayer;
        }
    }
}
