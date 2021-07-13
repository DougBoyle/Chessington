using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;
using Chessington.GameEngine.Bitboard;

namespace Chessington.GameEngine.Pieces
{
    public class Bishop : Piece
    {
        public Bishop(Player player)
            : base(player) { PieceType = PieceType.Bishop; }

       

        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, byte squareIdx, Player player, ulong mine, ulong yours)
        {
            ulong attackMap = BitMoves.BishopAttacks(squareIdx, mine, mine | yours);
            return BitMoves.GetMovesFromAttackMap(6 * (int)player + BitUtils.BISHOP_BOARD, squareIdx, board, attackMap);
        }
    }
}