using System.Collections.Generic;
using System.Linq;
using Chessington.GameEngine.Pieces;
using FluentAssertions;
using NUnit.Framework;

namespace Chessington.GameEngine.Tests.Pieces
{
    [TestFixture]
    public class KingTests
    {
        [Test]
        public void KingsCanMoveToAdjacentSquares()
        {
            var board = new Board();
            // need to indicate that king cannot castle (due to how castling checked for)
            board.Castling = 0;

            var king = new King(Player.White);
            board.AddPiece(Square.At(4, 4), king);

            var moves = board.GetAllAvailableMoves()
                .Select(move => BitUtils.IndexToSquare(move.ToIdx));

            var expectedMoves = new List<Square>
            {
                Square.At(3, 3),
                Square.At(3, 4),
                Square.At(3, 5),
                Square.At(4, 3),
                Square.At(4, 5),
                Square.At(5, 3),
                Square.At(5, 4),
                Square.At(5, 5)
            };

            moves.ShouldAllBeEquivalentTo(expectedMoves);
        }

        [Test]
        public void Kings_CannotLeaveTheBoard()
        {
            var board = new Board();
            // need to indicate that king cannot castle (due to how castling checked for)
            board.Castling = 0;

            var king = new King(Player.White);
            board.AddPiece(Square.At(0, 0), king);

            var moves = board.GetAllAvailableMoves()
                .Select(move => BitUtils.IndexToSquare(move.ToIdx));

            var expectedMoves = new List<Square>
            {
                Square.At(1, 0),
                Square.At(1, 1),
                Square.At(0, 1)
            };

            moves.ShouldAllBeEquivalentTo(expectedMoves);
        }

        [Test]
        public void Kings_CanTakeOpposingPieces()
        {
            var board = new Board();
            var king = new King(Player.White);
            board.AddPiece(Square.At(4, 4), king);
            var pawn = new Pawn(Player.Black);
            board.AddPiece(Square.At(4, 5), pawn);

            var moves = board.GetAllAvailableMoves()
                .Select(move => BitUtils.IndexToSquare(move.ToIdx));
            moves.Should().Contain(Square.At(4, 5));
        }

        [Test]
        public void Kings_CannotTakeFriendlyPieces()
        {
            var board = new Board();
            var king = new King(Player.White);
            board.AddPiece(Square.At(4, 4), king);
            var pawn = new Pawn(Player.White);
            board.AddPiece(Square.At(4, 5), pawn);

            var moves = board.GetAllAvailableMoves()
                .Where(move => move.FromIdx == BitUtils.SquareToIndex(Square.At(4, 4)))
                .Select(move => BitUtils.IndexToSquare(move.ToIdx));
            moves.Should().NotContain(Square.At(4, 5));
        }

        [Test]
        public void WhiteKingCanCastle()
        {
            var board = new Board();
            var king = new King(Player.White);
            board.AddPiece(Square.At(7, 4), king);
            var rook = new Rook(Player.White);
            board.AddPiece(Square.At(7, 0), rook);
            rook = new Rook(Player.White);
            board.AddPiece(Square.At(7, 7), rook);

            // add a piece so that long castling not currently valid
            var pawn = new Pawn(Player.White);
            board.AddPiece(Square.At(7, 1), pawn);

            var moves = board.GetAllAvailableMoves()
                .Where(move => move.FromIdx == BitUtils.SquareToIndex(Square.At(7, 4)))
                .Select(move => BitUtils.IndexToSquare(move.ToIdx));
            
            var expectedMoves = new List<Square>
            {
                // normal moves
                Square.At(7, 3),
                Square.At(6, 3),
                Square.At(6, 4),
                Square.At(7, 5),
                Square.At(6, 5),
                Square.At(7, 6) // castling move
            };

            moves.ShouldAllBeEquivalentTo(expectedMoves);
        }
    }
}