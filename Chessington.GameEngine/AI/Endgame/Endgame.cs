using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine.AI.Endgame
{
    public class Endgame
    {
        public static Move MakeMove(Board board)
        {
            // count pieces/check for 2 kings
            int numPieces = 0;
            List<Piece> whitePieces = new List<Piece>();
            List<Piece> blackPieces = new List<Piece>();

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Piece piece = board.GetPiece(row, col);
                    if (piece == null) continue;
                    else
                    {
                        numPieces++;
                        if (piece.PieceType == PieceType.King) continue;
                        else if (piece.Player == Player.White) whitePieces.Add(piece);
                        else blackPieces.Add(piece);
                    }
                }
            }


            // determine if specific case known by tablebases or not
            if (numPieces == 3)
            {
                Board boardCopy = new Board(board);

                // reverse colours if more black pieces than white
                if (whitePieces.Count < blackPieces.Count)
                {
                    boardCopy = NormalForm.FlipColour(boardCopy);
                    whitePieces = blackPieces; // only contains a single piece
                }

                if (whitePieces.First().PieceType == PieceType.Queen)
                {
                    return KQK(boardCopy, board);
                } else if (whitePieces.First().PieceType == PieceType.Rook)
                {
                    return KRK(boardCopy, board);
                }
            }

            // TODO: Handle more cases

            return null;
        }

        public static Move KQK(Board board, Board original) // original used if a move is found, undo invariances
        {
            var transform = NormalForm.NormaliseBoard(board);

             // each table entry is 4 bytes
            int index = ComputeIndices.SimpleThreePieceBoardToIndex(board) * 4;
            if (board.CurrentPlayer == Player.Black) index += 2;

            int result = Properties.Resources.KQK[index];
            result = (result << 8) + Properties.Resources.KQK[index + 1];
            // result = [ 32 0's, FromRow, FromCol, ToRow, ToCol, 4 bit outcome ]

            // check some move found (could be 0 if no move possible i.e. in checkmate/stalemate)
            if ((result & 0xFFF0) == 0) return null;
            
            // compute move in terms of original board
            Square from = transform(Square.At((result >> 13) & 0x7, (result >> 10) & 0x7));
            Square to = transform(Square.At((result >> 7) & 0x7, (result >> 4) & 0x7));

            Console.WriteLine("Played endgame move");

            return new Move(from, to, original);
        }

        // TODO: Remove repetition
        public static Move KRK(Board board, Board original) // original used if a move is found, undo invariances
        {
            var transform = NormalForm.NormaliseBoard(board);

            // each table entry is 4 bytes
            int index = ComputeIndices.SimpleThreePieceBoardToIndex(board) * 4;
            if (board.CurrentPlayer == Player.Black) index += 2;

            int result = Properties.Resources.KRK[index];
            result = (result << 8) + Properties.Resources.KRK[index + 1];
            // result = [ 32 0's, FromRow, FromCol, ToRow, ToCol, 4 bit outcome ]

            // check some move found (could be 0 if no move possible i.e. in checkmate/stalemate)
            if ((result & 0xFFF0) == 0) return null;

            // compute move in terms of original board
            Square from = transform(Square.At((result >> 13) & 0x7, (result >> 10) & 0x7));
            Square to = transform(Square.At((result >> 7) & 0x7, (result >> 4) & 0x7));

            Console.WriteLine("Played endgame move");

            return new Move(from, to, original);
        }
    }
}
