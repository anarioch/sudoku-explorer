using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuExplorer
{
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

		public static Dictionary<int, int> Eliminate(SudokuBoard board)
		{
			Dictionary<int, int> result = new Dictionary<int, int>();

			// Find the missing entries on each row/column/box
			int[] rowCandidates = new int[9];
			int[] colCandidates = new int[9];
			int[] boxCandidates = new int[9];
			for (int i = 0; i < 9; i++)
			{
				rowCandidates[i] = MissingEntries(board.Row(i));
				colCandidates[i] = MissingEntries(board.Col(i));
				boxCandidates[i] = MissingEntries(board.Box(i));
			}

			// Find the intersection of these at each cell
			for (int row = 0; row < 9; row++)
			{
				for (int col = 0; col < 9; col++)
				{
					int ordinal = BoardMath.RowColToOrdinal(row, col);
					if (board[ordinal].Value != 0)
						continue;

					int box = BoardMath.RowColToBox(row, col);
					int intersect = rowCandidates[row] & colCandidates[col] & boxCandidates[box];

					if (intersect != 0 && (intersect & (intersect - 1)) == 0)
						result[ordinal] = FromMask(intersect);
				}
			}

			return result;
		}

		public static Dictionary<int, int> Soles(SudokuBoard board)
		{
			Dictionary<int, int> result = new Dictionary<int, int>();

            // Find the missing entries on each row/column/box
            int[] rowCandidates = new int[9];
            int[] colCandidates = new int[9];
            int[] boxCandidates = new int[9];
            for (int i = 0; i < 9; i++)
            {
                rowCandidates[i] = MissingEntries(board.Row(i));
                colCandidates[i] = MissingEntries(board.Col(i));
                boxCandidates[i] = MissingEntries(board.Box(i));
            }

            // Find the intersection of these at each cell
            int[] cellCandidates = new int[81];
            for (int row = 0; row < 9; row++)
            {
                for (int col = 0; col < 9; col++)
                {
                    int ordinal = BoardMath.RowColToOrdinal(row, col);
                    if (board[ordinal].Value != 0)
                        continue;

                    int box = BoardMath.RowColToBox(row, col);
                    int intersect = rowCandidates[row] & colCandidates[col] & boxCandidates[box];
                    cellCandidates[ordinal] = intersect;
                }
            }

            // Count number of places that each rowCandidate can go within this row
            FindSolesInLine(rowCandidates, BoardMath.RowColToOrdinal, cellCandidates, result);

            // Count number of places that each colCandidate can go within this col
            FindSolesInLine(colCandidates, (col, row) => BoardMath.RowColToOrdinal(row, col), cellCandidates, result);

            // Count number of places that each boxCandidate can go within this box
            FindSolesInLine(boxCandidates, BoardMath.BoxToOrdinal, cellCandidates, result);

            return result;
        }

        private delegate int LineOrdinalMethod(int outer, int inner);

        private static void FindSolesInLine(int[] lineCandidates, LineOrdinalMethod lineOrdinal, int[] cellCandidates, Dictionary<int, int> result)
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
                        result[lastOrdinal] = candidate;
                }
            }
        }
    }
}
