using Chessington.GameEngine;
using Chessington.GameEngine.AI;
using Chessington.GameEngine.AI.Endgame;
using Chessington.GameEngine.Pieces;

using static Chessington.GameEngine.AI.Endgame.NormalForm;
using System.IO;
using static Chessington.GameEngine.AI.Endgame.ComputeIndices;
using static Chessington.GameEngine.BitUtils;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analysis
{
    // separate class, as it relies on the other two tables and needs normalising differently
    // finds longest checkmate for white as 55 ply (28 moves), matches online results:
    // http://kirill-kryukov.com/chess/longest-checkmates/longest-checkmates.shtml
    public class ThreePiecesPawn
    {
        public ThreePieces queenTable;
        public ThreePieces rookTable;

        // each table has 98k entries rather than 41k, so only actually an increase of about 2x
        // (ignoring any issues of how many are actually valid or not)
        // Can still ignore en-passant as only 1 pawn on the board
        public TableEntry[] whiteTable = new TableEntry[32 * 64 * 48];
        public TableEntry[] blackTable = new TableEntry[32 * 64 * 48];

        public ThreePiecesPawn()
        {
            // computes a couple 100KB of data - use this rather than stored tablebases to access DTM (and DTZ) field
            Console.WriteLine("Initialising Queen table:");
            queenTable = new ThreePieces();
            queenTable.SolveForQueen();
            Console.WriteLine("Initialising Rook table:");
            rookTable = new ThreePieces();
            rookTable.SolveForRook();
            Console.WriteLine("Both tables initialised, can now solve Pawns");
        }

        public void SolveForPawn()
        {
            int whitePositions = 0;
            int blackPositions = 0;

            // populate table
            for (int wk = 0; wk < 32; wk++)
            {
                Square KingSquare = Square.At(wk / 4, wk % 4);
                for (int bk = 0; bk < 64; bk++)
                {
                    // avoid obviously useless positions
                    if (bk / 8 - KingSquare.Row <= 1 && KingSquare.Row - bk / 8 <= 1
                        && (bk % 8) - KingSquare.Col <= 1 && KingSquare.Col - (bk % 8) <= 1) continue;
                    Square BlackKingSquare = Square.At(bk / 8, bk % 8);

                    for (int p = 8; p < 56; p++) // never placed on end rows
                    {
                        Square PawnSquare = Square.At(p / 8, p % 8);
                        if (PawnSquare == KingSquare || PawnSquare == BlackKingSquare) continue; // not actually 3 pieces

                        Board b = new Board();
                        b.Castling = 0;
                        b.AddPiece(KingSquare, new King(Player.White));
                        b.AddPiece(BlackKingSquare, new King(Player.Black));
                        b.AddPiece(PawnSquare, new Pawn(Player.White));


                        // can't be your turn if the other player is already in check
                        if (!b.InCheck(Player.Black))
                        {
                            whiteTable[wk * 64 * 48 + bk * 48 + (p-8)] = new TableEntry(b);
                            whitePositions++;
                        }
                        if (!b.InCheck(Player.White))
                        {
                            Board b2 = new Board(b);
                            b2.CurrentPlayer = Player.Black;
                            blackTable[wk * 64 * 48 + bk * 48 + (p-8)] = new TableEntry(b2);
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

                    var allAvailableMoves = board.GetAllAvailableMoves();

                    if (allAvailableMoves.Count == 0) // either checkmate or stalemate
                    {
                        changing = true;
                        entry.DTM = 0;
                        entry.DTZ = 0;
                        entry.Outcome = board.InCheck(Player.Black) ? Outcome.Lose : Outcome.Draw;
                    }
                    else if (allAvailableMoves.Exists(move => move.CapturedPiece >= 0)) // pawn captured (best black can expect is draw)
                    {
                        changing = true;
                        entry.DTM = 0;
                        entry.DTZ = 0;
                        entry.Outcome = Outcome.Draw;
                    }
                    else
                    {
                        // black can't promote, so no different to in ThreePieces, just normalise/lookup differently

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
                            boardCopy.MakeMove(move);
                            NormalisePawnBoard(boardCopy); // reason a copy of board needed, can't undo
                            return new Tuple<Move, TableEntry>(move, whiteTable[ThreePiecePawnBoardToIndex(boardCopy)]);
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

                    var allAvailableMoves = board.GetAllAvailableMoves();

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
                            boardCopy.MakeMove(move);

                            TableEntry result;

                            // table to check changes depending on (if) promotion made
                            // need to change board depending on which pieces now present
                            if (move.PromotionPiece == NO_PIECE)
                            {
                                // still a KP-K position
                                NormalisePawnBoard(boardCopy); // reason a copy of board needed, can't undo
                                result = blackTable[ThreePiecePawnBoardToIndex(boardCopy)];
                            }
                            else if (move.PromotionPiece % 6 == QUEEN_BOARD)
                            {
                                NormaliseBoard(boardCopy);
                                result = queenTable.blackTable[SimpleThreePieceBoardToIndex(boardCopy)];
                            }
                            else if (move.PromotionPiece % 6 == ROOK_BOARD)
                            {
                                NormaliseBoard(boardCopy);
                                result = rookTable.blackTable[SimpleThreePieceBoardToIndex(boardCopy)];
                            }
                            else
                            {
                                // draw by insufficient material, generate a table entry for it
                                result = new TableEntry(boardCopy);
                                result.Outcome = Outcome.Draw;
                                result.DTM = 0;
                                result.DTZ = 0;
                            }

                            return new Tuple<Move, TableEntry>(move, result);
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

            /**************** every other position is a draw *******************************/
            FillInDraws();
        }

        // should be usable for any sort of search, only difference is when/which subtables are searched
        // e.g. handling captures/promotions in this case
        public void FillInDraws() {
            for (int i = 0; i < blackTable.Length; i++)
            {
                TableEntry entry = blackTable[i];
                if (entry == null || entry.Outcome != Outcome.Unknown) continue;

                var board = entry.Board;

                var allAvailableMoves = board.GetAllAvailableMoves();

                // Can be a drawn position with some moves resulting in losing, so still need to filter
                // Know position ends in a draw, so can ignore DTM/DTZ

                var choice = allAvailableMoves.Where(move =>
                {
                    if (move.CapturedPiece >= 0) return true; // guaranteed draw by insufficient material
                    var boardCopy = new Board(board);
                    boardCopy.MakeMove(move);

                    // black has no pawn to promote, so must still be KP-K game
                    NormalisePawnBoard(boardCopy); // reason a copy of board needed, can't undo
                    TableEntry result = whiteTable[ThreePiecePawnBoardToIndex(boardCopy)];
                    
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

                var allAvailableMoves = board.GetAllAvailableMoves();

                // Can be a drawn position with some moves resulting in losing, so still need to filter
                // Know position ends in a draw, so can ignore DTM/DTZ

                var choice = allAvailableMoves.Where(move =>
                {
                    // only black piece is a king, so don't need to consider captures
                    var boardCopy = new Board(board);
                    boardCopy.MakeMove(move);

                    TableEntry result;

                    // need to change board depending on which pieces now present
                    if (move.PromotionPiece == NO_PIECE)
                    {
                        // still a KP-K position
                        NormalisePawnBoard(boardCopy); // reason a copy of board needed, can't undo
                        result = blackTable[ThreePiecePawnBoardToIndex(boardCopy)];
                    }
                    else if (move.PromotionPiece % 6 == QUEEN_BOARD)
                    {
                        NormaliseBoard(boardCopy);
                        result = queenTable.blackTable[SimpleThreePieceBoardToIndex(boardCopy)];
                    }
                    else if (move.PromotionPiece % 6 == ROOK_BOARD)
                    {
                        NormaliseBoard(boardCopy);
                        result = rookTable.blackTable[SimpleThreePieceBoardToIndex(boardCopy)];
                    }
                    else
                    {
                        return true; // draw by insufficient material
                    }
                    return result.Outcome == Outcome.Draw || result.Outcome == Outcome.Unknown; // unknown = draw now
                }).First();

                entry.Outcome = Outcome.Draw;
                entry.BestMove = choice;
            }
        }

        /********** TODO: Different table size/possibility of promoting, so need to encode differently ****************/

        // storing to a file:

        // fully entry:
        // 17 bits for board (5 for wk, 6 for bk, 6 for pawn - very minimal inefficiency)
        // best move = 14 bits for white (2 bits for promotion = None/Rook/Queen), 12 bits for black (no promotion)
        // 2 bits each side for draw/win/lose
        // (additionally DTM/DTZ if I want to store the full table)
        // = 47 bits i.e. just shy of 6 bytes

        // leaving out board: fits in 32-bit integer
        // [ 2 bit promotion | 12 bit white move | 2 bit white outcome | 2 bit pad | 12 bit black move | 2 bit black outcome ]
        // index is [ white king 0-31 | black king 0-63 | pawn 0-47 but still 6 bits ] = 17 bit index (just use an int)
        public static long EncodeEntry(TableEntry whiteEntry, TableEntry blackEntry, int index)
        {
            // whiteEntry/blackEntry should be for the same position (key), one may be null
            long result = index; // BoardToIndex(whiteEntry == null ? blackEntry.Board : whiteEntry.Board);

            if (whiteEntry != null)
            {
                if (whiteEntry.BestMove != null)
                {
                    result <<= 2;
                    if (whiteEntry.BestMove.PromotionPiece >= 0)
                    {
                        if (whiteEntry.BestMove.PromotionPiece == ROOK_BOARD) result += 1;
                        else result += 2; // queen - a bishop/knight would mean a draw by insufficient material
                    }
                    result <<= 6;
                    result += SwapRow(whiteEntry.BestMove.FromIdx); 
                    result <<= 6;
                    result += SwapRow(whiteEntry.BestMove.ToIdx);
                }
                else
                {
                    result <<= 14; // may be in stale/checkmate, in which case no move possible, so set to 0
                }
                result <<= 2;
                result += whiteEntry.Outcome == Outcome.Win ? 1 : whiteEntry.Outcome == Outcome.Lose ? 2 : 0;
            }
            else
            {
                result <<= 16;
            }
            if (blackEntry != null)
            {
                if (blackEntry.BestMove != null)
                {
                    // can ignore promotion, as now pawns
                    result <<= 8;
                    result += SwapRow(blackEntry.BestMove.FromIdx);
                    result <<= 6;
                    result += SwapRow(blackEntry.BestMove.ToIdx);
                }
                else
                {
                    result <<= 14;
                }
                result <<= 2;
                result += blackEntry.Outcome == Outcome.Win ? 1 : blackEntry.Outcome == Outcome.Lose ? 2 : 0;
            }
            else
            {
                result <<= 16;
            }
            return result;
        }

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

        // Also stores the DTM/DTZ (DTZ not actually useful), for use when generating other tables
        // 14 bits move + 2 bits outcome + 8 bit DTM + 8 bits DTZ for each colour = 64 bits
        // [ promotion + move | outcome | DTM | DTZ ]
        // index is left implicit (all indices included in tablebase, so can infer from position)
        public static long FullEncodeEntry(TableEntry whiteEntry, TableEntry blackEntry)
        {
            long result = 0;

            // initially 0, so don't need to shift if nothing there already
            if (whiteEntry != null)
            {
                if (whiteEntry.BestMove != null)
                {
                    if (whiteEntry.BestMove.PromotionPiece >= 0)
                    {
                        if (whiteEntry.BestMove.PromotionPiece == ROOK_BOARD) result += 1;
                        else result += 2; // queen - a bishop/knight would mean a draw by insufficient material
                    }
                    result <<= 6;
                    result += SwapRow(whiteEntry.BestMove.FromIdx);
                    result <<= 6;
                    result += SwapRow(whiteEntry.BestMove.ToIdx);
                }
                result <<= 2;
                result += whiteEntry.Outcome == Outcome.Win ? 1 : whiteEntry.Outcome == Outcome.Lose ? 2 : 0;
                result <<= 8;
                result += whiteEntry.DTM;
                result <<= 8;
                result += whiteEntry.DTZ;
            }

            if (blackEntry != null)
            {
                if (blackEntry.BestMove != null)
                {
                    result <<= 8;
                    result += SwapRow(blackEntry.BestMove.FromIdx);
                    result <<= 6;
                    result += SwapRow(blackEntry.BestMove.ToIdx);
                }
                else
                {
                    result <<= 14;
                }
                result <<= 2;
                result += blackEntry.Outcome == Outcome.Win ? 1 : blackEntry.Outcome == Outcome.Lose ? 2 : 0;
                result <<= 8;
                result += blackEntry.DTM;
                result <<= 8;
                result += blackEntry.DTZ;
            }
            else
            {
                result <<= 32;
            }
            return result;
        }

        public void WriteFullTables(string filename)
        {
            using (FileStream fs = File.OpenWrite(filename))
            {
                for (int i = 0; i < whiteTable.Length; i++)
                {
                    //  if (whiteTable[i] != null || blackTable[i] != null)
                    //  {
                    long encoded = FullEncodeEntry(whiteTable[i], blackTable[i]);
                    byte[] bytes = BitConverter.GetBytes(encoded);
                    if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
                    fs.Write(bytes, 0, 8);
                }
            }
        }
    }
}
