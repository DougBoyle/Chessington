using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Chessington.GameEngine;
using Chessington.GameEngine.Pieces;
using Chessington.GameEngine.AI;
using static Chessington.GameEngine.AI.Endgame.NormalForm;
using System.IO;

using static Chessington.GameEngine.AI.Endgame.ComputeIndices;

namespace Analysis
{
    // TODO: Double check logic surrounding DTZ and how moves should be chosen (minimising DTM should be sufficient?)
    // TODO: Remember to flip win/lose between each side
    class ThreePieces
    {
        // 10 squares for white king (with symmetry invariance) * 60 for black king * 62 for other piece
        // many such positions will be illegal, not relevant to calculation
        // actually use 10*64*64 array just for convenience of calculating indexes here
        // (~82k entries in total, just for a single extra piece.
        //   Need to do for rook/queen and then also pawn, rest insufficient)
        public TableEntry[] whiteTable = new TableEntry[10 * 64 * 64];
        public TableEntry[] blackTable = new TableEntry[10 * 64 * 64];


        // another invariance is to always assume white has more material, can just flip colours if not the case
        public void SolveForQueen()
        {

            int whitePositions = 0;
            int blackPositions = 0;

            // populate table
            for (int wk = 0; wk < 10; wk++)
            {
                Square KingSquare = KingSquares[wk];
                for (int bk = 0; bk < 64; bk++)
                {
                    // avoid obviously useless positions
                    if (bk / 8 - KingSquare.Row <= 1 && KingSquare.Row - bk / 8 <= 1
                        && (bk % 8) - KingSquare.Col <= 1 && KingSquare.Col - (bk % 8) <= 1) continue;
                    Square BlackKingSquare = Square.At(bk / 8, bk % 8);

                    for (int q = 0; q < 64; q++)
                    {
                        Square QueenSquare = Square.At(q / 8, q % 8);
                        if (QueenSquare == KingSquare || QueenSquare == BlackKingSquare) continue; // not actually 3 pieces

                        Board b = new Board();
                        b.LeftWhiteCastling = false;
                        b.LeftBlackCastling = false;
                        b.RightBlackCastling = false;
                        b.RightWhiteCastling = false;
                        b.AddPiece(KingSquare, new King(Player.White));
                        b.AddPiece(BlackKingSquare, new King(Player.Black));
                        b.AddPiece(QueenSquare, new Queen(Player.White));


                        // can't be your turn if the other player is already in check
                        if (!b.InCheck(Player.Black))
                        {
                            whiteTable[wk * 64 * 64 + bk * 64 + q] = new TableEntry(b);
                            whitePositions++;
                        }
                        if (!b.InCheck(Player.White))
                        {
                            Board b2 = new Board(b);
                            b2.CurrentPlayer = Player.Black;
                            blackTable[wk * 64 * 64 + bk * 64 + q] = new TableEntry(b2);
                            blackPositions++;
                        }
                    }
                }
            }

            Console.WriteLine("Table populated {0} {1}", whitePositions, blackPositions);

            /******************* Table initialised, now start marking ***********************/

            bool changing = true;
            while (changing)
            {
                Console.WriteLine("Loop");
                changing = false;

                /******************** for black **********************************/
                for (int i = 0; i < blackTable.Length; i++)
                {
                    TableEntry entry = blackTable[i];
                    if (entry == null || entry.Outcome != Outcome.Unknown) continue;

                    var board = entry.Board;

                    var allAvailableMoves = board.GetAllAvailableMoves2();

                    if (allAvailableMoves.Count == 0) // either checkmate or stalemate
                    {
                        changing = true;
                        entry.DTM = 0;
                        entry.DTZ = 0;
                        entry.Outcome = board.InCheck(Player.Black) ? Outcome.Lose : Outcome.Draw;
                    } else if (allAvailableMoves.Exists(move => move.Captured != null)) // queen captured
                    {
                        changing = true;
                        entry.DTM = 0;
                        entry.DTZ = 0;
                        entry.Outcome = Outcome.Draw;
                    } else
                    {
                        // Recursively find best outcome based on whiteTable
                        // 1. All losing => losing, pick largest DTM.
                        // 2. Any win (without 50 move counter hit) => win, pick smallest DTM
                        // 3. All losing/drawing (at least 1 draw) => drawing, pick draw with smallest DTM
                        // *** technically, should try to set up stalemate traps, but can't detect with this method
                        // 4. Any unknown (and no win) => unknown, must wait
                        // 5. A capture => check smaller table. In this case, immediate draw, done above (black can never win)
                        var allEntries = allAvailableMoves.Select(move =>
                        {
                            var boardCopy = new Board(board);
                            boardCopy.GetPiece(move.From).MoveTo(boardCopy, move.To);
                            NormaliseBoard(boardCopy); // reason a copy of board needed, can't undo
                            return new Tuple<Move, TableEntry>(move, whiteTable[SimpleThreePieceBoardToIndex(boardCopy)]);
                        });

                        // no need to check for a win for black, insufficient material.
                        // most common case - can't tell yet
                        if (allEntries.Any(tuple => tuple.Item2.Outcome == Outcome.Unknown)) continue;

                        changing = true;
                        Tuple<Move, TableEntry> choice;

                        // draw by 50 move rule
                        if (allEntries.All(tuple => tuple.Item2.Outcome == Outcome.Win) &&
                            allEntries.Any(tuple => !TableEntry.ResetsFiftyMoveCounter(board, tuple.Item1)
                            && tuple.Item2.DTZ >= 99))
                        {
                            choice = allEntries.Where(tuple => !TableEntry.ResetsFiftyMoveCounter(board, tuple.Item1)
                                                                && tuple.Item2.DTZ >= 99).First();
                            entry.Outcome = Outcome.Draw; // far enough from any losing position to be a draw
                        } 
                        else
                        {
                            // if any gives a draw (shouldn't), will pick that, else move that gives furthest away mate
                            choice = allEntries.MaxBy(tuple => tuple.Item2.Outcome == Outcome.Draw ? 200 - tuple.Item2.DTM : tuple.Item2.DTM);
                            entry.Outcome = TableEntry.Opposite(choice.Item2.Outcome);
                        }
                        entry.BestMove = choice.Item1;
                        entry.DTM = choice.Item2.DTM + 1;
                        // zero's the 50 move counter or not (can be >100 if move chosen already maxed out counter)
                        entry.DTZ = TableEntry.ResetsFiftyMoveCounter(board, choice.Item1) ? 0 : choice.Item2.DTZ + 1;
                    }
                }

                /******************** for white **********************************/
                for (int i = 0; i < whiteTable.Length; i++)
                {
                    TableEntry entry = whiteTable[i];
                    if (entry == null || entry.Outcome != Outcome.Unknown) continue;

                    var board = entry.Board;

                    var allAvailableMoves = board.GetAllAvailableMoves2();

                    if (allAvailableMoves.Count == 0) // either checkmate or stalemate
                    {
                        changing = true;
                        entry.DTM = 0;
                        entry.DTZ = 0;
                        entry.Outcome = board.InCheck(Player.White) ? Outcome.Lose : Outcome.Draw;
                    }
                    // black has no pieces besides king, need not check for capture
                    else
                    {
                        // Recursively find best outcome based on blackTable
                        // White cannot lose, only win or draw
                        // 1. All losing => losing, pick largest DTM.
                        // 2. Any win (without 50 move counter hit) => win, pick smallest DTM
                        // 3. All losing/drawing (at least 1 draw) => drawing, pick draw with smallest DTM
                        // *** technically, should try to set up stalemate traps, but can't detect with this method
                        // 4. Any unknown (and no win) => unknown, must wait
                        var allEntries = allAvailableMoves.Select(move =>
                        {
                            var boardCopy = new Board(board);
                            boardCopy.GetPiece(move.From).MoveTo(boardCopy, move.To);
                            NormaliseBoard(boardCopy); // reason a copy of board needed, can't undo
                            return new Tuple<Move, TableEntry>(move, blackTable[SimpleThreePieceBoardToIndex(boardCopy)]);
                        });

                        Tuple<Move, TableEntry> choice;

                        // check for win - if making a move that doesn't reset counter, current count must be below 99 (relevant?)
                        if (allEntries.Any(tuple => tuple.Item2.Outcome == Outcome.Lose &&
                            (TableEntry.ResetsFiftyMoveCounter(board, tuple.Item1) || tuple.Item2.DTZ < 99)))
                        {
                            choice = allEntries.Where(tuple => tuple.Item2.Outcome == Outcome.Lose &&
                                    (TableEntry.ResetsFiftyMoveCounter(board, tuple.Item1) || tuple.Item2.DTZ < 99))
                                .MaxBy(tuple => -tuple.Item2.DTM); // shortest depth to mate
                            changing = true;
                            entry.BestMove = choice.Item1;
                            entry.DTM = choice.Item2.DTM + 1;
                            entry.DTZ = TableEntry.ResetsFiftyMoveCounter(board, choice.Item1) ? 0 : choice.Item2.DTZ + 1;
                            entry.Outcome = Outcome.Win;
                            continue;
                        }

                        // can't tell yet if no win found
                        if (allEntries.Any(tuple => tuple.Item2.Outcome == Outcome.Unknown)) continue;

                        changing = true;

                        // can't lose so just pick any draw, winning so maximise DTM (draw) in case they make mistake
                        // (shouldn't be possible for opponent to force a draw in KQ-K)

                        choice = allEntries.MaxBy(tuple => tuple.Item2.DTM);
                        entry.BestMove = choice.Item1;
                        entry.DTM = choice.Item2.DTM + 1;
                        // zero's the 50 move counter or not (can be >100 if move chosen already maxed out counter)
                        entry.DTZ = TableEntry.ResetsFiftyMoveCounter(board, choice.Item1) ? 0 : choice.Item2.DTZ + 1;
                        entry.Outcome = TableEntry.Opposite(choice.Item2.Outcome);
                    }
                }
            }

            /**************** every other position is a draw (shouldn't be any for KQ-K) *******************************/
            for (int i = 0; i < blackTable.Length; i++)
            {
                TableEntry entry = blackTable[i];
                if (entry == null || entry.Outcome != Outcome.Unknown) continue;

                var board = entry.Board;

                var allAvailableMoves = board.GetAllAvailableMoves2();

                // Can be a drawn position with some moves resulting in losing, so still need to filter
                // Know position ends in a draw, so can ignore DTM/DTZ

                var choice = allAvailableMoves.Where(move =>
                {
                    var boardCopy = new Board(board);
                    boardCopy.GetPiece(move.From).MoveTo(boardCopy, move.To);
                    NormaliseBoard(boardCopy); // reason a copy of board needed, can't undo
                        TableEntry result = whiteTable[SimpleThreePieceBoardToIndex(boardCopy)];
                    return result.Outcome == Outcome.Draw || result.Outcome == Outcome.Unknown; // unknown = draw now
                    }).First();

                entry.Outcome = Outcome.Draw;
                entry.BestMove = choice;
            }

            for (int i = 0; i < whiteTable.Length; i++)
            {
                TableEntry entry = whiteTable[i];
                if (entry == null || entry.Outcome != Outcome.Unknown) continue;

                var board = entry.Board;

                var allAvailableMoves = board.GetAllAvailableMoves2();

                // Can be a drawn position with some moves resulting in losing, so still need to filter
                // Know position ends in a draw, so can ignore DTM/DTZ

                var choice = allAvailableMoves.Where(move =>
                {
                    var boardCopy = new Board(board);
                    boardCopy.GetPiece(move.From).MoveTo(boardCopy, move.To);
                    NormaliseBoard(boardCopy); // reason a copy of board needed, can't undo
                    TableEntry result = blackTable[SimpleThreePieceBoardToIndex(boardCopy)];
                    return result.Outcome == Outcome.Draw || result.Outcome == Outcome.Unknown; // unknown = draw now
                }).First();

                entry.Outcome = Outcome.Draw;
                entry.BestMove = choice;

            }
        }


        // TODO: Refactor out a generic search function, especially filling in draws at the end
        public void SolveForRook()
        {

            int whitePositions = 0;
            int blackPositions = 0;

            // populate table
            for (int wk = 0; wk < 10; wk++)
            {
                Square KingSquare = KingSquares[wk];
                for (int bk = 0; bk < 64; bk++)
                {
                    // avoid obviously useless positions
                    if (bk / 8 - KingSquare.Row <= 1 && KingSquare.Row - bk / 8 <= 1
                        && (bk % 8) - KingSquare.Col <= 1 && KingSquare.Col - (bk % 8) <= 1) continue;
                    Square BlackKingSquare = Square.At(bk / 8, bk % 8);

                    for (int q = 0; q < 64; q++)
                    {
                        Square RookSquare = Square.At(q / 8, q % 8);
                        if (RookSquare == KingSquare || RookSquare == BlackKingSquare) continue; // not actually 3 pieces

                        Board b = new Board();
                        b.LeftWhiteCastling = false;
                        b.LeftBlackCastling = false;
                        b.RightBlackCastling = false;
                        b.RightWhiteCastling = false;
                        b.AddPiece(KingSquare, new King(Player.White));
                        b.AddPiece(BlackKingSquare, new King(Player.Black));
                        b.AddPiece(RookSquare, new Rook(Player.White));


                        // can't be your turn if the other player is already in check
                        if (!b.InCheck(Player.Black))
                        {
                            whiteTable[wk * 64 * 64 + bk * 64 + q] = new TableEntry(b);
                            whitePositions++;
                        }
                        if (!b.InCheck(Player.White))
                        {
                            Board b2 = new Board(b);
                            b2.CurrentPlayer = Player.Black;
                            blackTable[wk * 64 * 64 + bk * 64 + q] = new TableEntry(b2);
                            blackPositions++;
                        }
                    }
                }
            }

            Console.WriteLine("Table populated {0} {1}", whitePositions, blackPositions);

            /******************* Table initialised, now start marking ***********************/

            bool changing = true;
            while (changing)
            {
                Console.WriteLine("Loop");
                changing = false;

                /******************** for black **********************************/
                for (int i = 0; i < blackTable.Length; i++)
                {
                    TableEntry entry = blackTable[i];
                    if (entry == null || entry.Outcome != Outcome.Unknown) continue;

                    var board = entry.Board;

                    var allAvailableMoves = board.GetAllAvailableMoves2();

                    if (allAvailableMoves.Count == 0) // either checkmate or stalemate
                    {
                        changing = true;
                        entry.DTM = 0;
                        entry.DTZ = 0;
                        entry.Outcome = board.InCheck(Player.Black) ? Outcome.Lose : Outcome.Draw;
                    }
                    else if (allAvailableMoves.Exists(move => move.Captured != null)) // queen captured
                    {
                        changing = true;
                        entry.DTM = 0;
                        entry.DTZ = 0;
                        entry.Outcome = Outcome.Draw;
                    }
                    else
                    {
                        // Recursively find best outcome based on whiteTable
                        // 1. All losing => losing, pick largest DTM.
                        // 2. Any win (without 50 move counter hit) => win, pick smallest DTM
                        // 3. All losing/drawing (at least 1 draw) => drawing, pick draw with smallest DTM
                        // *** technically, should try to set up stalemate traps, but can't detect with this method
                        // 4. Any unknown (and no win) => unknown, must wait
                        // 5. A capture => check smaller table. In this case, immediate draw, done above (black can never win)
                        var allEntries = allAvailableMoves.Select(move =>
                        {
                            var boardCopy = new Board(board);
                            boardCopy.GetPiece(move.From).MoveTo(boardCopy, move.To);
                            NormaliseBoard(boardCopy); // reason a copy of board needed, can't undo
                            return new Tuple<Move, TableEntry>(move, whiteTable[SimpleThreePieceBoardToIndex(boardCopy)]);
                        });

                        // no need to check for a win for black, insufficient material.
                        // most common case - can't tell yet
                        if (allEntries.Any(tuple => tuple.Item2.Outcome == Outcome.Unknown)) continue;

                        changing = true;
                        Tuple<Move, TableEntry> choice;

                        // draw by 50 move rule
                        if (allEntries.All(tuple => tuple.Item2.Outcome == Outcome.Win) &&
                            allEntries.Any(tuple => !TableEntry.ResetsFiftyMoveCounter(board, tuple.Item1)
                            && tuple.Item2.DTZ >= 99))
                        {
                            choice = allEntries.Where(tuple => !TableEntry.ResetsFiftyMoveCounter(board, tuple.Item1)
                                                                && tuple.Item2.DTZ >= 99).First();
                            entry.Outcome = Outcome.Draw; // far enough from any losing position to be a draw
                        }
                        else
                        {
                            // if any gives a draw (shouldn't), will pick that, else move that gives furthest away mate
                            choice = allEntries.MaxBy(tuple => tuple.Item2.Outcome == Outcome.Draw ? 200 - tuple.Item2.DTM : tuple.Item2.DTM);
                            entry.Outcome = TableEntry.Opposite(choice.Item2.Outcome);
                        }
                        entry.BestMove = choice.Item1;
                        entry.DTM = choice.Item2.DTM + 1;
                        // zero's the 50 move counter or not (can be >100 if move chosen already maxed out counter)
                        entry.DTZ = TableEntry.ResetsFiftyMoveCounter(board, choice.Item1) ? 0 : choice.Item2.DTZ + 1;
                    }
                }

                /******************** for white **********************************/
                for (int i = 0; i < whiteTable.Length; i++)
                {
                    TableEntry entry = whiteTable[i];
                    if (entry == null || entry.Outcome != Outcome.Unknown) continue;

                    var board = entry.Board;

                    var allAvailableMoves = board.GetAllAvailableMoves2();

                    if (allAvailableMoves.Count == 0) // either checkmate or stalemate
                    {
                        changing = true;
                        entry.DTM = 0;
                        entry.DTZ = 0;
                        entry.Outcome = board.InCheck(Player.White) ? Outcome.Lose : Outcome.Draw;
                    }
                    // black has no pieces besides king, need not check for capture
                    else
                    {
                        // Recursively find best outcome based on blackTable
                        // White cannot lose, only win or draw
                        // 1. All losing => losing, pick largest DTM.
                        // 2. Any win (without 50 move counter hit) => win, pick smallest DTM
                        // 3. All losing/drawing (at least 1 draw) => drawing, pick draw with smallest DTM
                        // *** technically, should try to set up stalemate traps, but can't detect with this method
                        // 4. Any unknown (and no win) => unknown, must wait
                        var allEntries = allAvailableMoves.Select(move =>
                        {
                            var boardCopy = new Board(board);
                            boardCopy.GetPiece(move.From).MoveTo(boardCopy, move.To);
                            NormaliseBoard(boardCopy); // reason a copy of board needed, can't undo
                            return new Tuple<Move, TableEntry>(move, blackTable[SimpleThreePieceBoardToIndex(boardCopy)]);
                        });

                        Tuple<Move, TableEntry> choice;

                        // check for win - if making a move that doesn't reset counter, current count must be below 99 (relevant?)
                        if (allEntries.Any(tuple => tuple.Item2.Outcome == Outcome.Lose &&
                            (TableEntry.ResetsFiftyMoveCounter(board, tuple.Item1) || tuple.Item2.DTZ < 99)))
                        {
                            choice = allEntries.Where(tuple => tuple.Item2.Outcome == Outcome.Lose &&
                                    (TableEntry.ResetsFiftyMoveCounter(board, tuple.Item1) || tuple.Item2.DTZ < 99))
                                .MaxBy(tuple => -tuple.Item2.DTM); // shortest depth to mate
                            changing = true;
                            entry.BestMove = choice.Item1;
                            entry.DTM = choice.Item2.DTM + 1;
                            entry.DTZ = TableEntry.ResetsFiftyMoveCounter(board, choice.Item1) ? 0 : choice.Item2.DTZ + 1;
                            entry.Outcome = Outcome.Win;
                            continue;
                        }

                        // can't tell yet if no win found
                        if (allEntries.Any(tuple => tuple.Item2.Outcome == Outcome.Unknown)) continue;

                        changing = true;

                        // can't lose so just pick any draw, winning so maximise DTM (draw) in case they make mistake
                        // (shouldn't be possible for opponent to force a draw in KQ-K)

                        choice = allEntries.MaxBy(tuple => tuple.Item2.DTM);
                        entry.BestMove = choice.Item1;
                        entry.DTM = choice.Item2.DTM + 1;
                        // zero's the 50 move counter or not (can be >100 if move chosen already maxed out counter)
                        entry.DTZ = TableEntry.ResetsFiftyMoveCounter(board, choice.Item1) ? 0 : choice.Item2.DTZ + 1;
                        entry.Outcome = TableEntry.Opposite(choice.Item2.Outcome);
                    }
                }
            }

            /**************** every other position is a draw (shouldn't be any for KQ-K) *******************************/
            for (int i = 0; i < blackTable.Length; i++)
            {
                TableEntry entry = blackTable[i];
                if (entry == null || entry.Outcome != Outcome.Unknown) continue;

                var board = entry.Board;

                var allAvailableMoves = board.GetAllAvailableMoves2();

                // Can be a drawn position with some moves resulting in losing, so still need to filter
                // Know position ends in a draw, so can ignore DTM/DTZ

                var choice = allAvailableMoves.Where(move =>
                {
                    var boardCopy = new Board(board);
                    boardCopy.GetPiece(move.From).MoveTo(boardCopy, move.To);
                    NormaliseBoard(boardCopy); // reason a copy of board needed, can't undo
                    TableEntry result = whiteTable[SimpleThreePieceBoardToIndex(boardCopy)];
                    return result.Outcome == Outcome.Draw || result.Outcome == Outcome.Unknown; // unknown = draw now
                }).First();

                entry.Outcome = Outcome.Draw;
                entry.BestMove = choice;
            }

            for (int i = 0; i < whiteTable.Length; i++)
            {
                TableEntry entry = whiteTable[i];
                if (entry == null || entry.Outcome != Outcome.Unknown) continue;

                var board = entry.Board;

                var allAvailableMoves = board.GetAllAvailableMoves2();

                // Can be a drawn position with some moves resulting in losing, so still need to filter
                // Know position ends in a draw, so can ignore DTM/DTZ

                var choice = allAvailableMoves.Where(move =>
                {
                    var boardCopy = new Board(board);
                    boardCopy.GetPiece(move.From).MoveTo(boardCopy, move.To);
                    NormaliseBoard(boardCopy); // reason a copy of board needed, can't undo
                    TableEntry result = blackTable[SimpleThreePieceBoardToIndex(boardCopy)];
                    return result.Outcome == Outcome.Draw || result.Outcome == Outcome.Unknown; // unknown = draw now
                }).First();

                entry.Outcome = Outcome.Draw;
                entry.BestMove = choice;

            }
        }


        // storing to a file:
        // entry size:
        // Board = 16 bits (4 bits for wk, 6 for bk, 6 for other piece)
        // Best move = 12 bits (ignoring promotion - 6 bits per square) per side (best for black/white)
        // Leaves 8 bits spare to indicate outcome: 0 = draw, 1 = win, 2 = lose for each side
        // Overall 48 bits = 6 byte entries:
        // [ Board | White Move | White Outcome | Black Move | Black Outcome ]
        // BoardToIndex computes board value: [ wk | bk | other piece ]
        // Moves stored as [ FromRow | FromCol | ToRow | ToCol ] (still have 2 bits free to encode promotion if needed)
        public static long EncodeEntry(TableEntry whiteEntry, TableEntry blackEntry, int index)
        {
            // whiteEntry/blackEntry should be for the same position (key), one may be null
            long result = index; // BoardToIndex(whiteEntry == null ? blackEntry.Board : whiteEntry.Board);

            if (whiteEntry != null)
            {
                if (whiteEntry.BestMove != null)
                {
                    result <<= 6;
                    result += whiteEntry.BestMove.From.Row * 8 + whiteEntry.BestMove.From.Col;
                    result <<= 6;
                    result += whiteEntry.BestMove.To.Row * 8 + whiteEntry.BestMove.To.Col;
                } else
                {
                    result <<= 12; // may be in stale/checkmate, in which case no move possible, so set to 0
                }
                result <<= 4;
                result += whiteEntry.Outcome == Outcome.Win ? 1 : whiteEntry.Outcome == Outcome.Lose ? 2 : 0;
            } else
            {
                result <<= 16;
            }
            if (blackEntry != null)
            {
                if (blackEntry.BestMove != null)
                {
                    result <<= 6;
                    result += blackEntry.BestMove.From.Row * 8 + blackEntry.BestMove.From.Col;
                    result <<= 6;
                    result += blackEntry.BestMove.To.Row * 8 + blackEntry.BestMove.To.Col;
                } else
                {
                    result <<= 12;
                }
                result <<= 4;
                result += blackEntry.Outcome == Outcome.Win ? 1 : blackEntry.Outcome == Outcome.Lose ? 2 : 0;
            }
            else
            {
                result <<= 16;
            }
            return result;
        }

        // Use board value as lookup key rather than index, so that invalid positions ignored
        // (reduces tablebase size, and binary search ensures only logarithmic cost to lookup)

        // TODO: Storing every index, even invalid positions, means index need not be included explicity.
        //       Reduces size from ~205KB to 160KB, and makes lookup faster/easier too
        public void WriteTables(string filename)
        {
            using (FileStream fs = File.OpenWrite(filename))
            {
                for (int i = 0; i < whiteTable.Length; i++)
                {
                  //  if (whiteTable[i] != null || blackTable[i] != null)
                  //  {
                    long encoded = EncodeEntry(whiteTable[i], blackTable[i], i);
                    byte[] bytes = BitConverter.GetBytes(encoded);
                    if (BitConverter.IsLittleEndian) Array.Reverse(bytes); 
                     //  fs.Write(bytes, 2, 6); // only 48 bits, not 64, so skip first 2 bytes
                    fs.Write(bytes, 4, 4); // ignore the index itself
                  //  }
                }
            }
        }
    }
}
