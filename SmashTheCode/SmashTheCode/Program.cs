using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Player
{
    const int empty = -1;
    static Random rnd = new Random();
    static void Main(string[] args)
    {
        var game = new GameState();

        while (true)
        {
            LoadState(game);

            var timer = Stopwatch.StartNew();

            var bestC = 0;
            var bestRotate = 0;
            var bestScore = int.MinValue;
            var n = 0;
            while(timer.ElapsedMilliseconds < 100)
            {
                n++;
                var c = rnd.Next(6);
                var r = rnd.Next(4);
                var score = CalcScoreDeep(game, c, r);
                if (score > bestScore)
                {
                    D("best", c, r, score);
                    bestScore = score;
                    bestC = c;
                    bestRotate = r;
                }
            }

            D(n);
            Console.WriteLine($"{bestC} {bestRotate}");
        }
    }

    static int CalcScoreDeep(GameState game, int c, int r)
    {
        var g = new GameState(game);
        var bestCompressed = int.MinValue;
        foreach(var b in g.NextBlocks.Take(4))
        {
            var valid = PlaceBlock(g, b, c, r);
            if (valid)
            {
                var compressed = Compress(g);
                if (compressed > bestCompressed)
                {
                    bestCompressed = compressed;
                }
                c = rnd.Next(6);
                r = rnd.Next(4);
            }
            else
            {
                break;
            }
        }

        return bestCompressed;
    }

    public static int Compress(GameState game)
    {
        var groups = new List<(int Color, int Count)>();
        while(true)
        {
            var group = CompressStep(game);
            if (group.Count == 0) break;
            groups.Add(group);
        }

        var b = groups.Sum(x => x.Count);

        var cp = groups.Count < 2 ? 0 : 8;
        for (var i = 3; i <= groups.Count;i++) cp *= 2;

        var colors = groups.Select(x => x.Color).Distinct().Count();
        var cb = colors<2 ? 0: 1 << (colors - 1);
        var gb = groups.Sum(x => CalcGb(x.Count));

        return (10 * b) * (cp + cb + gb);
    }

    private static int CalcGb(int count)
    {
        if (count < 5) return 0;
        if (count > 10) return 8;
        return count - 4;
    }

    public static (int Color, int Count) CompressStep(GameState game)
    {
        var count = 0;
        var color = -1;
        for (var r = 0; r < 12; r++)
        {
            for (var c = 0; c < 6; c++)
            {
                var cells = new List<RC>();
                FindAdjacent(game.MyBlocks, game.MyBlocks[r][c], new RC(r, c), cells);
                if (cells.Count > 3)
                {
                    count += cells.Count;
                    foreach (var x in cells)
                    {
                        color = game.MyBlocks[x.Row][x.Column];
                        game.MyBlocks[x.Row][x.Column] = -1;
                    }
                }
            }
        }

        for (var c = 0; c < 6; c++)
        {
            CompressColumn(game.MyBlocks, c);
        }

        return (color, count);
    }

    private static void CompressColumn(int[][] blocks, int c)
    {
        var fullBlocks = new List<int>();
        for (var r = 0; r < 12; r++)
        {
            if (blocks[r][c] >= 0)
            {
                fullBlocks.Add(blocks[r][c]);
                blocks[r][c] = -1;
            }
        }

        var d = 12 - fullBlocks.Count;
        for (var r = 0; r < fullBlocks.Count; r++)
        {
            blocks[r+d][c] = fullBlocks[r];
        }
    }

    private static void FindAdjacent(int[][] blocks, int color, RC rc, List<RC> cells)
    {
        if (!rc.IsValid() || cells.Contains(rc) || blocks[rc.Row][rc.Column] <= 0) return;

        if (blocks[rc.Row][rc.Column] == color)
        {
            cells.Add(rc);
            FindAdjacent(blocks, color, new RC(rc.Row + 1, rc.Column), cells);
            FindAdjacent(blocks, color, new RC(rc.Row - 1, rc.Column), cells);
            FindAdjacent(blocks, color, new RC(rc.Row, rc.Column + 1), cells);
            FindAdjacent(blocks, color, new RC(rc.Row, rc.Column - 1), cells);
        }
    }

    public static bool PlaceBlock(GameState g, int[] block, int x, int rotate)
    {
        int x1 = 0, x2 = 0;
        int c1 = 0, c2 = 0;
        switch (rotate)
        {
            case 0:
                c1 = block[0];
                c2 = block[1];
                x1 = x;
                x2 = x + 1;
                break;
            case 1:
                c1 = block[0];
                c2 = block[1];
                x1 = x;
                x2 = x;
                break;
            case 2:
                c1 = block[0];
                c2 = block[1];
                x1 = x;
                x2 = x - 1;
                break;
            case 3:
                c1 = block[1];
                c2 = block[0];
                x1 = x;
                x2 = x;
                break;
        }

        if (x1 >= 0 && x1 < 6 && x2 >= 0 && x2 < 6)
        {
            var r = 0;
            for (int row = 0; row < 12; row++)
            {
                if (g.MyBlocks[row][x1] >= 0)
                {
                    r = row - 1;
                    break;
                }
            }

            if (r < 0) return false;
            g.MyBlocks[r][x1] = c1;

            r = 0;
            for (int row = 0; row < 12; row++)
            {
                if (g.MyBlocks[row][x2] >= 0)
                {
                    r = row - 1;
                    break;
                }
            }

            if (r < 0) return false;
            g.MyBlocks[r][x2] = c2;

            return true;
        }
        else
        {
            return false;
        }
    }

    private static void LoadState(GameState game)
    {
        for (int i = 0; i < 8; i++)
        {
            game.NextBlocks[i] = Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
        }

        game.MyScore = int.Parse(Console.ReadLine());

        for (int i = 0; i < 12; i++)
        {
            game.MyBlocks[i] = Console.ReadLine().ToCharArray().Select(x => x - '0').ToArray();
        }

        var opScore = int.Parse(Console.ReadLine());

        for (int i = 0; i < 12; i++)
        {
            game.OpponentBlocks[i] = Console.ReadLine().ToCharArray().Select(x => x - '0').ToArray();
        }
    }

    public static void D(params object[] args)
    {
        Console.Error.WriteLine(string.Join(" ", args.Select(x => x.ToString())));
    }
}

struct RC : IEquatable<RC>
{
    public RC(int row, int column)
    {
        Row = row;
        Column = column;
    }

    public int Row;
    public int Column;

    public bool IsValid()
    {
        return !(Row < 0 || Row > 11 || Column < 0 || Column > 5);
    }

    public bool Equals(RC other)
    {
        return Row == other.Row && Column == other.Column;
    }
}

public class GameState
{
    public int[][] NextBlocks = new int[8][];
    public int[][] MyBlocks = new int[12][];
    public int[][] OpponentBlocks = new int[12][];

    public int MyScore;
    public int OpponentScore;

    public GameState()
    {
    }

    public GameState(GameState game)
    {
        for (int i = 0; i < 8; i++)
        {
            NextBlocks[i] = new int[2];

            if (game.NextBlocks[i] != null)
                Array.Copy(game.NextBlocks[i], NextBlocks[i], 2);
        }

        for (int i = 0; i < 12; i++)
        {
            MyBlocks[i] = new int[6];
            OpponentBlocks[i] = new int[6];

            if (game.MyBlocks[i] != null)
                Array.Copy(game.MyBlocks[i], MyBlocks[i], 6);

            if (game.OpponentBlocks[i] != null)
                Array.Copy(game.OpponentBlocks[i], OpponentBlocks[i], 6);
        }

        MyScore = game.MyScore;
        OpponentScore = game.OpponentScore;
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var row in MyBlocks)
        {
            sb.AppendLine(new string(row.Select(x => (char)(x + '0')).ToArray()));
        }
        return sb.ToString();
    }
}