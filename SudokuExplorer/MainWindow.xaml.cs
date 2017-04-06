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

namespace WpfApplication1
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

		private void fillSequentialButton_Click(object sender, RoutedEventArgs e)
		{
			BoardFactory.fillSequential(Board);
		}

		private void fillStripedButton_Click(object sender, RoutedEventArgs e)
		{
			BoardFactory.fillStriped(Board);
		}

		private void fillSeedEasyButton_Click(object sender, RoutedEventArgs e)
		{
			int index = new Random().Next() % 2;
			BoardFactory.fillSeed(Board, index);
		}

		private void fillSeedMediumButton_Click(object sender, RoutedEventArgs e)
		{
			int index = new Random().Next() % 2;
			BoardFactory.fillSeed(Board, 1000 + index);
		}

		private void clearButton_Click(object sender, RoutedEventArgs e)
		{
			Board.clear();
		}

		private void eliminateButton_Click(object sender, RoutedEventArgs e)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			IEnumerable<KeyValuePair<int, int>> candidates = EliminationSolver.eliminate(Board);
			stopwatch.Stop();
			foreach (KeyValuePair<int, int> pair in candidates)
				Board[pair.Key].Value = pair.Value;

			statusText.Text = String.Format("Found {0} entries in {1}ms", candidates.Count(), stopwatch.ElapsedMilliseconds);
		}

		private void solesButton_Click(object sender, RoutedEventArgs e)
		{
			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			Dictionary<int, int> candidates = EliminationSolver.soles(Board);
			stopwatch.Stop();
			foreach (KeyValuePair<int, int> pair in candidates)
				Board[pair.Key].Value = pair.Value;

			statusText.Text = String.Format("Found {0} entries in {1}ms", candidates.Count(), stopwatch.ElapsedMilliseconds);
		}
	}
}
