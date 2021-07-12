using System;
using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

using static Chessington.GameEngine.BitUtils;

namespace Chessington.GameEngine.Pieces
{
    // ultimately, don't want to be dealing with class instances, use efficient ints etc. instead
    public enum PieceType { Pawn, Knight, Bishop, Rook, Queen, King };

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
                MoveTo(tempBoard, move);
                if (!tempBoard.InCheck(Player))
                {
                    actual.Add(move);
                }
                UndoMove(tempBoard, move, gameInfo);
            }
            return actual;
        }

        // Takes the position of the piece as an input, rather than having to find it twice
        public abstract IEnumerable<Move> GetRelaxedAvailableMoves(Board board, Square position);

        // TODO: replace to do move on board, rather than needing instances of pieces
        public virtual void MoveTo(Board board, Move move)
        {
            board.MakeMove(move);
          //  board.MovePiece(move.From, move.To);
          //  board.EnPassantSquare = null;
        }

        // for computer evaluation
        // GameExtraInfo restores all information about castling/en passant (and also current player) that can't be reversed otherwise
        public virtual void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            if (move.Promotion != null)
            {
                board.AddPiece(move.To, new Pawn(info.CurrentPlayer));
            }
            board.QuietMovePiece(move.To, move.From, move.Captured, move.MovingPiece);
            info.RestoreInfo(board);
        }

       
    }
}