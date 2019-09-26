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
    static Vector closestVoronoi = new Vector();
    static Vector closestEmpty = new Vector();

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

        public bool Valid;

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

        public Rect()
        {
            Valid = false;
        }

        public Rect(int x1, int y1, int x2, int y2)
        {
            Valid = true;

            X1 = Math.Min(x1, x2);
            X2 = Math.Max(x1, x2);
            Y1 = Math.Min(y1, y2);
            Y2 = Math.Max(y1, y2);
        }

        public void Update()
        {
            dx = Math.Abs(X2 - X1);
            dy = Math.Abs(Y2 - Y1);

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
            if (!Valid) return "invalid";

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

    static Rect target = new Rect();
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
            timer.Restart();
            ticks = 0;

            LoadState(enemyCount);

            UpdateVoronoi();

            if (target.Valid)
            {
                target.Update();
                if (target.EnemyCells > 0 || target.MyCells >= target.Area) target.Valid = false;
            }

            if (!target.Valid)
            {
                FindNewTarget();
            }

            D(target);
            D("ticks", ticks, timer.ElapsedMilliseconds);

            Vector t = null;

            if (target.Valid)
            {
                t = target
                .GetPerimeter()
                .Where(x => cells[x.X, x.Y] < 0)
                .OrderBy(x => x.GetDistance(pos[0]))
                .FirstOrDefault();
            }

            if (t == null)
            {
                if (closestVoronoi.X >= 0) t = closestVoronoi;
                else if (closestEmpty.X >= 0) t = closestEmpty;
                else t = pos[0];
            }

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
            - (rect.MyCells / area) * 0.5
            - (rect.EnemyCells > 0 ? 1000000 : 0)
            - (rect.MyCells >= area ? 1000000 : 0)
            - rect.Distance * 0.5
            ;
    }

    private static void FindNewTarget()
    {
        var maxScore = double.MinValue;
        var tempRect = new Rect(0, 0, 0, 0);
        target.Valid = false;

        for (int y1 = 0; y1 < 18; y1++)
        {
            for (int x1 = 0; x1 < 33; x1++)
            {
                for (var y2 = Math.Min(19, y1 + 10); y2 > y1; y2--)
                {
                    for (var x2 = Math.Min(34, x1 + 20); x2 >= x1; x2--)
                    {
                        if (timer.ElapsedMilliseconds > 95) return;

                        ticks++;

                        tempRect.X1 = x1;
                        tempRect.X2 = x2;
                        tempRect.Y1 = y1;
                        tempRect.Y2 = y2;
                        tempRect.Update();

                        var score = CalcRectScore(tempRect);

                        if (score > maxScore)
                        {
                            maxScore = score;
                            target.Valid = true;
                            target.X1 = x1;
                            target.X2 = x2;
                            target.Y1 = y1;
                            target.Y2 = y2;
                        };
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

    static void UpdateVoronoi()
    {
        var closestVoronoiDist = int.MaxValue;
        var closestDist = int.MaxValue;
        closestVoronoi.X = -1;
        closestVoronoi.Y = -1;
        closestEmpty.X = -1;
        closestEmpty.Y = -1;

        for (var y = 0; y < 20; y++)
        {
            for (var x = 0; x < 35; x++)
            {
                if (cells[x, y] >= 0) continue;

                var index = -1;
                var minDist = int.MaxValue;

                for (var i = 0; i < pos.Length; i++)
                {
                    var dist = Math.Abs(x - pos[i].X) + Math.Abs(y - pos[i].Y);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        index = i;
                    }

                    if (i == 0 && dist < closestDist)
                    {
                        closestEmpty.X = x;
                        closestEmpty.Y = y;
                    }
                }

                voronoi[x, y] = index;

                if (index == 0 && minDist < closestVoronoiDist)
                {
                    closestVoronoi.X = x;
                    closestVoronoi.Y = y;
                }
            }
        }
    }

    static bool IsValid(int x, int y)
    {
        if (x < 0 || x > 34 || y < 0 || y > 19) return false;
        return true;
    }
}