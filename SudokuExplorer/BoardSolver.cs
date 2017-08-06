using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuExplorer
{
	public class SolveCandidate
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

	public class ElimnationConfiguration : INotifyPropertyChanged
	{
		private bool _rows = true;
		private bool _cols = true;
		private bool _boxes = true;
		private bool _pairs = true;
		private bool _spades = true;

		public event PropertyChangedEventHandler PropertyChanged;

		public bool Rows { get { return _rows; } set { _rows = value; NotifyPropertyChanged(); } }
		public bool Cols { get { return _cols; } set { _cols = value; NotifyPropertyChanged(); } }
		public bool Boxes { get { return _boxes; } set { _boxes = value; NotifyPropertyChanged(); } }
		public bool Pairs { get { return _pairs; } set { _pairs = value; NotifyPropertyChanged(); } }
		public bool Spades { get { return _spades; } set { _spades = value; NotifyPropertyChanged(); } }

		public bool BasicEliminationsActive { get { return _rows && _cols && _boxes; } }

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public delegate void EventHandler();

	public interface IEliminationSolver
	{
		/// <summary>
		/// Returns the board that this solver is operating on
		/// </summary>
		SudokuBoard Board { get; }

		ElimnationConfiguration Configuration { get; }

		int[] CellCandidates { get; }

		List<SolveCandidate> Solutions { get; }

		event EventHandler CandidatesChanged;
	}

	/// <summary>
	/// Generates a set of candidates for each row, column and box in a Sudoku board.
	/// 
	/// On top of this, combines these into candidates for each cell based on the configuration.
	/// </summary>
	public class EliminationSolver : IEliminationSolver
	{
		private SudokuBoard _board;

		private readonly int[] _rows  = new int[9];
		private readonly int[] _cols  = new int[9];
		private readonly int[] _boxes = new int[9];

		private readonly int[] _cellCandidates = new int[81];

		private readonly ElimnationConfiguration _configuration = new ElimnationConfiguration();

		public event EventHandler CandidatesChanged;

		public EliminationSolver()
		{
			_configuration.PropertyChanged += OnConfigChanged;
		}

		public SudokuBoard Board
		{
			get => _board;
			set
			{
				if (_board != null)
					_board.BoardChanged -= OnBoardChanged;
				_board = value;
				if (_board != null)
					_board.BoardChanged += OnBoardChanged;
				OnBoardChanged(_board);
			}
		}

		public ElimnationConfiguration Configuration { get => _configuration; }

		public int[] CellCandidates { get => _cellCandidates; }
		public List<SolveCandidate> Solutions { get; private set; } // FIXME is this list really the cleanest way?

		private void OnBoardChanged(SudokuBoard sender)
		{
			// Find the missing entries on each row/column/box
			for (int i = 0; i < 9; i++)
			{
				_rows[i]  = MissingEntries(_board.Row(i));
				_cols[i]  = MissingEntries(_board.Col(i));
				_boxes[i] = MissingEntries(_board.Box(i));
			}

			RecomputeCellCandidates();
		}

		private void OnConfigChanged(object sender, PropertyChangedEventArgs args)
		{
			// We only need to rebuild the cell candidates here
			RecomputeCellCandidates();
		}

		private void RecomputeCellCandidates()
		{
			for (int i = 0; i < 81; i++)
			{
				_cellCandidates[i] = _board[i] == 0 ? (1 << 10) - 2 : 0;
				int row = BoardMath.OrdinalToRow(i);
				int col = BoardMath.OrdinalToCol(i);
				int box = BoardMath.OrdinalToBox(i);
				if (_configuration.Rows)
					_cellCandidates[i] &= _rows[row];
				if (_configuration.Cols)
					_cellCandidates[i] &= _cols[col];
				if (_configuration.Boxes)
					_cellCandidates[i] &= _boxes[box];
			}

			// Further elimination assumes that all row/col/box elimination is done
			if (_configuration.BasicEliminationsActive)
			{
				// TODO: These last two could perhaps be run in a loop around each other, until neither find any eliminations

				if (_configuration.Pairs)
				{
					EliminatePairsInLine(_cellCandidates, BoardMath.RowColToOrdinal);
					EliminatePairsInLine(_cellCandidates, BoardMath.ColRowToOrdinal);
					EliminatePairsInLine(_cellCandidates, BoardMath.BoxToOrdinal);
				}

				if (_configuration.Spades)
					EliminateSpades(_cellCandidates);
			}

			// Regenerate solutions
			Solutions = Solve(true, true);

			CandidatesChanged?.Invoke();
		}

		private List<SolveCandidate> Solve(bool eliminate, bool soles)
		{
			List<SolveCandidate> result = new List<SolveCandidate>();

			// If only one bit is set then there is a single candidate in this cell
			if (eliminate)
			{
				for (int ordinal = 0; ordinal < 81; ordinal++)
				{
					int intersect = _cellCandidates[ordinal];
					if (intersect != 0 && (intersect & (intersect - 1)) == 0)
						result.Add(new SolveCandidate(ordinal, SolverUtils.FromMask(intersect), "All other digits eliminated"));
				}
			}

			if (soles)
			{
				// Count number of places that each rowCandidate can go within this row
				SolverUtils.FindSolesInLine(_rows, BoardMath.RowColToOrdinal, _cellCandidates, result);

				// Count number of places that each colCandidate can go within this col
				SolverUtils.FindSolesInLine(_cols, BoardMath.ColRowToOrdinal, _cellCandidates, result);

				// Count number of places that each boxCandidate can go within this box
				SolverUtils.FindSolesInLine(_boxes, BoardMath.BoxToOrdinal, _cellCandidates, result);
			}

			return result;
		}

		private delegate int LineOrdinalMethod(int outer, int inner);

		private static void EliminatePairsInLine(int[] cellCandidates, LineOrdinalMethod lineOrdinal)
		{
			List<int> pairs = new List<int>(9);
			for (int outer = 0; outer < 9; outer++)
			{
				// Find all cells that contain only two candidates
				pairs.Clear();
				for (int inner = 0; inner < 9; inner++)
				{
					int ordinal = lineOrdinal(outer, inner);
					int c = cellCandidates[ordinal];
					if (c != 0)
					{
						int numBitsSet = SolverUtils.NumberOfSetBits(c);
						if (numBitsSet == 2)
							pairs.Add(c);
					}
				}
				// From these, find pairs of cells that contain the same two candidate
				pairs.Sort();
				for (int i = 0; i < pairs.Count - 1; i++)
				{
					if (pairs[i] == pairs[i + 1])
					{
						// Unset these bits from all other candidates in the row
						int pair = pairs[i];
						int unsetMask = ~pair;
						for (int inner = 0; inner < 9; inner++)
						{
							int ordinal = lineOrdinal(outer, inner);
							if (cellCandidates[ordinal] != pair)
								cellCandidates[ordinal] &= unsetMask;
						}
						i++;
					}
				}
			}
		}

		// If a digit appears only within a single row of a box, then eliminate it from the entire row
		private static void EliminateSpades(int[] cellCandidates)
		{
			// TODO: This could be extended to digits that appear only in a single box within a row

			// foreach box
			//    foreach row
			//        candidates = union (candidates in row)
			//        candidates &= ~(union of candidates from other rows)
			//        if (candidates != 0)
			//            eliminate these candidates from remainder of whole row
			bool foundSomething;
			do
			{
				foundSomething = false;
				for (int box = 0; box < 9; box++)
				{
					// First find the set of candidates available in each row and column
					int[] candidatesInRow = new int[3];
					int[] candidatesInCol = new int[3];
					for (int i = 0; i < 3; i++)
					{
						for (int j = 0; j < 3; j++)
						{
							candidatesInRow[i] |= cellCandidates[BoardMath.BoxToOrdinal(box, i, j)];
							candidatesInCol[i] |= cellCandidates[BoardMath.BoxToOrdinal(box, j, i)];
						}
					}
					// Then use these to find the candidates _only_ in each row and column
					int[] candidatesOnlyInRow = new int[3];
					int[] candidatesOnlyInCol = new int[3];
					candidatesOnlyInRow[0] =  candidatesInRow[0] & ~candidatesInRow[1] & ~candidatesInRow[2];
					candidatesOnlyInRow[1] = ~candidatesInRow[0] &  candidatesInRow[1] & ~candidatesInRow[2];
					candidatesOnlyInRow[2] = ~candidatesInRow[0] & ~candidatesInRow[1] &  candidatesInRow[2];
					candidatesOnlyInCol[0] =  candidatesInCol[0] & ~candidatesInCol[1] & ~candidatesInCol[2];
					candidatesOnlyInCol[1] = ~candidatesInCol[0] &  candidatesInCol[1] & ~candidatesInCol[2];
					candidatesOnlyInCol[2] = ~candidatesInCol[0] & ~candidatesInCol[1] &  candidatesInCol[2];
					for (int outer = 0; outer < 3; outer++)
					{
						// Yay we found one!  Eliminate this from the entire row
						int eliminationRow = candidatesOnlyInRow[outer];
						if (eliminationRow != 0)
						{
							int ordinalInBox = BoardMath.BoxToOrdinal(box, 3 * outer);
							int realRow = BoardMath.OrdinalToRow(ordinalInBox);
							for (int i = 0; i < 9; i++) // Iterate along the row
							{
								// Skip the box we found candidates in
								int innerBox = BoardMath.RowColToBox(realRow, i);
								if (box == innerBox)
									continue;

								// Eliminate the candidates, and note that we did something
								int ordinal = BoardMath.RowColToOrdinal(realRow, i);
								if ((cellCandidates[ordinal] & eliminationRow) != 0)
								{
									cellCandidates[ordinal] &= ~eliminationRow;
									foundSomething = true;
								}
							}
						}

						// Yay we found one!  Eliminate this from the entire column
						int eliminationCol = candidatesOnlyInCol[outer];
						if (eliminationCol != 0)
						{
							int ordinalInBox = BoardMath.BoxToOrdinal(box, outer);
							int realCol = BoardMath.OrdinalToCol(ordinalInBox);
							for (int i = 0; i < 9; i++) // Iterate down the column
							{
								// Skip the box we found candidates in
								int innerBox = BoardMath.RowColToBox(i, realCol);
								if (box == innerBox)
									continue;

								// Eliminate the candidates, and note that we did something
								int ordinal = BoardMath.RowColToOrdinal(i, realCol);
								if ((cellCandidates[ordinal] & eliminationCol) != 0)
								{
									cellCandidates[ordinal] &= ~eliminationCol;
									foundSomething = true;
								}
							}
						}
					}
				}
			} while (foundSomething);
		}

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
	}

	class SolverUtils
	{
		public static int FromMask(int mask)
		{
			for (int i = 1; i < 10; i++)
				if (mask == (1 << i))
					return i;
			return 0;
		}

		private static List<int> MaskHack(int mask)
		{
			List<int> result = new List<int>();
			for (int i = 1; i < 10; i++)
				if ((mask & (1 << i)) != 0)
					result.Add(i);
			return result;
		}

		// Helper method to count number of nonzero bits in an integer
		public static int NumberOfSetBits(int i)
		{
			i = i - ((i >> 1) & 0x55555555);
			i = (i & 0x33333333) + ((i >> 2) & 0x33333333);
			return (((i + (i >> 4)) & 0x0F0F0F0F) * 0x01010101) >> 24;
		}

		//public static List<SolveCandidate> Solve(SudokuBoard board, bool eliminate, bool soles)
		//{
		//	CandidateFinder candidates = Candidates(board);
		//	return Solve(candidates, eliminate, soles);
		//}


		public delegate int LineOrdinalMethod(int outer, int inner);
		public static void FindSolesInLine(int[] lineCandidates, LineOrdinalMethod lineOrdinal, int[] cellCandidates, List<SolveCandidate> result)
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
