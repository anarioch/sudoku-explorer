using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApplication1
{
	public class BoardMath
	{
		public static int rowColToOrdinal(int row, int col)
		{
			return 9 * row + col;
		}

		public static void ordinalToRowCol(int ordinal, out int row, out int col)
		{
			row = ordinal / 9;
			col = ordinal % 9;
		}

		public static int rowColToBox(int row, int col)
		{
			int boxrow = row / 3;
			int boxcol = col / 3;
			return 3 * boxrow + boxcol;
		}

		public static int boxToOrdinal(int box, int index)
		{
			// Initial indices of each box
			//    0   3   6
			//    27  30  33
			//    54  57  60
			int boxStart = (box % 3) * 3 + (box / 3) * 3 * 9;
			// Board-space indices of the squares within the 0th box
			// 0   1   2
			// 9   10  11
			// 18  19  20

			int boxOffset = (index / 3) * 9 + (index % 3);

			return boxStart + boxOffset;
		}
	}

	public interface BoardLine
	{
		int this[int index] { get;  set; }
	}
	public class BoardRow : BoardLine
	{
		private readonly SudokuBoard _board;
		private readonly int _row;
		internal BoardRow(SudokuBoard board, int row)
		{
			_board = board;
			_row = row;
		}
		public int this[int col]
		{
			get { return _board.getCell(_row, col); }
			set { _board.setCell(_row, col, value); }
		}
	}

	public class BoardCol : BoardLine
	{
		private readonly SudokuBoard _board;
		private readonly int _col;
		internal BoardCol(SudokuBoard board, int col)
		{
			_board = board;
			_col = col;
		}
		public int this[int row]
		{
			get { return _board.getCell(row, _col); }
			set { _board.setCell(row, _col, value); }
		}
	}

	public class BoardBox : BoardLine
	{
		private readonly SudokuBoard _board;
		private readonly int _box;
		internal BoardBox(SudokuBoard board, int box)
		{
			_board = board;
			_box = box;
		}
		public int this[int index]
		{
			get { return _board[BoardMath.boxToOrdinal(_box, index)].Value; }
			set { _board[BoardMath.boxToOrdinal(_box, index)].Value = value; }
		}
	}

	public class BoardCell : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		// This method is called by the Set accessor of each property.
		// The CallerMemberName attribute that is applied to the optional propertyName
		// parameter causes the property name of the caller to be substituted as an argument.
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private int _value = 0;
		private bool _preset = false;

		public int Value
		{
			get { return _value; }
			set
			{
				if (!_preset && value != _value)
				{
					_value = value;
					NotifyPropertyChanged();

				}
			}
		}

		public bool IsPreset
		{
			get { return _preset; }
			set
			{
				if (value != _preset)
				{
					_preset = value;
					NotifyPropertyChanged();
				}
			}
		}
	}

	public delegate void BoardChangedEventHandler(SudokuBoard sender);

	public class SudokuBoard : INotifyPropertyChanged
	{
		private BoardCell[] _data = new BoardCell[9 * 9];

		private BoardRow[] _rows;
		private BoardCol[] _cols;

		public event BoardChangedEventHandler BoardChanged;

		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyBoardChanged()
		{
			if (BoardChanged != null)
				BoardChanged(this);
		}
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		private void OnCellChanged(object sender, PropertyChangedEventArgs args)
		{
			NotifyBoardChanged();
		}

		public SudokuBoard()
		{
			for (int i = 0; i < 81; i++)
			{
				_data[i] = new BoardCell();
				_data[i].PropertyChanged += OnCellChanged;
			}

			_rows = new BoardRow[9];
			_cols = new BoardCol[9];
			for (int i = 0; i < 9; i++)
			{
				_rows[i] = new BoardRow(this, i);
				_cols[i] = new BoardCol(this, i);
			}
		}

		public BoardLine[] Rows
		{
			get { return _rows; }
		}

		public BoardLine[] Cols
		{
			get { return _cols; }
		}

		public BoardLine row(int row)
		{
			return _rows[row];
		}

		public BoardLine col(int col)
		{
			return _cols[col];
		}

		public BoardLine box(int box)
		{
			return new BoardBox(this, box);
		}

		public int getCell(int row, int col)
		{
			return this[9 * row + col].Value;
		}

		public void setCell(int row, int col, int value)
		{
			this[9 * row + col].Value = value;
		}

		public BoardCell this[int index]
		{
			get { return _data[index]; }
		}

		public void preset(int[] data)
		{
			for (int index = 0; index < 81; index++)
			{
				BoardCell cell = this[index];
				cell.IsPreset = false;
				cell.Value = data[index];
				cell.IsPreset = (data[index] != 0);
			}
		}

		public void clear()
		{
			for (int index = 0; index < 81; index++)
			{
				BoardCell cell = this[index];
				cell.IsPreset = false;
				cell.Value = 0;
			}
		}
	}

	public class BoardFactory
	{
		public static void fillSequential(SudokuBoard board)
		{
			board.clear();
			for (int row = 0; row < 9; row++)
				for (int col = 0; col < 9; col++)
					board.setCell(row, col, ((col + row) % 9) + 1);
		}

		public static void fillStriped(SudokuBoard board)
		{
			board.clear();
			for (int row = 0; row < 9; row++)
				for (int col = 0; col < 9; col++)
				{
					int rowOffset = 3 * (row % 3) + row / 3;
					board.setCell(row, col, ((col + rowOffset) % 9) + 1);
				}
		}

		public static void fillSeed(SudokuBoard board, int seed)
		{
			if (seed == 0)
			{
				// http://www.websudoku.com/?level=1&set_id=301385430
				int[] data =
				{
					0, 0, 0,  0, 5, 4,  0, 7, 3,
					0, 0, 0,  0, 0, 2,  0, 0, 9,
					0, 0, 0,  1, 8, 0,  5, 0, 0,

					1, 0, 7,  0, 9, 0,  4, 0, 6,
					8, 4, 0,  0, 6, 0,  0, 1, 7,
					5, 0, 6,  0, 1, 0,  3, 0, 8,

					0, 0, 2,  0, 7, 9,  0, 0, 0,
					7, 0, 0,  6, 0, 0,  0, 0, 0,
					9, 5, 0,  3, 2, 0,  0, 0, 0,
				};

				board.preset(data);
			}
			else if (seed == 1)
			{
				// http://www.websudoku.com/?level=1&set_id=1206243121
				int[] data =
				{
					0, 0, 0,  0, 0, 0,  0, 0, 0,
					9, 0, 0,  0, 0, 4,  0, 2, 8,
					5, 6, 8,  0, 0, 2,  4, 3, 1,

					6, 5, 0,  0, 0, 7,  8, 0, 2,
					0, 7, 0,  8, 0, 1,  0, 4, 0,
					4, 0, 1,  3, 0, 0,  0, 5, 7,

					1, 2, 5,  7, 0, 0,  3, 9, 4,
					7, 9, 0,  2, 0, 0,  0, 0, 5,
					0, 0, 0,  0, 0, 0,  0, 0, 0,
				};

				board.preset(data);
			}
			else if (seed == 1000)
			{
				// http://www.websudoku.com/?level=2&set_id=9247042447
				int[] data =
				{
					0, 0, 0,  0, 7, 5,  0, 0, 4,
					0, 9, 0,  8, 0, 0,  0, 7, 0,
					4, 0, 1,  6, 0, 0,  5, 0, 0,

					0, 0, 9,  5, 0, 0,  0, 0, 6,
					5, 0, 0,  0, 3, 0,  0, 0, 1,
					1, 0, 0,  0, 0, 7,  9, 0, 0,

					0, 0, 6,  0, 0, 8,  4, 0, 9,
					0, 4, 0,  0, 0, 2,  0, 1, 0,
					3, 0, 0,  4, 1, 0,  0, 0, 0,
				};

				board.preset(data);
			}
			else if (seed == 1001)
			{
				// http://www.websudoku.com/?level=2&set_id=6580048890
				int[] data =
				{
					6, 0, 0,  0, 0, 3,  0, 0, 0,
					3, 9, 0,  0, 5, 0,  0, 4, 0,
					0, 8, 5,  0, 7, 0,  0, 0, 0,

					0, 0, 0,  9, 0, 0,  1, 3, 5,
					0, 0, 0,  6, 2, 5,  0, 0, 0,
					5, 4, 7,  0, 0, 1,  0, 0, 0,

					0, 0, 0,  0, 6, 0,  5, 1, 0,
					0, 3, 0,  0, 8, 0,  0, 7, 9,
					0, 0, 0,  3, 0, 0,  0, 0, 4,
				};

				board.preset(data);
			}
		}
	}
}
