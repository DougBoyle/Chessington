using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Chessington.GameEngine.Bitboard.BestMagics;
using static Chessington.GameEngine.BitUtils;
using Chessington.GameEngine.AI;
using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine.Bitboard
{
    public class BitMoves
    {
        // my/allPieces passed in to avoid recomputing in many places
        public static ulong RookAttacks(Square square, Board board, ulong myPieces, ulong allPieces)
        {
            int index = SquareToIndex(square);
            int lookup = (int)((rookMagics[index] * (rookMasks[index] & allPieces)) >> (64 - rookShifts[index]));
            return rookAttacks[rookOffsets[index] + lookup] & (~myPieces); // can't attack self
        }

        public static ulong BishopAttacks(Square square, Board board, ulong myPieces, ulong allPieces)
        {
            int index = SquareToIndex(square);
            int lookup = (int)((bishopMagics[index] * (bishopMasks[index] & allPieces)) >> (64 - bishopShifts[index]));
            return bishopAttacks[bishopOffsets[index] + lookup] & (~myPieces); // can't attack self
        }

        // TODO: Just use bitboards wherever possible. e.g. working out the captured piece
        // Not for pawns/king - doesn't do anything interesting e.g. castling/en passant/promotions
        public static List<Move> GetMovesFromAttackMap(Piece moving, Square from, Board board, ulong attacks)
        {
            List<Move> result = new List<Move>();
            while (attacks != 0UL)
            {
                ulong bit = GetLSB(attacks);
                byte bitIndex = BitToIndex(bit);
                attacks = DropLSB(attacks);
                result.Add(new Move(SquareToIndex(from), bitIndex, PieceToBoardIndex(moving), board.GetPieceIndex(bitIndex), NO_PIECE));
            }
            return result;
        }

        public static List<Move> GetMovesFromAttackMap(int moving, byte fromIdx, Board board, ulong attacks)
        {
            List<Move> result = new List<Move>();
            while (attacks != 0UL)
            {
                ulong bit = GetLSB(attacks);
                byte bitIndex = (byte) BitToIndex(bit);
                attacks = DropLSB(attacks);
                result.Add(new Move(fromIdx, bitIndex, moving, board.GetPieceIndex(bitIndex), NO_PIECE));
            }
            return result;
        }
    }
}
