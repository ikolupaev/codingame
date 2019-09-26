using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace XmasRush.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(new Vector(1, 1), new Vector(1, 1));
            var stack = new Stack<Vector>();
            stack.Push(new Vector(1, 1));
            stack.Push(new Vector(1, 2));
            stack.Push(new Vector(1, 5));

            Assert.IsTrue(stack.Contains(new Vector(1, 2)));
        }

        [TestMethod]
        public void FindPath()
        {
            var board = new Board();
            board.Deserialize("AAkMCwoHDgcGDQsDCgkFDgMKCg8NBg0KCgcJCQYHDQcNBwMGCgoKCQwMCgwKDwMHDQoBAAAAAwAAAAEAAAAGAwAAAAAAAAACAAAADQMAAAAEAAAAQ0FORQYAAAADAAAAAQAAAAQAAABNQVNL//////////8AAAAABQAAAEFSUk9XBQAAAAUAAAABAAAAAwAAAAQAAABNQVNLAAAAAAQAAABDQU5FAQAAAAUAAABBUlJPVwEAAAA=");

            //XmaxRush.FindPath(board);
        }
    }
}
