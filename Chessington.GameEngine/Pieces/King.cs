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


        public override IEnumerable<Move> GetAvailableMoves(Board board, Square currentPosition)
        {
            List<Move> availableMoves = base.GetAvailableMoves(board, currentPosition).ToList();

            // just determines castling (can't do for RelaxedMoves as it involves calling InCheck)
            if (!board.InCheck(Player))
            {
                foreach (var direction in new int[] { -1, 1 })
                {
                    var blackBool = direction == -1 ? board.LeftBlackCastling : board.RightBlackCastling;
                    var whiteBool = direction == -1 ? board.LeftWhiteCastling : board.RightWhiteCastling;
                    if (Player == Player.White && whiteBool ||
                        Player == Player.Black && blackBool)
                    {
                        if (direction == -1 && !board.IsSquareEmpty(Square.At(currentPosition.Row, currentPosition.Col - 3)))
                        {
                            continue;
                        }

                        Square firstSquare = Square.At(currentPosition.Row, currentPosition.Col + 1 * direction);
                        Square secondSquare = Square.At(currentPosition.Row, currentPosition.Col + 2 * direction);
                        if (board.IsSquareEmpty(firstSquare) &&
                            board.IsSquareEmpty(secondSquare))
                        {
                            board.AddPiece(firstSquare, this);
                            board.AddPiece(currentPosition, null);
                            if (board.InCheck(Player))
                            {
                                board.AddPiece(firstSquare, null);
                                board.AddPiece(currentPosition, this);
                                continue;
                            }
                            board.AddPiece(secondSquare, this);
                            board.AddPiece(firstSquare, null);
                            if (!board.InCheck(Player))
                            {
                                // this constructor needed, as piece is not actually on 'currentPosition' at this point
                                availableMoves.Add(new Move(currentPosition, secondSquare, this, null, null));
                            }
                            board.AddPiece(secondSquare, null);
                            board.AddPiece(currentPosition, this);
                        }
                    }
                }
            }

            return availableMoves;
        }

        // TODO: Relaxed available moves doesn't consider castling (issue for engine)
        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition)
        {
            ulong myPieces = BoardOccupancy(board, Player);
            int index = SquareToIndex(currentPosition);
            ulong attackMap = kingMasks[index] & (~myPieces);
            return GetMovesFromAttackMap(this, currentPosition, board, attackMap);
        }

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
        }

        public override void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            // Undo moving the rook for castling
            // Detected differently to above
            if (move.From.Col == 4 && move.To.Col == 6)
            {
                board.QuietMovePiece(Square.At(move.From.Row, 5), Square.At(move.From.Row, 7), null);
            } else if (move.From.Col == 4 && move.To.Col == 2)
            {
                board.QuietMovePiece(Square.At(move.From.Row, 3), Square.At(move.From.Row, 0), null);
            }

            base.UndoMove(board, move, info);
        }
    }
}