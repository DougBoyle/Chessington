﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Chessington.GameEngine.Pieces {
    public class Pawn : Piece {
        public Pawn(Player player)
            : base(player) { }

        public override IEnumerable<Square> GetAvailableMoves(Board board) {
            
            Square here = board.FindPiece(this);
            
            var direction = Player == Player.White ? -1 : 1;
            var homeRow = Player == Player.White ? GameSettings.BoardSize - 2 : 1;
            
            var available = new List<Square>()
            {
                new Square(here.Row + direction, here.Col - 1), new Square(here.Row + direction, here.Col + 1)
            }.Where(square => square.IsValid() && board.IsOpponent(square, Player) || 
                                                  square.Equals(board.EnPassantSquare)).ToList();
            
            Square targetMove = new Square(here.Row + direction, here.Col);

            if (!targetMove.IsValid() || !board.IsSquareEmpty(targetMove)) {
                return available;
            }
            available.Add(targetMove);
                
            Square targetMove2 = new Square(here.Row + 2 * direction, here.Col);
            if (here.Row == homeRow && board.IsSquareEmpty(targetMove2)) 

            {
                available.Add(targetMove2);
            }

            return available;
        }
        
        public override void MoveTo(Board board, Square newSquare)
        {
            var currentSquare = board.FindPiece(this);
            
            if (newSquare.Equals(board.EnPassantSquare))
            {
                board.OnPieceCaptured(board.GetPiece(Square.At(currentSquare.Row, newSquare.Col)));
                board.AddPiece(Square.At(currentSquare.Row, newSquare.Col),null);
            }
            base.MoveTo(board, newSquare);
            if (currentSquare.Row - newSquare.Row == 2 || currentSquare.Row - newSquare.Row == -2)
            {
                board.EnPassantSquare = Square.At((currentSquare.Row + newSquare.Row) / 2, newSquare.Col);
            }
        }
    }
}