using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Solution
{
    static private TextReader reader;

    struct Position
    {
        public int l;
        public int c;
        public int dir;
        public bool b;
        public int nextTryDelta;

        public override string ToString()
        {
            return string.Format("l:{0} c:{1} {2}", l, c, directions[dir]);
        }
    }

    static Position p = new Position { dir = 0 };

    static StringBuilder[] f;
    static int L;
    static int C;

    static int nextTryDelta = 1;
    static bool beerMode = false;

    static string[] directions = { "SOUTH", "EAST", "NORTH", "WEST" };

    static List<Position> path = new List<Position>();

    static List<Position> teleports = new List<Position>(2);

    static int lastXStep = 0;

    static char GetCurrent()
    {
        var ch = f[p.l][p.c];

        if (beerMode && ch == 'X')
        {
            ch = ' ';
            f[p.l][p.c] = ' ';
            lastXStep = path.Count();
        }

        return ch;
    }

    static void Step()
    {
        switch (p.dir)
        {
            case 0:
                p.l++;
                break;
            case 1:
                p.c++;
                break;
            case 2:
                p.l--;
                break;
            case 3:
                p.c--;
                break;
        }
    }

    static void Teleport()
    {

        Console.Error.WriteLine("teleporting: {0}", p);

        var t = teleports.First(x => !(x.c == p.c && x.l == p.l));
        p.c = t.c;
        p.l = t.l;
    }

    static void Adjust()
    {
        var ch = GetCurrent();

        switch (ch)
        {
            case '#':
            case 'X':
                TryAround();
                Adjust();
                break;
            case 'I':
                nextTryDelta *= -1;
                break;
            case 'B':
                beerMode = !beerMode;
                break;
            case 'T':
                Teleport();
                break;
            case 'S':
                p.dir = 0;
                break;
            case 'E':
                p.dir = 1;
                break;
            case 'N':
                p.dir = 2;
                break;
            case 'W':
                p.dir = 3;
                break;
        }
    }

    static void TryAround()
    {
        var ch = GetCurrent();
        var nextTryDir = nextTryDelta > 0 ? 0 : 3;
        while (ch == '#' || ch == 'X')
        {
            p = path.Last();
            p.dir = nextTryDir;
            Console.Error.WriteLine( " ->" + p.ToString() );
            Step();
            nextTryDir += nextTryDelta;
            ch = GetCurrent();
        }

        Console.Error.WriteLine("OK");

        path.Remove(path.Last());
        AddToPath();
    }

    static void StepNext()
    {
        Step();
        Adjust();
    }

    static bool IsLoopDetected()
    {
        return path.Skip(lastXStep).Take(path.Count - lastXStep - 1)
            .Count(x => x.l == p.l && x.c == p.c && x.dir == p.dir && x.b == beerMode && x.nextTryDelta == nextTryDelta) > 0;
    }

    static void AddToPath()
    {
        p.b = beerMode;
        p.nextTryDelta = nextTryDelta;
        path.Add(p);
    }

    static string ReadLine()
    {
        var s = reader.ReadLine();
        Console.Error.WriteLine(s);
        return s;
    }

    public static void Main(string[] args)
    {
        reader = Console.In;
        Run(reader);
    }

    public static void Run(TextReader commandsReader)
    {
        reader = commandsReader;

        string[] inputs = ReadLine().Split(' ');
        L = int.Parse(inputs[0]);
        C = int.Parse(inputs[1]);
        f = new StringBuilder[L];

        for (int i = 0; i < L; i++)
        {
            f[i] = new StringBuilder(ReadLine());

            var b = f[i].ToString().IndexOf('@');
            if (b >= 0)
            {
                p.l = i;
                p.c = b;
                f[i][b] = ' ';
            }

            b = f[i].ToString().IndexOf('T');
            if (b >= 0)
            {
                teleports.Add(new Position { l = i, c = b });
            }
        }

        while (GetCurrent() != '$')
        {
            //DebugState();

            if (IsLoopDetected())
            {
                Console.WriteLine("LOOP");
                return;
            }

            AddToPath();
            StepNext();
        }

        foreach (var x in path)
        {
            Console.WriteLine(directions[x.dir]);
        }
    }

    static void DebugState()
    {
        Console.SetCursorPosition(0, 10);

        for (int l = 0; l < f.Length; l++)
        {
            if (p.l == l)
            {
                var s = new StringBuilder(f[l].ToString());
                s[p.c] = '@';
                Console.WriteLine(s.ToString());

            }
            else
            {
                Console.WriteLine(f[l].ToString());
            }
        }

        Console.WriteLine($"beer-mode: {beerMode}, delta: {nextTryDelta}");
        Console.ReadLine();
        Console.Clear();

    }
}