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
        private IEnumerable<Move> AttackMapToMoves(Board board, Square here, ulong attacks)
        {
            List<Move> result = new List<Move>();
            while (attacks != 0UL)
            {
                ulong bit = GetLSB(attacks);
                int bitIndex = BitToIndex(bit);
                attacks = DropLSB(attacks);
                Square to = IndexToSquare(bitIndex);

                // captured piece constructed explicitly to avoid using GetPiece
                // slower until 'Piece captured' replaced with 'int captured' in Move
                Piece captured = null;
                Player otherPlayer = Player == Player.White ? Player.Black : Player.White;
                // lots of tests, may be a way to do this quicker (binary search it? doesn't save much)
                // have to include possibility of capturing king due to how 'relaxed' moves work/testing for check
                if ((bit & board.Bitboards[(int)otherPlayer * 6]) != 0) captured = new Pawn(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 1]) != 0) captured = new Knight(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 2]) != 0) captured = new Bishop(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 3]) != 0) captured = new Rook(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 4]) != 0) captured = new Queen(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 5]) != 0) captured = new King(otherPlayer);

                if ((bit & PromotionRanks) == 0)
                {
                    result.Add(new Move(here, to, this, captured, null));
                } else
                {
                    result.Add(new Move(here, to, this, captured, new Knight(Player)));
                    result.Add(new Move(here, to, this, captured, new Rook(Player)));
                    result.Add(new Move(here, to, this, captured, new Bishop(Player)));
                    result.Add(new Move(here, to, this, captured, new Queen(Player)));
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
            
            result |= (Player == Player.White ? (result & PawnPushedRanks) << 8 : (result & PawnPushedRanks) >> 8) // 2 move
                & freeSquares;

            // captures (include en-passant tile)
            if (board.EnPassantSquare is Square square) yourPieces |= 1UL << SquareToIndex(square);
            // rather than moveRight(bit & notH), can equally do moveRight(bit) & notA
            // capture right
            result |= (Player == Player.White ? bit << 9 : bit >> 7) & yourPieces & Not_A_File;
            // capture left
            result |= (Player == Player.White ? bit << 7 : bit >> 9) & yourPieces & Not_H_File;

            // BitMoves.GetMovesFromAttackMap not used due to needing to handle promotions
            return AttackMapToMoves(board, here, result);
        }

        public static void MakeMove(Board board, Move move)
        {
            int fromIdx = SquareToIndex(move.From);
            int toIdx = SquareToIndex(move.To);
            ulong bitTo = 1UL << toIdx;


            if (board.EnPassantSquare is Square square && toIdx == SquareToIndex(square))
            {
                // TODO: Change OnPieceCaptured to not use Piece class
                // square is just bitTo shifted left/right 8
                board.OnPieceCaptured(6 - move.MovingPiece);
                // as pawns are either 0 or 6
                board.Bitboards[6 - move.MovingPiece] ^= SquareToBit(Square.At(move.From.Row, move.To.Col));
            }

            Piece.MakeMove(board, move);

            // set up en-passant
            if (fromIdx - toIdx == 16 || toIdx - fromIdx == 16)
            {
                board.EnPassantSquare = IndexToSquare((toIdx + fromIdx) / 2);
            }

            if (move.Promotion != null)
            {
                // piece has now moved, so is on move.To square
                board.OnPieceCaptured(move.MovingPiece);

                board.Bitboards[move.MovingPiece] ^= bitTo;
                board.Bitboards[PieceToBoardIndex(move.Promotion)] ^= bitTo; // &= ~bitFrom;
            }
        }



        public override void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            // handles en-passant capture, otherwise just fall back to base case
            if (move.To.Col != move.From.Col && move.CapturedPiece == NO_PIECE)
            {
                // en-passant, restore piece
                board.AddPiece(Square.At(move.From.Row, move.To.Col), new Pawn(board.CurrentPlayer));

            }
            base.UndoMove(board, move, info);
        }
    }
}