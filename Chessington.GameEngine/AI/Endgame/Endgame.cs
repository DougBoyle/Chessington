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

                switch (whitePieces.First().PieceType)
                {
                    case PieceType.Queen: return KQK(boardCopy, board);
                    case PieceType.Rook: return KRK(boardCopy, board);
                    case PieceType.Pawn: return KPK(boardCopy, board);
                    default: return null;
                }
            }

            // TODO: Handle more cases

            return null;
        }

        public static Move MoveFromIndexAndTable(byte[] table, int index, NormalForm.SquareMapper transform, Board original)
        {
            int result = table[index];
            result = (result << 8) + table[index + 1];
            // result = [ 32 0's, Promotion,  FromRow, FromCol, ToRow, ToCol, 2 bit outcome ]

            // check some move found (could be 0 if no move possible i.e. in checkmate/stalemate)
            if ((result & 0xFFFC) == 0) return null;

            // compute move in terms of original board (can ignore promotion value, just 00 placeholder)
            Square from = transform(Square.At((result >> 11) & 0x7, (result >> 8) & 0x7));
            Square to = transform(Square.At((result >> 5) & 0x7, (result >> 2) & 0x7));

            int promotion = BitUtils.NO_PIECE;
            int promotionKey = (int)((uint)result >> 14); // to achieve logical rather than arithmetic shift
            if (promotionKey == 1) promotion = BitUtils.ROOK_BOARD + 6 * (int)original.CurrentPlayer;
            else if (promotionKey == 2) promotion = BitUtils.QUEEN_BOARD + 6 * (int)original.CurrentPlayer;

            Console.WriteLine("Played endgame move");
            return new Move(from, to, original.GetPieceIndex(from), original.GetPieceIndex(to), promotion);
        }

        public static Move KQK(Board board, Board original) // original used if a move is found, undo invariances
        {
            var transform = NormalForm.NormaliseBoard(board);

             // each table entry is 4 bytes
            int index = ComputeIndices.SimpleThreePieceBoardToIndex(board) * 4;
            if (board.CurrentPlayer == Player.Black) index += 2;

            return MoveFromIndexAndTable(Properties.Resources.KQK, index, transform, original);
        }

        public static Move KRK(Board board, Board original) // original used if a move is found, undo invariances
        {
            var transform = NormalForm.NormaliseBoard(board);

            // each table entry is 4 bytes
            int index = ComputeIndices.SimpleThreePieceBoardToIndex(board) * 4;
            if (board.CurrentPlayer == Player.Black) index += 2;

            return MoveFromIndexAndTable(Properties.Resources.KRK, index, transform, original);
        }

        public static Move KPK(Board board, Board original)
        {
            var transform = NormalForm.NormalisePawnBoard(board);

            // each table entry is 4 bytes
            int index = ComputeIndices.ThreePiecePawnBoardToIndex(board) * 4;
            if (board.CurrentPlayer == Player.Black) index += 2;

            return MoveFromIndexAndTable(Properties.Resources.KPK, index, transform, original);
        }
    }
}
