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
		
		public MainWindow()
		{
			Board = new SudokuBoard();
			InitializeComponent();
			DataContext = this;
		}

		private SudokuBoard _board;
		public SudokuBoard Board
		{
			get { return _board; }
			set
			{
				_board = value;
				if (_board != null)
				{
					_validator.Board = _board;
				}
			}
		}

		private BoardValidator _validator = new BoardValidator();
		public BoardValidator Validator { get { return _validator; } }

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
			IEnumerable<KeyValuePair<int, int>> candidates = EliminationSolver.Eliminate(Board);
			stopwatch.Stop();
			foreach (KeyValuePair<int, int> pair in candidates)
				Board[pair.Key].Value = pair.Value;

			statusText.Text = String.Format("Found {0} entries in {1}ms", candidates.Count(), stopwatch.ElapsedMilliseconds);
		}

		private void SolesButton_Click(object sender, RoutedEventArgs e)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Dictionary<int, int> candidates = EliminationSolver.Soles(Board);
			stopwatch.Stop();
			foreach (KeyValuePair<int, int> pair in candidates)
				Board[pair.Key].Value = pair.Value;

			statusText.Text = String.Format("Found {0} entries in {1}ms", candidates.Count(), stopwatch.ElapsedMilliseconds);
		}

		private void SolveButton_Click(object sender, RoutedEventArgs e)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			bool foundCandidates = false;
			int iterations = 0;
			do
			{
				Dictionary<int, int> candidates = EliminationSolver.Soles(Board);
				foreach (KeyValuePair<int, int> pair in candidates)
					Board[pair.Key].Value = pair.Value;
				bool foundSolesCandidates = candidates.Count != 0;

				candidates = EliminationSolver.Eliminate(Board);
				foreach (KeyValuePair<int, int> pair in candidates)
					Board[pair.Key].Value = pair.Value;
				bool foundEliminationCandidates = candidates.Count != 0;

				foundCandidates = foundSolesCandidates || foundEliminationCandidates;
				iterations++;
			} while (foundCandidates);
			stopwatch.Stop();

			statusText.Text = String.Format("Ran {0} iterations in {1}ms", iterations, stopwatch.ElapsedMilliseconds);
		}
	}
}
