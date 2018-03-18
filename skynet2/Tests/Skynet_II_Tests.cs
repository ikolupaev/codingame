using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skynet_II;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UnitTestProject1
{
    [TestClass]
    public class Skynet_II_Tests
    {
        [TestInitialize]
        public void Init()
        {
            SkynetIIMain.nodesNumber = 10;

            SkynetIIMain.nodesDistances = new int[SkynetIIMain.nodesNumber];

            SkynetIIMain.links = new List<NodeLink>();

            using (var sr = new StreamReader(@"..\..\Skynet_II_2_data.txt"))
            {
                while (!sr.EndOfStream)
                {
                    var data = sr.ReadLine().Split(' ').Select(x => int.Parse(x)).ToArray();
                    SkynetIIMain.links.Add(new NodeLink { nodes = data });
                }
            }

            SkynetIIMain.gateways = new List<int>() { 4, 3, 5, 6 };
        }

        [TestMethod]
        public void NodeLinkTests2()
        {
            SkynetIIMain.UpdateDistanceToAgent(0);
            Assert.AreEqual(3, SkynetIIMain.nodesDistances[4]);
            Assert.AreEqual(3, SkynetIIMain.nodesDistances[3]);
            Assert.AreEqual(3, SkynetIIMain.nodesDistances[5]);
            Assert.AreEqual(3, SkynetIIMain.nodesDistances[6]);

            var linkToServ = new NodeLink { nodes = new int[]{ 3,1 } };
            SkynetIIMain.RemoveNode(SkynetIIMain.links, linkToServ);

            SkynetIIMain.UpdateDistanceToAgent(1);
            Assert.AreEqual(1, SkynetIIMain.nodesDistances[4]);
            Assert.AreEqual(int.MaxValue, SkynetIIMain.nodesDistances[3]);
            Assert.AreEqual(2, SkynetIIMain.nodesDistances[5]);
            Assert.AreEqual(2, SkynetIIMain.nodesDistances[6]);
        }
    }
}
