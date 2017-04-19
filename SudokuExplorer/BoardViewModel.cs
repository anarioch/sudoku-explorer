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

		private void SetHelper(ref bool field, bool value, [CallerMemberName] string propertyName = "")
		{
			if (value != field)
			{
				field = value;
				NotifyPropertyChanged(propertyName);
			}
		}

		private BoardViewModel _board;
		private int _index;

		private int _cachedValue;
		private bool _cachedIsPreset;

		private bool _isCandidate_1;
		private bool _isCandidate_2;
		private bool _isCandidate_3;
		private bool _isCandidate_4;
		private bool _isCandidate_5;
		private bool _isCandidate_6;
		private bool _isCandidate_7;
		private bool _isCandidate_8;
		private bool _isCandidate_9;

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
			private set { SetHelper(ref _cachedIsPreset, value); }
		}

		public bool IsCandidate_1 { get { return _isCandidate_1; } set { SetHelper(ref _isCandidate_1, value); } }
		public bool IsCandidate_2 { get { return _isCandidate_2; } set { SetHelper(ref _isCandidate_2, value); } }
		public bool IsCandidate_3 { get { return _isCandidate_3; } set { SetHelper(ref _isCandidate_3, value); } }
		public bool IsCandidate_4 { get { return _isCandidate_4; } set { SetHelper(ref _isCandidate_4, value); } }
		public bool IsCandidate_5 { get { return _isCandidate_5; } set { SetHelper(ref _isCandidate_5, value); } }
		public bool IsCandidate_6 { get { return _isCandidate_6; } set { SetHelper(ref _isCandidate_6, value); } }
		public bool IsCandidate_7 { get { return _isCandidate_7; } set { SetHelper(ref _isCandidate_7, value); } }
		public bool IsCandidate_8 { get { return _isCandidate_8; } set { SetHelper(ref _isCandidate_8, value); } }
		public bool IsCandidate_9 { get { return _isCandidate_9; } set { SetHelper(ref _isCandidate_9, value); } }

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

		public void FindCandidates()
		{
			BoardCandidates candidates = EliminationSolver.Candidates(Board);
			for (int i = 0; i < 81; i++)
			{
				BoardCell cell = _data[i];
				int c = candidates.cellCandidates[i];
				cell.IsCandidate_1 = (c & (1 << 1)) != 0;
				cell.IsCandidate_2 = (c & (1 << 2)) != 0;
				cell.IsCandidate_3 = (c & (1 << 3)) != 0;
				cell.IsCandidate_4 = (c & (1 << 4)) != 0;
				cell.IsCandidate_5 = (c & (1 << 5)) != 0;
				cell.IsCandidate_6 = (c & (1 << 6)) != 0;
				cell.IsCandidate_7 = (c & (1 << 7)) != 0;
				cell.IsCandidate_8 = (c & (1 << 8)) != 0;
				cell.IsCandidate_9 = (c & (1 << 9)) != 0;
			}
		}

		public void ClearCandidates()
		{
			for (int i = 0; i < 81; i++)
			{
				BoardCell cell = _data[i];
				cell.IsCandidate_1 = false;
				cell.IsCandidate_2 = false;
				cell.IsCandidate_3 = false;
				cell.IsCandidate_4 = false;
				cell.IsCandidate_5 = false;
				cell.IsCandidate_6 = false;
				cell.IsCandidate_7 = false;
				cell.IsCandidate_8 = false;
				cell.IsCandidate_9 = false;
			}
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
