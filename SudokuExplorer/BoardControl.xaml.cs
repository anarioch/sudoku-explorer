using System;
using System.Collections.Generic;
using System.Globalization;
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
	public class SudokuIntToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int intValue = (int)value;
			return intValue > 0 && intValue < 10 ? intValue.ToString() : "";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			int result = 0;
			bool success = int.TryParse((string)value, out result);
			return result;
		}
	}

	/// <summary>
	/// Interaction logic for BoardControl.xaml
	/// </summary>
	public partial class BoardControl : UserControl
	{

		public int CellFontSize { get; set; } = 20;

		public BoardControl()
		{
			InitializeComponent();
		}

		private static int fudge(int line, int incr)
		{
			int newline = line + incr;
			if (newline == 3 || newline == 7)
				newline += incr;
			return newline < 0 ? 0 : newline > 10 ? 10 : newline;
		}

		private void onCellKeyDown(object sender, KeyEventArgs e)
		{
			if (sender is TextBox)
			{
				UIElement control = (UIElement)sender;
				int row = Grid.GetRow(control);
				int col = Grid.GetColumn(control);

				int newRow = row;
				int newCol = col;
				if (e.Key == Key.Left)
					newCol = fudge(col, -1);
				else if (e.Key == Key.Right)
					newCol = fudge(col, 1);
				else if (e.Key == Key.Up)
					newRow = fudge(row, -1);
				else if (e.Key == Key.Down)
					newRow = fudge(row, 1);

				if (newRow != row || newCol != col)
				{
					UIElement newControl = theGrid.Children.Cast<UIElement>()
							.FirstOrDefault(ee => Grid.GetRow(ee) == newRow && Grid.GetColumn(ee) == newCol);
					if (newControl != null)
						FocusManager.SetFocusedElement(this, newControl);
					e.Handled = true;

				}
			}
		}
	}
}
