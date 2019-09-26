using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Threading;

class GameState
{
    public bool MeAlive = true;
    public int BoxesExploded = 0;
    public int Width;
    public int Height;
    public int MyId;
    public List<Box> Boxes;
    public List<Entity> Entities = new List<Entity>();
    public List<Action> Actions = new List<Action>();

    public Entity Me() => Entities.First(x => x.EntityType == EntityType.Player && x.OwnerId == MyId);
    public IEnumerable<Entity> Bombs() => Entities.Where(x => x.EntityType == EntityType.Bomb);

    public int CalcScore()
    {
        if (!MeAlive) return int.MinValue;

        var me = Me();

        return 10 * BoxesExploded
        //+ 2 * Entities.Count(x => x.EntityType == EntityType.Item && x.ItemType == ItemType.ExtraBomb)
        //+ me.ExplosionRange
        + me.Bombs
        ;
    }

    public void ApplyAction(Action action)
    {
        foreach (var b in Bombs().ToArray())
        {
            b.TurnsTillExplosion--;
            if (b.TurnsTillExplosion == 0)
            {
                ExpolodeBomb(b);
            }
        }

        var me = Me();
        if (action.ActionType == ActionType.Bomb)
        {
            if (me.Bombs < 1) throw new ArgumentException("no bombs available");

            me.Bombs--;
            Entities.Add(CreateBomb(me));
        }

        me.Point.X = action.Point.X;
        me.Point.Y = action.Point.Y;

        var e = Entities.FirstOrDefault(x => x.EntityType == EntityType.Item && x.Point.Equals(me.Point));
        if (e != null)
        {
            if (e.ItemType == ItemType.ExtraBomb) me.Bombs++;
            if (e.ItemType == ItemType.ExtraRange) me.ExplosionRange++;
            Entities.Remove(e);
        }
    }

    public void ExpolodeBomb(Entity bomb)
    {
        Entities.Remove(bomb);

        var me = Me();
        if (GetDeathRange(bomb).Any(x => x.Equals(me.Point)))
        {
            MeAlive = false;
            return;
        }

        var deathRange = GetDeathRange(bomb).ToArray();

        foreach (var box in Boxes.ToArray())
        {
            if (deathRange.Any(x => x.Equals(box.Point)))
            {
                if (bomb.OwnerId == MyId) BoxesExploded++;
                if (box.ItemType != ItemType.None)
                {
                    Entities.Add(new Entity
                    {
                        EntityType = EntityType.Item,
                        Point = box.Point,
                        ItemType = box.ItemType
                    });
                }
                Boxes.Remove(box);
            }
        }

        Entities.RemoveAll(e => deathRange.Any(b => b.Equals(e.Point)));

        foreach (var b in Bombs())
        {
            if (deathRange.Contains(b.Point))
            {
                ExpolodeBomb(b);
            }
        }
    }

    private static IEnumerable<Vector> GetDeathRange(Entity bomb)
    {
        for (var i = 0 - bomb.ExplosionRange + 1; i < bomb.ExplosionRange; i++)
        {
            yield return new Vector { X = bomb.Point.X + i, Y = bomb.Point.Y };

            if (i != 0)
            {
                yield return new Vector { X = bomb.Point.X, Y = bomb.Point.Y + i };
            }
        }
    }

    internal static Entity CreateBomb(Entity me)
    {
        return new Entity
        {
            EntityType = EntityType.Bomb,
            OwnerId = me.OwnerId,
            Point = me.Point,
            TurnsTillExplosion = 8,
            ExplosionRange = me.ExplosionRange
        };
    }

    public IEnumerable<Action> GetValidActions()
    {
        var me = Me();
        var cells = GetValidPointsAround(me.Point).Where(IsFree).Concat(new[] { me.Point });
        foreach (var c in cells)
        {
            if (me.Bombs > 0) yield return Action.Bomb(c);
            yield return Action.Move(c);
        }
    }

    private bool IsFree(Vector x)
    {
        return IsValid(x) && !IsBoxHere(x) && FindBombHere(x) == null;
    }

    bool IsBoxHere(Vector p) => Boxes.Any(b => p.Equals(b.Point));

    private Entity FindBombHere(Vector p)
    {
        return Entities.FirstOrDefault(b => b.EntityType == EntityType.Bomb && b.Point.Equals(p));
    }

    public void ReadInitData()
    {
        var inputs = Console.ReadLine().Split(' ');
        Width = int.Parse(inputs[0]);
        Height = int.Parse(inputs[1]);
        MyId = int.Parse(inputs[2]);
        Boxes = new List<Box>(Width * Height);
    }

    public void ReadStep()
    {
        ReadBoxes();
        ReadEntities();
    }

    void ReadBoxes()
    {
        Boxes.Clear();
        for (int i = 0; i < Height; i++)
        {
            var row = Console.ReadLine();
            for (int k = 0; k < row.Length; k++)
            {
                if (row[k] != '.')
                {
                    var itemType = ItemType.Wall;
                    if (row[k] != 'X')
                    {
                        itemType = (ItemType)(row[k] - '0');
                    }

                    Boxes.Add(new Box
                    {
                        ItemType = itemType,
                        Point = new Vector { X = k, Y = i }
                    });
                }
            }
        }
    }

    private void ReadEntities()
    {
        int count = int.Parse(Console.ReadLine());
        Entities.Clear();
        for (int i = 0; i < count; i++)
        {
            var entry = Entity.Read(Console.In);
            Entities.Add(entry);
        }
    }

    public IEnumerable<Vector> GetValidPointsAround(Vector p)
    {
        return p.GetAroundPoints().Where(IsValid);
    }

    public bool IsValid(Vector p)
    {
        return p.X >= 0 && p.X < Width && p.Y >= 0 && p.Y < Height;
    }

    public GameState Clone()
    {
        return new GameState
        {
            MeAlive = this.MeAlive,
            BoxesExploded = this.BoxesExploded,
            Width = this.Width,
            Height = this.Height,
            MyId = this.MyId,

            Boxes = new List<Box>(this.Boxes.Select(x => x.Clone())),
            Entities = new List<Entity>(this.Entities.Select(x => x.Clone())),
            Actions = new List<Action>(this.Actions)
        };
    }

    internal int CalcHash()
    {
        return (MeAlive.ToString()
            + BoxesExploded.ToString()
            + string.Join("", Boxes.Select(x => x.Point.ToString()))
            + string.Join("", Entities.Where(x => (x.EntityType != EntityType.Player || x.OwnerId == MyId) && x.param1 != 99).Select(x => x.Point.ToString())))
            .GetHashCode();
    }
}

class Action
{
    internal ActionType ActionType;
    internal Vector Point;

    internal static Action Bomb(Vector c)
    {
        return new Action
        {
            ActionType = ActionType.Bomb,
            Point = c
        };
    }

    internal static Action Move(Vector c)
    {
        return new Action
        {
            ActionType = ActionType.Move,
            Point = c
        };
    }

    public override string ToString()
    {
        var action = ActionType == ActionType.Bomb ? "BOMB" : "MOVE";
        return $"{action} {Point.X} {Point.Y}";
    }
}

enum ActionType
{
    Bomb,
    Move
}

class Box
{
    public Vector Point;
    public ItemType ItemType;

    internal Box Clone()
    {
        return new Box
        {
            Point = this.Point,
            ItemType = this.ItemType,
        };
    }
}

class Entity
{
    public EntityType EntityType;
    public int OwnerId;
    public Vector Point;
    public int param1;
    int param2;

    public int Bombs { get { return param1; } set { param1 = value; } }
    public int TurnsTillExplosion { get { return param1; } set { param1 = value; } }
    public int ExplosionRange { get { return param2; } set { param2 = value; } }

    public ItemType ItemType
    {
        get
        {
            return (ItemType)param1;
        }

        set
        {
            param1 = (int)value;
        }
    }

    public static Entity Read(TextReader reader)
    {
        var inputs = reader.ReadLine().Split(' ');

        return new Entity
        {
            EntityType = (EntityType)int.Parse(inputs[0]),
            OwnerId = int.Parse(inputs[1]),
            Point = new Vector
            {
                X = int.Parse(inputs[2]),
                Y = int.Parse(inputs[3])
            },
            param1 = int.Parse(inputs[4]),
            param2 = int.Parse(inputs[5])
        };
    }

    internal Entity Clone()
    {
        return new Entity
        {
            EntityType = this.EntityType,
            OwnerId = this.OwnerId,
            Point = this.Point,
            param1 = this.param1,
            param2 = this.param2
        };
    }
}

struct Vector
{
    public Vector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        var p = (Vector)obj;
        return p.X == X && p.Y == Y;
    }

    public override int GetHashCode()
    {
        return (Y << 8) + X;
    }

    public override string ToString()
    {
        return $"{X} {Y}";
    }

    public int X;
    public int Y;

    public Vector Up => new Vector { X = this.X, Y = this.Y - 1 };
    public Vector Down => new Vector { X = this.X, Y = this.Y + 1 };
    public Vector Left => new Vector { X = this.X - 1, Y = this.Y };
    public Vector Right => new Vector { X = this.X + 1, Y = this.Y };

    public IEnumerable<Vector> GetAroundPoints()
    {
        yield return Right;
        yield return Left;
        yield return Up;
        yield return Down;
    }

    internal int CalcDistanceTo(Vector p)
    {
        var dx = p.X - X;
        var dy = p.Y - Y;
        return (int)Math.Sqrt(dx * dx + dy * dy);
    }
}

enum EntityType
{
    Player = 0,
    Bomb = 1,
    Item = 2
}

enum ItemType
{
    Empty = 0,
    ExtraRange = 1,
    ExtraBomb = 2,
    Wall = 99,
    None = 999
}

class Player
{
    static List<Action> bestActions;
    static int bestScore;
    static int iterations;
    static Stopwatch timer = new Stopwatch();
    static HashSet<int> gameHashes = new HashSet<int>();
    static void Main(string[] args)
    {
        var g = new GameState();
        g.ReadInitData();
        while (true)
        {
            gameHashes.Clear();

            g.ReadStep();
            bestScore = int.MinValue;
            iterations = 0;
            timer.Restart();
            try
            {
                Search(g, 6);
            }
            catch (TimeoutException)
            {
                D("timeout");
            }

            D(iterations, " ", bestScore, timer.ElapsedMilliseconds, "ms");
            Console.WriteLine(bestActions.First());
        }
    }

    static void Search(GameState g, int levels)
    {
        iterations++;
        Thread.CurrentThread.CurrentCulture = new CultureInfo("en-GB");
        //var h = g.CalcHash();
        //if (gameHashes.Contains(h)) return;

        if (timer.ElapsedMilliseconds > 99) throw new TimeoutException();
        if (levels == 0)
        {
            var score = g.CalcScore();
            if (score > bestScore)
            {
                D("exploded:", g.BoxesExploded, "score: ", score, "actions: ", string.Join(" ", g.Actions));
                bestScore = score;
                bestActions = g.Actions;
            }
            return;
        }

        foreach (var action in g.GetValidActions())
        {
            var gnext = g.Clone();
            gnext.ApplyAction(action);
            gnext.Actions.Add(action);
            Search(gnext, levels - 1);
        }
    }

    public static void D(params object[] args)
    {
        Console.Error.WriteLine(string.Join(" ", args.Select(x => x.ToString())));
    }
}