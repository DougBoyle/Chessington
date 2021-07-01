using System;
using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

namespace Chessington.GameEngine.Pieces {
    public class Pawn : Piece {
        public Pawn(Player player)
            : base(player) { PieceType = PieceType.Pawn; }

        public override IEnumerable<Square> GetRelaxedAvailableMoves(Board board) {
            
            Square here = board.FindPiece(this);
            
            var direction = Player == Player.White ? -1 : 1;
            var homeRow = Player == Player.White ? GameSettings.BoardSize - 2 : 1;
            
            var available = new List<Square>()
            {

                new Square(here.Row + direction, here.Col - 1), 
                new Square(here.Row + direction, here.Col + 1)
            }.Where(square => square.IsValid() && board.IsOpponent(square, Player)
                              || square.Equals(board.EnPassantSquare)).ToList();
            Square targetMove = new Square(here.Row + direction, here.Col);

            if (!targetMove.IsValid() || !board.IsSquareEmpty(targetMove)) {
                return available;
            }
            available.Add(targetMove);
                
            Square targetMove2 = new Square(here.Row + 2 * direction, here.Col);
            if (here.Row == homeRow && board.IsSquareEmpty(targetMove2)) 

            {
                available.Add(targetMove2);
            }

            return available;
        }
        
        public override void MoveTo(Board board, Square newSquare)
        {
            var currentSquare = board.FindPiece(this);
            if (newSquare.Equals(board.EnPassantSquare))
            {
                board.OnPieceCaptured(board.GetPiece(Square.At(currentSquare.Row, newSquare.Col)));
                board.AddPiece(Square.At(currentSquare.Row, newSquare.Col),null);
            }
            base.MoveTo(board, newSquare); // changes the current player
            if (currentSquare.Row - newSquare.Row == 2 || currentSquare.Row - newSquare.Row == -2)
            {
                board.EnPassantSquare = Square.At((currentSquare.Row + newSquare.Row) / 2, newSquare.Col);
            }
            if (newSquare.Row == 0 || newSquare.Row == 7) // TODO: Allow choosing how to promote
            {
                board.OnPieceCaptured(this);
                board.AddPiece(newSquare, new Queen(Player));
            }
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