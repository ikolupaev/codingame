using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    const string EXPLORER = "EXPLORER";
    const string WANDERER = "WANDERER";
    const string SLASHER = "SLASHER";
    const string EFFECT_PLAN = "EFFECT_PLAN";
    const string EFFECT_LIGHT = "EFFECT_LIGHT";
    const string EFFECT_SHELTER = "EFFECT_SHELTER";

    const int STUNNED = 4;

    private static Playfield map;
    private static Playfield map1;
    private static Unit me;
    private static Unit[] units;

    static void Main(string[] args)
    {
        map = PlayfieldFactory.Create(Console.In);
        map1 = new Playfield(map.Dimentions);

        var inputs = Console.ReadLine().Split(' ');
        int sanityLossLonely = int.Parse(inputs[0]); // how much sanity you lose every turn when alone, always 3 until wood 1
        int sanityLossGroup = int.Parse(inputs[1]); // how much sanity you lose every turn when near another player, always 1 until wood 1
        int wandererSpawnTime = int.Parse(inputs[2]); // how many turns the wanderer take to spawn, always 3 until wood 1
        int wandererLifeTime = int.Parse(inputs[3]); // how many turns the wanderer is on map after spawning, always 40 until wood 1

        var restPlan = 2;
        var turn = 0;
        var lastPlanTurn = 0;
        // game loop
        while (true)
        {
            turn++;

            LoadUnits();

            var closeExp = units.Skip(1).Count(x => x.UnitType == EXPLORER && x.Pos.ManhattanDistance(me.Pos) < 3);
            if (restPlan > 0 && me.Sanity < 200 && lastPlanTurn < turn -5 && closeExp > 2)
            {
                Console.WriteLine("PLAN");
                restPlan--;
                lastPlanTurn = turn;
                continue;
            }

            Vector2D t = null;

            var shelters = units
                .Where(x => x.UnitType == EFFECT_SHELTER && x.Param0 > 0)
                .OrderBy(x => x.Pos.ManhattanDistance(me.Pos))
                .ToArray();

            if (shelters.Any())
            {
                t = FindNextCell(map1, me.Pos, shelters.Select(x => x.Pos));
            }

            if (t == null)
            {
                t = Vector2D
                    .DirectionsAndMe
                    .Select(x => Vector2D.Add(me.Pos, x))
                    .Where(x => map.IsWalkable(x))
                    .OrderBy(x => CalcBadCellRank(x))
                    .FirstOrDefault();
            }

            if (t != null)
                Console.WriteLine($"MOVE {t.X} {t.Y}");
            else
                Console.WriteLine("WAIT");
        }
    }

    private static int CalcBadCellRank(Vector2D p)
    {
        var badRank = 0;

        badRank -= 10 *
            units
            .Where(x => x.UnitType == WANDERER || (x.UnitType == SLASHER && x.Param1 != STUNNED))
            .MinDistance(p);

        badRank -= 100 *
            (units.Count(x => x.UnitType == WANDERER && x.Pos.ManhattanDistance(p) == 1) > 1 ? 1 : 0);

        badRank += 2 * units
            .Skip(1)
            .Where(x => x.UnitType == EXPLORER)
            .MinDistance(p);

        badRank -= 50 * units
            .Skip(1)
            .Where(x => x.UnitType == EXPLORER && x.Pos.ManhattanDistance(p) < 3)
            .Count();

        badRank += 5 * (units.Where(x => x.UnitType == SLASHER && x.Param1 != STUNNED && x.Pos.IsInlineTo(p)).Any() ? 1 : 0);


        D("bad:", p, badRank);

        return badRank;
    }

    private static void LoadUnits()
    {
        int unitsCount = int.Parse(Console.ReadLine()); // the first given entity corresponds to your explorer
        units = new Unit[unitsCount];
        for (int i = 0; i < unitsCount; i++)
        {
            var inputs = Console.ReadLine().Split(' ');
            var e = new Unit();
            e.UnitType = inputs[0];
            e.Id = int.Parse(inputs[1]);
            e.Pos.X = int.Parse(inputs[2]);
            e.Pos.Y = int.Parse(inputs[3]);
            e.Param0 = int.Parse(inputs[4]);
            e.Param1 = int.Parse(inputs[5]);
            e.Param2 = int.Parse(inputs[6]);
            units[i] = e;

            e.Distance = e.Pos.ManhattanDistance(units[0].Pos);
        }

        me = units[0];
    }

    static Vector2D FindNextCell(Playfield p, Vector2D from, IEnumerable<Vector2D> tos)
    {
        var pathFinder = new PathFinder(p).From(from);
        var path = tos
            .Select(x => pathFinder.To(x).FindPath())
            .Where(x => x != PathFinderResult.NO_PATH)
            .OrderBy(x => x.WeightedLength).FirstOrDefault();

        if (path == null)
        {
            D("shortest: none");
            return null;
        }

        D("shortest: ", path.GetNextCell(), path.Path.Last(), path.WeightedLength);

        if (path.HasNextCell())
            return path.Path[1].Pos;
        else
            return from;
    }

    public static void D(params object[] oo)
    {
        if (oo == null)
        {
            Console.WriteLine("(null)");
            return;
        }

        foreach (var o in oo)
        {
            Console.Error.Write(o);
            Console.Error.Write(" ");
        }

        Console.Error.WriteLine();
    }

    private static Vector2D FindMoveAwayFrom(Playfield pf, IEnumerable<Vector2D> wanderers)
    {
        Vector2D best = null;
        int minDist = int.MinValue;
        foreach (var dir in Vector2D.Directions)
        {
            var p = Vector2D.Add(me.Pos, dir);

            D("move away", p, pf[p].CellType);

            if (pf.IsWalkable(p))
            {
                var min = wanderers.Min(x => x.ManhattanDistance(p));
                D("min dist:", min);
                if (min > minDist) best = p;
            }
        }

        return best;
    }
}

static class PlayerExtentions
{
    public static int MinDistance(this IEnumerable<Unit> units, Vector2D p)
    {
        var distances = units.Select(x => x.Pos.ManhattanDistance(p)).ToArray();
        if (distances.Any())
            return distances.Min();
        else
            return 0;
    }
}