﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.Pieces
{
    public class Pawn : Piece
    {
        public Pawn(Player player) 
            : base(player) { }

        public override IEnumerable<Square> GetAvailableMoves(Board board) {
            
            Square here = board.FindPiece(this);

            int index = Player==Player.White ? -1 : 1 ;
            List<Square> available = new List<Square>()
            {
                new Square(here.Row + index, here.Col - 1), new Square(here.Row + index, here.Col + 1)
            };
            available = available.Where(square => square.IsValid() && board.IsOpponent(square, Player) || 
                                                  square.Equals(board.EnPassantSquare)).ToList();
            if (Player == Player.White)
            {
                Square targetMove = new Square(here.Row - 1, here.Col);
                if (targetMove.IsValid() && board.IsSquareEmpty(targetMove))
                {
                    Square targetMove2 = new Square(here.Row - 2, here.Col);
                    if (here.Row != 0)
                    {
                        available.Add(targetMove);
                    }
                    if (here.Row == GameSettings.BoardSize - 2 && board.IsSquareEmpty(targetMove2))
                    {
                        available.Add(targetMove2);
                    }
                }
            } else {
                Square targetMove = new Square(here.Row +1, here.Col);
                if (targetMove.IsValid() && board.IsSquareEmpty(targetMove))
                {
                    Square targetMove2 = new Square(here.Row +2, here.Col);   
                    if (here.Row != GameSettings.BoardSize - 1)
                    {
                        available.Add(targetMove);
                    }
                    if (here.Row == 1 && board.IsSquareEmpty(targetMove2))
                    {
                        available.Add(targetMove2);
                    }
                }
            }
            return available;
        }
        
        public override void MoveTo(Board board, Square newSquare)
        {
            var currentSquare = board.FindPiece(this);
            base.MoveTo(board, newSquare);
            if (newSquare.Equals(board.EnPassantSquare))
            {
                board.OnPieceCaptured(board.GetPiece(Square.At(currentSquare.Row, newSquare.Col)));
                board.AddPiece(Square.At(currentSquare.Row, newSquare.Col),null);
            }
            if (currentSquare.Row - newSquare.Row == 2 || currentSquare.Row - newSquare.Row == -2)
            {
                board.EnPassantSquare = Square.At((currentSquare.Row + newSquare.Row) / 2, newSquare.Col);
            }

            
        }
    }
}