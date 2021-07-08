using Chessington.GameEngine.AI;
using System;
using System.Collections.Generic;

namespace Chessington.GameEngine.Pieces
{
    public class Moves
    {
        public static List<Move> GetBeamMoves(Board board, Square startPosition, 
            Player currentPlayer, Func<Square, Square> iterator) {
            Square currentPosition = startPosition;
            List<Move> availableMoves = new List<Move>();
            while ((currentPosition = iterator.Invoke(currentPosition)).IsValid()) {
                if (board.IsEmptyOrOpponent(currentPosition, currentPlayer)) {
                    availableMoves.Add(new Move(startPosition, currentPosition, board));
                }

                if (!board.IsSquareEmpty(currentPosition)) {
                    break;
                }
            }
            
            return availableMoves;
        }

        public static IEnumerable<Move> GetLateralMoves(Board board, Square currentPosition, Player currentPlayer)
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

        public static IEnumerable<Move> GetDiagonalMoves(Board board, Square currentPosition, Player currentPlayer)
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