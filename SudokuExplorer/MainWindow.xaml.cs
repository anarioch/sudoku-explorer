using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SudokuExplorer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		
		private SudokuBoard _board;
		private readonly BoardValidator _validator = new BoardValidator();
		private readonly BoardViewModel _viewModel = new BoardViewModel();

		public MainWindow()
		{
			Board = new SudokuBoard();
			InitializeComponent();
			_viewModel.Validator = _validator;
			boardControl.ViewModel = _viewModel;
			DataContext = _viewModel;
		}

#region Properties
		public SudokuBoard Board
		{
			get { return _board; }
			set
			{
				_board = value;
				_validator.Board = _board;
				_viewModel.Board = _board;
			}
		}
#endregion

#region Button Handlers
		private void FillSequentialButton_Click(object sender, RoutedEventArgs e)
		{
			BoardFactory.FillSequential(Board);
		}

		private void FillStripedButton_Click(object sender, RoutedEventArgs e)
		{
			BoardFactory.FillStriped(Board);
		}

		private int _fillSeed = 0;

		private void FillSeedEasyButton_Click(object sender, RoutedEventArgs e)
		{
			BoardFactory.FillSeed(Board, 0, _fillSeed++);
		}

		private void FillSeedMediumButton_Click(object sender, RoutedEventArgs e)
		{
			BoardFactory.FillSeed(Board, 1, _fillSeed++);
		}

		private void FillSeedHardButton_Click(object sender, RoutedEventArgs e)
		{
			BoardFactory.FillSeed(Board, 2, _fillSeed++);
		}

		private void FillSeedEvilButton_Click(object sender, RoutedEventArgs e)
		{
			BoardFactory.FillSeed(Board, 3, _fillSeed++);
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			Board.Clear();
		}

		private void EliminateButton_Click(object sender, RoutedEventArgs e)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			IEnumerable<KeyValuePair<int, int>> candidates = EliminationSolver.Solve(Board, true, false);
			stopwatch.Stop();

			// Apply the solutions (pausing UI updates until the end)
			Board.SuppressChangeEvents();
			foreach (KeyValuePair<int, int> pair in candidates)
				Board[pair.Key] = pair.Value;
			Board.ResumeChangeEvents();

			statusText.Text = String.Format("Found {0} entries in {1}ms", candidates.Count(), stopwatch.ElapsedMilliseconds);
		}

		private void SolesButton_Click(object sender, RoutedEventArgs e)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Dictionary<int, int> candidates = EliminationSolver.Solve(Board, false, true);
			stopwatch.Stop();

			// Apply the solutions (pausing UI updates until the end)
			Board.SuppressChangeEvents();
			foreach (KeyValuePair<int, int> pair in candidates)
				Board[pair.Key] = pair.Value;
			Board.ResumeChangeEvents();

			statusText.Text = String.Format("Found {0} entries in {1}ms", candidates.Count(), stopwatch.ElapsedMilliseconds);
		}

		private void SolveButton_Click(object sender, RoutedEventArgs e)
		{
			// We do not need the UI to keep updating while doing the solve
			Board.SuppressChangeEvents();

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			bool foundCandidates = false;
			int iterations = 0;
			do
			{
				// Try looking for candidates
				Dictionary<int, int> candidates = EliminationSolver.Solve(Board, true, true);
				foreach (KeyValuePair<int, int> pair in candidates)
					Board[pair.Key] = pair.Value;

				// Repeat until nothing is found
				foundCandidates = candidates.Count != 0;
				iterations++;
			} while (foundCandidates);
			stopwatch.Stop();

			Board.ResumeChangeEvents();

			statusText.Text = String.Format("Ran {0} iterations in {1}ms", iterations, stopwatch.ElapsedMilliseconds);
		}
#endregion
	}
}
