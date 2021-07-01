using System;
using System.Collections.Generic;
using System.Linq;

using Chessington.GameEngine.AI;

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

        // TODO: Currently very inefficient copying the board every time. Need to make reversible
        public virtual IEnumerable<Square> GetAvailableMoves(Board board) {
            IEnumerable<Square> possible = GetRelaxedAvailableMoves(board);
            List<Square> actual = new List<Square>();

            var position = board.FindPiece(this);
            var tempBoard = new Board(board);
            var gameInfo = new GameExtraInfo(board);

            foreach (var square in possible) {
                //   var newBoard = new Board(board);
                var move = new Move(position, square, board);

                MoveTo(tempBoard, square);
                if (!tempBoard.InCheck(Player)) {
                    actual.Add(square);
                }
                UndoMove(tempBoard, move, gameInfo);
            }
            return actual;
        }
        
        public abstract IEnumerable<Square> GetRelaxedAvailableMoves(Board board);

        public virtual void MoveTo(Board board, Square newSquare)
        {
            var currentSquare = board.FindPiece(this);
            board.MovePiece(currentSquare, newSquare);
            board.EnPassantSquare = null;
        }

        // for computer evaluation
        // GameExtraInfo restores all information about castling/en passant (and also current player) that can't be reversed otherwise
        // TODO: Handle pawns (en-passant), kings (castling)
        public virtual void UndoMove(Board board, Move move, GameExtraInfo info)
        {
            if (move.Promotion)
            {
                board.AddPiece(move.To, new Pawn(info.CurrentPlayer));
            }
            board.QuietMovePiece(move.To, move.From, move.Captured);
            info.RestoreInfo(board);
        }
    }
}