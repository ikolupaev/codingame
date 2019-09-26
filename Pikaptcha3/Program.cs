using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    enum Dir
    {
        Right = 0,
        Down = 1,
        Left = 2,
        Top = 3
    };

    static int[,] grid;
    static int width;
    static int height;
    static int x;
    static int y;
    static int startx;
    static int starty;
    static int dx;
    static int dy;
    static Dir direction;
    static bool sideLeft;

    static void Main(string[] args)
    {
        //Console.WriteLine("#1##1#");
        //Console.WriteLine("121121");
        //Console.WriteLine("#1##1#");

        string[] inputs = Console.ReadLine().Split(' ');
        width = int.Parse(inputs[0]);
        height = int.Parse(inputs[1]);

        Console.Error.WriteLine(width + " " + height);

        grid = new int[height, width];

        for (int i = 0; i < height; i++)
        {
            var s = Console.ReadLine();
            for (int j = 0; j < width; j++)
            {
                if (s[j] == '0') grid[i, j] = 0;
                else if (s[j] == '#') grid[i, j] = -1;
                else
                {
                    grid[i, j] = 0;
                    starty = i;
                    startx = j;
                    direction = (Dir)">v<^".IndexOf(s[j]);
                }
            }
            Console.Error.WriteLine(s);
        }

        var side = Console.ReadLine();
        Console.Error.WriteLine(side);
        sideLeft = side == "L";

        x = startx;
        y = starty;
        (dx, dy) = GetDeltas(direction);

        while (true)
        {
            if (CalcAdjacent(y, x) == 0) break;
            grid[y, x]++;
            TryTurn(sideLeft);

            while (CalcPassage(y + dy, x + dx) == 0)
            {
                direction = Turn(direction, !sideLeft);
                (dx, dy) = GetDeltas(direction);
            }

            x += dx;
            y += dy;

            MobiusTransform(ref y, ref x);
            //)
            //{
            //    direction = (Dir)(((int)direction + 2) % 4);
            //}

            if (x == startx && y == starty) break;
        }

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                Console.Write(grid[i, j] < 0 ? "#" : grid[i, j].ToString());
            }
            Console.WriteLine();
        }
    }
    private static (int xx, int yy) GetDeltas(Dir direction)
    {
        switch (direction)
        {
            case Dir.Right:
                return (1, 0);
            case Dir.Down:
                return (0, 1);
            case Dir.Left:
                return (-1, 0);
            case Dir.Top:
                return (0, -1);
            default:
                throw new ArgumentException();
        }
    }

    private static void TryTurn(bool left)
    {
        Dir d = Turn(direction, left);
        var (ddx, ddy) = GetDeltas(d);

        if (CalcPassage(y + ddy, x + ddx) > 0)
        {
            dx = ddx;
            dy = ddy;
            direction = d;
        }
    }

    private static Dir Turn(Dir d, bool left)
    {
        return (Dir)(((int)d + (left ? 3 : 1)) % 4);
    }

    private static int CalcAdjacent(int r, int c)
    {
        return
            CalcPassage(r - 1, c) +
            CalcPassage(r + 1, c) +
            CalcPassage(r, c + 1) +
            CalcPassage(r, c - 1);
    }

    private static int CalcPassage(int r, int c)
    {
        MobiusTransform(ref r, ref c);

        if (r < 0 || r > height - 1 || c < 0 || c > width - 1) return 0;
        return grid[r, c] >= 0 ? 1 : 0;
    }

    private static bool MobiusTransform(ref int r, ref int c)
    {
        if (r < 0)
        {
            r = height - 1;
            c = (c + width / 2) % width;
            return true;
        }

        if (r > height - 1)
        {
            r = 0;
            c = (c + width / 2) % width;
            return true;
        }

        if (c < 0)
        {
            c = width - 1;
            return true;
        }

        if (c > width - 1)
        {
            c = 0;
            return true;
        }

        return false;
    }
}