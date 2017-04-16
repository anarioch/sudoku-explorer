namespace SudokuExplorer
{
	public class BoardFactory
	{
		public static void FillSequential(SudokuBoard board)
		{
			board.Clear();
			for (int row = 0; row < 9; row++)
				for (int col = 0; col < 9; col++)
					board[BoardMath.RowColToOrdinal(row, col)] = ((col + row) % 9) + 1;
		}

		public static void FillStriped(SudokuBoard board)
		{
			board.Clear();
			for (int row = 0; row < 9; row++)
			{
				int rowOffset = 3 * (row % 3) + row / 3;
				for (int col = 0; col < 9; col++)
					board[BoardMath.RowColToOrdinal(row, col)] = ((col + rowOffset) % 9) + 1;
			}
		}

		public static void FillSeed(SudokuBoard board, int difficulty, int seed)
		{
			if (difficulty == 0)
			{
				// Easy
				seed = seed % 2;
				if (seed == 0)
				{
					// http://www.websudoku.com/?level=1&set_id=301385430
					int[] data =
					{
						0, 0, 0,  0, 5, 4,  0, 7, 3,
						0, 0, 0,  0, 0, 2,  0, 0, 9,
						0, 0, 0,  1, 8, 0,  5, 0, 0,

						1, 0, 7,  0, 9, 0,  4, 0, 6,
						8, 4, 0,  0, 6, 0,  0, 1, 7,
						5, 0, 6,  0, 1, 0,  3, 0, 8,

						0, 0, 2,  0, 7, 9,  0, 0, 0,
						7, 0, 0,  6, 0, 0,  0, 0, 0,
						9, 5, 0,  3, 2, 0,  0, 0, 0,
					};

					board.Preset(data);
				}
				else if (seed == 1)
				{
					// http://www.websudoku.com/?level=1&set_id=1206243121
					int[] data =
					{
						0, 0, 0,  0, 0, 0,  0, 0, 0,
						9, 0, 0,  0, 0, 4,  0, 2, 8,
						5, 6, 8,  0, 0, 2,  4, 3, 1,

						6, 5, 0,  0, 0, 7,  8, 0, 2,
						0, 7, 0,  8, 0, 1,  0, 4, 0,
						4, 0, 1,  3, 0, 0,  0, 5, 7,

						1, 2, 5,  7, 0, 0,  3, 9, 4,
						7, 9, 0,  2, 0, 0,  0, 0, 5,
						0, 0, 0,  0, 0, 0,  0, 0, 0,
					};

					board.Preset(data);
				}
			}
			else if (difficulty == 1)
			{
				// Medium
				seed = seed % 2;
				if (seed == 0)
				{
					// http://www.websudoku.com/?level=2&set_id=9247042447
					int[] data =
					{
						0, 0, 0,  0, 7, 5,  0, 0, 4,
						0, 9, 0,  8, 0, 0,  0, 7, 0,
						4, 0, 1,  6, 0, 0,  5, 0, 0,

						0, 0, 9,  5, 0, 0,  0, 0, 6,
						5, 0, 0,  0, 3, 0,  0, 0, 1,
						1, 0, 0,  0, 0, 7,  9, 0, 0,

						0, 0, 6,  0, 0, 8,  4, 0, 9,
						0, 4, 0,  0, 0, 2,  0, 1, 0,
						3, 0, 0,  4, 1, 0,  0, 0, 0,
					};

					board.Preset(data);
				}
				else if (seed == 1)
				{
					// http://www.websudoku.com/?level=2&set_id=6580048890
					int[] data =
					{
						6, 0, 0,  0, 0, 3,  0, 0, 0,
						3, 9, 0,  0, 5, 0,  0, 4, 0,
						0, 8, 5,  0, 7, 0,  0, 0, 0,

						0, 0, 0,  9, 0, 0,  1, 3, 5,
						0, 0, 0,  6, 2, 5,  0, 0, 0,
						5, 4, 7,  0, 0, 1,  0, 0, 0,

						0, 0, 0,  0, 6, 0,  5, 1, 0,
						0, 3, 0,  0, 8, 0,  0, 7, 9,
						0, 0, 0,  3, 0, 0,  0, 0, 4,
					};

					board.Preset(data);
				}
			}
			else if (difficulty == 2)
			{
				// Hard
				seed = seed % 2;
				if (seed == 0)
				{
					// http://www.websudoku.com/?level=3&set_id=4153100657
					int[] data =
					{
						0, 0, 0,  4, 0, 0,  9, 0, 0,
						0, 0, 0,  2, 5, 0,  1, 8, 7,
						0, 9, 0,  0, 3, 1,  0, 0, 0,

						1, 0, 0,  0, 0, 0,  7, 9, 0,
						5, 0, 0,  0, 0, 0,  0, 0, 4,
						0, 3, 6,  0, 0, 0,  0, 0, 1,

						0, 0, 0,  8, 7, 0,  0, 1, 0,
						7, 6, 1,  0, 4, 2,  0, 0, 0,
						0, 0, 2,  0, 0, 3,  0, 0, 0,
					};

					board.Preset(data);
				}
				else if (seed == 1)
				{
					// http://www.websudoku.com/?level=3&set_id=8648415926
					int[] data =
					{
						5, 2, 1,  9, 0, 0,  3, 8, 0,
						0, 0, 0,  0, 0, 8,  7, 0, 0,
						0, 0, 0,  0, 1, 0,  0, 6, 0,

						0, 0, 3,  0, 0, 0,  8, 0, 7,
						0, 0, 0,  0, 3, 0,  0, 0, 0,
						2, 0, 9,  0, 0, 0,  5, 0, 0,

						0, 9, 0,  0, 7, 0,  0, 0, 0,
						0, 0, 5,  6, 0, 0,  0, 0, 0,
						0, 4, 7,  0, 0, 3,  6, 2, 1,
					};

					board.Preset(data);
				}
			}
			else if (difficulty == 3)
			{
				// Evil
				seed = seed % 2;
				if (seed == 0)
				{
					// http://www.websudoku.com/?level=4&set_id=7135271335
					int[] data =
					{
						0, 8, 0,  0, 7, 3,  0, 0, 0,
						4, 0, 0,  5, 0, 0,  0, 0, 0,
						3, 0, 7,  0, 0, 2,  0, 0, 0,

						6, 0, 0,  0, 0, 8,  0, 3, 4,
						0, 5, 0,  0, 0, 0,  0, 9, 0,
						7, 1, 0,  2, 0, 0,  0, 0, 6,

						0, 0, 0,  6, 0, 0,  4, 0, 2,
						0, 0, 0,  0, 0, 7,  0, 0, 8,
						0, 0, 0,  1, 2, 0,  0, 6, 0,
					};

					board.Preset(data);
				}
				else if (seed == 1)
				{
					// http://www.websudoku.com/?level=4&set_id=2323427128
					int[] data =
					{
						0, 4, 2,  0, 0, 0,  0, 0, 0,
						5, 3, 0,  0, 0, 6,  7, 0, 0,
						6, 0, 0,  0, 1, 4,  0, 0, 0,

						0, 1, 9,  8, 0, 0,  0, 0, 0,
						0, 0, 0,  0, 4, 0,  0, 0, 0,
						0, 0, 0,  0, 0, 3,  5, 8, 0,

						0, 0, 0,  6, 2, 0,  0, 0, 7,
						0, 0, 8,  4, 0, 0,  0, 1, 6,
						0, 0, 0,  0, 0, 0,  3, 5, 0,
					};

					board.Preset(data);
				}
			}
			else
			{
				if (seed >= 0)
				{
					// Template
					int[] data =
					{
						0, 0, 0,  0, 0, 0,  0, 0, 0,
						0, 0, 0,  0, 0, 0,  0, 0, 0,
						0, 0, 0,  0, 0, 0,  0, 0, 0,

						0, 0, 0,  0, 0, 0,  0, 0, 0,
						0, 0, 0,  0, 0, 0,  0, 0, 0,
						0, 0, 0,  0, 0, 0,  0, 0, 0,

						0, 0, 0,  0, 0, 0,  0, 0, 0,
						0, 0, 0,  0, 0, 0,  0, 0, 0,
						0, 0, 0,  0, 0, 0,  0, 0, 0,
					};

					board.Preset(data);
				}
			}
		}
	}
}
