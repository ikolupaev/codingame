using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    static List<Cell> cells = new List<Cell>();
    static Links links;

    static int width;
    static int height;


    static void Main(string[] args)
    {
#if DEBUG
        Console.SetIn((TextReader)File.OpenText("in.txt"));
#endif

        Init();

        while (true)
        {
            dfield();

            var maxCell = cells.OrderByDescending(x => x.V).First();

            d("max cell", maxCell);

            var neighbors = GetNeighbors(maxCell).Where(x => x != null && x.V > 0).ToList();

            d("neighbors", neighbors.Count());

            var possibleToLink = GetCellsPossibleToLink(maxCell, neighbors).ToList();

            d("possible to link", possibleToLink.Count());

            var betterToLink = possibleToLink.OrderByDescending(x => x.V).FirstOrDefault();

            if (betterToLink == null)
            {
                break;
            }

            maxCell.WriteConsole();
            Console.Write(" ");
            betterToLink.WriteConsole();
            Console.WriteLine(" 1");

            links.Link(maxCell, betterToLink);
        }

        d("done", true);
    }

    private static void Init()
    {
        width = int.Parse(Console.ReadLine());
        height = int.Parse(Console.ReadLine());

        Console.Error.WriteLine(width);
        Console.Error.WriteLine(height);

        for (int i = 0; i < height; i++)
        {
            var line = Console.ReadLine();
            Console.Error.WriteLine(line);
            Parse(line, i);
        }

        links = new Links();
    }

    private static void dfield()
    {
        var field = new char[height][];
        for (var row = 0; row < height; row++)
        {
            field[row] = new string('.', width).ToCharArray();
        }

        foreach (var c in cells)
        {
            field[c.Y][c.X] = c.V.ToString()[0];
        }

        for (var row = 0; row < height; row++)
        {
            Console.Error.WriteLine(field[row]);
        }
    }

    static void d(string message, object o)
    {
        Console.Error.Write(message + ": ");
        Console.Error.WriteLine(o);
    }

    private static IEnumerable<Cell> GetNeighbors(Cell cell)
    {
        var vert = cells.Where(a => a.X == cell.X).ToList();
        yield return vert.Where(a => a.Y > cell.Y).OrderBy(a => a.Y).FirstOrDefault();
        yield return vert.Where(a => a.Y < cell.Y).OrderBy(a => a.Y).LastOrDefault();

        var horiz = cells.Where(a => a.Y == cell.Y).ToList();
        yield return horiz.Where(a => a.X < cell.X).OrderBy(a => a.X).LastOrDefault();
        yield return horiz.Where(a => a.X > cell.X).OrderBy(a => a.X).FirstOrDefault();
    }

    private static IEnumerable<Cell> GetCellsPossibleToLink(Cell maxCell, IEnumerable<Cell> cc)
    {
        foreach (var x in cc)
        {
            var linksNum = links.GetLinks(maxCell, x);
            if (linksNum < 2)
            {
                yield return x;
            }
        }
    }

    static void Parse(string line, int i)
    {
        int x = 0;
        foreach (var ch in line.ToCharArray())
        {
            if (ch != '.')
            {
                cells.Add(new Cell { X = x, Y = i, V = int.Parse(ch.ToString()) });
            }

            x++;
        }
    }

    class Cell
    {
        public int X;
        public int Y;

        public int V;

        public int UniqueId { get { return Y * width + X; } }

        public override string ToString()
        {
            return X.ToString() + " " + Y.ToString();
        }

        public void WriteConsole()
        {
            Console.Write(X);
            Console.Write(" ");
            Console.Write(Y);
        }
    }

    class Links
    {
        Dictionary<int, int> linksCount = new Dictionary<int, int>();

        public int GetLinks(params Cell[] cc)
        {
            var key = GetKey(cc);
            return linksCount[key];
        }

        private int GetKey(params Cell[] cc)
        {
            cc = cc.OrderBy(c => c.UniqueId).ToArray();

            var key = cc[0].UniqueId * 1000 + cc[1].UniqueId;

            if (!linksCount.ContainsKey(key))
            {
                linksCount.Add(key, 0);
            }

            return key;
        }

        public void Link(params Cell[] cc)
        {
            var key = GetKey(cc);
            linksCount[key]++;

            cc[0].V--;
            cc[1].V--;
        }
    }
}