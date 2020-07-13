using System;
using System.Collections.Generic;

namespace Chessington.GameEngine.Pieces
{
    public class Moves
    {
        public static List<Square> GetBeamMoves(Board board, Square currentPosition, 
            Player currentPlayer, Func<Square, Square> iterator) {
            List<Square> availableMoves = new List<Square>();
            while ((currentPosition = iterator.Invoke(currentPosition)).IsValid()) {
                if (board.IsEmptyOrOpponent(currentPosition, currentPlayer)) availableMoves.Add(currentPosition);
                if (!board.IsSquareEmpty(currentPosition)) break;

            }

            return availableMoves;
        }

        public static IEnumerable<Square> GetLateralMoves(Board board, Square currentPosition,Player currentPlayer)
        {
            var availableMoves = GetBeamMoves(board, currentPosition, currentPlayer, 
                square => new Square(square.Row + 1, square.Col));
            availableMoves.AddRange(GetBeamMoves(board, currentPosition, currentPlayer, 
                square => new Square(square.Row - 1, square.Col)));
            availableMoves.AddRange(GetBeamMoves(board, currentPosition, currentPlayer, 
                square => new Square(square.Row, square.Col + 1)));
            availableMoves.AddRange(GetBeamMoves(board, currentPosition, currentPlayer, 
                square => new Square(square.Row, square.Col - 1)));
            return availableMoves;
        }

        public static IEnumerable<Square> GetDiagonalMoves(Board board, Square currentPosition, Player currentPlayer)
        {
            var availableMoves = GetBeamMoves(board, currentPosition, currentPlayer, 
                square => new Square(square.Row + 1, square.Col + 1));
            availableMoves.AddRange(GetBeamMoves(board, currentPosition, currentPlayer, 
                square => new Square(square.Row + 1, square.Col - 1)));
            availableMoves.AddRange(GetBeamMoves(board, currentPosition, currentPlayer, 
                square => new Square(square.Row - 1, square.Col + 1)));
            availableMoves.AddRange(GetBeamMoves(board, currentPosition, currentPlayer, 
                square => new Square(square.Row - 1, square.Col - 1)));
            return availableMoves;
        }
    }
}