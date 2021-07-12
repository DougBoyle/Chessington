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

        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition)
        {
            ulong myPieces = BoardOccupancy(board, Player);
            int index = SquareToIndex(currentPosition);
            ulong attackMap = knightMasks[index] & (~myPieces);
            return GetMovesFromAttackMap(this, currentPosition, board, attackMap);
        }

        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition, Player player, ulong mine, ulong yours)
        {
            int index = SquareToIndex(currentPosition);
            ulong attackMap = knightMasks[index] & (~mine);
            return GetMovesFromAttackMap(6 * (int)player + KNIGHT_BOARD, currentPosition, board, attackMap);
        }
    }
}