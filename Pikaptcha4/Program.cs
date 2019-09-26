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
        Up = 3
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

            while (CalcPassage(y + dy, x + dx, direction) == 0)
            {
                direction = Turn(direction, !sideLeft);
                (dx, dy) = GetDeltas(direction);
            }

            MoveOnCube(ref y, ref x, ref direction);
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
            case Dir.Up:
                return (0, -1);
            default:
                throw new ArgumentException();
        }
    }

    private static void TryTurn(bool left)
    {
        Dir d = Turn(direction, left);
        var (ddx, ddy) = GetDeltas(d);

        if (CalcPassage(y + ddy, x + ddx, d) > 0)
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
        var d1 = Dir.Left;
        var d2 = Dir.Right;
        var d3 = Dir.Down;
        var d4 = Dir.Up;
        return
            CalcPassage(r, c, d1) +
            CalcPassage(r, c, d2) +
            CalcPassage(r, c, d3) +
            CalcPassage(r, c, d4);
    }

    private static int CalcPassage(int r, int c, Dir dir)
    {
        MoveOnCube(ref r, ref c, ref dir);

        if (r < 0 || r > height - 1 || c < 0 || c > width - 1) return 0;
        return grid[r, c] >= 0 ? 1 : 0;
    }

    private static void MoveOnCube(ref int r, ref int c, ref Dir dir)
    {
        var side = (r / 6) + 1;
        var (dc, dr) = GetDeltas(dir);
        r += dr;
        c += dc;

        switch (side)
        {
            case 1:
                if (c < 0)
                {
                    side = 2;
                    var newr = width;
                    var newc = r;

                    dir = Dir.Down;
                    r = newr;
                    c = newc;
                    return;
                }
                if (c > width - 1)
                {
                    side = 4;
                    var newr = 3 * width;
                    var newc = r;

                    dir = Dir.Down;
                    r = newr;
                    c = newc;
                    return;
                }
                if (r < 0)
                {
                    side = 6;
                    var newr = 5 * width - 1;
                    var newc = c;

                    dir = Dir.Up;
                    r = newr;
                    c = newc;
                    return;
                }
                if (r > width - 1)
                {
                    side = 3;
                    var newr = 2 * width;
                    var newc = c;

                    dir = Dir.Down;
                    r = newr;
                    c = newc;
                    return;
                }
                return;
            case 2:
                if (c < 0)
                {
                    side = 6;
                    var newc = 1;
                    var newr = 6 * width - (r % width) - 1;

                    dir = Dir.Right;
                    c = newc;
                    r = newr;
                    return;
                }
                if (c > width - 1)
                {
                    side = 3;
                    var newc = 1;
                    var newr = 2 * width + r % width;

                    dir = Dir.Right;
                    c = newc;
                    r = newr;
                    return;
                }
                if (r < width + 1)
                {
                    side = 1;
                    var newc = 1;
                    var newr = c;

                    dir = Dir.Right;
                    c = newc;
                    r = newr;
                    return;
                }
                if (GetSide(r) == 3)
                {
                    side = 5;
                    var newc = 1;
                    var newr = 4 * width + c;

                    dir = Dir.Right;
                    c = newc;
                    r = newr;
                    return;
                }
                return;
            case 3:
                return;
            case 4:
                if (c < 0)
                {
                    side = 3;
                    var newc = width - 1;
                    var newr = 2 * width + r % width;

                    dir = Dir.Left;
                    r = newr;
                    c = newc;
                    return;
                }
                if (GetSide(c) == 2)
                {
                    side = 6;
                    var newc = width - 1;
                    var newr = 6 * width - (r % width) - 1;

                    dir = Dir.Left;
                    r = newr;
                    c = newc;
                    return;
                }
                if( GetSide(r )> 4)
                {
                    side = 5;
                    var newc = width - 1;
                    var newr = 4 * width + c;

                    dir = Dir.Left;
                    r = newr;
                    c = newc;
                    return;
                }
                return;
            case 5:
                if (c < 0)
                {
                    return;
                }
                if (GetSide(c) == 2)
                {
                    side = 4;
                    var newc = r %width;
                    var newr = 4 * width-1;

                    dir = Dir.Up;
                    r = newr;
                    c = newc;
                    return;
                }
                if (GetSide(r) > 5)
                {
                    side = 6;
                    var newc = c;
                    var newr = 5 * width;

                    dir = Dir.Left;
                    r = newr;
                    c = newc;
                    return;
                }
                return;
            case 6:
                if (GetSide(c) == 2)
                {
                    side = 4;
                    var newc = width-1;
                    var newr = 4 * width - (r /width)-1;

                    dir = Dir.Left;
                    r = newr;
                    c = newc;
                    return;
                }
                if (GetSide(r) < 6)
                {
                    side = 5;
                    var newc = c;
                    var newr = 5 * width-1;

                    dir = Dir.Up;
                    r = newr;
                    c = newc;
                    return;
                }
                return;
        }
    }

    private static int GetSide(int r)
    {
        return r / 6 + 1;
    }
}