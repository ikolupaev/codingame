using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Letter
{
    public char Char;
    public int Player;
}

class Score
{
    public char Letter;
    public int My;
    public int Op;

    public long Diff => My - Op;
}

class Solution
{
    private static Letter[] letters;
    private static List<Tuple<string, int>> words = new List<Tuple<string, int>>();

    static void Main(string[] args)
    {
        string[] inputs;
        var s = inputs = Read().Split(' ');
        int n = int.Parse(inputs[0]);
        int q = int.Parse(inputs[1]);
        letters = Read().Split(' ').Select(x => new Letter { Char = x[0] }).ToArray();

        for (int i = 0; i < q; i++)
        {
            inputs = Read().Split(' ');
            words.Add(Tuple.Create(inputs[0], int.Parse(inputs[1])));
        }

        var score = GetBestMove(1, Int16.MinValue, Int16.MaxValue);

        Console.WriteLine($"{score.Letter} {score.My}-{score.Op}");
    }

    private static string Read()
    {
        var s = Console.ReadLine();
        Console.Error.WriteLine(s);
        return s;
    }

    private static Score GetBestMove(int player, long a, long b)
    {
        if (letters.All(x => x.Player != 0))
        {
            return CalcScore();
        }

        var bestScore = new Score
        {
            My = Int16.MaxValue * -player,
            Op = Int16.MaxValue * player
        };

        foreach (var l in letters.Where(x => x.Player == 0).Take(2))
        {
            l.Player = player;
            var score = GetBestMove(-1 * player, a, b);
            if (player > 0)
            {
                if (bestScore.Diff < score.Diff)
                {
                    bestScore = score;
                    a = Math.Max(a, bestScore.Diff);
                }

            }
            else
            {
                if (bestScore.Diff > score.Diff)
                {
                    bestScore = score;
                    b = Math.Min(b, bestScore.Diff);
                }
            }
            l.Player = 0;
            if (b <= a) break;
        }

        return bestScore;
    }

    private static Score CalcScore()
    {
        var myLetters = letters.Where(x => x.Player > 0).Select(x => x.Char).ToHashSet();
        var opLetters = letters.Where(x => x.Player < 0).Select(x => x.Char).ToHashSet();

        var score = new Score
        {
            Letter = myLetters.First()
        };

        foreach (var w in words)
        {
            if (myLetters.Contains(w.Item1))
                score.My += w.Item2;

            if (opLetters.Contains(w.Item1))
                score.Op += w.Item2;
        }

        return score;
    }
}

static class Ext
{
    public static bool Contains<T>(this HashSet<T> a1, IEnumerable<T> a2)
    {
        foreach (var a in a2)
        {
            if (!a1.Contains(a)) return false;
        }

        return true;
    }
}