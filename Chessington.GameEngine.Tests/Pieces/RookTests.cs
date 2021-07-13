using System.Collections.Generic;
using System.Linq;
using Chessington.GameEngine.Pieces;
using FluentAssertions;
using NUnit.Framework;

namespace Chessington.GameEngine.Tests.Pieces
{
    [TestFixture]
    public class RookTests
    {
        [Test]
        public void Rook_CanMove_Laterally()
        {
            var board = new Board();
            var rook = new Rook(Player.White);
            board.AddPiece(Square.At(4, 4), rook);

            var moves = board.GetAllAvailableMoves()
                .Select(move => BitUtils.IndexToSquare(move.ToIdx));
            var expectedMoves = new List<Square>();

            for (var i = 0; i < 8; i++)
                expectedMoves.Add(Square.At(4, i));

            for (var i = 0; i < 8; i++)
                expectedMoves.Add(Square.At(i, 4));

            //Get rid of our starting location.
            expectedMoves.RemoveAll(s => s == Square.At(4, 4));

            moves.Should().Contain(expectedMoves);
        }

        [Test]
        public void Rook_CannnotPassThrough_OpposingPieces()
        {
            var board = new Board();
            var rook = new Rook(Player.White);
            board.AddPiece(Square.At(4, 4), rook);
            var pieceToTake = new Pawn(Player.Black);
            board.AddPiece(Square.At(4, 6), pieceToTake);

            var moves = board.GetAllAvailableMoves()
                .Select(move => BitUtils.IndexToSquare(move.ToIdx));
            moves.Should().NotContain(Square.At(4, 7));
        }

        [Test]
        public void Rook_CannnotPassThrough_FriendlyPieces()
        {
            var board = new Board();
            var rook = new Rook(Player.White);
            board.AddPiece(Square.At(4, 4), rook);
            var friendlyPiece = new Pawn(Player.White);
            board.AddPiece(Square.At(4, 6), friendlyPiece);

            var moves = board.GetAllAvailableMoves()
                .Where(move => move.FromIdx == BitUtils.SquareToIndex(Square.At(4, 4)))
                .Select(move => BitUtils.IndexToSquare(move.ToIdx));
            moves.Should().NotContain(Square.At(4, 7));
        }

        [Test]
        public void Rook_CanTake_OpposingPieces()
        {
            var board = new Board();
            var rook = new Rook(Player.White);
            board.AddPiece(Square.At(4, 4), rook);
            var pieceToTake = new Pawn(Player.Black);
            board.AddPiece(Square.At(4, 6), pieceToTake);

            var moves = board.GetAllAvailableMoves()
                .Select(move => BitUtils.IndexToSquare(move.ToIdx));
            moves.Should().Contain(Square.At(4, 6));
        }

        [Test]
        public void Rook_CannotTake_FriendlyPieces()
        {
            var board = new Board();
            var rook = new Rook(Player.White);
            board.AddPiece(Square.At(4, 4), rook);
            var friendlyPiece = new Pawn(Player.White);
            board.AddPiece(Square.At(4, 6), friendlyPiece);

            var moves = board.GetAllAvailableMoves()
                 .Where(move => move.FromIdx == BitUtils.SquareToIndex(Square.At(4, 4)))
                 .Select(move => BitUtils.IndexToSquare(move.ToIdx));
            moves.Should().NotContain(Square.At(4, 6));
        }
    }
}