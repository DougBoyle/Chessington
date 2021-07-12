using Chessington.GameEngine.Pieces;

namespace Chessington.UI.Notifications
{
    public class PieceTaken
    {
        public PieceTaken(int index)
        {
            PieceIndex = index;
        }

        public int PieceIndex { get; set; }
    }
}