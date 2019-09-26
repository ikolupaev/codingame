using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

enum Player
{
    Me,
    Opponent,
    None
};

class Cell
{
    public int Row;
    public int Col;
    public Player Who;

    public bool IsDiagonal() => Row == Col;
}

class Program
{
    static List<Cell> cells = new List<Cell>();
    static List<Cell> avail = new List<Cell>();
    static Stopwatch timer = new Stopwatch();
    static int ops = 0;

    static void Main(string[] args)
    {
        while (true)
        {
            var inputs = Console.ReadLine().Split(' ');
            int opponentRow = int.Parse(inputs[0]);
            int opponentCol = int.Parse(inputs[1]);

            if (opponentCol != -1)
            {
                cells.Add(new Cell { Row = opponentRow, Col = opponentCol, Who = Player.Opponent });
            }

            int validActionCount = int.Parse(Console.ReadLine());
            avail.Clear();
            for (int i = 0; i < validActionCount; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                avail.Add(new Cell { Row = int.Parse(inputs[0]), Col = int.Parse(inputs[1]), Who = Player.None });
            }

            timer.Restart();
            ops = 0;
            var bestMove = GetBestMove(Player.Me);
            Console.Error.WriteLine(ops);
            Console.Error.WriteLine(timer.ElapsedMilliseconds);
            Console.WriteLine($"{bestMove.Row} {bestMove.Col}");
        }
    }

    private static Cell GetBestMove(Player player)
    {
        var bestScore = int.MinValue;
        Cell bestCell = null;

        foreach (var c in avail.ToArray())
        {
            avail.Remove(c);
            c.Who = player;
            cells.Add(c);
            var score = MiniMax(SwitchPlayer(player));
            if (score > bestScore)
            {
                bestScore = score;
                bestCell = c;
            }
            c.Who = Player.None;
            cells.Remove(c);
            avail.Add(c);
        }

        return bestCell;
    }

    static int MiniMax(Player player)
    {
        ops++;
        if (avail.Count == 0)
        {
            return WhoWon() == player ? 1 : -1;
        }

        var bestScore = int.MinValue;
        if (player == Player.Me) bestScore = int.MaxValue;
        foreach (var c in avail.ToArray())
        {
            if (timer.ElapsedMilliseconds > 90) break;

            c.Who = player;
            cells.Add(c);
            avail.Remove(c);

            var score = MiniMax(SwitchPlayer(player));

            if (player == Player.Opponent)
            {
                bestScore = Math.Max(bestScore, score);
            }
            else
            {
                bestScore = Math.Min(bestScore, score);
            }

            c.Who = Player.None;
            cells.Remove(c);
            avail.Add(c);
        }

        return bestScore;
    }

    static Player SwitchPlayer(Player player)
    {
        return player == Player.Me ? Player.Opponent : Player.Me;
    }

    static Player WhoWon()
    {
        var winner = cells.GroupBy(x => new { x.Row, x.Who }).FirstOrDefault(x => x.Count() == 3)?.First()?.Who;
        if (winner != null) return winner.Value;

        winner = cells.GroupBy(x => new { x.Row, x.Who }).FirstOrDefault(x => x.Count() == 3)?.First()?.Who;
        if (winner != null) return winner.Value;

        winner = cells
            .Where(x => x.IsDiagonal())
            .GroupBy(x => new { x.Row, x.Who }).FirstOrDefault(x => x.Count() == 3)?.First()?.Who;

        if (winner != null) return winner.Value;

        return Player.None;
    }
}