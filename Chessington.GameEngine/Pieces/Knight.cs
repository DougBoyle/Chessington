using System;
using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

using static Chessington.GameEngine.Bitboard.OtherMasks;
using static Chessington.GameEngine.BitUtils;
using static Chessington.GameEngine.Bitboard.BitMoves;

namespace Chessington.GameEngine.Pieces
{
    public class Knight : Piece
    {
        public Knight(Player player)
            : base(player) { PieceType = PieceType.Knight; }

        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, byte squareIdx, Player player, ulong mine, ulong yours)
        {
            ulong attackMap = knightMasks[squareIdx] & (~mine);
            return GetMovesFromAttackMap(6 * (int)player + KNIGHT_BOARD, squareIdx, board, attackMap);
        }
    }
}