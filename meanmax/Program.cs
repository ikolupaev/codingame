using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Point
{
    public Point()
    {
    }

    public Point(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public int x;
    public int y;
    public string label;

    public double DistanceTo(Point item)
    {
        return Math.Sqrt(Distance2To(item));
    }

    public double Distance2To(Point item)
    {
        var xx = x - item.x;
        var yy = y - item.y;
        return xx * xx + yy * yy;
    }

    public double Distance2To(int x, int y)
    {
        var xx = this.x - x;
        var yy = this.y - y;
        return xx * xx + yy * yy;
    }
}

class GameItem : Point
{
    public int unitId;
    public int unitType;
    public int player;
    public float mass;
    public int radius;
    public int vx;
    public int vy;
    public int extra;
    public int extra2;

    public GameItem() { }
    public GameItem(string line)
    {
        var inputs = line.Split(' ');
        unitId = int.Parse(inputs[0]);
        unitType = int.Parse(inputs[1]);
        player = int.Parse(inputs[2]);
        mass = float.Parse(inputs[3]);
        radius = int.Parse(inputs[4]);
        x = int.Parse(inputs[5]);
        y = int.Parse(inputs[6]);
        vx = int.Parse(inputs[7]);
        vy = int.Parse(inputs[8]);
        extra = int.Parse(inputs[9]);
        extra2 = int.Parse(inputs[10]);

        label = $"{unitId} ({extra})";
    }

    public bool IsReaper() => unitType == 0;
    public bool IsDestroyer() => unitType == 1;
    public bool IsDoof() => unitType == 2;
    public bool IsTanker() => unitType == 3;
    public bool IsWreck() => unitType == 4;
    public bool IsTar() => unitType == 5;
    public bool IsOil() => unitType == 6;

    public bool NotIn(IEnumerable<GameItem> items)
    {
        return items.Any(x => x.DistanceTo(this) < x.radius);
    }

    public double GetSpeedVector()
    {
        return Math.Sqrt(vx * vx + vy * vy);
    }

    public double AngleTo(Point p)
    {
        var dx = p.x - x;
        var dy = p.y - y;

        var vproduct = vx * dx + vy * dy;
        var l0 = Math.Sqrt(vx * vx + vy * vy);
        var l1 = Math.Sqrt(dx * dx + dy * dy);

        var angle = Math.Acos(vproduct / (l0 * l1));

        return angle / Math.PI * 180;
    }

    public GameItem Clone()
    {
        return new GameItem
        {
            unitId = unitId,
            unitType = unitType,
            player = player,
            mass = mass,
            radius = radius,
            vx = vx,
            vy = vy,
            extra = extra,
            extra2 = extra2
        };
    }

    public GameItem GetNext()
    {
        var ret = Clone();
        ret.x = ret.x + ret.vx;
        ret.y = ret.y + ret.vy;

        return ret;
    }
}

class Player
{
    static void Main(string[] args)
    {
        // game loop
        while (true)
        {
            int myScore = int.Parse(Console.ReadLine());
            int enemyScore1 = int.Parse(Console.ReadLine());
            int enemyScore2 = int.Parse(Console.ReadLine());
            int myRage = int.Parse(Console.ReadLine());
            int enemyRage1 = int.Parse(Console.ReadLine());
            int enemyRage2 = int.Parse(Console.ReadLine());
            int unitCount = int.Parse(Console.ReadLine());

            //dbg($"rage: {myRage}");

            var units = new GameItem[unitCount];

            for (int i = 0; i < unitCount; i++)
            {
                var str = Console.ReadLine();
                //dbg($"{i}: {str}");
                units[i] = new GameItem(str);
            }

            var reaper = units.First(x => x.player == 0 && x.IsReaper());
            var destroyer = units.First(x => x.player == 0 && x.IsDestroyer());
            var doof = units.First(x => x.player == 0 && x.IsDoof());

            string reaperStr = null;
            string destroyerStr = null;
            string doofStr = null;

            var wreaks = units.Where(x => x.IsWreck()).ToArray();
            var wreak = wreaks.Concat(FindIntersections(wreaks))
                    .ToArray()
                    .OrderByDescending(x => CalcWreakRank(x, reaper, units))
                    .FirstOrDefault();

            if (wreak != null)
            {
                if (myRage >= 60 && wreak.extra > 1 && reaper.DistanceTo(wreak) < wreak.radius && reaper.DistanceTo(destroyer) < 2000)
                {
                    var n = reaper;
                    reaperStr = "WAIT waiting for BOOM!";
                    destroyerStr = $"SKILL {n.x} {n.y}";
                }
                else
                {
                    reaperStr = $"{wreak.x - reaper.vx} {wreak.y - reaper.vy} 300 {wreak.label}";
                }
            }

            if (destroyerStr == null)
            {
                var bestFullTank = units
                        .Where(x => x.IsTanker() && x.DistanceTo(new Point(0, 0)) < 6000)
                        .OrderBy(x => x.DistanceTo(destroyer))
                        .FirstOrDefault();

                if (bestFullTank != null)
                {
                    destroyerStr = $"{bestFullTank.x + bestFullTank.vx} {bestFullTank.y + bestFullTank.vy} 300";
                }
                else
                {
                    destroyerStr = $"0 0 150";
                }

            }

            if (reaperStr == null)
            {
                reaperStr = "0 0 150 ?????";
            }

            if (destroyerStr == null)
            {
                destroyerStr = "0 0 150 ?????";
            }

            Console.WriteLine(reaperStr);
            Console.WriteLine(destroyerStr);

            GameItem higerEnemy;
            if (enemyScore1 > enemyScore2)
            {
                higerEnemy = units.First(x => x.player == 1 && x.IsReaper());
            }
            else
            {
                higerEnemy = units.First(x => x.player == 1 && x.IsReaper());
            }

            if (myRage >= 30)
            {
                var enemiesInDoofRange = units
                    .Where(x => x.player != 0 && x.IsReaper() && x.DistanceTo(reaper) > 1000 && wreaks.Any(w => w.DistanceTo(x) < w.radius))
                    .OrderByDescending(x => x == higerEnemy ? 1 : 0).FirstOrDefault();

                if (enemiesInDoofRange != null)
                {
                    doofStr = $"SKILL {enemiesInDoofRange.x} {enemiesInDoofRange.y}";
                }
            }

            if (doofStr == null)
            {
                doofStr = $"{higerEnemy.x + higerEnemy.vx} {higerEnemy.y + higerEnemy.vy} 300";
            }

            Console.WriteLine(doofStr);
            //RoundTripMove(doof, 5000);
        }
    }

    private static double CalcWreakRank(GameItem w, GameItem myReaper, IEnumerable<GameItem> units)
    {
        var dist = w.DistanceTo(myReaper);
        var otherReapers = units.Where(x => x.player != 0 && x.IsReaper());
        var otherDistMin = otherReapers.Min(x => w.DistanceTo(x));

        var tankers = units.Where(x => x.IsTanker());

        return dist * -1 +
               (dist < w.radius ? 300 : 0) +
               (otherDistMin > dist && dist > w.radius ? 2000 * (w.extra - 1) : 0) +
               (w.extra > 1 ? 300 : 0) +
               (tankers.Any( t => t.DistanceTo(w) < t.radius ) ? -300 : 0) +
               0;
    }
    static IEnumerable<GameItem> FindIntersections(IEnumerable<GameItem> items)
    {
        foreach (var a in items)
        {
            foreach (var b in items)
            {
                if (a.unitId == b.unitId)
                    continue;

                if (a.radius + b.radius > Math.Sqrt(a.Distance2To(b)))
                {
                    var x = (a.x + b.x) / 2;
                    var y = (a.y + b.y) / 2;
                    var cap = a.extra + b.extra;

                    dbg($"intersect: {a.unitId}+{b.unitId} water: {cap}");

                    yield return new GameItem
                    {
                        x = x,
                        y = y,
                        extra = cap,
                        label = $"{a.unitId}+{b.unitId} ({cap})"
                    };
                }
            }
        }
    }

    static void RoundTripMove(GameItem item, int radius, double deltaDegree = 30)
    {
        var angle = Math.Acos(item.x / Math.Sqrt((double)item.x * item.x + (double)item.y * item.y));

        if (item.y < 0)
        {
            angle = 2 * Math.PI - angle;
        }

        angle = (angle + deltaDegree * Math.PI / 180) % (2 * Math.PI);

        var xx = (int)(radius * Math.Cos(angle));
        var yy = (int)(radius * Math.Sin(angle));
        Console.WriteLine($"{xx} {yy} 300");
    }

    static void dbg(object s)
    {
        Console.Error.WriteLine(s);
    }
}