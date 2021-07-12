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

        public virtual IEnumerable<Move> GetAvailableMoves(Board board, Square position)
        {
            IEnumerable<Move> possible = GetRelaxedAvailableMoves(board, position);
            List<Move> actual = new List<Move>();

            // could also just be done at the whole board level
            var tempBoard = new Board(board);
            var gameInfo = new GameExtraInfo(board);

            foreach (var move in possible)
            {
                tempBoard.MakeMove(move);
                if (!tempBoard.InCheck(Player))
                {
                    actual.Add(move);
                }
                tempBoard.UndoMove(move, gameInfo);
            }
            return actual;
        }

        // Takes the position of the piece as an input, rather than having to find it twice
        public abstract IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square position);

        // for computer evaluation
        // GameExtraInfo restores all information about castling/en passant (and also current player) that can't be reversed otherwise
        public static void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            if (move.PromotionPiece != NO_PIECE)
            {
                ulong to = SquareToBit(move.To);
                board.Bitboards[move.PromotionPiece] ^= to;
                board.Bitboards[6 * (int)info.CurrentPlayer] |= to;
            }
            board.QuietMovePiece(move.To, move.From, move.CapturedPiece, move.MovingPiece);
            info.RestoreInfo(board);
        }


        public static void MakeMove(Board board, Move move)
        {
            int fromIdx = SquareToIndex(move.From);
            int toIdx = SquareToIndex(move.To);
            ulong bitFrom = 1UL << fromIdx;
            ulong bitTo = 1UL << toIdx;

            // Board.MovePiece allowed possibility that moving piece does not exist.
            // no longer allowed. Also don't check that piece belongs to correct player

            // var movingPiece = board[from.Row, from.Col];
            //  if (movingPiece == null) { return; }

            // also testing fromIdx handles moving rooks initially too
            if (toIdx == 0 || fromIdx == 0) board.LeftWhiteCastling = false;
            if (toIdx == 7 || fromIdx == 7) board.RightWhiteCastling = false;
            if (toIdx == 56 || fromIdx == 56) board.LeftBlackCastling = false;
            if (toIdx == 63 || fromIdx == 63) board.RightBlackCastling = false;


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

            board.EnPassantSquare = null;
        }
    }
}