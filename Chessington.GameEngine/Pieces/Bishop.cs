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

        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square position)
        {
            // TODO: Compute these as part of the board
            ulong myPieces = BitUtils.BoardOccupancy(board, Player);
            ulong yourPieces = BitUtils.BoardOccupancy(board, Player == Player.White ? Player.Black : Player.White);

            ulong attackMap = BitMoves.BishopAttacks(position, board, myPieces, myPieces | yourPieces);
            return BitMoves.GetMovesFromAttackMap(this, position, board, attackMap);
        }

        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition, Player player, ulong mine, ulong yours)
        {
            ulong attackMap = BitMoves.BishopAttacks(currentPosition, board, mine, mine | yours);
            return BitMoves.GetMovesFromAttackMap(6 * (int)player + BitUtils.BISHOP_BOARD, BitUtils.SquareToIndex(currentPosition), board, attackMap);
        }
    }
}