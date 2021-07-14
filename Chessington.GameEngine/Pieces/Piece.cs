using System;
using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

using static Chessington.GameEngine.BitUtils;

namespace Chessington.GameEngine.Pieces
{
    // ultimately, don't want to be dealing with class instances, use efficient ints etc. instead
    public enum PieceType { Pawn = 0, Knight = 1, Bishop = 2, Rook = 3, Queen = 4, King = 5 };

    public abstract class Piece
    {
        public PieceType PieceType;

        protected Piece(Player player)
        {
            Player = player;
        }

        public Player Player { get; private set; }


        // for computer evaluation
        // GameExtraInfo restores all information about castling/en passant (and also current player) that can't be reversed otherwise
        public static void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            if (move.PromotionPiece != NO_PIECE)
            {
                ulong to = 1UL << move.ToIdx;
                board.Bitboards[move.PromotionPiece] ^= to;
                board.Bitboards[6 * (int)info.CurrentPlayer] |= to;
            }
            board.QuietMovePiece(move.ToIdx, move.FromIdx, move.CapturedPiece, move.MovingPiece);
            info.RestoreInfo(board);
        }


        public static void MakeMove(Board board, Move move)
        {
            int fromIdx = move.FromIdx;
            int toIdx = move.ToIdx;
            ulong bitFrom = 1UL << fromIdx;
            ulong bitTo = 1UL << toIdx;

            // Board.MovePiece allowed possibility that moving piece does not exist.
            // no longer allowed. Also don't check that piece belongs to correct player

            // var movingPiece = board[from.Row, from.Col];
            //  if (movingPiece == null) { return; }

            // also testing fromIdx handles moving rooks initially too
            if (toIdx == 0 || fromIdx == 0) board.Castling &= Board.NOT_LEFT_WHITE_CASTLING_MASK;
            if (toIdx == 7 || fromIdx == 7) board.Castling &= Board.NOT_RIGHT_WHITE_CASTLING_MASK;
            if (toIdx == 56 || fromIdx == 56) board.Castling &= Board.NOT_LEFT_BLACK_CASTLING_MASK;
            if (toIdx == 63 || fromIdx == 63) board.Castling &= Board.NOT_RIGHT_BLACK_CASTLING_MASK;


            // if (movingPiece.Player != CurrentPlayer)
            // {
            //     throw new ArgumentException("The supplied piece does not belong to the current player.");
            // }

            // TODO: Switch to using ints rather than Piece objects
            if (move.CapturedPiece >= 0)
            {
                board.OnPieceCaptured(move.CapturedPiece);
                board.Bitboards[move.CapturedPiece] ^= bitTo; // should be same as &= (~bitTo)
            }

            //Move the piece and set the 'from' square to be empty.
            board.Bitboards[move.MovingPiece] |= bitTo;
            board.Bitboards[move.MovingPiece] ^= bitFrom; // &= ~bitFrom;

            board.CurrentPlayer = (Player)(move.MovingPiece / 6) == Player.White ? Player.Black : Player.White;
            board.OnCurrentPlayerChanged(board.CurrentPlayer);

            board.EnPassantIndex = Board.NO_SQUARE;
        }
    }
}