using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

class Arrow
{
    public int Row;
    public int Column;
    public char Direction;

    private Arrow() { }

    public Arrow(Arrow a)
    {
        Row = a.Row;
        Column = a.Column;
        Direction = a.Direction;
    }

    public Arrow(int row, int column, char direction)
    {
        Row = row;
        Column = column;
        Direction = direction;
    }

    public static char[] AllDirections = new[] { 'R', 'U', 'L', 'D' };

    public override string ToString()
    {
        return $"{Column} {Row} {Direction}";
    }
}

class Board
{
    public char[][] Cells = new char[10][];
    public Robot[] Robots;

    public void Load()
    {
        for (int i = 0; i < 10; i++)
        {
            Cells[i] = ReadInput().ToCharArray();
        }

        int robotCount = int.Parse(ReadInput());
        Robots = new Robot[robotCount];

        for (int i = 0; i < robotCount; i++)
        {
            string[] inputs = ReadInput().Split(' ');
            int x = int.Parse(inputs[0]);
            int y = int.Parse(inputs[1]);
            string direction = inputs[2];
            Robots[i] = new Robot(y, x, direction[0]);
        }
    }

    string ReadInput()
    {
        var s = Console.ReadLine();
        //Console.Error.WriteLine(s);
        return s;
    }

    public int Simulate()
    {
        var score = 0;
        while (true)
        {
            var alive = false;
            var length = Robots.Length;
            for (var i = 0; i < length; i++)
            {
                var r = Robots[i];
                if (!r.Alive) continue;
                var ch = Cells[r.Row][r.Column];
                if (ch != '.') r.Direction = ch;

                if (ch == '#' || r.IsInfinitLoop())
                {
                    r.Alive = false;
                    continue;
                }

                alive = true;

                score++;

                r.RegisterState();
                DoRobotStep(r);
            }

            if (!alive) break;
        }

        return score;
    }

    private void DoRobotStep(Robot r)
    {
        switch (r.Direction)
        {
            case 'U':
                r.Row--;
                break;
            case 'D':
                r.Row++;
                break;
            case 'R':
                r.Column++;
                break;
            case 'L':
                r.Column--;
                break;
        }

        if (r.Row < 0) r.Row = 9;
        if (r.Row > 9) r.Row = 0;
        if (r.Column < 0) r.Column = 18;
        if (r.Column > 18) r.Column = 0;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var c in Cells)
        {
            sb.AppendLine(new string(c));
        }

        return sb.ToString();
    }
}

class Robot
{
    public int Row;
    public int Column;
    public char Direction;

    public bool Alive = true;
    public HashSet<int> Visited = new HashSet<int>();
    public Arrow StartPosition;
    public Arrow LastPosition;

    public void Reset()
    {
        Row = StartPosition.Row;
        Column = StartPosition.Column;
        Direction = StartPosition.Direction;
        Alive = true;
        Visited.Clear();
    }

    public Robot(int row, int column, char direction)
    {
        Row = row;
        Column = column;
        Direction = direction;

        StartPosition = new Arrow(row, column, direction);
        LastPosition = new Arrow(row, column, direction);
    }

    public bool IsInfinitLoop()
    {
        //return Path.Any(x => x.Row == this.Row && x.Column == this.Column && x.Direction == this.Direction);
        return Visited.Contains(GetStateId());
    }

    internal void RegisterState()
    {
        LastPosition.Row = Row;
        LastPosition.Column = Column;
        LastPosition.Direction = Direction;

        Visited.Add(GetStateId());
    }

    private int GetStateId()
    {
        return Row * 100000 + Column * 100 + Direction;
    }
}

public class Player
{
    static Stopwatch timer;
    static Arrow[] maxArrows;
    static Stack<Arrow> arrows = new Stack<Arrow>();
    static int maxScore = 0;
    static int ticks = 0;
    public static void Main()
    {
        maxScore = 0;
        arrows.Clear();

        var board = new Board();
        board.Load();

        timer = Stopwatch.StartNew();

        try
        {
            FindBestSolution(board);
        }
        catch (TimeoutException)
        {
            Console.Error.WriteLine("timeout " + ticks);
        }

        Console.Error.WriteLine("score: " + maxScore);
        Console.WriteLine(string.Join(" ", maxArrows.Select(x => x.ToString())));
    }

    private static void FindBestSolution(Board board)
    {
#if !DEBUG
        if (timer.ElapsedMilliseconds > 990) throw new TimeoutException();
#endif
        ticks++;

        foreach (var r in board.Robots) r.Reset();

        var score = board.Simulate();
        if (score > maxScore)
        {
            maxScore = score;
            maxArrows = arrows.ToArray();
        }

        var robot = board.Robots.OrderBy(x => x.Visited.Count).First();
        var d = new Arrow(robot.LastPosition);

        if (board.Cells[d.Row][d.Column] != '.') return;

        foreach (var a in Arrow.AllDirections)
        {
            if (a == robot.Direction) continue;

            board.Cells[d.Row][d.Column] = a;
            arrows.Push(new Arrow(d.Row, d.Column, a));
            FindBestSolution(board);
            arrows.Pop();
        }

        board.Cells[d.Row][d.Column] = '.';
    }
}