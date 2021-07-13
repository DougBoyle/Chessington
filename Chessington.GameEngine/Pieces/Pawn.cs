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



        // TODO: Know only diagonal moves can be captures (but could be en-passant, so can reduce number of tests)
        private static IEnumerable<Move> AttackMapToMoves(Board board, byte hereIdx, Player player, ulong attacks)
        {
            List<Move> result = new List<Move>();
            while (attacks != 0UL)
            {
                ulong bit = GetLSB(attacks);
                byte bitIndex = BitToIndex(bit);
                attacks = DropLSB(attacks);

                int captured = board.GetPieceIndex(bitIndex);
                if ((bit & PromotionRanks) == 0)
                {
                    result.Add(new Move(hereIdx, bitIndex, 6 * (int)player, captured, NO_PIECE));
                }
                else
                {
                    int playerOffset = (int)player * 6;
                    result.Add(new Move(hereIdx, bitIndex, 6 * (int)player, captured, playerOffset + KNIGHT_BOARD));
                    result.Add(new Move(hereIdx, bitIndex, 6 * (int)player, captured, playerOffset + BISHOP_BOARD));
                    result.Add(new Move(hereIdx, bitIndex, 6 * (int)player, captured, playerOffset + ROOK_BOARD));
                    result.Add(new Move(hereIdx, bitIndex, 6 * (int)player, captured, playerOffset + QUEEN_BOARD));
                }
            }
            return result;
        }

        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, byte hereIdx, Player player, ulong mine, ulong yours)
        {
            // TODO: Calculate moves for all pawns at once, rather than one at a time
            ulong freeSquares = ~(mine | yours);
            ulong bit = 1UL << hereIdx;

            ulong result = (player == Player.White ? bit << 8 : bit >> 8) & freeSquares; // 1 move

            result |= (player == Player.White ? (result & Rank3) << 8 : (result & Rank6) >> 8) // 2 move
                & freeSquares;

            // captures (include en-passant tile)
            if (board.EnPassantSquare is Square square) yours |= 1UL << SquareToIndex(square);
            // rather than moveRight(bit & notH), can equally do moveRight(bit) & notA
            // capture right
            result |= (player == Player.White ? bit << 9 : bit >> 7) & yours & Not_A_File;
            // capture left
            result |= (player == Player.White ? bit << 7 : bit >> 9) & yours & Not_H_File;

            // BitMoves.GetMovesFromAttackMap not used due to needing to handle promotions
            return AttackMapToMoves(board, hereIdx, player, result);
        }

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

            /****************************** Double move ***********************************/
            // can't be a promotion or capture
            oneMove |= (player == Player.White ? (oneMove & Rank3) << 8 : (oneMove & Rank6) >> 8) & freeSquares;
            while (oneMove != 0UL)
            {
                ulong bit = GetLSB(oneMove);
                byte bitIndex = BitToIndex(bit);
                oneMove = DropLSB(oneMove);

                result.Add(new Move((byte)(bitIndex - direction), bitIndex, playerOffset, NO_PIECE, NO_PIECE));
            }

            /****************************** En passant ***********************************/
            // can't be a promotion, can't capture anything
            if (board.EnPassantSquare is Square square)
            {
                byte epIndex = SquareToIndex(square);
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


            if (board.EnPassantSquare is Square square && toIdx == SquareToIndex(square))
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
                board.EnPassantSquare = IndexToSquare((toIdx + fromIdx) / 2);
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