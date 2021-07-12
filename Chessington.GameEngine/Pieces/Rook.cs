using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;
using Chessington.GameEngine.Bitboard;

namespace Chessington.GameEngine.Pieces
{
    public class Rook : Piece
    {
        public Rook(Player player)
            : base(player) { PieceType = PieceType.Rook; }

        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition)
        {
            // TODO: Compute these as part of the board
            ulong myPieces = BitUtils.BoardOccupancy(board, Player);
            ulong yourPieces = BitUtils.BoardOccupancy(board, Player == Player.White ? Player.Black : Player.White);

            ulong attackMap = BitMoves.RookAttacks(currentPosition, board, myPieces, myPieces | yourPieces);
            return BitMoves.GetMovesFromAttackMap(this, currentPosition, board, attackMap);
        }

        /*
        public override void MoveTo(Board board, Move move)
        {
            var currentSquare = move.From;
            if (Player == Player.White)
            {
                if (currentSquare.Equals(Square.At(7, 0)))
                {
                    board.LeftWhiteCastling = false;
                }
                else if (currentSquare.Equals(Square.At(7, 7)))
                {
                    board.RightWhiteCastling = false;
                }
            }
            else
            {
                if (currentSquare.Equals(Square.At(0, 0)))
                {
                    board.LeftBlackCastling = false;
                }
                else if (currentSquare.Equals(Square.At(0, 7)))
                {
                    board.RightBlackCastling = false;
                }
            }
            base.MoveTo(board, move);
        }*/
    }
}