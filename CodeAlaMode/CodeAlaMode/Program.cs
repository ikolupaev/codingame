using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class Game
{
    public Game(Game g)
    {
        Players = new[]
        {
            new Player( g.Players[0] ),
            new Player( g.Players[1] )
        };

        Tables = g.Tables.Select(x => new Table(x)).ToList();
        OvenContents = g.OvenContents;
        OvenTimer = g.OvenTimer;
        Orders = g.Orders.Select(x => new Order(x.Item, x.Award)).ToArray();
        CurrentPlayerIndex = g.CurrentPlayerIndex;
        Score = g.Score;
    }

    public Game()
    {
        Players = new Player[2];
        Tables = new List<Table>(7 * 11);
        Orders = new Order[3];
    }

    public Item OvenContents;
    public int OvenTimer;
    public int CurrentPlayerIndex;
    public int Score;

    public List<Table> Tables;
    public Order[] Orders;
    public Player[] Players;

    public Player Me => Players[0];

    public static int[][] TablesMap = new[]
    {
        new[] {1,1,1,1,1,1,1,1,1,1,1},
        new[] {1,0,0,0,0,0,0,0,0,0,1},
        new[] {1,0,1,1,1,1,0,1,1,0,1},
        new[] {1,0,1,0,0,1,0,0,1,0,1},
        new[] {1,0,1,1,0,1,1,1,1,0,1},
        new[] {1,0,0,0,0,0,0,0,0,0,1},
        new[] {1,1,1,1,1,1,1,1,1,1,1}
    };

    public Game(string base64)
    {
        using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
        {
            using (var reader = new BinaryReader(stream))
            {
                OvenContents = (Item)reader.ReadInt32();
                OvenTimer = reader.ReadInt32();
                CurrentPlayerIndex = reader.ReadInt32();
                Score = reader.ReadInt32();

                var tablesCount = reader.ReadInt32();
                Tables = new List<Table>(tablesCount);
                for (int i = 0; i < tablesCount; i++)
                {
                    Tables.Add(Table.Load(reader));
                }

                var ordersCount = reader.ReadInt32();
                Orders = new Order[ordersCount];
                for (int i = 0; i < ordersCount; i++)
                {
                    Orders[i] = Order.Load(reader);
                }

                Players = new Player[2];
                Players[0] = Player.Load(reader);
                Players[1] = Player.Load(reader);
            }
        }
    }

    public override string ToString()
    {
        using (var stream = new MemoryStream())
        {
            using (var writer = new BinaryWriter(stream))
            {
                writer.Write((int)OvenContents);
                writer.Write(OvenTimer);
                writer.Write(CurrentPlayerIndex);
                writer.Write(Score);

                writer.Write(Tables.Count);
                foreach (var t in Tables)
                {
                    t.Serialize(writer);
                }

                writer.Write(Orders.Length);
                foreach (var o in Orders)
                {
                    o.Serialize(writer);
                }

                Players[0].Serialize(writer);
                Players[1].Serialize(writer);
            }

            return Convert.ToBase64String(stream.ToArray());
        }
    }
}

public class Table
{
    public Table(Table t)
    {
        Position = new Position(t.Position);
        TableFunction = t.TableFunction;
        Item = t.Item;
    }

    public Table()
    {
    }

    public Position Position;
    public TableFunction TableFunction;
    public Item Item;

    internal void Serialize(BinaryWriter writer)
    {
        Position.Serialize(writer);
        writer.Write((((int)TableFunction) << 16) | (int)Item);
    }

    internal static Table Load(BinaryReader reader)
    {
        var pos = Position.Load(reader);
        var p = reader.ReadInt32();

        return new Table
        {
            Position = pos,
            TableFunction = (TableFunction)(p >> 16),
            Item = (Item)(p & 0xffff)
        };
    }
}

public class Order
{
    public Order(Item item, int award)
    {
        Item = item;
        Award = award;
    }

    public Item Item;
    public int Award;

    internal void Serialize(BinaryWriter writer)
    {
        writer.Write((int)Item);
        writer.Write(Award);
    }

    internal static Order Load(BinaryReader reader)
    {
        var item = (Item)reader.ReadInt32();
        var award = reader.ReadInt32();

        return new Order(item, award);
    }
}

[Flags]
public enum Item
{
    NONE = 0,
    BLUEBERRIES = 1,
    ICE_CREAM = 2,
    CHOPPED_STRAWBERRIES = 4,
    CROISSANT = 8,
    DOUGH = 16,
    STRAWBERRIES = 32,
    DISH = 64,
    RAW_TART = 128,
    TART = 256,
    CHOPPED_DOUGH = 512,

    DESSERT = ICE_CREAM | BLUEBERRIES | CHOPPED_STRAWBERRIES | CROISSANT | TART
}

public enum TableFunction
{
    Dishwasher = 1,
    Window = 2,
    Blueberry = 3,
    Strawberry = 4,
    IceCream = 5,
    Dough = 6,
    ChoppingBoard = 7,
    Oven = 8
}

public class Player
{
    public Position Position;
    public Item Item;

    public Player(Player p) : this(new Position(p.Position), p.Item) { }

    public Player(Position position, Item item)
    {
        Position = position;
        Item = item;
    }

    public void Update(Position position, Item item)
    {
        Position = position;
        Item = item;
    }

    public void Serialize(BinaryWriter w)
    {
        Position.Serialize(w);
        w.Write((Int16)Item);
    }

    internal static Player Load(BinaryReader r)
    {
        var p = Position.Load(r);
        var item = (Item)r.ReadInt16();

        return new Player(p, item);
    }
}

public class Position
{
    public readonly int X, Y;

    public Position(Position p) : this(p.X, p.Y) { }

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public bool Equals(Position p)
    {
        return p.X == X && p.Y == Y;
    }

    public int Manhattan(Position p2) => Math.Abs(X - p2.X) + Math.Abs(Y - p2.Y);

    public override string ToString()
    {
        return X + " " + Y;
    }

    public override int GetHashCode()
    {
        return ((X << 4) | Y);
    }

    public static Position Load(BinaryReader reader)
    {
        var xy = reader.ReadByte();
        return new Position(xy >> 4, xy & 0xf);
    }

    internal void Serialize(BinaryWriter writer)
    {
        writer.Write((byte)((X << 4) | Y));
    }
}

public class Move
{
    public MoveType MoveType;
    public Position Position;

    public override string ToString()
    {
        return (MoveType == MoveType.Move ? "MOVE " : "USE ") + Position;
    }

    public override int GetHashCode()
    {
        return (Position.X << 5) | (Position.Y << 1) | (int)MoveType;
    }

    public override bool Equals(object obj)
    {
        var t = obj as Move;
        if (obj == null) return false;
        return t.GetHashCode() == GetHashCode();
    }
}

public enum MoveType
{
    Move = 0,
    Use = 1
}

public class MainClass
{
    public static bool Debug = true;

    public static Game ReadGame()
    {
        var game = new Game();
        game.Players[0] = new Player(null, Item.NONE);
        game.Players[1] = new Player(null, Item.NONE);

        for (int i = 0; i < 7; i++)
        {
            string kitchenLine = ReadLine();
            for (var x = 0; x < kitchenLine.Length; x++)
            {
                Table t = null;

                if (kitchenLine[x] == 'W') t = new Table { Position = new Position(x, i), TableFunction = TableFunction.Window };
                else if (kitchenLine[x] == 'D') t = new Table { Position = new Position(x, i), TableFunction = TableFunction.Dishwasher };
                else if (kitchenLine[x] == 'I') t = new Table { Position = new Position(x, i), TableFunction = TableFunction.IceCream };
                else if (kitchenLine[x] == 'B') t = new Table { Position = new Position(x, i), TableFunction = TableFunction.Blueberry };
                else if (kitchenLine[x] == 'S') t = new Table { Position = new Position(x, i), TableFunction = TableFunction.Strawberry };
                else if (kitchenLine[x] == 'H') t = new Table { Position = new Position(x, i), TableFunction = TableFunction.Dough };
                else if (kitchenLine[x] == 'C') t = new Table { Position = new Position(x, i), TableFunction = TableFunction.ChoppingBoard };
                else if (kitchenLine[x] == 'O') t = new Table { Position = new Position(x, i), TableFunction = TableFunction.Oven };
                else if (kitchenLine[x] == '#') t = new Table { Position = new Position(x, i) };

                if (t != null) game.Tables.Add(t);
            }
        }

        return game;
    }

    //private static void Move(Position p, string message) =>
    //    Console.WriteLine("MOVE " + p + ";" + message);

    //private static void Use(Position p, string message) =>
    //    Console.WriteLine("USE " + p + ";" + message);

    private static string ReadLine()
    {
        var s = Console.ReadLine();
        if (Debug) Console.Error.WriteLine(s);
        return s;
    }

    static void Main()
    {
        var state = "AAAAAAAAAAAAAAAAAAAAAC8AAAAAAAAAABAAAAMAIAAAAAAwAAAAAEAAAAAAUAAAAQBgAAAAAHAAAAAAgAAAAACQAAAAAKAAAAAAAQAAAAChAAAFAAIAAAgAIgAAAAAyAAAAAEIAAAAAUgAAAAByAAAAAIIAAAAAogAAAAADAAAHACMAAAAAUwAAAACDAAAAAKMAAAAABAAAAAAkAAAAADQAAAAAVAAAAABkAAAAAHQAAAAAhAAABgCkAAAAAAUAAAAApQAAAAAGAAAAABYAAAAAJgAAAAA2AAAEAEYAAAAAVgAAAgBmAAAAAHYAAAAAhgAAAACWAAAAAKYAAAAAAwAAAEUAAABSAwAARwEAAAIIAABKAAAAGgQAABEAAJUAAA==";
        var g = new Game(state);

        var moves = new HashSet<Move>();
        GetAvailableMoves(g, moves, 4);

        var m = GetBestMove(g);

        string[] inputs;

        int numAllCustomers = int.Parse(ReadLine());
        for (int i = 0; i < numAllCustomers; i++)
        {
            inputs = ReadLine().Split(' ');
            string customerItem = inputs[0]; // the food the customer is waiting for
            int customerAward = int.Parse(inputs[1]); // the number of points awarded for delivering the food
        }

        var game = ReadGame();

        while (true)
        {
            int turnsRemaining = int.Parse(ReadLine());

            inputs = ReadLine().Split(' ');
            game.Players[0].Update(new Position(int.Parse(inputs[0]), int.Parse(inputs[1])), CreateItem(inputs[2]));
            inputs = ReadLine().Split(' ');
            game.Players[1].Update(new Position(int.Parse(inputs[0]), int.Parse(inputs[1])), CreateItem(inputs[2]));
            var me = game.Players[0];

            foreach (var t in game.Tables)
            {
                t.Item = Item.NONE;
            }

            int numTablesWithItems = int.Parse(ReadLine()); // the number of tables in the kitchen that currently hold an item
            for (int i = 0; i < numTablesWithItems; i++)
            {
                inputs = ReadLine().Split();
                var x = int.Parse(inputs[0]);
                var y = int.Parse(inputs[1]);
                var table = game.Tables.First(t => t.Position.X == x && t.Position.Y == y);
                table.Item = CreateItem(inputs[2]);
            }

            inputs = ReadLine().Split(' ');
            game.OvenContents = CreateItem(inputs[0]); // ignore until bronze league
            game.OvenTimer = int.Parse(inputs[1]);
            int numCustomers = int.Parse(ReadLine()); // the number of customers currently waiting for food

            for (int i = 0; i < numCustomers; i++)
            {
                inputs = ReadLine().Split(' ');
                game.Orders[i] = new Order(CreateItem(inputs[0]), int.Parse(inputs[1]));
            }

            D(game.ToString());

            var move = GetBestMove(game);

            Console.WriteLine(move.ToString());
        }
    }


    private static Move GetBestMove(Game game)
    {
        Move maxMove = null;
        var maxScore = int.MinValue;

        var moves = new HashSet<Move>();
        GetAvailableMoves(game, moves, 4);
        foreach (var move in moves)
        {
            var score = CalcScore(game, move, 5);
            if (maxScore < score)
            {
                maxMove = move;
                maxScore = score;
            }
        }

        return maxMove;
    }

    private static int CalcScore(Game game, Move move, int level)
    {
        if (level == 0)
        {
            return CalcScore(game);
        }

        var g = new Game(game);
        var valid = g.ApplyMove(move);
        if (!valid) return int.MinValue;

        g.CurrentPlayerIndex = 1 - g.CurrentPlayerIndex;

        var maxScore = int.MinValue;
        var moves = new HashSet<Move>();
        GetAvailableMoves(game, moves, 4);
        foreach (var nextMove in moves)
        {
            maxScore = Math.Max(maxScore, CalcScore(game, nextMove, level - 1));
        }
        return maxScore;
    }

    private static int CalcScore(Game game)
    {
        return game.Score;
    }

    private static void GetAvailableMoves(Game game, HashSet<Move> moves, int level)
    {
        if (level == 0) return;

        if (moves.Count == 0) moves.Add(Move(game.Players[game.CurrentPlayerIndex].Position));

        foreach (var p in game.Players[game.CurrentPlayerIndex].Position.GetAdjustentPositions())
        {
            if (game.Players[1 - game.CurrentPlayerIndex].Position.Equals(p)) continue;

            if (Game.TablesMap[p.Y][p.X] == 0)
            {
                var move = Move(p);
                if (moves.Add(move))
                {
                    var g = new Game(game);
                    g.Players[game.CurrentPlayerIndex].Position = p;
                    GetAvailableMoves(g, moves, level - 1);
                }
            }
            else
            {
                moves.Add(Use(p));
            }
        }
    }

    private static Move Use(Position p)
    {
        return new Move { MoveType = MoveType.Use, Position = p };
    }

    private static Move Move(Position p)
    {
        return new Move { MoveType = MoveType.Move, Position = p };
    }

    private static Item CreateItem(string content)
    {
        var item = Item.NONE;

        foreach (var s in content.Split('-'))
        {
            if (s.Equals("BLUEBERRIES")) item |= Item.BLUEBERRIES;
            if (s.Equals("ICE_CREAM")) item |= Item.ICE_CREAM;
            if (s.Equals("CHOPPED_STRAWBERRIES")) item |= Item.CHOPPED_STRAWBERRIES;
            if (s.Equals("CROISSANT")) item |= Item.CROISSANT;
            if (s.Equals("DOUGH")) item |= Item.DOUGH;
            if (s.Equals("STRAWBERRIES")) item |= Item.STRAWBERRIES;
            if (s.Equals("DISH")) item |= Item.DISH;
            if (s.Equals("RAW_TART")) item |= Item.RAW_TART;
            if (s.Equals("TART")) item |= Item.TART;
            if (s.Equals("CHOPPED_DOUGH")) item |= Item.CHOPPED_DOUGH;
        }

        return item;
    }

    static void D(object m)
    {
        Console.Error.WriteLine(m.ToString());
    }
}

public static class ext
{
    public static bool ApplyMove(this Game g, Move p)
    {
        if (p.MoveType == MoveType.Use)
        {
            return ApplyUse(g, p.Position);
        }
        else
        {
            g.Players[g.CurrentPlayerIndex].Position = p.Position;
            return true;
        }
    }

    public static bool ApplyUse(this Game g, Position p)
    {
        var t = g.Tables.First(x => x.Position.Equals(p));
        var player = g.Players[g.CurrentPlayerIndex];

        if (Game.TablesMap[p.Y][p.X] == 0) throw new ArgumentException("p is not table");

        if (player.Item != Item.CHOPPED_DOUGH && (
            t.TableFunction == TableFunction.Blueberry ||
            t.TableFunction == TableFunction.IceCream ||
            t.TableFunction == TableFunction.Strawberry ||
            t.TableFunction == TableFunction.Dough)) return false;

        switch (t.TableFunction)
        {
            case TableFunction.Dishwasher:
                if (player.Item == Item.NONE)
                {
                    player.Item = Item.DISH;
                    return true;
                }

                if ((player.Item ^ Item.DISH) != Item.NONE)
                {
                    player.Item = Item.DISH;
                    return true;
                }

                if (player.Item != Item.NONE && !player.Item.Constains(Item.DISH))
                {
                    player.Item |= Item.DISH;
                    return true;
                }

                return false;

            case TableFunction.Blueberry:
                if (player.Item == Item.NONE)
                {
                    player.Item = Item.BLUEBERRIES;
                    return true;
                }

                if (player.Item == Item.CHOPPED_DOUGH)
                {
                    player.Item = Item.RAW_TART;
                    return true;
                }

                if (player.Item == Item.DISH)
                {
                    player.Item |= Item.BLUEBERRIES;
                    return true;
                }

                return false;

            case TableFunction.Strawberry:
                if (player.Item == Item.NONE)
                {
                    player.Item = Item.STRAWBERRIES;
                    return true;
                }

                return false;

            case TableFunction.IceCream:
                if (player.Item == Item.NONE)
                {
                    player.Item = Item.ICE_CREAM;
                    return true;
                }

                if (player.Item == Item.DISH)
                {
                    player.Item |= Item.ICE_CREAM;
                    return true;
                }

                return false;

            case TableFunction.Dough:
                if (player.Item == Item.NONE)
                {
                    player.Item = Item.DOUGH;
                    return true;
                }

                return false;

            case TableFunction.ChoppingBoard:
                switch (player.Item)
                {
                    case Item.STRAWBERRIES:
                        player.Item = Item.CHOPPED_STRAWBERRIES;
                        return true;
                    case Item.DOUGH:
                        player.Item = Item.CHOPPED_DOUGH;
                        return true;
                    default:
                        return false;
                }

            case TableFunction.Oven:
                if ((player.Item == Item.DOUGH || player.Item == Item.RAW_TART) && g.OvenContents == Item.NONE)
                {
                    g.OvenContents = player.Item;
                    player.Item = Item.NONE;
                    g.OvenTimer = 10;
                    return true;
                }

                if ((g.OvenContents == Item.DOUGH || g.OvenContents == Item.RAW_TART) && (player.Item == Item.NONE || player.Item == Item.DISH))
                {
                    player.Item |= g.OvenContents;
                    g.OvenContents = Item.NONE;
                    g.OvenTimer = 10;
                    return true;
                }

                return false;

            case TableFunction.Window:
                if (g.Orders.Any(x => x.Item == g.Players[g.CurrentPlayerIndex].Item))
                {
                    g.Score += 100;
                }

                player.Item = Item.NONE;
                return true;

            default:
                if (player.Item == Item.NONE || t.Item == Item.NONE)
                {
                    var swap = player.Item;
                    player.Item = t.Item;
                    t.Item = swap;
                    return true;
                }

                if (player.Item == Item.DISH)
                {
                    if ((t.Item & Item.DESSERT) != 0)
                    {
                        player.Item |= t.Item;
                        t.Item = Item.NONE;
                        return true;
                    }
                    return false;
                }

                if (t.Item == Item.DISH)
                {
                    if ((player.Item & Item.DESSERT) != 0 && !player.Item.Constains(Item.DISH))
                    {
                        player.Item |= t.Item;
                        t.Item = Item.NONE;
                        return true;
                    }
                    return false;
                }

                if ((player.Item | t.Item) == (Item.CHOPPED_DOUGH | Item.BLUEBERRIES))
                {
                    t.Item = Item.NONE;
                    player.Item = Item.RAW_TART;
                    return true;
                }

                return false;
        }
    }

    public static IEnumerable<Position> GetAdjustentPositions(this Position p)
    {
        if (p.X > 0)
        {
            if (p.Y > 0) yield return new Position(p.X - 1, p.Y - 1);
            yield return new Position(p.X - 1, p.Y);
            if (p.Y < 6) yield return new Position(p.X - 1, p.Y + 1);
        }

        if (p.X < 10)
        {
            if (p.Y > 0) yield return new Position(p.X + 1, p.Y - 1);
            yield return new Position(p.X + 1, p.Y);
            if (p.Y < 6) yield return new Position(p.X + 1, p.Y + 1);
        }

        if (p.Y > 0) yield return new Position(p.X, p.Y - 1);
        if (p.Y < 6) yield return new Position(p.X, p.Y + 1);
    }

    public static bool Constains(this Item item, Item what)
    {
        return (item & what) == what;
    }

    public static bool Missed(this Item item, Item what)
    {
        return (item & what) == 0;
    }

    public static Table FindClosestFreeTable(this Game game)
    {
        return game.FindClosestFreeTable(game.Players[0].Position);
    }

    public static Table FindClosestFreeTable(this Game game, Position p)
    {
        return game.Tables
            .Where(x => x.Item == Item.NONE)
            .OrderBy(x => x.Position.Manhattan(p))
            .First();
    }

    public static Table FindBest(this List<Table> tables, Item item)
    {
        return tables.Find(item).First();
    }

    public static IEnumerable<Table> Find(this List<Table> tables, Item item)
    {
        //return tables.Where(x => x.Item == item);
        return tables.Where(x => x.Item.Constains(item));
    }
}