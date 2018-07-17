using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Collections;

public class AStar
{
    Dictionary<Cell, PathItem> closedList = new Dictionary<Cell, PathItem>();
    List<PathItem> openList = new List<PathItem>();
    List<PathItem> path = new List<PathItem>();

    Playfield playfield;
    Cell from;
    Cell target;
    int dirOffset;
    private Func<Cell, int> weightFunction;

    public AStar(Playfield playfield, Cell from, Cell target, int dirOffset, Func<Cell, int> weightFunction)
    {
        this.playfield = playfield;
        this.from = from;
        this.target = target;
        this.dirOffset = dirOffset;
        this.weightFunction = weightFunction;
    }

    public List<PathItem> Find()
    {
        PathItem item = GetPathItemLinkedList();
        path.Clear();
        if (item != null)
        {
            CalculatePath(item);
        }
        return path;
    }

    void CalculatePath(PathItem item)
    {
        PathItem i = item;
        while (i != null)
        {
            path.Insert(0, i);
            i = i.Precedent;
        }
    }

    PathItem GetPathItemLinkedList()
    {
        var root = new PathItem
        {
            Pos = this.from
        };

        openList.Add(root);

        while (openList.Count > 0)
        {
            var visiting = openList.First();
            openList.Remove(visiting);

            var cell = visiting.Pos;

            if (cell == target)
            {
                return visiting;
            }

            if (closedList.ContainsKey(cell))
            {
                continue;
            }

            closedList[cell] = visiting;

            for (int i = 0; i < Vector2D.MAX_DIRS; i++)
            {
                int index = (i + dirOffset) % Vector2D.MAX_DIRS;
                var nextCell = playfield.GetCell(Vector2D.Add(Vector2D.Directions[index], cell.Pos));
                if (playfield.IsWalkable(nextCell))
                {
                    AddToOpenList(visiting, cell, nextCell);
                }
            }

            openList.Sort((a, b) => a.TotalPrevisionalLength - b.TotalPrevisionalLength);
        }
        return null; 
    }

    void AddToOpenList(PathItem visiting, Cell fromCell, Cell toCell)
    {
        if (closedList.ContainsKey(toCell))
        {
            return;
        }

        PathItem pi = new PathItem();
        pi.Pos = toCell;
        pi.CumulativeLength = visiting.CumulativeLength + weightFunction(toCell);
        int manh = fromCell.Pos.ManhattanDistance(toCell.Pos);
        pi.TotalPrevisionalLength = pi.CumulativeLength + manh;
        pi.Precedent = visiting;

        openList.Add(pi);
    }
}

public enum CellType
{
    //NONE,
    EMPTY,
    WALL,
    SHELTER
}

public class Cell
{
    public static readonly Cell INVALID_CELL = new Cell(-1, -1);

    public readonly Vector2D Pos;
    public CellType CellType;

    public Cell(int x, int y, CellType type) :this(x,y)
    {
        this.CellType = type;
    }

    public Cell(int x, int y)
    {
        this.Pos = new Vector2D(x, y);
    }

    public override string ToString()
    {
        return $"{Pos} {CellType}";
    }
}

public class PathFinder
{
    Cell from = null;
    Cell to = null;
    int directionOffest = 0;
    private Func<Cell, int> WeightFunction = (x) => 1;
    private Playfield map;

    public PathFinder(Playfield map)
    {
        this.map = map;
    }

    public PathFinder From(Vector2D p)
    {
        return From(map.GetCell(p));
    }

    public PathFinder From(Cell cell)
    {
        from = cell;
        return this;
    }

    public PathFinder To(Vector2D p)
    {
        return To(map.GetCell(p));
    }

    public PathFinder To(Cell cell)
    {
        to = cell;
        return this;
    }

    public PathFinder WithOffset(int offset)
    {
        this.directionOffest = offset;
        return this;
    }

    public PathFinder WithWeightFunction(Func<Cell, int> weightFunction)
    {
        this.WeightFunction = weightFunction;
        return this;
    }

    public PathFinderResult FindPath()
    {
        if (from == null || to == null)
        {
            return new PathFinderResult();
        }

        //D(map);
        //D("from:", from);
        //D("to:", to);

        var pathItems = new AStar(map, from, to, directionOffest, WeightFunction).Find();

        if (!pathItems.Any())
        {
            D("NO_PATH");
            return PathFinderResult.NO_PATH;
        }

        var p = new PathFinderResult
        {
            Path = pathItems.Select(x => x.Pos).ToArray(),
            WeightedLength = pathItems.Last().CumulativeLength
        };

        //D("found path:", p.WeightedLength);

        return p;
    }

    private static void D(params object[] oo)
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
}

public class PathFinderResult
{
    public static readonly PathFinderResult NO_PATH = new PathFinderResult();
    public Cell[] Path;

    public int WeightedLength = -1;

    public bool HasNextCell()
    {
        return Path.Length > 1;
    }

    public Cell GetNextCell()
    {
        return Path.First();
    }

    public bool HasNoPath()
    {
        return WeightedLength == -1;
    }
}

public class PathItem
{
    public int CumulativeLength = 0;
    public int TotalPrevisionalLength = 0;
    public PathItem Precedent = null;
    public Cell Pos;

    public int GetLength()
    {
        var i = this;
        var count = 0;
        while (i != null)
        {
            count++;
            i = i.Precedent;
        }
        return count;
    }

    public override string ToString()
    {
        return $"{Pos} {TotalPrevisionalLength}";
    }
}

public class Playfield
{
    public Vector2D Dimentions;
    readonly Cell[,] maze;

    public const char WALL = '#';
    public const char EMPTY = '.';
    public const char WANDERER_SPAWN = 'w';
    public const char OTHER_SPAWN = 's';
    public const char PLAYER_SPAWN = 'S';
    public const char SHELTER = 'U';

    public Playfield(int width, int height)
    {
        Dimentions = new Vector2D(width, height);
        maze = new Cell[width, height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                maze[x, y] = new Cell(x, y);
            }
        }
    }

    public Playfield(Vector2D dimentions) : this(dimentions.X, dimentions.Y)
    {
    }

    public void CopyCellTypesFrom(Playfield field)
    {
        for (var y = 0; y < Dimentions.Y; y++)
        {
            for (var x = 0; x < Dimentions.X; x++)
            {
                maze[x, y].CellType = field[x, y].CellType;
            }
        }
    }

    public void SetCell(int x, int y, char ch)
    {
        var type = GetCellType(ch);
        maze[x, y] = new Cell(x, y, type);
    }

    private CellType GetCellType(char ch)
    {
        switch (ch)
        {
            case WALL:
                return CellType.WALL;
            case EMPTY:
                return CellType.EMPTY;
            case SHELTER:
                return CellType.SHELTER;
            default:
                return CellType.EMPTY;
        }
    }

    public Cell this[Vector2D p]
    {
        get => this[p.X, p.Y];
    }
    public Cell this[int x, int y]
    {
        get => GetCell(x, y);
    }

    public Cell GetCell(int x, int y)
    {
        return IsInBoundaries(x, y) ? maze[x, y] : Cell.INVALID_CELL;
    }

    public Cell GetCell(Vector2D a)
    {
        return GetCell(a.X, a.Y);
    }

    public bool IsWalkable(Vector2D a)
    {
        var w = IsWalkable(GetCell(a));
        //D(a, w);
        return w;
    }

    private static void D(params object[] oo)
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

    public bool IsWalkable(Cell cell)
    {
        return cell.CellType != CellType.WALL && IsInBoundaries(cell);
    }

    public bool IsInBoundaries(Cell cell)
    {
        return IsInBoundaries(cell.Pos.X, cell.Pos.Y);
    }

    public bool IsInBoundaries(int x, int y)
    {
        return !(x < 0 || x >= Dimentions.X || y < 0 || y >= Dimentions.Y);
    }

    public override string ToString()
    {
        var o = new StringBuilder();

        for (var y = 0; y < Dimentions.Y; y++)
        {
            for (var x = 0; x < Dimentions.X; x++)
            {
                o.Append(GetCellChar(maze[x, y].CellType));
            }
            o.Append(Environment.NewLine);
        }
        return o.ToString();
    }

    private char GetCellChar(CellType c)
    {
        switch (c)
        {
            case CellType.WALL:
                return Playfield.WALL;
            case CellType.EMPTY:
                return Playfield.EMPTY;
            case CellType.SHELTER:
                return Playfield.EMPTY;
            default:
                throw new ArgumentException(c.ToString());
        }
    }
}

public class PlayfieldFactory
{
    public static Playfield Create(TextReader reader)
    {
        var width = int.Parse(reader.ReadLine());
        var height = int.Parse(reader.ReadLine());
        var map = new Playfield(width, height);
        for (int y = 0; y < height; y++)
        {
            string line = reader.ReadLine().Trim();
            var x = 0;
            foreach (var ch in line)
            {
                map.SetCell(x++, y, ch);
            }
        }

        return map;
    }
}

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
        D("bad:", p, badRank);

        badRank -= 100 *
            (units.Count(x => x.UnitType == WANDERER && x.Pos.ManhattanDistance(p) == 1) > 1 ? 1 : 0);
        D("bad:", p, badRank);

        badRank += 2 * units
            .Skip(1)
            .Where(x => x.UnitType == EXPLORER)
            .MinDistance(p);
        D("bad:", p, badRank);

        badRank -= 50 * units
            .Skip(1)
            .Where(x => x.UnitType == EXPLORER && x.Pos.ManhattanDistance(p) < 3)
            .Count();
        D("bad:", p, badRank);

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
class Unit
{
    public Vector2D Pos = new Vector2D();
    public string UnitType;
    public int Id;
    public int Param0;
    public int Param1;
    public int Param2;
    public int Distance;

    public int Sanity => Param0;
}

public class Vector2D : IEquatable<Vector2D>
{
    public static readonly Vector2D UP = new Vector2D(0, -1);
    public static readonly Vector2D RIGHT = new Vector2D(1, 0);
    public static readonly Vector2D DOWN = new Vector2D(0, 1);
    public static readonly Vector2D LEFT = new Vector2D(-1, 0);

    public const int MAX_DIRS = 4;
    public static readonly Vector2D[] Directions = new[] { Vector2D.UP, Vector2D.RIGHT, Vector2D.DOWN, Vector2D.LEFT };
    public static readonly Vector2D[] DirectionsAndMe = new[] { new Vector2D(), Vector2D.UP, Vector2D.RIGHT, Vector2D.DOWN, Vector2D.LEFT };

    public int X;
    public int Y;

    public Vector2D() : this(0)
    {
    }

    public Vector2D(int x) : this(x, x)
    {
    }

    public Vector2D(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public Vector2D(Vector2D vect)
    {
        this.X = vect.X;
        this.Y = vect.Y;
    }

    public bool Equals(Vector2D a)
    {
        return this.X == a.X && this.Y == a.Y;
    }

    public bool IsNull()
    {
        return (this.X | this.Y) == 0;
    }

    public Vector2D Set(int x, int y)
    {
        this.X = x;
        this.Y = y;
        return this;
    }

    public Vector2D Set(Vector2D a)
    {
        this.X = a.X;
        this.Y = a.Y;
        return this;
    }

    public Vector2D Add(Vector2D a)
    {
        this.X += a.X;
        this.Y += a.Y;
        return this;
    }

    public Vector2D Sub(Vector2D a)
    {
        this.X -= a.X;
        this.Y -= a.Y;
        return this;
    }

    public Vector2D Mult(int a)
    {
        this.X *= a;
        this.Y *= a;
        return this;
    }

    public Vector2D Div(int a)
    {
        this.X /= a;
        this.Y /= a;
        return this;
    }

    public Vector2D Negate()
    {
        this.X = -this.X;
        this.Y = -this.Y;
        return this;
    }

    public Vector2D Normalize()
    {
        if (IsNull())
            return this;

        int absx = Math.Abs(this.X);
        int absy = Math.Abs(this.Y);
        if (absx > absy)
        {
            this.X /= absx;
            this.Y = 0;
        }
        else if (absx < absy)
        {
            this.X = 0;
            this.Y /= absy;
        }
        else
        {
            this.X /= absx;
            this.Y /= absy;
        }
        return this;
    }

    public int ManhattanDistance()
    {
        return Math.Abs(X) + Math.Abs(Y);
    }

    public int ManhattanDistance(Vector2D a)
    {
        return Math.Abs(this.X - a.X) + Math.Abs(this.Y - a.Y);
    }

    public int TchebychevDistance()
    {
        return Math.Max(X, Y);
    }

    public int TchebychevDistance(Vector2D a)
    {
        return Math.Max(Math.Abs(this.X - a.X), Math.Abs(this.Y - a.Y));
    }

    public double EuclidianDistance2()
    {
        return X * X + Y * Y;
    }

    public double EuclidianDistance2(Vector2D a)
    {
        return Math.Pow(this.X - a.X, 2) + Math.Pow(this.Y - a.Y, 2);
    }

    public double EuclidianDistance()
    {
        return Math.Sqrt(EuclidianDistance());
    }

    public double EuclidianDistance(Vector2D a)
    {
        return Math.Sqrt(EuclidianDistance2(a));
    }

    public static Vector2D Add(Vector2D a, Vector2D b)
    {
        return new Vector2D(a).Add(b);
    }

    public static Vector2D Sub(Vector2D a, Vector2D b)
    {
        return new Vector2D(a).Sub(b);
    }

    public static Vector2D Mult(Vector2D a, int b)
    {
        return new Vector2D(a).Mult(b);
    }

    public static Vector2D Div(Vector2D a, int b)
    {
        return new Vector2D(a).Div(b);
    }

    public bool IsInlineToAny(IEnumerable<Vector2D> vectors)
    {
        return vectors.Any(x => this.IsInlineTo(x));
    }

    public bool IsInlineTo(Vector2D v)
    {
        return v.X == this.X || v.Y == this.Y;
    }

    public override String ToString()
    {
        return "[" + X + ":" + Y + "]";
    }
}
