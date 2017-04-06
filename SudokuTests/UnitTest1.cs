using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfApplication1;

namespace SudokuTests
{
	[TestClass]
	public class UnitTest1
	{
		[TestMethod]
		public void TestFillSequential()
		{
			SudokuBoard board = new SudokuBoard();

			BoardFactory.FillSequential(board);

			Assert.AreEqual(1, board.Rows[0][0]);
			Assert.AreEqual(2, board.Rows[1][0]);
			Assert.AreEqual(6, board.Rows[1][4]);
		}

		[TestMethod]
		public void TestFillStriped()
		{
			SudokuBoard board = new SudokuBoard();

			BoardFactory.FillSequential(board);

			Assert.AreEqual(1, board.Rows[0][0]);
			Assert.AreEqual(2, board.Rows[1][0]);
			Assert.AreEqual(6, board.Rows[1][4]);
		}

		[TestMethod]
		public void TestValidate()
		{
			SudokuBoard board = new SudokuBoard();
			Validity valid1 = BoardValidation.Validate(board);
			Assert.AreEqual(Validity.CONSISTENT, valid1);

			BoardFactory.FillSequential(board);
			Validity valid2 = BoardValidation.Validate(board);
			Assert.AreEqual(Validity.INCONSISTENT, valid2);

			BoardFactory.FillStriped(board);
			Validity valid3 = BoardValidation.Validate(board);
			Assert.AreEqual(Validity.COMPLETE, valid3);

			board[3].Value = 15;
			Validity valid4 = BoardValidation.Validate(board);
			Assert.AreEqual(Validity.INVALID, valid4);
		}
	}
}
