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

		private void preset(int index, int value)
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
				cell.Value = 0;
				cell.IsPreset = false;
			}
		}

		public void fillSequential()
		{
			clear();
			for (int row = 0; row < 9; row++)
				for (int col = 0; col < 9; col++)
					setCell(row, col, ((col + row) % 9) + 1);
		}

		public void fillStriped()
		{
			clear();
			for (int row = 0; row < 9; row++)
				for (int col = 0; col < 9; col++)
				{
					int rowOffset = 3 * (row % 3) + row / 3;
					setCell(row, col, ((col + rowOffset) % 9) + 1);
				}
		}

		public void fillSeed()
		{
			clear();

			// http://www.websudoku.com/?level=1&set_id=301385430
			preset(4, 5);
			preset(5, 4);
			preset(7, 7);
			preset(8, 3);

			preset(14, 2);
			preset(17, 9);

			preset(21, 1);
			preset(22, 8);
			preset(24, 5);

			preset(27, 1);
			preset(29, 7);
			preset(31, 9);
			preset(33, 4);
			preset(35, 6);

			preset(36, 8);
			preset(37, 4);
			preset(40, 6);
			preset(43, 1);
			preset(44, 7);

			preset(45, 5);
			preset(47, 6);
			preset(49, 1);
			preset(51, 3);
			preset(53, 8);

			preset(56, 2);
			preset(58, 7);
			preset(59, 9);

			preset(63, 7);
			preset(66, 6);

			preset(72, 9);
			preset(73, 5);
			preset(75, 3);
			preset(76, 2);

			//NotifyPropertyChanged("Item[]");
		}
	}
	/*
	public class SudokuBoardOld : INotifyPropertyChanged
	{
		private int[] _data = new int[9 * 9];
		private bool[] _preset = new bool[9* 9];

		private BoardCell[] _cells = new BoardCell[9 * 9];

		private BoardRow[] _rows;
		private BoardCol[] _cols;

		public event PropertyChangedEventHandler PropertyChanged;

		// This method is called by the Set accessor of each property.
		// The CallerMemberName attribute that is applied to the optional propertyName
		// parameter causes the property name of the caller to be substituted as an argument.
		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}

		public SudokuBoard()
		{
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
			return new BoardRow(this, row);
		}

		public BoardLine col(int col)
		{
			return new BoardCol(this, col);
		}

		public BoardLine box(int box)
		{
			return new BoardBox(this, box);
		}

		public int getCell(int row, int col)
		{
			return this[9 * row + col];
		}

		public void setCell(int row, int col, int value)
		{
			this[9 * row + col] = value;
		}

		public static int boxIndexToBoard(int box, int index)
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

		public int this[int index]
		{
			get { return _data[index]; }
			set
			{
				if (!_preset[index])
				{
					_data[index] = value; NotifyPropertyChanged("Item[]");
				}
			}
		}

		private void preset(int index, int value)
		{
			_data[index] = value;
			_preset[index] = true;
		}

		public void clear()
		{
			for (int index = 0; index < 81; index++)
			{
				_data[index] = 0;
				_preset[index] = false;
			}
			NotifyPropertyChanged("Item[]");
		}

		public void fillSequential()
		{
			for (int row = 0; row < 9; row++)
				for (int col = 0; col < 9; col++)
					setCell(row, col, ((col + row) % 9) + 1);
		}

		public void fillStriped()
		{
			for (int row = 0; row < 9; row++)
				for (int col = 0; col < 9; col++)
				{
					int rowOffset = 3 * (row % 3) + row / 3;
					setCell(row, col, ((col + rowOffset) % 9) + 1);
				}
		}

		public void fillSeed()
		{
			// http://www.websudoku.com/?level=1&set_id=301385430
			clear();
			preset(4, 5);
			preset(5, 4);
			preset(7, 7);
			preset(8, 3);

			preset(14, 2);
			preset(17, 9);

			preset(21, 1);
			preset(22, 8);
			preset(24, 5);

			preset(27, 1);
			preset(29, 7);
			preset(31, 9);
			preset(33, 4);
			preset(35, 6);

			preset(36, 8);
			preset(37, 4);
			preset(40, 6);
			preset(43, 1);
			preset(44, 7);

			preset(45, 5);
			preset(47, 6);
			preset(49, 1);
			preset(51, 3);
			preset(53, 8);

			preset(56, 2);
			preset(58, 7);
			preset(59, 9);

			preset(63, 7);
			preset(66, 6);

			preset(72, 9);
			preset(73, 5);
			preset(75, 3);
			preset(76, 2);

			NotifyPropertyChanged("Item[]");
		}
	}*/
}
