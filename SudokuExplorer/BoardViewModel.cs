using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuExplorer
{
	public class BoardCell : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private BoardViewModel _board;
		private int _index;

		private int _cachedValue;
		private bool _cachedIsPreset;

		public BoardCell(BoardViewModel board, int index)
		{
			_board = board;
			_index = index;

			OnBoardChanged();
		}

		public int Value
		{
			get { return _cachedValue; }
			set { _board.Board[_index] = value; }
		}

		public bool IsPreset
		{
			get { return _cachedIsPreset; }
			private set
			{
				if (value != _cachedIsPreset)
				{
					_cachedIsPreset = value;
					NotifyPropertyChanged();
				}
			}
		}

		internal void OnBoardChanged()
		{
			int boardValue = _board.Value(_index);
			if (boardValue != _cachedValue)
			{
				_cachedValue = boardValue;
				NotifyPropertyChanged("Value");
			}

			IsPreset = _board.IsPreset(_index);
		}
	}

	public class BoardViewModel : INotifyPropertyChanged
	{
		private BoardCell[] _data = new BoardCell[9 * 9];
		private SudokuBoard _board;
		private IBoardValidator _validator;

		public event PropertyChangedEventHandler PropertyChanged;

		public BoardViewModel()
		{
			for (int index = 0; index < 81; index++)
				_data[index] = new BoardCell(this, index);
		}

		public int Value(int index)
		{
			if (_board == null)
				return 0;

			return _board[index];
		}

		public bool IsPreset(int index)
		{
			if (_board == null)
				return false;

			return _board.IsPreset(index);
		}

		public SudokuBoard Board
		{
			get { return _board; }
			set
			{
				if (_board != null)
					_board.BoardChanged -= OnBoardChanged;
				_board = value;
				if (_board != null)
					_board.BoardChanged += OnBoardChanged;
				NotifyPropertyChanged();
			}
		}

		public IBoardValidator Validator
		{
			get { return _validator; }
			set
			{
				if (_validator != null)
					_validator.PropertyChanged -= OnValidatorChanged;
				_validator = value;
				if (_validator != null)
					_validator.PropertyChanged += OnValidatorChanged;
				NotifyPropertyChanged();
			}
		}

		public Validity Validity
		{
			get { return _validator != null ? _validator.IsValid : Validity.INVALID; }
		}

		public BoardCell this[int index]
		{
			get { return _data[index]; }
		}

		private void OnBoardChanged(SudokuBoard sender)
		{
			for (int index = 0; index < 81; index++)
				_data[index].OnBoardChanged();
		}

		private void OnValidatorChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsValid")
				NotifyPropertyChanged("Validity");
		}

		private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
