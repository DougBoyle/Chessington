using System;
using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

namespace Chessington.GameEngine.Pieces {
    public class Pawn : Piece {
        public Pawn(Player player)
            : base(player) { PieceType = PieceType.Pawn; }


        private IEnumerable<Move> SquaresToMoves(Board board, Square here, IEnumerable<Square> squares)
        {
            List<Move> result = new List<Move>();
            foreach (Square to in squares)
            {
                if (to.Row == 7 || to.Row == 0)
                {
                    result.Add(new Move(here, to, this, board.GetPiece(to), new Knight(Player)));
                    result.Add(new Move(here, to, this, board.GetPiece(to), new Rook(Player)));
                    result.Add(new Move(here, to, this, board.GetPiece(to), new Bishop(Player)));
                    result.Add(new Move(here, to, this, board.GetPiece(to), new Queen(Player)));
                } else
                {
                    result.Add(new Move(here, to, this, board.GetPiece(to), null));
                }
            }
            return result;
        }

        // creates list of possible squares, then generates promotions where needed
        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square here)
        {
            var direction = Player == Player.White ? -1 : 1;
            var homeRow = Player == Player.White ? GameSettings.BoardSize - 2 : 1;

            var available = new List<Square>()
            {
                new Square(here.Row + direction, here.Col - 1),
                new Square(here.Row + direction, here.Col + 1)
            }.Where(square => square.IsValid() && board.IsOpponent(square, Player)
                              || square.Equals(board.EnPassantSquare)).ToList();
            Square targetMove = new Square(here.Row + direction, here.Col);

            if (!targetMove.IsValid() || !board.IsSquareEmpty(targetMove))
            {
                return SquaresToMoves(board, here, available);
            }
            available.Add(targetMove);

            Square targetMove2 = new Square(here.Row + 2 * direction, here.Col);
            if (here.Row == homeRow && board.IsSquareEmpty(targetMove2))

            {
                available.Add(targetMove2);
            }

            return SquaresToMoves(board, here, available);
        }

      
        public override void MoveTo(Board board, Move move)
        {
            var currentSquare = move.From;
            var newSquare = move.To;
            if (newSquare.Equals(board.EnPassantSquare))
            {
                board.OnPieceCaptured(board.GetPiece(Square.At(currentSquare.Row, newSquare.Col)));
                board.AddPiece(Square.At(currentSquare.Row, newSquare.Col), null);
            }
            base.MoveTo(board, move); // changes the current player
            if (currentSquare.Row - newSquare.Row == 2 || currentSquare.Row - newSquare.Row == -2)
            {
                board.EnPassantSquare = Square.At((currentSquare.Row + newSquare.Row) / 2, newSquare.Col);
            }
            if (move.Promotion != null)
            {
                board.OnPieceCaptured(this);
                board.AddPiece(newSquare, move.Promotion);

            }
            /*
            if (newSquare.Row == 0 || newSquare.Row == 7) // TODO: Allow choosing how to promote
            {
                board.OnPieceCaptured(this);
                board.AddPiece(newSquare, new Queen(Player));
            }*/
        }

        public override void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            // handles en-passant capture, otherwise just fall back to base case
            if (move.To.Col != move.From.Col && move.Captured == null)
            {
                // en-passant, restore piece
                board.AddPiece(Square.At(move.From.Row, move.To.Col), new Pawn(board.CurrentPlayer));

            }
            base.UndoMove(board, move, info);
        }
    }
}