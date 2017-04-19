﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuExplorer
{
	class SolveCandidate
	{
		public int Ordinal   { get; set; }
		public int Candidate { get; set; }
		public string Reason { get; set; }

		public SolveCandidate(int ordinal, int candidate, string reason)
		{
			Ordinal = ordinal;
			Candidate = candidate;
			Reason = reason;
		}
	}

	class BoardCandidates
	{
		public readonly int[] rowCandidates = new int[9];
		public readonly int[] colCandidates = new int[9];
		public readonly int[] boxCandidates = new int[9];

		public readonly int[] cellCandidates = new int[81];
	}

	class EliminationSolver
	{
		//private SudokuBoard _board;
		//public SudokuBoard Board
		//{
		//	get { return _board; }
		//	set
		//	{
		//		if (_board != null)
		//			_board.BoardChanged -= onBoardChanged;
		//		_board = value;
		//		if (_board != null)
		//			_board.BoardChanged += onBoardChanged;
		//	}
		//}

		//private void onBoardChanged(SudokuBoard sender)
		//{
		//	_isValid = BoardValidation.validate(Board);
		//	NotifyPropertyChanged("IsValid");
		//}

		private static int MissingEntries(IBoardLine line)
		{
			int result = 0;
			for (int c = 1; c < 10; c++)
			{
				bool found = false;
				for (int j = 0; j < 9 && !found; j++)
					if (line[j] == c)
						found = true;
				if (!found)
					result |= (1 << c);
			}
			return result;
		}

		private static int FromMask(int mask)
		{
			for (int i = 1; i < 10; i++)
				if (mask == (1 << i))
					return i;
			return 0;
		}

		public static BoardCandidates Candidates(SudokuBoard board)
		{
			BoardCandidates candidates = new BoardCandidates();
			// Find the missing entries on each row/column/box
			for (int i = 0; i < 9; i++)
			{
				candidates.rowCandidates[i] = MissingEntries(board.Row(i));
				candidates.colCandidates[i] = MissingEntries(board.Col(i));
				candidates.boxCandidates[i] = MissingEntries(board.Box(i));
			}

			// Find the intersection of these at each cell
			// Each intersect is a bitmask, with bits corresponding to the candidates
			for (int row = 0; row < 9; row++)
			{
				for (int col = 0; col < 9; col++)
				{
					int ordinal = BoardMath.RowColToOrdinal(row, col);
					int box = BoardMath.RowColToBox(row, col);

					// Skip this cell if it already has a number
					if (board[ordinal] != 0)
						continue;

					// Find the intersection of candidates from the row/col/box
					int intersect = candidates.rowCandidates[row] & candidates.colCandidates[col] & candidates.boxCandidates[box];
					candidates.cellCandidates[ordinal] = intersect;
				}
			}

			return candidates;
		}

		public static List<SolveCandidate> Solve(SudokuBoard board, bool eliminate, bool soles)
		{
			BoardCandidates candidates = Candidates(board);
			return Solve(candidates, eliminate, soles);
		}

		public static List<SolveCandidate> Solve(BoardCandidates candidates, bool eliminate, bool soles)
		{
			List<SolveCandidate> result = new List<SolveCandidate>();

			// If only one bit is set then there is a single candidate in this cell
			if (eliminate)
			{
				for (int ordinal = 0; ordinal < 81; ordinal++)
				{
					int intersect = candidates.cellCandidates[ordinal];
					if (intersect != 0 && (intersect & (intersect - 1)) == 0)
						result.Add(new SolveCandidate(ordinal, FromMask(intersect), "All other digits eliminated"));
				}
			}

			if (soles)
			{
				// Count number of places that each rowCandidate can go within this row
				FindSolesInLine(candidates.rowCandidates, BoardMath.RowColToOrdinal, candidates.cellCandidates, result);

				// Count number of places that each colCandidate can go within this col
				FindSolesInLine(candidates.colCandidates, BoardMath.ColRowToOrdinal, candidates.cellCandidates, result);

				// Count number of places that each boxCandidate can go within this box
				FindSolesInLine(candidates.boxCandidates, BoardMath.BoxToOrdinal, candidates.cellCandidates, result);
			}

            return result;
        }

        private delegate int LineOrdinalMethod(int outer, int inner);

        private static void FindSolesInLine(int[] lineCandidates, LineOrdinalMethod lineOrdinal, int[] cellCandidates, List<SolveCandidate> result)
        {
            for (int outer = 0; outer < 9; outer++)
            {
                int thisLine = lineCandidates[outer];
                int[] candidateCount = new int[9]; // Not used yet, thinking of it for future solve
                for (int candidate = 1; candidate < 10; candidate++)
                {
                    if ((thisLine & (1 << candidate)) == 0)
                        continue;

                    int count = 0;
                    int lastOrdinal = -1;
                    for (int inner = 0; inner < 9; inner++)
                    {
                        int ordinal = lineOrdinal(outer, inner);
                        int candidates = cellCandidates[ordinal];
                        if ((candidates & (1 << candidate)) != 0)
                        {
                            count++;
                            lastOrdinal = ordinal;
                        }
                    }
                    candidateCount[candidate - 1] = count;

                    // If there was only one, then mark the position we saw as a solution for this candidate
                    if (count == 1)
						result.Add(new SolveCandidate(lastOrdinal, candidate, "Only location for this digit"));
				}
            }
        }
    }
}
