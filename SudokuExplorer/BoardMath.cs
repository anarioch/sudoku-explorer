namespace SudokuExplorer
{
	public class BoardMath
	{
		public static int RowColToOrdinal(int row, int col)
		{
			return 9 * row + col;
		}

		public static void OrdinalToRowCol(int ordinal, out int row, out int col)
		{
			row = ordinal / 9;
			col = ordinal % 9;
		}

		public static int RowColToBox(int row, int col)
		{
			int boxrow = row / 3;
			int boxcol = col / 3;
			return 3 * boxrow + boxcol;
		}

		public static int BoxToOrdinal(int box, int index)
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
}
