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

		public void preset(int index, int value)
		{
			BoardCell cell = this[index];
			cell.Value = value;
			cell.IsPreset = true;
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

		public static void fillSeed(SudokuBoard board)
		{
			board.clear();

			// http://www.websudoku.com/?level=1&set_id=301385430
			board.preset(4, 5);
			board.preset(5, 4);
			board.preset(7, 7);
			board.preset(8, 3);

			board.preset(14, 2);
			board.preset(17, 9);

			board.preset(21, 1);
			board.preset(22, 8);
			board.preset(24, 5);

			board.preset(27, 1);
			board.preset(29, 7);
			board.preset(31, 9);
			board.preset(33, 4);
			board.preset(35, 6);

			board.preset(36, 8);
			board.preset(37, 4);
			board.preset(40, 6);
			board.preset(43, 1);
			board.preset(44, 7);

			board.preset(45, 5);
			board.preset(47, 6);
			board.preset(49, 1);
			board.preset(51, 3);
			board.preset(53, 8);

			board.preset(56, 2);
			board.preset(58, 7);
			board.preset(59, 9);

			board.preset(63, 7);
			board.preset(66, 6);

			board.preset(72, 9);
			board.preset(73, 5);
			board.preset(75, 3);
			board.preset(76, 2);

			//NotifyPropertyChanged("Item[]");
		}

		public static void fillSeed2(SudokuBoard board)
		{
			board.clear();

			// http://www.websudoku.com/?level=1&set_id=1206243121
			board.preset(9,  9);
			board.preset(14, 4);
			board.preset(16, 2);
			board.preset(17, 8);

			board.preset(18, 5);
			board.preset(19, 6);
			board.preset(20, 8);
			board.preset(23, 2);
			board.preset(24, 4);
			board.preset(25, 3);
			board.preset(26, 1);

			board.preset(27, 6);
			board.preset(28, 5);
			board.preset(32, 7);
			board.preset(33, 8);
			board.preset(35, 2);

			board.preset(37, 7);
			board.preset(39, 8);
			board.preset(41, 1);
			board.preset(43, 4);

			board.preset(45, 4);
			board.preset(47, 1);
			board.preset(48, 3);
			board.preset(52, 5);
			board.preset(53, 7);

			board.preset(54, 1);
			board.preset(55, 2);
			board.preset(56, 5);
			board.preset(57, 7);
			board.preset(60, 3);
			board.preset(61, 9);
			board.preset(62, 4);

			board.preset(63, 7);
			board.preset(64, 9);
			board.preset(66, 2);
			board.preset(71, 5);

			//NotifyPropertyChanged("Item[]");
		}
	}
}
