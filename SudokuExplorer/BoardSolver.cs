using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
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

		private static int missingEntries(BoardLine line)
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

		private static int fromMask(int mask)
		{
			for (int i = 1; i < 10; i++)
				if (mask == (1 << i))
					return i;
			return 0;
		}

		public static IEnumerable<KeyValuePair<int, int>> eliminate(SudokuBoard board)
		{
			List<KeyValuePair<int, int>> result = new List<KeyValuePair<int, int>>();

			// Find the missing entries on each row/column/box
			int[] rowCandidates = new int[9];
			int[] colCandidates = new int[9];
			int[] boxCandidates = new int[9];
			for (int i = 0; i < 9; i++)
			{
				rowCandidates[i] = missingEntries(board.row(i));
				colCandidates[i] = missingEntries(board.col(i));
				boxCandidates[i] = missingEntries(board.box(i));
			}

			// Find the intersection of these at each cell
			for (int row = 0; row < 9; row++)
			{
				for (int col = 0; col < 9; col++)
				{
					int ordinal = BoardMath.rowColToOrdinal(row, col);
					if (board[ordinal].Value != 0)
						continue;

					int box = BoardMath.rowColToBox(row, col);
					int intersect = rowCandidates[row] & colCandidates[col] & boxCandidates[box];
					//int[] array = intersect.ToArray();

					if (intersect != 0 && (intersect & (intersect - 1)) == 0)
						result.Add(new KeyValuePair<int, int>(ordinal, fromMask(intersect)));
				}
			}

			return result;
		}

		public static IEnumerable<KeyValuePair<int, int>> soles(SudokuBoard board)
		{
			List<KeyValuePair<int, int>> result = new List<KeyValuePair<int, int>>();

			// Find the missing entries on each row/column/box
			int[] rowCandidates = new int[9];
			int[] colCandidates = new int[9];
			int[] boxCandidates = new int[9];
			for (int i = 0; i < 9; i++)
			{
				rowCandidates[i] = missingEntries(board.row(i));
				colCandidates[i] = missingEntries(board.col(i));
				boxCandidates[i] = missingEntries(board.box(i));
			}

			// Find the intersection of these at each cell
			int[] cellCandidates = new int[81];
			for (int row = 0; row < 9; row++)
			{
				for (int col = 0; col < 9; col++)
				{
					int ordinal = BoardMath.rowColToOrdinal(row, col);
					if (board[ordinal].Value != 0)
						continue;

					int box = BoardMath.rowColToBox(row, col);
					int intersect = rowCandidates[row] & colCandidates[col] & boxCandidates[box];
					cellCandidates[ordinal] = intersect;
				}
			}

			// Count number of places that each rowCandidate can go within this row
			for (int row = 0; row < 9; row++)
			{
				int thisLine = rowCandidates[row];
				int[] candidateCount = new int[9]; // Not used yet, thinking of it for future solve
				for (int candidate = 1; candidate < 10; candidate++)
				{
					if ((thisLine & (1 << candidate)) == 0)
						continue;

					int count = 0;
					int lastOrdinal = -1;
					for (int col = 0; col < 9; col++)
					{
						int ordinal = BoardMath.rowColToOrdinal(row, col);
						int candidates = cellCandidates[ordinal];
						if ((candidates & (1 << candidate)) != 0)
						{
							count++;
							lastOrdinal = ordinal;
						}
					}
					candidateCount[candidate - 1] = count;

					if (count == 1)
						result.Add(new KeyValuePair<int, int>(lastOrdinal, candidate));
				}
			}

			// Count number of places that each rowCandidate can go within this row
			for (int col = 0; col < 9; col++)
			{
				int thisLine = colCandidates[col];
				int[] candidateCount = new int[9]; // Not used yet, thinking of it for future solve
				for (int candidate = 1; candidate < 10; candidate++)
				{
					if ((thisLine & (1 << candidate)) == 0)
						continue;

					int count = 0;
					int lastOrdinal = -1;
					for (int row = 0; row < 9; row++)
					{
						int ordinal = BoardMath.rowColToOrdinal(row, col);
						int candidates = cellCandidates[ordinal];
						if ((candidates & (1 << candidate)) != 0)
						{
							count++;
							lastOrdinal = ordinal;
						}
					}
					candidateCount[candidate - 1] = count;

					if (count == 1)
						result.Add(new KeyValuePair<int, int>(lastOrdinal, candidate));
				}
			}

			return result;
		}

		public static Dictionary<int, int> soles2(SudokuBoard board)
		{
			Dictionary<int, int> result = new Dictionary<int, int>();

			// Find the missing entries on each row/column/box
			int[] rowCandidates = new int[9];
			int[] colCandidates = new int[9];
			int[] boxCandidates = new int[9];
			for (int i = 0; i < 9; i++)
			{
				rowCandidates[i] = missingEntries(board.row(i));
				colCandidates[i] = missingEntries(board.col(i));
				boxCandidates[i] = missingEntries(board.box(i));
			}

			// Find the intersection of these at each cell
			int[] cellCandidates = new int[81];
			for (int row = 0; row < 9; row++)
			{
				for (int col = 0; col < 9; col++)
				{
					int ordinal = BoardMath.rowColToOrdinal(row, col);
					if (board[ordinal].Value != 0)
						continue;

					int box = BoardMath.rowColToBox(row, col);
					int intersect = rowCandidates[row] & colCandidates[col] & boxCandidates[box];
					cellCandidates[ordinal] = intersect;
				}
			}

			// Count number of places that each rowCandidate can go within this row
			for (int row = 0; row < 9; row++)
			{
				int thisLine = rowCandidates[row];
				int[] candidateCount = new int[9]; // Not used yet, thinking of it for future solve
				for (int candidate = 1; candidate < 10; candidate++)
				{
					if ((thisLine & (1 << candidate)) == 0)
						continue;

					int count = 0;
					int lastOrdinal = -1;
					for (int col = 0; col < 9; col++)
					{
						int ordinal = BoardMath.rowColToOrdinal(row, col);
						int candidates = cellCandidates[ordinal];
						if ((candidates & (1 << candidate)) != 0)
						{
							count++;
							lastOrdinal = ordinal;
						}
					}
					candidateCount[candidate - 1] = count;

					if (count == 1)
						result[lastOrdinal] = candidate;
				}
			}

			// Count number of places that each colCandidate can go within this col
			for (int col = 0; col < 9; col++)
			{
				int thisLine = colCandidates[col];
				int[] candidateCount = new int[9]; // Not used yet, thinking of it for future solve
				for (int candidate = 1; candidate < 10; candidate++)
				{
					if ((thisLine & (1 << candidate)) == 0)
						continue;

					int count = 0;
					int lastOrdinal = -1;
					for (int row = 0; row < 9; row++)
					{
						int ordinal = BoardMath.rowColToOrdinal(row, col);
						int candidates = cellCandidates[ordinal];
						if ((candidates & (1 << candidate)) != 0)
						{
							count++;
							lastOrdinal = ordinal;
						}
					}
					candidateCount[candidate - 1] = count;

					if (count == 1)
						result[lastOrdinal] = candidate;
				}
			}

			// Count number of places that each colCandidate can go within this col
			for (int box = 0; box < 9; box++)
			{
				int thisLine = boxCandidates[box];
				int[] candidateCount = new int[9]; // Not used yet, thinking of it for future solve
				for (int candidate = 1; candidate < 10; candidate++)
				{
					if ((thisLine & (1 << candidate)) == 0)
						continue;

					int count = 0;
					int lastOrdinal = -1;
					for (int index = 0; index < 9; index++)
					{
						int ordinal = BoardMath.boxToOrdinal(box, index);
						int candidates = cellCandidates[ordinal];
						if ((candidates & (1 << candidate)) != 0)
						{
							count++;
							lastOrdinal = ordinal;
						}
					}
					candidateCount[candidate - 1] = count;

					if (count == 1)
						result[lastOrdinal] = candidate;
				}
			}

			return result;
		}

	}
}
