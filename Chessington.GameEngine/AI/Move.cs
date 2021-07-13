using System;

using Chessington.GameEngine.Pieces;

namespace Chessington.GameEngine.AI
{
    // For ordering all moves when doing alpha-beta pruning (rather than one piece at a time)
    // and to allow making/undoing moves

    // TODO: Add Piece 'moving' to allow finding correct bitboard to make/undo moves (really just need an int index)
    //       e.g. int movingPiece = pieceType + 6*player
    //       Depending on how moves generated, will generally be known e.g. specifically generate rook moves (new constructor)
    public class Move : IComparable<Move>
    {
        // TODO: Make format of everything more efficient e.g. byte vs 4 bools, 2 ints for Square rather than 1 byte 0-63
        // TODO: In terms of bitboard, still have the row reversed issue with Square vs index (BitUtils.SwapRow)
        public byte FromIdx;
        public byte ToIdx;

        // TODO: Switch to ints rather than Piece objects - (-1 = NO_PIECE) used as null (and en-passant)
        public int CapturedPiece;
        // Promotion = NO_PIECE (-1) if not promoting, else is the piece to replace with
        public int PromotionPiece;

        // TODO: Use this to add a MakeMove/UndoMove function to Board, so no longer need to do GetPiece/work with objects
        // using overriden method is inefficient, as it requires storing pieces as objects rather than single bits
        public int MovingPiece; // 0-5 = white, 6-11 = black. Corresponds to Bitboard[] to allow changing correct board

        // information about 50-move counter/castling status/en-passant can be remembered once from the board, not stored per move


        public Move(byte from, byte to, int moving, int captured, int promotion)
        {
        //    To = to;
            CapturedPiece = captured;
            PromotionPiece = promotion;
            MovingPiece = moving;

            FromIdx = from;
            ToIdx = to;
        }

        // TODO: Only used for pawn tests, rewrite tests + remove this
        public Move(Square from, Square to, Board before)
        {
            var captured = before.GetPiece(to);
            CapturedPiece = captured == null ? BitUtils.NO_PIECE : BitUtils.PieceToBoardIndex(captured);

            // doesn't detect promotions
            // System.Diagnostics.Debug.Assert(before.GetPiece(from).PieceType != PieceType.Pawn || !(to.Row == 0 || to.Row == 7));

            PromotionPiece = -1;
            MovingPiece = BitUtils.PieceToBoardIndex(before.GetPiece(from));

            FromIdx = (byte)BitUtils.SquareToIndex(from);
            ToIdx = (byte)BitUtils.SquareToIndex(to);
        }

        // for converting polyglot entries to move objects. See: http://hgm.nubati.net/book_format.html
        // only done for opening move choices, so need not be efficient
        public Move(ushort move, Board board)
        {
            Square From = Square.At(7 - ((move & 0b111000_000000) >> 9), (move & 0b111_000000) >> 6);
            Square To = Square.At(7 - ((move & 0b111000) >> 3), move & 0b111);

            // Polyglot treats castling as moving from file 4 (e) to file 0/7 so need to correct for this
            if (board.GetPiece(From).PieceType == PieceType.King && From.Col == 4 && To.Col == 0)
            {
                To = Square.At(From.Row, 2);
                CapturedPiece = BitUtils.NO_PIECE;
                PromotionPiece = BitUtils.NO_PIECE;
            }
            else if (board.GetPiece(From).PieceType == PieceType.King && From.Col == 4 && To.Col == 7)
            {
                To = Square.At(From.Row, 6);
                CapturedPiece = BitUtils.NO_PIECE;
                PromotionPiece = BitUtils.NO_PIECE;
            }
            else
            {
                // expensive, but only used when converting opening table entry to move, so not too bad
                // TODO: May even change board.GetPiece to return an int anyway
                CapturedPiece = board.GetPiece(To) == null ? BitUtils.NO_PIECE : BitUtils.PieceToBoardIndex(board.GetPiece(To));
                int promotion = (move & 0xF000) >> 12;
                if (promotion == 0) PromotionPiece = BitUtils.NO_PIECE;
                else PromotionPiece = promotion + 6 * (int)board.CurrentPlayer;
            }
            MovingPiece = BitUtils.PieceToBoardIndex(board.GetPiece(From));

            FromIdx = (byte)BitUtils.SquareToIndex(From);
            ToIdx = (byte)BitUtils.SquareToIndex(To);
        }

        public static char[] columns = { 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h' };
        
        // 0 is top of the board, 7 is bottom (white)
        public override string ToString()
        {
            return columns[FromIdx % 8] + (1 + FromIdx/8).ToString() + "-" + columns[ToIdx % 8] + (1 + ToIdx/8).ToString();
        }

        public int CompareTo(Move other)
        {
            return other.CapturedPiece - CapturedPiece; // more significant capture => earlier in list
        }
    }
}
