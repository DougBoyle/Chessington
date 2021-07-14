using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chessington.GameEngine.AI
{
    public struct GameExtraInfo
    {
        public byte Castling;
        public byte EnPassantIndex;

        // Not actually necessary, but easier for now
        public Player CurrentPlayer { get; set; }

        // TODO: 50 count to stalemate

        public GameExtraInfo(Board b)
        {
            Castling = b.Castling;
            EnPassantIndex = b.EnPassantIndex;
            CurrentPlayer = b.CurrentPlayer;
        }

        public void RestoreInfo(Board b)
        {
            b.Castling = Castling;
            b.EnPassantIndex = EnPassantIndex;
            b.CurrentPlayer = CurrentPlayer;
        }
    }
}
