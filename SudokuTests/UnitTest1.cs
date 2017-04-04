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

			board.fillSequential();

			Assert.AreEqual(1, board.Rows[0][0]);
			Assert.AreEqual(2, board.Rows[1][0]);
			Assert.AreEqual(6, board.Rows[1][4]);
		}

		[TestMethod]
		public void TestFillStriped()
		{
			SudokuBoard board = new SudokuBoard();

			board.fillSequential();

			Assert.AreEqual(1, board.Rows[0][0]);
			Assert.AreEqual(2, board.Rows[1][0]);
			Assert.AreEqual(6, board.Rows[1][4]);
		}

		[TestMethod]
		public void TestValidate()
		{
			SudokuBoard board = new SudokuBoard();
			Validity valid1 = BoardValidation.validate(board);
			Assert.AreEqual(Validity.CONSISTENT, valid1);

			board.fillSequential();
			Validity valid2 = BoardValidation.validate(board);
			Assert.AreEqual(Validity.INCONSISTENT, valid2);

			board.fillStriped();
			Validity valid3 = BoardValidation.validate(board);
			Assert.AreEqual(Validity.COMPLETE, valid3);

			board[3].Value = 15;
			Validity valid4 = BoardValidation.validate(board);
			Assert.AreEqual(Validity.INVALID, valid4);
		}
	}
}
