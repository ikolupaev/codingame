using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

class Vector : IEqualityComparer<Vector>
{
    public static readonly IEqualityComparer<Vector> Comparer = new Vector();

    public int X;
    public int Y;

    public Vector()
    {
    }

    public Vector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return $"{X}:{Y}";
    }

    public bool Equals(Vector v)
    {
        return v.X == X && v.Y == Y;
    }

    public int GetDistance(Vector v)
    {
        var dx = Math.Abs(X - v.X);
        var dy = Math.Abs(Y - v.Y);

        return dx + dy;
    }

    public bool Equals(Vector x, Vector y)
    {
        return x.Equals(y);
    }

    public int GetHashCode(Vector obj)
    {
        return Y * 1000 + X;
    }
}

class Cell
{
    public Vector Pos;
    public int Value;
}

class Player
{
    static Random rnd = new Random();

    static int[,] cells = new int[35, 20];
    static int[,] voronoi = new int[35, 20];

    class Rect
    {
        public int EnemyCells;
        public int MyCells;
        public int MyVoronoiCells;
        public int Area;
        public int Distance;

        public int X1;
        public int X2;
        public int Y1;
        public int Y2;

        int dx;
        int dy;

        public IEnumerable<Vector> GetVertexes()
        {
            yield return new Vector(X1, Y1);
            yield return new Vector(X2, Y1);
            yield return new Vector(X2, Y2);
            yield return new Vector(X1, Y2);
        }

        public IEnumerable<Vector> GetPerimeter()
        {
            for (var x = X1 + 1; x < X2; x++)
            {
                yield return new Vector(x, Y1);
                yield return new Vector(x, Y2);
            }

            for (var y = Y1; y <= Y2; y++)
            {
                yield return new Vector(X1, y);
                yield return new Vector(X2, y);
            }
        }

        public Rect(int x1, int y1, int x2, int y2)
        {
            Update(x1, y1, x2, y2);
        }

        public void Update(int x1, int y1, int x2, int y2)
        {
            X1 = Math.Min(x1, x2);
            X2 = Math.Max(x1, x2);
            Y1 = Math.Min(y1, y2);
            Y2 = Math.Max(y1, y2);
            dx = Math.Abs(x2 - x1);
            dy = Math.Abs(y2 - y1);
        }

        public void Update()
        {
            Distance = Math.Abs(pos[0].X - X1) + dx / 2 + Math.Abs(pos[0].Y - X1) + dy / 2;

            Area = dx * dy;

            EnemyCells = 0;
            MyCells = 0;
            MyVoronoiCells = 0;

            for (var y = Y1; y <= Y2; y++)
            {
                for (var x = X1; x <= X2; x++)
                {
                    if (cells[x, y] > 0) EnemyCells++;
                    if (cells[x, y] == 0) MyCells++;
                    if (voronoi[x, y] == 0) MyVoronoiCells++;
                }
            }
        }

        public override string ToString()
        {
            return $"({X1},{Y1}:{X2},{Y2}) area:{Area} my:{MyCells} vor:{MyVoronoiCells} enemy:{EnemyCells} dist:{Distance}";
        }
    }

    static Vector[] deltas = new[]
    {
        new Vector{ X = -1, Y = -1 },
        new Vector{ X =  0, Y = -1 },
        new Vector{ X =  1, Y = -1 },
        new Vector{ X = -1, Y =  0 },
        new Vector{ X =  1, Y =  0 },
        new Vector{ X = -1, Y =  1 },
        new Vector{ X =  0, Y =  1 },
        new Vector{ X =  1, Y =  1 }
    };

    static Vector[] pos;

    static Rect target = null;
    static Stopwatch timer = new Stopwatch();
    static int ticks = 0;
    static void Main(string[] args)
    {
        int enemyCount = int.Parse(Console.ReadLine()); // Opponent count

        pos = new Vector[enemyCount + 1];
        for (var i = 0; i <= enemyCount; i++)
        {
            pos[i] = new Vector();
        }

        // game loop
        while (true)
        {
            D("time:", timer.ElapsedMilliseconds, "ticks:", ticks);

            timer.Restart();
            ticks = 0;
            LoadState(enemyCount);

            UpdateVoronoi();

            if (target != null)
            {
                target.Update();
                if (target.EnemyCells > 0 || target.MyCells >= target.Area) target = null;
            }

            if (target == null)
            {
                CreateNewTarget();
            }

            if (target == null)
            {
                D("target is null!!!");
            }
            else
            {
                D(target);
            }

            var t = target?
                .GetPerimeter()?
                .Where(x => cells[x.X, x.Y] < 0)?
                .OrderBy(x => x.GetDistance(pos[0]))?
                .FirstOrDefault();

            if (t == null)
            {
                D("finding first empty");
                t = Flatten()
                    .Where(x => x.Value < 0)
                    .OrderBy(x => x.Pos.GetDistance(pos[0]))
                    .FirstOrDefault()?.Pos;
            }

            if (t == null) t = pos[0];

            Console.WriteLine($"{t.X} {t.Y}"); // action: "x y" to move or "BACK rounds" to go back in time
        }
    }

    private static void D(params object[] args)
    {
        Console.Error.WriteLine(string.Join(" ", args));
    }

    private static double CalcRectScore(Rect rect)
    {
        rect.Update();

        double area = rect.Area;
        return
              area +
              rect.MyVoronoiCells / area
            - (rect.EnemyCells > 0 ? 10000 : 0)
            - (rect.MyCells >= area ? 1000000 : 0)
            - rect.Distance * 0.5
            ;
    }

    private static void CreateNewTarget()
    {
        var maxScore = double.MinValue;
        for (var height = 10; height >= 2; height--)
        {
            for (var width = 10; width >= 2; width--)
            {
                for (int y = 0; y < 20; y++)
                {
                    for (int x = 0; x < 35; x++)
                    {
                        if (timer.ElapsedMilliseconds > 90) return;
                        ticks++;
                        var x2 = x + width;
                        var y2 = y + height;

                        if (IsValid(x2, y2))
                        {
                            var rect = new Rect(x, y, x2, y2);
                            var score = CalcRectScore(rect);
                            if (score > maxScore)
                            {
                                maxScore = score;
                                target = rect;
                            }
                        }
                    }
                }
            }
        }
    }

    private static int GetMyCellsAround(Vector cell)
    {
        return deltas.Count(c => !IsValid(cell.X + c.X, cell.Y + c.Y) || cells[cell.X + c.X, cell.Y + c.Y] == 0);
    }

    private static void LoadState(int enemyCount)
    {
        int gameRound = int.Parse(Console.ReadLine());
        var inputs = Console.ReadLine().Split(' ');
        pos[0].X = int.Parse(inputs[0]); // Your x position
        pos[0].Y = int.Parse(inputs[1]); // Your y position
        int backInTimeLeft = int.Parse(inputs[2]); // Remaining back in time
        for (int i = 0; i < enemyCount; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            pos[i + 1].X = int.Parse(inputs[0]); // X position of the opponent
            pos[i + 1].Y = int.Parse(inputs[1]); // Y position of the opponent
            int opponentBackInTimeLeft = int.Parse(inputs[2]); // Remaining back in time of the opponent
        }

        for (int y = 0; y < 20; y++)
        {
            // One line of the map ('.' = free, '0' = you, otherwise the id of the opponent)
            var cols = Console.ReadLine().Select(x => x == '.' ? -1 : x - '0').ToArray();
            for (int x = 0; x < 35; x++)
            {
                cells[x, y] = cols[x];
            }
        }
    }

    public static IEnumerable<Cell> Flatten()
    {
        for (int y = 0; y < 20; y++)
        {
            for (int x = 0; x < 35; x++)
            {
                yield return new Cell { Pos = new Vector { X = x, Y = y }, Value = cells[x, y] };
            }
        }
    }

    static void UpdateVoronoi()
    {
        foreach (var c in Flatten().Where(x => x.Value < 0))
        {
            var n = pos
                .Select((p, i) => new { Index = i, Dist = Math.Abs(c.Pos.X - p.X) + Math.Abs(c.Pos.Y - p.Y) })
                .OrderBy(a => a.Dist).First();

            voronoi[c.Pos.X, c.Pos.Y] = n.Index;
        }
    }

    static bool IsValid(int x, int y)
    {
        if (x < 0 || x > 34 || y < 0 || y > 19) return false;
        return true;
    }
}