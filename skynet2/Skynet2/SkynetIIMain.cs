using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Skynet_II
{
    public class NodeLink
    {
        public int[] nodes;

        public static NodeLink Read()
        {
            return new NodeLink
            {
                nodes = SkynetIIMain.ReadAllInt()
            };
        }

        public bool IsLinkTo(int number)
        {
            return nodes[0] == number || nodes[1] == number;
        }

        public int GetOther(int number)
        {
            return nodes[0] == number ? nodes[1] : nodes[0];
        }

        public void Write()
        {
            Console.WriteLine("{0} {1}", nodes[0], nodes[1]);
        }

        public override bool Equals(object obj)
        {
            var node = obj as NodeLink;

            if (obj == null)
            {
                return false;
            }

            return this.nodes[0] == node.nodes[0] && this.nodes[1] == node.nodes[1];
        }
    }

    public class SkynetIIMain
    {

        public static int[] nodesDistances;
        public static List<NodeLink> links;
        public static List<int> gateways;

        public static int nodesNumber;

        static void Main(string[] args)
        {
            var initData = ReadAllInt();

            nodesNumber = initData[0];

            var linksNumber = initData[1];
            var gatewaysNumber = initData[2];

            nodesDistances = new int[nodesNumber];
            links = new List<NodeLink>(linksNumber);
            gateways = new List<int>(gatewaysNumber);

            while (linksNumber-- > 0)
            {
                var link = NodeLink.Read();
                links.Add(link);
            }

            Debug("links: " + links.Count);

            while (gatewaysNumber-- > 0)
            {
                gateways.Add(ReadInt());
            }

            Debug("gateways: " + gateways.Count);

            while (true)
            {
                var agentNode = ReadInt();
                UpdateDistanceToAgent(agentNode);

                var nearestGateway = GetNearestGateway();
                Debug("nearest gateway: " + nearestGateway.ToString());
                Debug("dist: " + nodesDistances[nearestGateway].ToString());

                var nearestGatewayLinks = links.Where(x => x.IsLinkTo(nearestGateway));
                var linkToBlock = nearestGatewayLinks.OrderBy(x => nodesDistances[x.GetOther(nearestGateway)]).First();

                linkToBlock.Write();
                RemoveNode(links, linkToBlock);
            }
        }

        public static IEnumerable<NodeLink> GetGatewayLinks()
        {
            return links.Where(link => link.nodes.Any(x => gateways.Contains(x)));
        }

        public static void RemoveNode(List<NodeLink> list, NodeLink node)
        {
            list.Remove(list.First(x => x.Equals(node)));

            Debug(list.Count.ToString());
        }

        private static int GetNearestGateway()
        {
            int minIndex = gateways[0];
            foreach (var g in gateways)
            {
                if (nodesDistances[minIndex] > nodesDistances[g])
                {
                    minIndex = g;
                }
            }

            return minIndex;
        }

        static public void UpdateDistanceToAgent(int startNode)
        {
            ResetDistances();

            Queue<int> nodesToCalc = new Queue<int>();
            nodesToCalc.Enqueue(startNode);
            nodesDistances[startNode] = 0;

            while (nodesToCalc.Any())
            {
                var currentNode = nodesToCalc.Dequeue();

                foreach (var n in links.Where(x => x.IsLinkTo(currentNode)))
                {
                    var linkToNode = n.GetOther(currentNode);

                    if (nodesDistances[linkToNode] == int.MaxValue)
                    {
                        nodesDistances[linkToNode] = nodesDistances[currentNode] + 1;
                        nodesToCalc.Enqueue(linkToNode);
                    }
                }
            }
        }

        private static void ResetDistances()
        {
            for (var i = 0; i < nodesNumber; i++)
            {
                nodesDistances[i] = int.MaxValue;
            }
        }

        static void Debug(string s)
        {
            Console.Error.WriteLine(s);
        }

        public static int[] ReadAllInt()
        {
            var s = Console.ReadLine();

            Debug(s);

            return s.Split(' ').Select(x => int.Parse(x)).ToArray();
        }

        static int ReadInt()
        {
            return int.Parse(Console.ReadLine());
        }
    }
}
