﻿using System;
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
        private static IEnumerable<Move> AttackMapToMoves(Board board, Square here, Player player, ulong attacks)
        {
            byte hereIdx = SquareToIndex(here);
            List<Move> result = new List<Move>();
            while (attacks != 0UL)
            {
                ulong bit = GetLSB(attacks);
                byte bitIndex = BitToIndex(bit);
                attacks = DropLSB(attacks);

                int captured = board.GetPieceIndex(bitIndex);
                if ((bit & PromotionRanks) == 0)
                {
                    result.Add(new Move(hereIdx, bitIndex, 6*(int)player, captured, NO_PIECE));
                } else
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

        // creates list of possible squares, then generates promotions where needed
        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square here)
        {
            // TODO: Calculate moves for all pawns at once, rather than one at a time
            ulong myPieces = BoardOccupancy(board, Player);
            ulong yourPieces = BoardOccupancy(board, Player == Player.White ? Player.Black : Player.White);
            ulong freeSquares = ~(myPieces | yourPieces);
            int index = SquareToIndex(here);
            ulong bit = 1UL << index;

            ulong result = (Player == Player.White ? bit << 8 : bit >> 8) & freeSquares; // 1 move
            
            result |= (Player == Player.White ? (result & Rank3) << 8 : (result & Rank6) >> 8) // 2 move
                & freeSquares;

            // captures (include en-passant tile)
            if (board.EnPassantSquare is Square square) yourPieces |= 1UL << SquareToIndex(square);
            // rather than moveRight(bit & notH), can equally do moveRight(bit) & notA
            // capture right
            result |= (Player == Player.White ? bit << 9 : bit >> 7) & yourPieces & Not_A_File;
            // capture left
            result |= (Player == Player.White ? bit << 7 : bit >> 9) & yourPieces & Not_H_File;

            // BitMoves.GetMovesFromAttackMap not used due to needing to handle promotions
            return AttackMapToMoves(board, here, Player, result);
        }

        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square here, Player player, ulong mine, ulong yours)
        {
            // TODO: Calculate moves for all pawns at once, rather than one at a time
            ulong freeSquares = ~(mine | yours);
            int index = SquareToIndex(here);
            ulong bit = 1UL << index;

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
            return AttackMapToMoves(board, here, player, result);
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