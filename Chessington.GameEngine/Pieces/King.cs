using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

using static Chessington.GameEngine.Bitboard.OtherMasks;
using static Chessington.GameEngine.BitUtils;
using static Chessington.GameEngine.Bitboard.BitMoves;

namespace Chessington.GameEngine.Pieces
{
    public class King : Piece
    {
        public King(Player player)
            : base(player)
        {
            PieceType = PieceType.King;
        }

        // can use InCheck to compute castling now that it no longer recursively looks at GetAvailable/RelaxedMoves
        // doesn't check that move wouldn't leave king in check, that's the purpose of GetAvailableMoves vs Relaxed
        public IEnumerable<Move> GetCastling(Board board, Square here)
        {
            var result = new List<Move>();
            if (board.InCheck(Player, here)) return result;
            ulong occupancy = BoardOccupancy(board, Player.White) | BoardOccupancy(board, Player.Black);
            if (Player == Player.Black) occupancy >>= 56; // move to bottom row

            // due to the ways king could possibly be attacked, and given that king not in check currently,
            // no need to move king before computing if would move into check

            // short castling
            if ((Player == Player.White ? board.RightWhiteCastling : board.RightBlackCastling) &&
                (occupancy & 0x60UL) == 0UL && 
                !board.InCheck(Player, Square.At(here.Row, here.Col + 1)))
            {
                // can guarentee nothing captured/promoted
                result.Add(new Move(here, Square.At(here.Row, here.Col + 2), this, null, null));
            }

            // long castling
            if ((Player == Player.White ? board.LeftWhiteCastling : board.LeftBlackCastling) &&
                (occupancy & 0xeUL) == 0UL &&
                !board.InCheck(Player, Square.At(here.Row, here.Col - 1)))
            {
                result.Add(new Move(here, Square.At(here.Row, here.Col - 2), this, null, null));
            }

            return result;
        }

        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition)
        {
            ulong myPieces = BoardOccupancy(board, Player);
            int index = SquareToIndex(currentPosition);
            ulong attackMap = kingMasks[index] & (~myPieces);
            return GetMovesFromAttackMap(this, currentPosition, board, attackMap)
                .Concat(GetCastling(board, currentPosition));
        }

        /*
        public override void MoveTo(Board board, Move move)
        {
            var currentPosition = move.From;
            var newSquare = move.To;
            if (Player == Player.White)
            {
                if (currentPosition.Equals(Square.At(7, 4)))
                {
                    if (newSquare.Col == 6)
                    {
                        board.AddPiece(Square.At(7, 5), board.GetPiece(Square.At(7, 7)));
                        board.AddPiece(Square.At(7, 7), null);
                    }
                    else if (newSquare.Col == 2)
                    {
                        board.AddPiece(Square.At(7, 3), board.GetPiece(Square.At(7, 0)));
                        board.AddPiece(Square.At(7, 0), null);
                    }
                }
                board.RightWhiteCastling = false;
                board.LeftWhiteCastling = false;

            }
            else
            {
                if (currentPosition.Equals(Square.At(0, 4)))
                {
                    if (newSquare.Col == 6)
                    {
                        board.AddPiece(Square.At(0, 5), board.GetPiece(Square.At(0, 7)));
                        board.AddPiece(Square.At(0, 7), null);
                    }
                    else if (newSquare.Col == 2)
                    {
                        board.AddPiece(Square.At(0, 3), board.GetPiece(Square.At(0, 0)));
                        board.AddPiece(Square.At(0, 0), null);
                    }
                }
                board.RightBlackCastling = false;
                board.LeftBlackCastling = false;
            }

            base.MoveTo(board, move);
        }*/

        public override void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            // Undo moving the rook for castling
            // Detected differently to above
            if (move.From.Col == 4 && move.To.Col == 6)
            {
                // king = 5 so (MovingPiece - 2) is the rook of the same colour
                board.QuietMovePiece(Square.At(move.From.Row, 5), Square.At(move.From.Row, 7), null, move.MovingPiece - 2);
            } else if (move.From.Col == 4 && move.To.Col == 2)
            {
                board.QuietMovePiece(Square.At(move.From.Row, 3), Square.At(move.From.Row, 0), null, move.MovingPiece - 2);
            }

            base.UndoMove(board, move, info);
        }
    }
}