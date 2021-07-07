using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine;
using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine.AI.Endgame
{
    // only positions with white king on the 10 squares at angle 180 to 225 degrees from center considered
    // i.e a1, b1-2, c1-3, d1-4
    // all other positions mapped to this by reflection/rotation
    // only 10 squares rather than 64, so reduces table size by about 6x

    // assumes castling not possible
    // TODO: Possibility of en-passant if one pawn on either side (can't have as much symmetry if pawns present)
    public class NormalForm
    {
        // key feature of this normal form
        public static Square[] KingSquares = {Square.At(7, 0), Square.At(7, 1), Square.At(7, 2), Square.At(7, 3),
            Square.At(6, 1), Square.At(6, 2), Square.At(6, 3), Square.At(5, 2), Square.At(5, 3), Square.At(4, 3)};


        // flips along / diagonal as (0,0) is top left
        private static void TransposeBoard(Board b)
        {
            for (int i = 1; i < GameSettings.BoardSize; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    Piece tmp = b.board[GameSettings.BoardSize - 1 - i, j];
                    b.board[GameSettings.BoardSize - 1 - i, j] = b.board[GameSettings.BoardSize - 1 - j, i];
                    b.board[GameSettings.BoardSize - 1 - j, i] = tmp;
                }
            }
        }

        private static void ReverseRows(Board b)
        {
            for (int row = 0; row < GameSettings.BoardSize; row++)
            {
                for (int col = 0; col < GameSettings.BoardSize / 2; col++)
                {
                    Piece tmp = b.board[row, col];
                    b.board[row, col] = b.board[row, GameSettings.BoardSize - 1 - col];
                    b.board[row, GameSettings.BoardSize - 1 - col] = tmp;
                }
            }
        }

        private static void ReverseCols(Board b)
        {
            for (int col = 0; col < GameSettings.BoardSize; col++)
            {
                for (int row = 0; row < GameSettings.BoardSize / 2; row++)
                {
                    Piece tmp = b.board[row, col];
                    b.board[row, col] = b.board[GameSettings.BoardSize - 1 - row, col];
                    b.board[GameSettings.BoardSize - 1 - row, col] = tmp;
                }
            }
        }

        public delegate Square SquareMapper(Square square);

        // returns a function that allows mapping any resulting move back to the actual board coordinates
        public static SquareMapper NormaliseBoard(Board b)
        {
            Square king = b.FindKing(Player.White);
            List<SquareMapper> transforms = new List<SquareMapper>();

            // remember row 0 = top of board
            if (king.Col >= GameSettings.BoardSize / 2)
            {
                ReverseRows(b);
                king = Square.At(king.Row, GameSettings.BoardSize - 1 - king.Col);
                // TODO: Make part of return value of ReverseRows itself?
                transforms.Add(square => new Square(square.Row, GameSettings.BoardSize - 1 - square.Col));
            }
            if (king.Row < GameSettings.BoardSize / 2)
            {
                ReverseCols(b);
                king = Square.At(GameSettings.BoardSize - 1 - king.Row, king.Col);
                transforms.Add(square => new Square(GameSettings.BoardSize - 1 - square.Row, square.Col));
            }

            if (king.Col + king.Row + 1 < GameSettings.BoardSize)
            {
                TransposeBoard(b);
                transforms.Add(square => new Square(GameSettings.BoardSize - 1 - square.Col, GameSettings.BoardSize - 1 - square.Row));
            }

            transforms.Reverse(); // need to return the reverse operation

            // compose each of the transformations in reverse order
            return square =>
            {
                foreach (SquareMapper mapper in transforms)
                {
                    square = mapper(square);
                }
                return square;
            };
        }

        // Also flips board vertically to respect direction pawns move
        public static Board FlipColour(Board b)
        {
            // take a copy rather than modifying directly
            // used for endgames so can assume no castling
            Board board = new Board(b);
            if (b.EnPassantSquare is Square square) board.EnPassantSquare = Square.At(7 - square.Row, square.Col);
            board.CurrentPlayer = b.CurrentPlayer == Player.White ? Player.Black : Player.White;

            // can't easily copy pieces, so create new ones with flipped colours
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece original = b.GetPiece(row, col);
                    Piece newPiece;
                    
                    if (original == null) newPiece = null;
                    else
                    {
                        Player otherPlayer = original.Player == Player.White ? Player.Black : Player.White;
                        switch (original.PieceType) // TODO: Just add a method to produce copy, or use cloneable?
                        {
                            case PieceType.Pawn:
                                newPiece = new Pawn(otherPlayer);
                                break;
                            case PieceType.Knight:
                                newPiece = new Knight(otherPlayer);
                                break;
                            case PieceType.Bishop:
                                newPiece = new Bishop(otherPlayer);
                                break;
                            case PieceType.Rook:
                                newPiece = new Rook(otherPlayer);
                                break;
                            case PieceType.Queen:
                                newPiece = new Queen(otherPlayer);
                                break;
                            default:
                                newPiece = new King(otherPlayer);
                                break;
                        }
                    }
                    board.AddPiece(Square.At(7 - row, col), newPiece);
                }
            }
            return board;
        }
    }
}
