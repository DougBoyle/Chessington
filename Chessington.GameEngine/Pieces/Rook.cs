using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.Pieces
{
    public class Rook : Piece
    {
        public Rook(Player player)
            : base(player) { PieceType = PieceType.Rook; }

        public override IEnumerable<Square> GetRelaxedAvailableMoves(Board board, Square currentPosition)
        {
            return Moves.GetLateralMoves(board, currentPosition, Player);
        }
        
        public override void MoveTo(Board board, Square newSquare)
        {
            var currentSquare = board.FindPiece(this);
            if (Player == Player.White)
            {
                if (currentSquare.Equals(Square.At(7,0)))
                {
                    board.LeftWhiteCastling = false;
                }
                else if (currentSquare.Equals(Square.At(7,7)))
                {
                    board.RightWhiteCastling = false;
                }
            }
            else 
            {
                if (currentSquare.Equals(Square.At(0,0)))
                {
                    board.LeftBlackCastling = false;
                }
                else if (currentSquare.Equals(Square.At(0,7)))
                {
                    board.RightBlackCastling = false;
                }
            }
            base.MoveTo(board, newSquare);
        }
    }
}