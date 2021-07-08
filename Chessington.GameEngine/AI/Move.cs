using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine.AI
{
    // For ordering all moves when doing alpha-beta pruning (rather than one piece at a time)
    // and to allow making/undoing moves
    // TODO: Produce these rather than Lists of Squares when generating moves
    public class Move
    {
        // TODO: Make format of everything more efficient e.g. byte vs 4 bools, 2 ints for Square rather than 1 byte 0-63
        // Important: Original move was From->To, so now need to move in reverse direction
        public readonly Square From;
        public readonly Square To;

        public Piece Captured; // is null for en-passant
        // Promotion = null if not promoting, else is the piece to replace with
        public Piece Promotion; 

        // information about 50-move counter/castling status/en-passant can be remembered once from the board, not stored per move

        public Move(Square from, Square to, Piece captured, Piece promotion)
        {
            From = from;
            To = to;
            Captured = captured;
            Promotion = promotion;
        }

        // convert a pair of squares to a 'Move' - TODO: Produce Move instances directly
        // Note: Can't be used for pawn promotions, don't know what to promote to
        public Move(Square from, Square to, Board before)
        {
            From = from;
            To = to;
            Captured = before.GetPiece(to);

            // doesn't detect promotions
            // System.Diagnostics.Debug.Assert(before.GetPiece(from).PieceType != PieceType.Pawn || !(to.Row == 0 || to.Row == 7));
           
            Promotion = null; // (before.GetPiece(from).PieceType == PieceType.Pawn) && (to.Row == 0 || to.Row == 7);
        }

        // for converting polyglot entries to move objects. See: http://hgm.nubati.net/book_format.html
        // only done for opening move choices, so need not be efficient
        public Move(ushort move, Board board)
        {
            From = Square.At(7 - ((move & 0b111000_000000) >> 9), (move & 0b111_000000) >> 6);
            To = Square.At(7 - ((move & 0b111000) >> 3), move & 0b111);

            // Polyglot treats castling as moving from file 4 (e) to file 0/7 so need to correct for this
            if (board.GetPiece(From).PieceType == PieceType.King && From.Col == 4 && To.Col == 0)
            {
                To = Square.At(From.Row, 2);
                Captured = null;
                Promotion = null;
            }
            else if (board.GetPiece(From).PieceType == PieceType.King && From.Col == 4 && To.Col == 7)
            {
                To = Square.At(From.Row, 6);
                Captured = null;
                Promotion = null;
            }
            else
            {
                Captured = board.GetPiece(To);
                int promote = (move & 0xF000) >> 12;
                switch (promote)
                {
                    case 1: Promotion = new Knight(board.CurrentPlayer); break;
                    case 2: Promotion = new Bishop(board.CurrentPlayer); break;
                    case 3: Promotion = new Rook(board.CurrentPlayer); break;
                    case 4: Promotion = new Queen(board.CurrentPlayer); break;
                    default: Promotion = null; break;
                }
            }
        }

        public static char[] columns = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
        
        // 0 is top of the board, 7 is bottom (white)
        public override string ToString()
        {
            return columns[From.Col] + (8 - From.Row).ToString() + "-" + columns[To.Col] + (8 - To.Row).ToString();
        }
    }
}
