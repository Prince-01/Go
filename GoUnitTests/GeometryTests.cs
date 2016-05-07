using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Go;

namespace GoUnitTests
{
    [TestClass]
    public class GeometryTests
    {
        Field[,] board;

        [TestInitialize]
        public void SetUp()
        {
            board = new Field[9, 9];
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    board[i, j] = new Field(i, j);
        }

        [TestMethod]
        public void CheckBorderDetection_Rectangle()
        {
            
        }
    }
}
