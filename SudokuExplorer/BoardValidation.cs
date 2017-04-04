using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace WpfApplication1
{
	public enum Validity
	{
		COMPLETE,
		CONSISTENT,
		INCONSISTENT,
		INVALID
	}

	public class BoardValidation
	{
		private static Validity validateLine(BoardLine line)
		{
			Validity result = Validity.COMPLETE;
			for (int i = 0; i < 9; i++)
			{
				var val = line[i];
				if (val < 0 || val > 9)
					return Validity.INVALID;
				if (val == 0 && result != Validity.INCONSISTENT)
				{
					result = Validity.CONSISTENT;
					continue;
				}
				for (int j = i + 1; j < 9; j++)
				{
					int jval = line[j];
					if (jval == val)
						result = Validity.INCONSISTENT;
				}
			}
			return result;
		}

		public static Validity validate(SudokuBoard board)
		{
			Validity result = Validity.COMPLETE;
			for (int i = 0; i < 9; i++)
			{
				// Check the ith row is consistent
				Validity rowResult = validateLine(board.row(i));

				// Check the ith column
				Validity colResult = validateLine(board.col(i));

				// Check the ith box
				Validity boxResult = validateLine(board.box(i));

				// Bail out if any INVALID was found
				if (rowResult == Validity.INVALID || colResult == Validity.INVALID || boxResult == Validity.INVALID)
					return Validity.INVALID;

				// Drop back from COMPLETE to CONSISTENT if any such results were found
				if (result == Validity.COMPLETE && (rowResult == Validity.CONSISTENT || colResult == Validity.CONSISTENT || boxResult == Validity.CONSISTENT))
					result = Validity.CONSISTENT;
				// Drop back to INCONSISTENT if any such results were found
				if (rowResult == Validity.INCONSISTENT || colResult == Validity.INCONSISTENT || boxResult == Validity.INCONSISTENT)
					result = Validity.INCONSISTENT;
			}

			return result;
		}
	}

	public class BoardValidator : INotifyPropertyChanged
	{
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

		private SudokuBoard _board;
		public SudokuBoard Board
		{
			get { return _board; }
			set
			{
				if (_board != null)
					_board.BoardChanged -= onBoardChanged;
				_board = value;
				if (_board != null)
					_board.BoardChanged += onBoardChanged;
			}
		}

		private Validity _isValid = Validity.INVALID;
		public Validity IsValid
		{
			get { return _isValid; }
		}

		private void onBoardChanged(SudokuBoard sender)
		{
			Validity isValid = BoardValidation.validate(Board);
			if (isValid != _isValid)
			{
				_isValid = isValid;
				NotifyPropertyChanged("IsValid");
			}
		}
	}
}
