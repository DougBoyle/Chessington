using System;
using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

using static Chessington.GameEngine.Bitboard.OtherMasks;
using static Chessington.GameEngine.BitUtils;

namespace Chessington.GameEngine.Pieces {
    public class Pawn : Piece {
        public Pawn(Player player)
            : base(player) { PieceType = PieceType.Pawn; }


        public static IEnumerable<Move> GetAllRelaxedPawnMoves(Board board, ulong pawnBoard, Player player, ulong mine, ulong yours)
        {
            var result = new List<Move>();
            // early check for empty board
            if (pawnBoard == 0UL) return result;

            ulong freeSquares = ~(mine | yours);

            // TODO: Do all the Player conditionals in one branch
            int direction = player == Player.White ? 8 : -8;
            int playerOffset = (int)player * 6;

            /****************************** Standard move ***********************************/
            // can't be a capture
            ulong oneMove = (player == Player.White ? pawnBoard << 8 : pawnBoard >> 8) & freeSquares;

           
            /****************************** Double move ***********************************/
            // can't be a promotion or capture
            ulong twoMove = (player == Player.White ? (oneMove & Rank3) << 8 : (oneMove & Rank6) >> 8) & freeSquares;

            ulong promoted = oneMove & PromotionRanks;
            oneMove &= (~PromotionRanks);
            while (promoted != 0UL)
            {
                ulong bit = GetLSB(promoted);
                byte bitIndex = BitToIndex(bit);
                promoted = DropLSB(promoted);

                result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, NO_PIECE, playerOffset + KNIGHT_BOARD));
                result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, NO_PIECE, playerOffset + BISHOP_BOARD));
                result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, NO_PIECE, playerOffset + ROOK_BOARD));
                result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, NO_PIECE, playerOffset + QUEEN_BOARD));

            }
            while (oneMove != 0UL)
            {
                ulong bit = GetLSB(oneMove);
                byte bitIndex = BitToIndex(bit);
                oneMove = DropLSB(oneMove);
                result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, NO_PIECE, NO_PIECE));
            }
            direction *= 2;
            while (twoMove != 0UL)
            {
                ulong bit = GetLSB(twoMove);
                byte bitIndex = BitToIndex(bit);
                twoMove = DropLSB(twoMove);
                result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, NO_PIECE, NO_PIECE));
            }

            /****************************** En passant ***********************************/
            // can't be a promotion, can't capture anything
            if (board.EnPassantIndex != Board.NO_SQUARE)
            {
                byte epIndex = board.EnPassantIndex;
                ulong epBoard = 1UL << epIndex; // TODO: Could do all of the shifts here
                ulong epAttacks = (player == Player.White ? ((epBoard & Not_A_File) >> 9) | ((epBoard & Not_H_File) >> 7)
                    : ((epBoard & Not_A_File) << 7) | ((epBoard & Not_H_File) << 9)) & pawnBoard;

                while (epAttacks != 0UL)
                {
                    ulong bit = GetLSB(epAttacks);
                    byte bitIndex = BitToIndex(bit);
                    epAttacks = DropLSB(epAttacks);

                    result.Add(new Move(bitIndex, epIndex, playerOffset, NO_PIECE, NO_PIECE));
                }
            }

            // GetPieceIndex requires an & with each board anyway, so may as well use each board rather than yourPieces
            /****************************** Capture Right ***********************************/
            direction = player == Player.White ? 9 : -7;
            ulong attacks = (player == Player.White ? pawnBoard << 9 : pawnBoard >> 7) & Not_A_File;
            for (int i = 6 - playerOffset; i < 12 - playerOffset; i++)
            {
                ulong captures = attacks & board.Bitboards[i];
                while (captures != 0UL)
                {
                    ulong bit = GetLSB(captures);
                    byte bitIndex = BitToIndex(bit);
                    captures = DropLSB(captures);

                    if ((bit & PromotionRanks) == 0)
                    {
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, NO_PIECE));
                    }
                    else
                    {
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, playerOffset + KNIGHT_BOARD));
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, playerOffset + BISHOP_BOARD));
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, playerOffset + ROOK_BOARD));
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, playerOffset + QUEEN_BOARD));
                    }
                }
            }

            /****************************** Capture Left ***********************************/
            // TODO: Left captures
            direction = player == Player.White ? 7 : -9;
            attacks = (player == Player.White ? pawnBoard << 7 : pawnBoard >> 9) & Not_H_File;
            for (int i = 6 - playerOffset; i < 12 - playerOffset; i++)
            {
                ulong captures = attacks & board.Bitboards[i];
                while (captures != 0UL)
                {
                    ulong bit = GetLSB(captures);
                    byte bitIndex = BitToIndex(bit);
                    captures = DropLSB(captures);

                    if ((bit & PromotionRanks) == 0)
                    {
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, NO_PIECE));
                    }
                    else
                    {
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, playerOffset + KNIGHT_BOARD));
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, playerOffset + BISHOP_BOARD));
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, playerOffset + ROOK_BOARD));
                        result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, i, playerOffset + QUEEN_BOARD));
                    }
                }
            }

            return result;
        }

        public static new void MakeMove(Board board, Move move)
        {
            int fromIdx = move.FromIdx;
            int toIdx = move.ToIdx;
            ulong bitTo = 1UL << toIdx;

            if (board.EnPassantIndex == toIdx)
            {
                // TODO: Change OnPieceCaptured to not use Piece class
                // square is just bitTo shifted left/right 8
                board.OnPieceCaptured(6 - move.MovingPiece);
                // as pawns are either 0 or 6
                board.Bitboards[6 - move.MovingPiece] ^= 1UL << 8*(move.FromIdx / 8) + (move.ToIdx % 8);
            }

            Piece.MakeMove(board, move);

            // set up en-passant
            if (fromIdx - toIdx == 16 || toIdx - fromIdx == 16)
            {
                board.EnPassantIndex = (byte)((toIdx + fromIdx) / 2);
            }

            if (move.PromotionPiece != NO_PIECE)
            {
                // piece has now moved, so is on move.To square
                board.OnPieceCaptured(move.MovingPiece);

                board.Bitboards[move.MovingPiece] ^= bitTo;
                board.Bitboards[move.PromotionPiece] ^= bitTo; // &= ~bitFrom;
            }
        }

        public static new void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            // handles en-passant capture, otherwise just fall back to base case
            if ((move.ToIdx % 8) != (move.FromIdx % 8) && move.CapturedPiece == NO_PIECE)
            {
                // en-passant, restore piece
                board.Bitboards[6 - move.MovingPiece] ^= 1UL << 8 * (move.FromIdx / 8) + (move.ToIdx % 8);
            }
            Piece.UndoMove(board, move, info);
        }
    }
}