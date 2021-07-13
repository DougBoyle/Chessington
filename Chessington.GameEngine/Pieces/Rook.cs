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

        public override IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition)
        {
            // TODO: Compute these as part of the board
            ulong myPieces = BitUtils.BoardOccupancy(board, Player);
            ulong yourPieces = BitUtils.BoardOccupancy(board, Player == Player.White ? Player.Black : Player.White);

            ulong attackMap = BitMoves.RookAttacks(currentPosition, board, myPieces, myPieces | yourPieces);
            return BitMoves.GetMovesFromAttackMap(this, currentPosition, board, attackMap);
        }

        public static IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square currentPosition, Player player, ulong mine, ulong yours)
        {
            ulong attackMap = BitMoves.RookAttacks(currentPosition, board, mine, mine | yours);
            return BitMoves.GetMovesFromAttackMap(6*(int)player + BitUtils.ROOK_BOARD, BitUtils.SquareToIndex(currentPosition), board, attackMap);
        }
    }
}