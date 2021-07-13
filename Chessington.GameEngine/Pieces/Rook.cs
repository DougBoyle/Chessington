using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;
using Chessington.GameEngine.Bitboard;

namespace Chessington.GameEngine.Pieces
{
    public class Rook : Piece
    {
        public Rook(Player player)
            : base(player) { PieceType = PieceType.Rook; }

        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, byte squareIdx, Player player, ulong mine, ulong yours)
        {
            ulong attackMap = BitMoves.RookAttacks(squareIdx, mine, mine | yours);
            return BitMoves.GetMovesFromAttackMap((byte)(6 * (int)player + BitUtils.ROOK_BOARD), squareIdx, board, attackMap);
        }
    }
}