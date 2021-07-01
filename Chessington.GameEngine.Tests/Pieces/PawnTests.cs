using System.Linq;
using Chessington.GameEngine.Pieces;
using FluentAssertions;
using NUnit.Framework;

namespace Chessington.GameEngine.Tests.Pieces
{
    [TestFixture]
    public class PawnTests
    {
        [Test]
        public void WhitePawns_CanMoveOneSquareUp()
        {
            var board = new Board();
            var pawn = new Pawn(Player.White);
            board.AddPiece(Square.At(6, 0), pawn);

            var moves = pawn.GetAvailableMoves(board, Square.At(6, 0));

            moves.Should().Contain(Square.At(5, 0));
        }

        [Test]
        public void BlackPawns_CanMoveOneSquareDown()
        {
            var board = new Board(Player.Black);
            var pawn = new Pawn(Player.Black);
            board.AddPiece(Square.At(1, 0), pawn);

            var moves = pawn.GetAvailableMoves(board, Square.At(1, 0));

            moves.Should().Contain(Square.At(2, 0));
        }
        [Test]
        public void WhitePawns_WhichHaveNeverMoved_CanMoveTwoSquareUp()
        {
            var board = new Board();
            var pawn = new Pawn(Player.White);
            board.AddPiece(Square.At(6, 5), pawn);

            var moves = pawn.GetAvailableMoves(board, Square.At(6, 5));

            moves.Should().Contain(Square.At(4, 5));
        }

        [Test]
        public void BlackPawns_WhichHaveNeverMoved_CanMoveTwoSquareUp()
        {
            var board = new Board(Player.Black);
            var pawn = new Pawn(Player.Black);
            board.AddPiece(Square.At(1, 3), pawn);

            var moves = pawn.GetAvailableMoves(board, Square.At(1, 3));

            moves.Should().Contain(Square.At(3, 3));
        }

        [Test]
        public void WhitePawns_WhichHaveAlreadyMoved_CanOnlyMoveOneSquare()
        {
            var board = new Board();
            var pawn = new Pawn(Player.White);
            board.AddPiece(Square.At(6, 2), pawn);

            pawn.MoveTo(board, Square.At(5, 2));
            board.CurrentPlayer = Player.White;
            var moves = pawn.GetAvailableMoves(board, Square.At(5, 2)).ToList();

            moves.Should().HaveCount(1);
            moves.Should().Contain(square => square.Equals(Square.At(4, 2)));
        }

        [Test]
        public void BlackPawns_WhichHaveAlreadyMoved_CanOnlyMoveOneSquare()
        {
            var board = new Board(Player.Black);
            var pawn = new Pawn(Player.Black);
            board.AddPiece(Square.At(5, 2), pawn);

            pawn.MoveTo(board, Square.At(6, 2));
            board.CurrentPlayer = Player.Black;
            var moves = pawn.GetAvailableMoves(board, Square.At(6, 2)).ToList();

            moves.Should().HaveCount(1);
            moves.Should().Contain(square => square.Equals(Square.At(7, 2)));
        }

        [Test]
        public void Pawns_CannotMove_IfThereIsAPieceInFront()
        {
            var board = new Board(Player.Black);
            var pawn = new Pawn(Player.Black);
            var blockingPiece = new Rook(Player.White);
            board.AddPiece(Square.At(1, 3), pawn);
            board.AddPiece(Square.At(2, 3), blockingPiece);

            var moves = pawn.GetAvailableMoves(board, Square.At(1, 3));

            moves.Should().BeEmpty();
        }

        [Test]
        public void Pawns_CannotMoveTwoSquares_IfThereIsAPieceTwoSquaresInFront()
        {
            var board = new Board(Player.Black);
            var pawn = new Pawn(Player.Black);
            var blockingPiece = new Rook(Player.White);
            board.AddPiece(Square.At(1, 3), pawn);
            board.AddPiece(Square.At(3, 3), blockingPiece);

            var moves = pawn.GetAvailableMoves(board, Square.At(1, 3));

            moves.Should().NotContain(Square.At(3, 3));
        }

        [Test]
        public void WhitePawns_CannotMove_AtTheTopOfTheBoard()
        {
            var board = new Board();
            var pawn = new Pawn(Player.White);
            board.AddPiece(Square.At(0, 3), pawn);

            var moves = pawn.GetAvailableMoves(board, Square.At(0, 3));

            moves.Should().BeEmpty();
        }

        [Test]
        public void BlackPawns_CannotMove_AtTheBottomOfTheBoard()
        {
            var board = new Board();
            var pawn = new Pawn(Player.Black);
            board.AddPiece(Square.At(7, 3), pawn);

            var moves = pawn.GetAvailableMoves(board, Square.At(7, 3));

            moves.Should().BeEmpty();
        }

        [Test]
        public void BlackPawns_CanMoveDiagonally_IfThereIsAPieceToTake()
        {
            var board = new Board(Player.Black);
            var pawn = new Pawn(Player.Black);
            var firstTarget = new Pawn(Player.White);
            var secondTarget = new Pawn(Player.White);
            board.AddPiece(Square.At(5, 3), pawn);
            board.AddPiece(Square.At(6, 4), firstTarget);
            board.AddPiece(Square.At(6, 2), secondTarget);

            var moves = pawn.GetAvailableMoves(board, Square.At(5, 3)).ToList();

            moves.Should().Contain(Square.At(6, 2));
            moves.Should().Contain(Square.At(6, 4));
        }

        [Test]
        public void WhitePawns_CanMoveDiagonally_IfThereIsAPieceToTake()
        {
            var board = new Board();
            var pawn = new Pawn(Player.White);
            var firstTarget = new Pawn(Player.Black);
            var secondTarget = new Pawn(Player.Black);
            board.AddPiece(Square.At(7, 3), pawn);
            board.AddPiece(Square.At(6, 4), firstTarget);
            board.AddPiece(Square.At(6, 2), secondTarget);

            var moves = pawn.GetAvailableMoves(board, Square.At(7, 3)).ToList();

            moves.Should().Contain(Square.At(6, 2));
            moves.Should().Contain(Square.At(6, 4));
        }

        [Test]
        public void BlackPawns_CannotMoveDiagonally_IfThereIsNoPieceToTake()
        {
            var board = new Board(Player.Black);
            var pawn = new Pawn(Player.Black);
            board.AddPiece(Square.At(5, 3), pawn);

            var friendlyPiece = new Pawn(Player.Black);
            board.AddPiece(Square.At(6, 2), friendlyPiece);

            var moves = pawn.GetAvailableMoves(board, Square.At(5, 3)).ToList();

            moves.Should().NotContain(Square.At(6, 2));
            moves.Should().NotContain(Square.At(6, 4));
        }

        [Test]
        public void WhitePawns_CannotMoveDiagonally_IfThereIsNoPieceToTake()
        {
            var board = new Board();
            var pawn = new Pawn(Player.White);
            board.AddPiece(Square.At(7, 3), pawn);

            var friendlyPiece = new Pawn(Player.White);
            board.AddPiece(Square.At(6, 2), friendlyPiece);

            var moves = pawn.GetAvailableMoves(board, Square.At(7, 3)).ToList();

            moves.Should().NotContain(Square.At(6, 2));
            moves.Should().NotContain(Square.At(6, 4));
        }
        
        [Test]
        public void WhitePawns_CanTakeEnPassant()
        {
            var board = new Board(Player.Black);
            var whitePawn = new Pawn(Player.White);
            board.AddPiece(Square.At(3, 4), whitePawn);
            
            var blackPawn =new Pawn(Player.Black);
            board.AddPiece(Square.At(1,3), blackPawn);
            
            blackPawn.MoveTo(board, Square.At(3, 3));
            var moves = whitePawn.GetAvailableMoves(board, Square.At(3, 4)).ToList();

            moves.Should().Contain(Square.At(2, 3));
            
            whitePawn.MoveTo(board, Square.At(2,3));
            board.IsSquareEmpty(Square.At(3, 3)).Should().BeTrue();
        }
        
        [Test]
        public void BlackPawns_CanTakeEnPassant()
        {
            var board = new Board();
            var whitePawn = new Pawn(Player.White);
            board.AddPiece(Square.At(6, 6), whitePawn);
            
            var blackPawn =new Pawn(Player.Black);
            board.AddPiece(Square.At(4,5), blackPawn);
            
            whitePawn.MoveTo(board, Square.At(4, 6));
            var moves = blackPawn.GetAvailableMoves(board, Square.At(4, 5)).ToList();

            moves.Should().Contain(Square.At(5, 6));
            
            blackPawn.MoveTo(board, Square.At(5,6));
            board.IsSquareEmpty(Square.At(4, 6)).Should().BeTrue();
        }
        
         
        [Test]
        public void WhitePawns_CanNoLonger_TakeEnPassant()
        {
            var board = new Board(Player.Black);
            var whitePawn = new Pawn(Player.White);
            board.AddPiece(Square.At(3, 4), whitePawn);
            var blackPawn =new Pawn(Player.Black);
            board.AddPiece(Square.At(1,3), blackPawn);
            
            var whiteBishop = new Bishop(Player.White);
            var blackBishop = new Bishop(Player.Black);
            board.AddPiece(Square.At(7,7), whiteBishop);
            board.AddPiece(Square.At(7,6),blackBishop);
            
            blackPawn.MoveTo(board, Square.At(3, 3));
            whiteBishop.MoveTo(board,Square.At(6,6));
            blackBishop.MoveTo(board,Square.At(6,7));
            
            var moves = whitePawn.GetAvailableMoves(board, Square.At(3, 4)).ToList();
            moves.Should().NotContain(Square.At(2, 3));
        }
        
        [Test]
        public void BlackPawns_CanNoLonger_TakeEnPassant()
        {
            var board = new Board();
            var whitePawn = new Pawn(Player.White);
            board.AddPiece(Square.At(6, 6), whitePawn);
            var blackPawn =new Pawn(Player.Black);
            board.AddPiece(Square.At(4,5), blackPawn);

            var whiteBishop = new Bishop(Player.White);
            var blackBishop = new Bishop(Player.Black);
            board.AddPiece(Square.At(0,0), whiteBishop);
            board.AddPiece(Square.At(1,0),blackBishop);
            
            whitePawn.MoveTo(board, Square.At(4, 6));
            blackBishop.MoveTo(board,Square.At(2,1));
            whiteBishop.MoveTo(board,Square.At(1,1));
            
            var moves = blackPawn.GetAvailableMoves(board, Square.At(4, 5)).ToList();
            moves.Should().NotContain(Square.At(5, 6));
        }

    }
}