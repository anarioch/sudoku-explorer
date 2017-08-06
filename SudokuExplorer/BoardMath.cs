namespace SudokuExplorer
{
	public class BoardMath
	{
		public static int RowColToOrdinal(int row, int col)
		{
			return 9 * row + col;
		}

		public static int ColRowToOrdinal(int col, int row)
		{
			return 9 * row + col;
		}

		public static void OrdinalToRowCol(int ordinal, out int row, out int col)
		{
			row = ordinal / 9;
			col = ordinal % 9;
		}

		public static int OrdinalToRow(int ordinal)
		{
			return ordinal / 9;
		}

		public static int OrdinalToCol(int ordinal)
		{
			return ordinal % 9;
		}

		public static int OrdinalToBox(int ordinal)
		{
			int row = ordinal / 9;
			int col = ordinal % 9;
			return RowColToBox(row, col);
		}

		public static int RowColToBox(int row, int col)
		{
			int boxrow = row / 3;
			int boxcol = col / 3;
			return 3 * boxrow + boxcol;
		}

		public static int BoxToOrdinal(int box, int index)
		{
			return BoxToOrdinal(box, index / 3, index % 3);
		}

		public static int BoxToOrdinal(int box, int rowInBox, int colInBox)
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

			int boxOffset = rowInBox * 9 + colInBox;

			return boxStart + boxOffset;
		}

		public static string CandidateMaskToString(int mask)
		{
			string result = "";
			for (int i = 1; i < 10; i++)
			{
				if ((mask & (1 << i)) != 0)
					result += "|" + i;
			}

			if (result.Length == 0)
				return "<None>";

			return result.Substring(1);
		}
	}
}
