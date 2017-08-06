using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using static SudokuExplorer.BoardMath;

namespace SudokuTests
{
	/// <summary>
	/// Tests for the BoardMath helper utilities
	/// </summary>
	[TestClass]
	public class BoardMathTests
	{
		[TestMethod]
		public void TestOrdinalCalculations()
		{
			// Validate row,col => ordinal
			Assert.AreEqual(0,  RowColToOrdinal(0, 0));
			Assert.AreEqual(12, RowColToOrdinal(1, 3));
			Assert.AreEqual(44, RowColToOrdinal(4, 8));

			// Validate col,row => ordinal
			Assert.AreEqual(0,  ColRowToOrdinal(0, 0));
			Assert.AreEqual(12, ColRowToOrdinal(3, 1));
			Assert.AreEqual(44, ColRowToOrdinal(8, 4));

			// Validate ordinal => row
			Assert.AreEqual(0, OrdinalToRow(0));
			Assert.AreEqual(1, OrdinalToRow(12));
			Assert.AreEqual(4, OrdinalToRow(44));

			// Validate ordinal => row
			Assert.AreEqual(0, OrdinalToCol(0));
			Assert.AreEqual(3, OrdinalToCol(12));
			Assert.AreEqual(8, OrdinalToCol(44));

			// Validate row,col => box
			Assert.AreEqual(0, RowColToBox(0, 0));
			Assert.AreEqual(1, RowColToBox(1, 3));
			Assert.AreEqual(5, RowColToBox(4, 8));

			// Validate box,index => ordinal
			Assert.AreEqual(0,  BoxToOrdinal(0, 0));
			Assert.AreEqual(12, BoxToOrdinal(1, 3));
			Assert.AreEqual(44, BoxToOrdinal(5, 5));

		}

		[TestMethod]
		public void TestCandidatesToString()
		{
			// Note: I use hex notation in this test to simplify the mask

			// Validate non-valid masks
			Assert.AreEqual("<None>", CandidateMaskToString(0));
			Assert.AreEqual("<None>", CandidateMaskToString(1));
			Assert.AreEqual("<None>", CandidateMaskToString(0x400));

			// Validate two hard-coded masks
			Assert.AreEqual("1", CandidateMaskToString(0x2));
			Assert.AreEqual("9", CandidateMaskToString(0x200));

			// Validate each digit using shift operation
			for (int i = 1; i < 9; i++)
				Assert.AreEqual(i.ToString(), CandidateMaskToString(1 << i));

			// Validate a couple of combination masks
			Assert.AreEqual("1|9", CandidateMaskToString(0x202));
			Assert.AreEqual("2|7|9", CandidateMaskToString(0x284));
		}
	}
}
