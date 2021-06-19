using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.Pieces
{
    public class King : Piece
    {
        public King(Player player)
            : base(player)
        {
            PieceType = PieceType.King;
        }

        public override IEnumerable<Square> GetAvailableMoves(Board board)
        {
            List<Square> availableMoves = base.GetAvailableMoves(board).ToList();
            var currentPosition = board.FindPiece(this);
            
            if (!board.InCheck(Player))
            {
                foreach (var direction in new int[] {-1, 1} )
                {
                    var blackBool = direction == -1 ? board.LeftBlackCastling : board.RightBlackCastling;
                    var whiteBool = direction == -1 ? board.LeftWhiteCastling : board.RightWhiteCastling;
                    if (Player == Player.White && whiteBool ||
                        Player == Player.Black && blackBool)
                    {
                        if (direction ==-1 && !board.IsSquareEmpty(Square.At(currentPosition.Row, currentPosition.Col - 3)))
                        {
                            continue;
                        }

                        Square firstSquare = Square.At(currentPosition.Row, currentPosition.Col + 1 * direction);
                        Square secondSquare = Square.At(currentPosition.Row, currentPosition.Col + 2 * direction);
                        if (board.IsSquareEmpty(firstSquare) &&
                            board.IsSquareEmpty(secondSquare))
                        {
                            board.AddPiece(firstSquare,this);
                            board.AddPiece(currentPosition, null);
                            if (board.InCheck(Player))
                            {
                                board.AddPiece(firstSquare,null);
                                board.AddPiece(currentPosition, this);
                                continue;
                            }
                            board.AddPiece(secondSquare,this);
                            board.AddPiece(firstSquare, null);
                            if (!board.InCheck(Player))
                            {
                                availableMoves.Add(secondSquare);
                            }
                            board.AddPiece(secondSquare,null);
                            board.AddPiece(currentPosition, this);
                        }
                    }
                }
            }

            return availableMoves;
        }

        public override IEnumerable<Square> GetRelaxedAvailableMoves(Board board)
        {
            List<Square> availableMoves = new List<Square>();
            var currentPosition = board.FindPiece(this);

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x != 0 || y != 0)
                    {
                        availableMoves.Add(new Square(currentPosition.Row + x, currentPosition.Col + y));
                    }
                }
            }

            return availableMoves.Where(square => square.IsValid() && board.IsEmptyOrOpponent(square, Player));
        }


        public override void MoveTo(Board board, Square newSquare)
        {
            var currentPosition = board.FindPiece(this);
            if (Player == Player.White)
            {
                if (currentPosition.Equals(Square.At(7,4)))
                {
                    if (newSquare.Col == 6)
                    {
                        board.AddPiece(Square.At(7,5), board.GetPiece(Square.At(7,7)));
                        board.AddPiece(Square.At(7,7), null);
                    }
                    else if (newSquare.Col == 2)
                    {
                        board.AddPiece(Square.At(7,3), board.GetPiece(Square.At(7,0)));
                        board.AddPiece(Square.At(7,0), null);
                    }
                }
                board.RightWhiteCastling = false;
                board.LeftWhiteCastling = false;

            }
            else
            {
                if (currentPosition.Equals(Square.At(0,4)))
                {
                    if (newSquare.Col == 6)
                    {
                        board.AddPiece(Square.At(0,5), board.GetPiece(Square.At(0,7)));
                        board.AddPiece(Square.At(0,7), null);
                    }
                    else if (newSquare.Col == 2)
                    {
                        board.AddPiece(Square.At(0,3), board.GetPiece(Square.At(0,0)));
                        board.AddPiece(Square.At(0,0), null);
                    }
                }
                board.RightBlackCastling = false;
                board.LeftBlackCastling = false;
            }
            
            
            base.MoveTo(board, newSquare);
        }
    }
}