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
        public static IEnumerable<Move> GetCastling(Board board, Square here, Player player)
        {
            var result = new List<Move>();
            if (board.InCheck(player, here)) return result;

            byte hereIdx = SquareToIndex(here);
            ulong occupancy = BoardOccupancy(board, Player.White) | BoardOccupancy(board, Player.Black);
            if (player == Player.Black) occupancy >>= 56; // move to bottom row

            // due to the ways king could possibly be attacked, and given that king not in check currently,
            // no need to move king before computing if would move into check

            // short castling
            if ((board.Castling & (player == Player.White ? Board.RIGHT_WHITE_CASTLING_MASK : Board.RIGHT_BLACK_CASTLING_MASK)) != 0 &&
                (occupancy & 0x60UL) == 0UL && 
                !board.InCheck(player, Square.At(here.Row, here.Col + 1)))
            {
                // can guarentee nothing captured/promoted
                result.Add(new Move(hereIdx, (byte)(hereIdx + 2), KING_BOARD + 6*(int)player, NO_PIECE, NO_PIECE));
            }

            // long castling
            if ((board.Castling & (player == Player.White ? Board.LEFT_WHITE_CASTLING_MASK : Board.LEFT_BLACK_CASTLING_MASK)) != 0 &&
                (occupancy & 0xeUL) == 0UL &&
                !board.InCheck(player, Square.At(here.Row, here.Col - 1)))
            {
                result.Add(new Move(hereIdx, (byte)(hereIdx - 2), KING_BOARD + 6 * (int)player, NO_PIECE, NO_PIECE));
            }

            return result;
        }

        public static IEnumerable<Move> GetCastling(Board board, byte hereIdx, Player player)
        {
            var result = new List<Move>();
            if (board.InCheck(player, hereIdx)) return result;

            ulong occupancy = BoardOccupancy(board, Player.White) | BoardOccupancy(board, Player.Black);
            if (player == Player.Black) occupancy >>= 56; // move to bottom row

            // due to the ways king could possibly be attacked, and given that king not in check currently,
            // no need to move king before computing if would move into check

            // short castling
            if ((board.Castling & (player == Player.White ? Board.RIGHT_WHITE_CASTLING_MASK : Board.RIGHT_BLACK_CASTLING_MASK)) != 0 &&
                (occupancy & 0x60UL) == 0UL &&
                !board.InCheck(player, (byte)(hereIdx + 1)))
            {
                // can guarentee nothing captured/promoted
                result.Add(new Move(hereIdx, (byte)(hereIdx + 2), KING_BOARD + 6 * (int)player, NO_PIECE, NO_PIECE));
            }

            // long castling
            if ((board.Castling & (player == Player.White ? Board.LEFT_WHITE_CASTLING_MASK : Board.LEFT_BLACK_CASTLING_MASK)) != 0 &&
                (occupancy & 0xeUL) == 0UL &&
                !board.InCheck(player, (byte)(hereIdx - 1)))
            {
                result.Add(new Move(hereIdx, (byte)(hereIdx - 2), KING_BOARD + 6 * (int)player, NO_PIECE, NO_PIECE));
            }

            return result;
        }


        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, byte squareIdx, Player player, ulong mine, ulong yours)
        {
            ulong attackMap = kingMasks[squareIdx] & (~mine);
            return GetMovesFromAttackMap(6 * (int)player + KING_BOARD, squareIdx, board, attackMap)
                .Concat(GetCastling(board, squareIdx, player));
        }

        public static new void MakeMove(Board board, Move move)
        {
            int fromIdx = move.FromIdx;
            int toIdx = move.ToIdx;

            // should be able to use either board.CurrentPlayer or move.MovingPiece/6
            if (board.CurrentPlayer == Player.White)
            {
                if (fromIdx == 4)
                {
                    board.Castling &= Board.NOT_WHITE_CASTLING_MASK;
                    // short castling
                    if (toIdx == 6)
                    {
                        board.Bitboards[ROOK_BOARD] ^= 0xa0UL; // 0000_0101
                    }
                    else if (toIdx == 2)
                    {
                        board.Bitboards[ROOK_BOARD] ^= 0x9UL; // 1001_0000
                    }
                }
            }
            else
            {
                if (fromIdx == 60)
                {
                    board.Castling &= Board.NOT_BLACK_CASTLING_MASK;
                    // short castling
                    if (toIdx == 62)
                    {
                        board.Bitboards[6 + ROOK_BOARD] ^= 0xa000000000000000UL; // 0000_0101
                    }
                    else if (toIdx == 58)
                    {
                        board.Bitboards[6 + ROOK_BOARD] ^= 0x900000000000000UL; // 1001_0000
                    }
                }
            }

            Piece.MakeMove(board, move);
        }

        public static new void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            // Undo moving the rook for castling
            // Detected differently to above
            if (move.FromIdx % 8 == 4 && move.ToIdx % 8 == 6)
            {
                // king = 5 so (MovingPiece - 2) is the rook of the same colour
                board.QuietMovePiece(move.FromIdx + 1, move.FromIdx + 3, NO_PIECE, move.MovingPiece - 2);
            } else if (move.FromIdx % 8 == 4 && move.ToIdx % 8 == 2)
            {
                board.QuietMovePiece(move.FromIdx - 1, move.FromIdx - 4, NO_PIECE, move.MovingPiece - 2);
            }

            Piece.UndoMove(board, move, info);
        }
    }
}