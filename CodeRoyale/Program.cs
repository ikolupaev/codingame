using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 * Auto-generated code below aims at helping you parse
 * the standard input according to the problem statement.
 **/

interface IPoint
{
    int X { get; }
    int Y { get; }
}

class Point : IPoint
{
    public int X { get; set; }
    public int Y { get; set; }
}

class Unit : IPoint
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Owner;
    public int UnitType;
    public int Health;
}

class Site : IPoint
{
    public int SiteId;
    public int X { get; set; }
    public int Y { get; set; }
    public int Radius;
    public int Gold;
    public int MaxMineSize;
    public int StructureType; // -1 = No structure, 2 = Barracks
    public int Owner; // -1 = No structure, 0 = Friendly, 1 = Enemy
    public int Param1;
    public int Param2;

    public int HitRadius => Param2;
}

static class Extentions
{
    public static double Dist(this IPoint point, IPoint point1)
    {
        var x = point.X - point1.X;
        var y = point.Y - point1.Y;

        return Math.Sqrt(x * x + y * y);
    }

    public static IPoint GetNextPoint(this IPoint p, IPoint towards, int speed)
    {
        var dx = towards.X - p.X;
        var dy = towards.Y - p.Y;

        var rate = speed / p.Dist(towards);

        return new Point
        {
            X = (int)(dx * rate),
            Y = (int)(dy * rate)
        };
    }
}
class Player
{
    static void Main(string[] args)
    {
        string[] inputs;
        int numSites = int.Parse(Console.ReadLine());
        Site[] sites = new Site[numSites];
        IPoint startPoint = null;

        for (int i = 0; i < numSites; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            sites[i] = new Site();
            sites[i].SiteId = int.Parse(inputs[0]);
            sites[i].X = int.Parse(inputs[1]);
            sites[i].Y = int.Parse(inputs[2]);
            sites[i].Radius = int.Parse(inputs[3]);
        }

        int? expandId = null;

        // game loop
        while (true)
        {
            inputs = Console.ReadLine().Split(' ');
            int gold = int.Parse(inputs[0]);
            int touchedSite = int.Parse(inputs[1]); // -1 if none
            for (int i = 0; i < numSites; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                var siteId = int.Parse(inputs[0]);
                sites[siteId].Gold = int.Parse(inputs[1]);
                sites[siteId].MaxMineSize = int.Parse(inputs[2]);
                sites[siteId].StructureType = int.Parse(inputs[3]); // -1 = No structure, 0 = Goldmine, 1 = Tower, 2 = Barracks
                sites[siteId].Owner = int.Parse(inputs[4]); // -1 = No structure, 0 = Friendly, 1 = Enemy
                sites[siteId].Param1 = int.Parse(inputs[5]);
                sites[siteId].Param2 = int.Parse(inputs[6]); // 0 for KNIGHT, 1 for ARCHER, 2 for GIANT
            }

            int numUnits = int.Parse(Console.ReadLine());
            var units = new Unit[numUnits];
            Unit queen = null;
            for (int i = 0; i < numUnits; i++)
            {
                inputs = Console.ReadLine().Split(' ');
                units[i] = new Unit();
                units[i].X = int.Parse(inputs[0]);
                units[i].Y = int.Parse(inputs[1]);
                units[i].Owner = int.Parse(inputs[2]);
                units[i].UnitType = int.Parse(inputs[3]); // -1 = QUEEN, 0 = KNIGHT, 1 = ARCHER
                units[i].Health = int.Parse(inputs[4]);

                if (units[i].UnitType == -1 && units[i].Owner == 0)
                {
                    queen = units[i];
                    if (startPoint == null)
                    {
                        startPoint = new Point { X = queen.X, Y = queen.Y };
                    }
                }
            }

            // Write an action using Console.WriteLine()
            // To debug: Console.Error.WriteLine("Debug messages...");

            var m = sites.Count(x => x.StructureType == 0 && x.Owner == 0);
            var bk = sites.Count(x => x.StructureType == 2 && x.Param2 == 0 && x.Owner == 0);
            var ba = sites.Count(x => x.StructureType == 2 && x.Param2 == 1 && x.Owner == 0);
            var towers = sites.Where(x => x.StructureType == 1 && x.Owner == 0).OrderBy(x => x.Dist(queen)).ToArray();
            var t = towers.Length;

            d($"mines: {m}");
            d($"bk: {bk}");
            d($"ba: {ba}");
            d($"towers: {t}");

            var closestEnemy = units.Where(x => x.Owner > 0).OrderBy(x => x.Dist(queen)).FirstOrDefault();
            var edist = closestEnemy.Dist(queen);
            d($"closest  enemy dist: {edist}");

            if (edist < 300 && m > 1 && t > 1)
            {
                d("escape");

                Console.WriteLine($"MOVE {startPoint.X} {startPoint.Y}");
            }
            else
            {
                if (expandId == null)
                {
                    expandId = towers
                        .Where(x => x.HitRadius < 300)
                        .OrderBy(x => x.Dist(queen))
                        .FirstOrDefault()?.SiteId;
                }
                else if (sites[expandId.Value].HitRadius > 500)
                {
                    expandId = null;
                }

                var mineToImprove = sites
                    .Where(x => x.StructureType == 0 && x.Owner == 0 && x.MaxMineSize > x.Param1 && x.Gold > 0)
                    .OrderBy(x => x.Dist(queen))
                    .FirstOrDefault();

                if (mineToImprove != null)
                {
                    d($"improve mine {mineToImprove.SiteId}");
                    Console.WriteLine($"BUILD {mineToImprove.SiteId} MINE");
                }
                else if (expandId != null)
                {
                    d($"{expandId}");
                    Console.WriteLine($"BUILD {expandId} TOWER");
                }
                else
                {
                    var enemyTowers = sites.Where(x => x.StructureType == 1 && x.Owner == 1);

                    var safeSites = sites
                        .Where(x => x.StructureType == -1)
                        .Where(x => enemyTowers.All(e => x.HitRadius < x.Dist(e)))
                        .Where(x => enemyTowers.All(e => x.HitRadius < queen.GetNextPoint(x, 60).Dist(x)))
                        .OrderBy(x => x.Dist(startPoint))
                        .ToArray();

                    if (safeSites.Any())
                    {
                        var site = safeSites[0];
                        var goldSites = safeSites.Where(x => x.Gold > 0).Concat(safeSites.Where(x => x.Gold == -1));
                        if (m < 2 && goldSites.Any())
                        {
                            var ss = string.Join(", ", goldSites.Select(x =>
                                $"{x.SiteId} ({x.Gold})").ToArray());
                            d($"gold: {ss}");
                            Console.WriteLine($"BUILD {goldSites.First().SiteId} MINE");
                        }
                        else if (bk < 1)
                        {
                            Console.WriteLine($"BUILD {site.SiteId} BARRACKS-KNIGHT");
                        }
                        else if (t < 3)
                        {
                            Console.WriteLine($"BUILD {site.SiteId} TOWER");
                        }
                        else if (bk < 2)
                        {
                            Console.WriteLine($"BUILD {site.SiteId} BARRACKS-KNIGHT");
                        }
                        else if (ba < 1)
                        {
                            Console.WriteLine($"BUILD {site.SiteId} BARRACKS-ARCHER");
                        }
                        else
                        {
                        Console.WriteLine($"MOVE {startPoint.X} {startPoint.Y}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"MOVE {startPoint.X} {startPoint.Y}");
                    }
                }
            }

            var barracks = sites
                .Where(x => x.Owner == 0 && x.Param1 == 0)
                .Select(x => x.SiteId.ToString()).ToArray();

            if (gold >= 80 * 2 + 100)
            {
                var ids = string.Join(" ", barracks);
                Console.WriteLine($"TRAIN {ids}");
            }
            else
            {
                Console.WriteLine($"TRAIN");
            }
        }
    }

    static void d(string s)
    {
        Console.Error.WriteLine(s);
    }
}