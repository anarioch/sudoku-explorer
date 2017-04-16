using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;

namespace SudokuExplorer
{
	public interface IBoardLine
	{
		int this[int index] { get;  set; }
	}
	public class BoardRow : IBoardLine
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
			get { return _board[BoardMath.RowColToOrdinal(_row, col)]; }
			set { _board[BoardMath.RowColToOrdinal(_row, col)] = value; }
		}
	}

	public class BoardCol : IBoardLine
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
			get { return _board[BoardMath.RowColToOrdinal(row, _col)]; }
			set { _board[BoardMath.RowColToOrdinal(row, _col)] = value; }
		}
	}

	public class BoardBox : IBoardLine
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
			get { return _board[BoardMath.BoxToOrdinal(_box, index)]; }
			set { _board[BoardMath.BoxToOrdinal(_box, index)] = value; }
		}
	}

	public delegate void BoardChangedEventHandler(SudokuBoard sender);

	public class SudokuBoard
	{
		private int[]  _data   = new int[9 * 9];
		private bool[] _preset = new bool[9 * 9];

		private BoardRow[] _rows;
		private BoardCol[] _cols;
		private BoardBox[] _boxes;

		public event BoardChangedEventHandler BoardChanged;

		private void NotifyBoardChanged()
		{
			BoardChanged?.Invoke(this);
		}

		public SudokuBoard()
		{
			_rows  = new BoardRow[9];
			_cols  = new BoardCol[9];
			_boxes = new BoardBox[9];
			for (int i = 0; i < 9; i++)
			{
				_rows[i]  = new BoardRow(this, i);
				_cols[i]  = new BoardCol(this, i);
				_boxes[i] = new BoardBox(this, i);
			}
		}

		public IBoardLine Row(int row)
		{
			return _rows[row];
		}

		public IBoardLine Col(int col)
		{
			return _cols[col];
		}

		public IBoardLine Box(int box)
		{
			return _boxes[box];
		}

		public int this[int index]
		{
			get { return _data[index]; }
			set
			{
				if (!_preset[index] && value != _data[index])
				{
					_data[index] = value;
					NotifyBoardChanged();
				}
			}
		}

		public bool IsPreset(int index)
		{
			return _preset[index];
		}

		public void Preset(int[] data)
		{
			Array.Copy(data, _data, 81);
			for (int index = 0; index < 81; index++)
				_preset[index] = (data[index] != 0);
			NotifyBoardChanged();
		}

		public void Clear()
		{
			for (int index = 0; index < 81; index++)
			{
				_preset[index] = false;
				_data[index] = 0;
			}
			NotifyBoardChanged();
		}
	}
}
