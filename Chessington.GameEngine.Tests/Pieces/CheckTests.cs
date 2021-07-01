using Chessington.GameEngine.Pieces;
using FluentAssertions;
using NUnit.Framework;

namespace Chessington.GameEngine.Tests.Pieces
{
    [TestFixture]
    public class CheckTests
    {
        [Test]
        public void CanBeInCheck() {
            var board = new Board();
            var queen = new Queen(Player.Black);
            var king = new King(Player.White);
            board.AddPiece(Square.At(0,0), queen);
            board.AddPiece(Square.At(0,7), king);
            board.InCheck(Player.White).Should().BeTrue();
        }

        [Test]
        public void CantPutSelfInCheck() {
            var board = new Board();
            var queen = new Queen(Player.Black);
            var king = new King(Player.White);
            board.AddPiece(Square.At(1,0), queen);
            board.AddPiece(Square.At(0,7), king);

            var moves = king.GetAvailableMoves(board, Square.At(0, 7));
            moves.Should().Contain(Square.At(0,6)).And.NotContain(new Square(1, 7));
        }
    }
}