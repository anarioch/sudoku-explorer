using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SudokuExplorer
{
	public class BoardCellCandidate : INotifyPropertyChanged
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

		private int _label;
		private bool _isActive;
		private bool _isSolution;

		public BoardCellCandidate(int label)
		{
			_label = label;
		}

		public string Label
		{
			get { return _label.ToString(); }
		}

		public bool IsActive
		{
			get { return _isActive; }
			set { SetHelper(ref _isActive, value); }
		}

		public bool IsSolution
		{
			get { return _isSolution; }
			set { SetHelper(ref _isSolution, value); }
		}
	}

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

		private bool _areCandidatesVisible;

		private readonly BoardCellCandidate[] _candidates = new BoardCellCandidate[9];

		public BoardCell(BoardViewModel board, int index)
		{
			_board = board;
			_index = index;

			for (int i = 0; i < 9; i++)
				_candidates[i] = new BoardCellCandidate(i + 1);

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

		public bool AreCandidatesVisible
		{
			get { return _areCandidatesVisible; }
			set { SetHelper(ref _areCandidatesVisible, value); }
		}

		public BoardCellCandidate[] Candidates
		{
			get { return _candidates; }
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

		private bool _candidatesActive;

		private BoardCandidates _candidates;

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

		public bool CandidatesActive
		{
			get { return _candidatesActive; }
			set { _candidatesActive = value; NotifyPropertyChanged(); RefreshCellCandidateVisibility(); }
		}

		public void FindCandidates()
		{
			_candidates = EliminationSolver.Candidates(Board);
			RefreshCandidates(_candidates);
		}

		private void RefreshCellCandidateVisibility()
		{
			foreach (BoardCell cell in _data)
				cell.AreCandidatesVisible = _candidatesActive;
		}

		private void RefreshCandidates(BoardCandidates candidates)
		{
			for (int i = 0; i < 81; i++)
			{
				BoardCell cell = _data[i];
				int c = candidates.cellCandidates[i];
				for (int j = 1; j < 10; j++)
				{
					cell.Candidates[j - 1].IsActive = (c & (1 << j)) != 0;
					cell.Candidates[j - 1].IsSolution = false;
				}
			}

			// TODO: Perhaps this should be bound to the UI?
			// TODO: Stash the reason also to display upon focus
			var solutions = EliminationSolver.Solve(candidates, true, true);
			foreach (var solution in solutions)
			{
				_data[solution.Ordinal].Candidates[solution.Candidate - 1].IsSolution = true;
			}
		}

		public void ClearCandidates()
		{
			RefreshCandidates(EliminationSolver.NullCandidates());
		}

		public void SetEmptyCandidates()
		{
			_candidates = EliminationSolver.EmptyCandidates(Board);
			RefreshCandidates(_candidates);
		}

		public void EliminateRows()
		{
			EliminationSolver.EliminateRows(_candidates);
			RefreshCandidates(_candidates);
		}

		public void EliminateCols()
		{
			EliminationSolver.EliminateCols(_candidates);
			RefreshCandidates(_candidates);
		}

		public void EliminateBoxes()
		{
			EliminationSolver.EliminateBoxes(_candidates);
			RefreshCandidates(_candidates);
		}

		public void EliminatePairs()
		{
			EliminationSolver.EliminatePairs(_candidates);
			RefreshCandidates(_candidates);
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
