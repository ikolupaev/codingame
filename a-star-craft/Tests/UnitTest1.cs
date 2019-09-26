using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Tests
{
    [TestClass]
    public class UnitTest1
    {
        private StringWriter outString;
        private StringWriter errorString;

        [TestInitialize]
        public void Setup()
        {
            outString = new StringWriter();
            errorString = new StringWriter();

            Console.SetOut(outString);
            Console.SetError(errorString);
        }

        [TestMethod]
        public void TestMethod1()
        {
            Console.SetIn(File.OpenText("01-Simple.txt"));
            Player.Main();

            Assert.AreEqual("14 4 L\r\n", outString.ToString());
            Assert.IsTrue(errorString.ToString().EndsWith("score: 23\r\n"));
        }

        [TestMethod]
        public void TestMethod8()
        {
            Console.SetIn(File.OpenText("08-Portal.txt"));
            Player.Main();

            Assert.AreEqual("9 3 U 9 4 D\r\n", outString.ToString());
            Assert.IsTrue(errorString.ToString().EndsWith("score: 29\r\n"));
        }

        [TestMethod]
        public void TestMethod5()
        {
            Console.SetIn(File.OpenText("05-3x3-Platform.txt"));
            Player.Main();

            Assert.IsTrue(errorString.ToString().EndsWith("score: 11\r\n"));
        }
    }
}
