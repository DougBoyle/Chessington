using Chessington.GameEngine.Pieces;
using FluentAssertions;
using NUnit.Framework;

namespace Chessington.GameEngine.Tests
{
    [TestFixture]
    public class BoardTests
    {
        [Test]
        public void PawnCanBeAddedToBoard()
        {
            var board = new Board();
            var pawn = new Pawn(Player.White);
            board.AddPiece(Square.At(0, 0), pawn);

            // no longer use board[,], so can't guarentee same instance
            //  board.GetPiece(Square.At(0, 0)).Should().BeSameAs(pawn);
            board.GetPiece(Square.At(0, 0)).Should().Match(piece => piece is Pawn && ((Pawn)piece).Player == Player.White);
        }
    }
}
