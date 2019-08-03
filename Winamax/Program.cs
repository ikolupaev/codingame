using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;

struct Cell
{
    public readonly int X;
    public readonly int Y;
    public readonly char Char;

    public Cell(int x, int y) : this(x, y, '?') { }
    public Cell(int x, int y, char ch)
    {
        X = x;
        Y = y;
        Char = ch;
    }

    public static bool operator ==(Cell a, Cell b) => a.X == b.X && a.Y == b.Y;
    public static bool operator !=(Cell a, Cell b) => !(a == b);

    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (!(obj is Cell)) return false;

        return (Cell)obj == this;
    }

    public override int GetHashCode()
    {
        return Y * 1001 + X;
    }

    public override string ToString()
    {
        return $"{X} {Y} {Char}";
    }
}

public enum SegmentResult
{
    Failed,
    FinishedInAHole,
    Succeded
}

class Solution
{
    static HashSet<Cell> Holes, Balls, Water;
    static HashSet<Cell> WholeTrack;
    static int width, height;

    static int counter = 0;

    public static SegmentResult AddSegment(List<Cell> track, Cell start, Cell end, HashSet<Cell> balls)
    {
        var dx = 0;
        var dy = 0;
        char ch;

        if (start.X == end.X)
        {
            dy = Math.Sign(end.Y - start.Y);
            ch = dy > 0 ? 'v' : '^';
        }
        else
        {
            dx = Math.Sign(end.X - start.X);
            ch = dx > 0 ? '>' : '<';
        }

        var prev = new Cell(start.X, start.Y, ch);

        while (true)
        {
            if (prev.X < 0 || prev.X >= width || prev.Y < 0 || prev.Y >= height) return SegmentResult.Failed;
            if (track.Contains(prev)) return SegmentResult.Failed;
            if (WholeTrack.Contains(prev)) return SegmentResult.Failed;
            if (balls.Contains(prev)) return SegmentResult.Failed;

            if (prev == end)
            {
                if (Water.Contains(prev)) return SegmentResult.Failed;

                track.Add(new Cell(prev.X, prev.Y, '.'));

                if (Holes.Contains(prev))
                {
                    return SegmentResult.FinishedInAHole;
                }

                return SegmentResult.Succeded;
            }

            track.Add(prev);
            prev = new Cell(prev.X + dx, prev.Y + dy, ch);
        }
    }

    static void Main(string[] args)
    {
        //Console.SetIn(File.OpenText(@"C:\Users\ikolu\sources\codingame\Winamax\in9.txt"));

        var row = Console.ReadLine();
        Console.Error.WriteLine(row);
        string[] inputs = row.Split(' ');
        width = int.Parse(inputs[0]);
        height = int.Parse(inputs[1]);

        Holes = new HashSet<Cell>();
        Balls = new HashSet<Cell>();
        Water = new HashSet<Cell>();
        WholeTrack = new HashSet<Cell>();

        for (int y = 0; y < height; y++)
        {
            row = Console.ReadLine();
            Console.Error.WriteLine(row);
            for (var x = 0; x < row.Length; x++)
            {
                if (row[x] == 'X') Water.Add(new Cell(x, y, row[x]));
                else if (row[x] == 'H') Holes.Add(new Cell(x, y, row[x]));
                else if (row[x] != '.') Balls.Add(new Cell(x, y, (char)(row[x] - '0')));
            }
        }

        //Console.WriteLine("v<<<<<..");
        //Console.WriteLine("v.>>>>v.");
        //Console.WriteLine("vvv<<<v.");
        //Console.WriteLine("vvv...v.");
        //Console.WriteLine("vvv.>.v.");
        //Console.WriteLine("vv..^.v.");
        //Console.WriteLine("v>>>^.<.");
        //Console.WriteLine(">>......");

        //Console.WriteLine("v<<<<<<<");
        //Console.WriteLine("v>>>..<<");
        //Console.WriteLine("v^>>>>>v");
        //Console.WriteLine(".^.v<<vv");
        //Console.WriteLine(".^..>.vv");
        //Console.WriteLine("v<<<^.<v");
        //Console.WriteLine("v...^<<<");
        //Console.WriteLine(".>>^.<<<");

        //Console.WriteLine("v<<<<<..v.>>>>>vv<<<<<<<.<<<...<>>..>>>v");
        //Console.WriteLine("v.>>>>v.v^<<<<<vv>>>..<<.v<<<<<^v<<<<..v");
        //Console.WriteLine("vvv<<<v.v>>>>>.vv^>>>>>v.v.....^v..>>v.<");
        //Console.WriteLine("vvv...v.v^>>>.vv.^.v<<vvvv..<.v.v^<<....");
        //Console.WriteLine("vvv.>.v.v^^>>vvv.^..>.vvvv..^.v.>>..^<<^");
        //Console.WriteLine("vv..^.v.v^^^..vvv<<<^.<vv>>>^.<^....>>>^");
        //Console.WriteLine("v>>>^.<.v^^^^<<vv...^<<<...>>>.^.>>..>>v");
        //Console.WriteLine(">>......>>.^.<<<.>>^.<<<.>>>...^.<<.....");

        //return;

        var timer = Stopwatch.StartNew();

        if (!Solve(Balls))
        {
            Console.WriteLine("failed");
            return;
        }

        Console.Error.WriteLine("time: " + timer.Elapsed.TotalMilliseconds);
        Console.Error.WriteLine("calcs: " + counter);

        var grid = new StringBuilder[height];
        for (var r = 0; r < height; r++)
        {
            grid[r] = new StringBuilder(new string('.', width));
        }

        foreach (var x in WholeTrack)
        {
            grid[x.Y][x.X] = x.Char;
        }

        for (var r = 0; r < height; r++)
        {
            Console.WriteLine(grid[r]);
        }
    }

    public static bool Solve(HashSet<Cell> balls)
    {
        if (balls.Count == 0) return true;

        var savedTrack = WholeTrack.ToList();
        foreach (var b in balls)
        {
            var bb = balls.ToHashSet();
            bb.Remove(b);

            var allTracks = GetAllTracks(b, bb);
            foreach (var p in allTracks)
            {
                WholeTrack = savedTrack.ToHashSet();
                WholeTrack.UnionWith(p);

                if (Solve(bb)) return true;
            }
        }

        return false;
    }

    static List<Cell> dirs = new List<Cell> { new Cell(-1, 0), new Cell(1, 0), new Cell(0, 1), new Cell(0, -1) };

    static IEnumerable<List<Cell>> GetAllTracks(Cell ball, HashSet<Cell> balls)
    {
        var track = new List<Cell>();
        track.Add(ball);

        foreach (var t in GetPossibleSubTracks(track, ball.Char, balls).ToArray())
        {
            yield return t;
        }
    }

    private static Random rng = new Random();

    public static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private static IEnumerable<List<Cell>> GetPossibleSubTracks(List<Cell> track, int count, HashSet<Cell> balls)
    {
        counter++;
        //Console.Error.WriteLine(count + " " + counter);
        //Shuffle(dirs);

        foreach (var d in dirs)
        {
            var end = track.Last();
            var futherTrack = new List<Cell>(track);
            futherTrack.Remove(end);
            var res = AddSegment(futherTrack, end, new Cell(end.X + d.X * count, end.Y + d.Y * count), balls);

            if (res == SegmentResult.FinishedInAHole)
            {
                yield return futherTrack;
            }

            if (res == SegmentResult.Failed || count == 1) continue;

            foreach (var t in GetPossibleSubTracks(futherTrack, count - 1, balls).ToArray())
            {
                yield return t;
            }
        }
    }
}