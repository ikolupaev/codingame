using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }


        [Test]
        public void Test1()
        {
            var w = new World();
            var s = @"

E68AAYYAAQIICQoLEBESExQVFhcYGRobICEiIyQlJicoKSorMDEyMzQ1Njc4OTo7QEFCQ0RFRkdISUpL
UFFSU1RVVldYWVpbYGFiY2RlZmdoaWprcHFyc3R1dnd4eXp7gIGCg4SFhoeIiYqLkJGSk5SVlpeYmZqb
oKGio6SlpqeoqaqrsLGys7m6uwwAAQACAAEBAAABAQEBAAECAAIABJurursBuwACuroCAQGbmwMBARIB
CRsgJiorR0twdJCRlZugsro=

".Replace("\r\n", "").Replace(" ", "");

            w.Load(s);

            var s1 = w.Serialize();
            Assert.AreEqual(s, s1);
            var cells = w.GetValidCellsToTrain(0).ToArray();

            Player.DoMoves(w);

            //var cells1 = w.GetValidCellsToMove(u.Value).ToArray();
        }

        [Test]
        public void Serialize()
        {
            //var unit = new Unit { Cell = new Cell(1, 2), ID = 9, Level = 3, Mine = true };

            //var m = new MemoryStream();
            //var w = new BinaryWriter(m);
            //unit.Serialize(w);
            //m.Seek(0, SeekOrigin.Begin);
            //var r = new BinaryReader(m);
            //var unit1 = Unit.Deserialize(r);

            //Assert.AreEqual(unit, unit1);
        }
    }
}