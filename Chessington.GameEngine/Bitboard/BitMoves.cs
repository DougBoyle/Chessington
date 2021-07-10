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
            int index = BitUtils.SquareToIndex(square);
            int lookup = (int)((rookMagics[index] * (rookMasks[index] & allPieces)) >> rookShifts[index]);
            return rookAttacks[rookOffsets[index] + lookup] & (~myPieces); // can't attack self
        }

        public static ulong BishopAttacks(Square square, Board board, ulong myPieces, ulong allPieces)
        {
            int index = BitUtils.SquareToIndex(square);
            int lookup = (int)((bishopMagics[index] * (bishopMasks[index] & allPieces)) >> bishopShifts[index]);
            return bishopAttacks[bishopOffsets[index] + lookup] & (~myPieces); // can't attack self
        }

        // TODO: Just use bitboards wherever possible. e.g. working out the captured piece
        // Not for pawns/king - doesn't do anything interesting e.g. castling/en passant/promotions
        public List<Move> GetMovesFromAttackMap(Piece moving, Square from, Board board, ulong attacks)
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
                Player otherPlayer = board.CurrentPlayer == Player.White ? Player.Black : Player.White;
                // lots of tests, may be a way to do this quicker (binary search it? doesn't save much)
                // have to include possibility of capturing king due to how 'relaxed' moves work/testing for check
                if ((bit & board.Bitboards[(int)otherPlayer * 6]) != 0) captured = new Pawn(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 1]) != 0) captured = new Knight(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 2]) != 0) captured = new Bishop(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 3]) != 0) captured = new Rook(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 4]) != 0) captured = new Queen(otherPlayer);
                else if ((bit & board.Bitboards[(int)otherPlayer * 6 + 5]) != 0) captured = new King(otherPlayer);

                result.Add(new Move(from, to, moving, captured, null));
            }
            return result;
        }
    }
}
