using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;
using Chessington.GameEngine.Bitboard;

namespace Chessington.GameEngine.Pieces {
    public class Queen : Piece {
        public Queen(Player player)
            : base(player) { PieceType = PieceType.Queen; }


        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, byte squareIdx, Player player, ulong mine, ulong yours)
        {
            ulong attackMap = BitMoves.RookAttacks(squareIdx, mine, mine | yours) |
                BitMoves.BishopAttacks(squareIdx, mine, mine | yours);
            return BitMoves.GetMovesFromAttackMap(6 * (int)player + BitUtils.QUEEN_BOARD, squareIdx, board, attackMap);
        }
    }
}