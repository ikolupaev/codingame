using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    static List<Cell> cells = new List<Cell>();

    static int width;
    static int height;

    static void Main(string[] args)
    {
        //Console.SetIn((TextReader)File.OpenText("in.txt"));

        width = int.Parse(Console.ReadLine());
        height = int.Parse(Console.ReadLine());

        for (int i = 0; i < height; i++)
        {
            var line = Console.ReadLine();
            Console.Error.WriteLine(line);
            Parse(line, i);
        }

        foreach (var cell in cells)
        {
            cell.WriteConsole();
            Console.Write(" ");

            var right = cells.Where(a => a.Y == cell.Y && a.X > cell.X).OrderBy(a => a.X).FirstOrDefault();
            WriteIfNotNull(right);

            Console.Write(" ");

            var down = cells.Where(a => a.X == cell.X && a.Y > cell.Y).OrderBy(a => a.Y).FirstOrDefault();
            WriteIfNotNull(down);
            Console.WriteLine();
        }
    }

    private static void WriteIfNotNull(Cell cell)
    {
        if (cell == null)
        {
            Console.Write("-1 -1");
        }
        else
        {
            cell.WriteConsole();
        }
    }

    static void Parse(string line, int i)
    {
        int x = 0;
        foreach (var ch in line.ToCharArray())
        {
            if (ch != '.')
            {
                cells.Add(new Cell { X = x, Y = i });
            }
            x++;
        }
    }

    class Cell
    {
        public int X;
        public int Y;

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
}