using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Game
{
    public Player[] Players;
    public Table Dishwasher;
    public Table Window;
    public Table Blueberry;
    public Table Strawberry;
    public Table IceCream;
    public Table Dough;
    public Table ChoppingBoard;
    public Table Oven;
    public List<Table> Tables;
    public Order[] Orders;
    public Item OvenContents;
    public int OvenTimer;

    public Player Me => Players[0];

    public Game()
    {
        Players = new Player[2];
        Tables = new List<Table>(7 * 11);
        Orders = new Order[3];
    }

    public Game(string base64)
    {
        using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
        {
            using (var reader = new BinaryReader(stream))
            {
                OvenContents = (Item)reader.ReadInt32();
                OvenTimer = reader.ReadInt32();

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

    public override string ToString()
    {
        return Position + " " + Item;
    }
}

public enum TableFunction
{
    NONE = 0,
    Dishwasher = 1,
    Window = 2,
    Blueberry = 3,
    Strawberry = 4,
    IceCream = 5,
    Dough = 6,
    ChoppingBoard = 7,
    Oven = 8
}

public class Order
{
    public Item Item;
    public int Award;

    public Order(Item item, int award)
    {
        Item = item;
        Award = award;
    }

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
    STRAWBERRIES = 2,
    DOUGH = 4,
    ICE_CREAM = 8,
    DISH = 16,
    CHOPPED_STRAWBERRIES = 32,
    CHOPPED_DOUGH = 64,
    RAW_TART = 128,
    CROISSANT = 256,
    TART = 512
}

public class Player
{
    public Position Position;
    public Item Item;
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
    public int X, Y;

    public Position(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int Manhattan(Position p2) => Math.Abs(X - p2.X) + Math.Abs(Y - p2.Y);

    public override string ToString()
    {
        return X + " " + Y;
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

public class MainClass
{
    public static bool Debug = true;

    public static Game ReadGame()
    {
        // ALL CUSTOMERS INPUT: to ignore until Bronze
        int numAllCustomers = int.Parse(ReadLine());
        for (int i = 0; i < numAllCustomers; i++)
        {
            var inputs = ReadLine().Split(' ');
            string customerItem = inputs[0]; // the food the customer is waiting for
            int customerAward = int.Parse(inputs[1]); // the number of points awarded for delivering the food
        }

        var game = new Game();
        game.Players[0] = new Player(null, Item.NONE);
        game.Players[1] = new Player(null, Item.NONE);

        for (int i = 0; i < 7; i++)
        {
            string kitchenLine = ReadLine();
            for (var x = 0; x < kitchenLine.Length; x++)
            {
                if (kitchenLine[x] == 'W') game.Window = new Table { Position = new Position(x, i) };
                if (kitchenLine[x] == 'D') game.Dishwasher = new Table { Position = new Position(x, i) };
                if (kitchenLine[x] == 'I') game.IceCream = new Table { Position = new Position(x, i) };
                if (kitchenLine[x] == 'B') game.Blueberry = new Table { Position = new Position(x, i) };
                if (kitchenLine[x] == 'S') game.Strawberry = new Table { Position = new Position(x, i) };
                if (kitchenLine[x] == 'H') game.Dough = new Table { Position = new Position(x, i) };
                if (kitchenLine[x] == 'C') game.ChoppingBoard = new Table { Position = new Position(x, i) };
                if (kitchenLine[x] == 'O') game.Oven = new Table { Position = new Position(x, i) };

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

    private static void Move(Position p, string message) =>
        Console.WriteLine("MOVE " + p + ";" + message);

    private static void Use(Position p, string message) =>
        Console.WriteLine("USE " + p + ";" + message);

    private static string ReadLine()
    {
        var s = Console.ReadLine();
        if (Debug) Console.Error.WriteLine(s);
        return s;
    }

    static void Main()
    {
        Game game;

#if !DEBUG
        game = ReadGame();
#endif
        while (true)
        {
#if !DEBUG
            ReadGameTurn(game);
            D(game.ToString());
#else
            game = new Game("AAAAAAAAAAAvAAAAAAAAAAAQAAAAACAAAAAAMBACAABAAAAHAFAAAAEAYAIAAABwAAAAAIAAAAAAkAAAAwCgAAAAAAEAAAAAoQAAAAACAAAIACIAAAAAMgAAAABCIAAAAFIAAAUAcgAABACCAAAAAKIAAAAAAwAAAAAjAAAAAFMAAAAAgwAAAACjAAAAAAQAAAAAJAAAAAA0AAAAAFQAAAAAZAAAAAB0AAAAAIQAAAAApAAAAAAFAAAAAKUAAAAABgAAAAAWAAAAACYAAAYANgAAAABGAAAAAFYAAAIAZgAAAAB2AAAAAIYAAAAAlgAAAACmAAAAAAMAAAAZAAAACwIAABADAAABBwAAOAAAABcDAABhEABBAAA=");
#endif

            var tableDishes = game.Tables.Find(Item.DISH).Where(x=> x.Item != Item.DISH).ToArray();

            Order order = null;

            //order = game.Orders.FirstOrDefault(o => game.Me.Item == o.Item);

            //if (order == null)
            //{
            //    order = game.Orders.Where(o => tableDishes.Any(x => x.Item.Constains(Item.DISH) && (x.Item | game.Me.Item) == o.Item))
            //        .OrderByDescending(x => x.Award).FirstOrDefault();
            //}

            //if (order == null)
            //{
            //    order = game.Orders.Where(o => tableDishes.Any(d => !IsRedundant(o, d.Item)))
            //        .OrderByDescending(x => x.Award).FirstOrDefault();
            //}

            if (order == null) order = game.Orders[0];

            D("order: " + order.Item);

            if (CookWhatMissed(game, order)) continue;

            D("None missed to cook");

            if (ServeOrder(game, order)) continue;

            Console.WriteLine("WAIT");
        }
    }

    private static void ReadGameTurn(Game game)
    {
        string[] inputs;
        int turnsRemaining = int.Parse(ReadLine());

        // PLAYERS INPUT
        inputs = ReadLine().Split(' ');
        game.Players[0].Update(new Position(int.Parse(inputs[0]), int.Parse(inputs[1])), CreateItem(inputs[2]));
        inputs = ReadLine().Split(' ');
        game.Players[1].Update(new Position(int.Parse(inputs[0]), int.Parse(inputs[1])), CreateItem(inputs[2]));
        var me = game.Players[0];

        //Clean other tables
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

        game.Orders = new Order[numCustomers];
        for (int i = 0; i < numCustomers; i++)
        {
            inputs = ReadLine().Split(' ');
            game.Orders[i] = new Order(CreateItem(inputs[0]), int.Parse(inputs[1]));
        }
    }

    static bool IsMissedInHand(Game game, Order order, Item item)
    {
        return order.Item.Constains(item) && !game.Me.Item.Constains(item);
    }

    static bool IsRedundant(Order order, Item item)
    {
        return ((order.Item ^ item) & item) != Item.NONE;
    }

    private static bool ServeOrder(Game game, Order order)
    {
        var tableDish = game.Tables.Find(Item.DISH).Where(x=> x.Item != Item.DISH).FirstOrDefault(d => !IsRedundant(order, d.Item));

        if( tableDish != null )
        {
            if( game.Me.Item == Item.NONE )
            {
                Use(tableDish.Position, "take dish with " + tableDish.Item);
                return true;
            }
            else if( game.Me.Item < tableDish.Item )
            {
                Use(game.FindClosestFreeTable().Position, "drop all to take dish with " + tableDish.Item);
                return true;
            }
        }

        if (((order.Item ^ game.Me.Item) & game.Me.Item) != Item.NONE)
        {
            Use(game.FindClosestFreeTable().Position, "drop redundant Items");
            return true;
        }

        var missedItems = new List<Table>();

        if (!game.Me.Item.Constains(Item.DISH))
        {
            //todo: get dish with somehting
            //missedItems.Add(game.Dishwasher);
            var tableDishes = game.Tables.Find(Item.DISH).ToArray();
            var playersDishes = game.Players.Where(x => x.Item.Constains(Item.DISH)).ToArray();

            D("dishes in tables: " + tableDishes.Length);
            D("dishes in playes: " + playersDishes.Length);

            if (tableDishes.Length + playersDishes.Length < 3)
            {
                Use(game.Dishwasher.Position, "use dishwasher");
                return true;
            }

            var dishes = tableDishes.Where(x => x.Item == Item.DISH || (x.Item | game.Me.Item) == order.Item).ToArray();

            if (dishes.Any())
            {
                Use(dishes[0].Position, "take dish from table");
                return true;
            }

            //todo if cannot find then put it to the right (non redundant) plate

            D("Cannot find free dish");
            return false;
        }

        if (IsMissedInHand(game, order, Item.TART))
        {
            missedItems.AddRange(game.Tables.Where(x => x.Item == Item.TART));
            if (game.OvenContents == Item.TART) missedItems.Add(game.Oven);
        }

        if (IsMissedInHand(game, order, Item.CROISSANT))
        {
            missedItems.AddRange(game.Tables.Where(x => x.Item == Item.CROISSANT));
            if (game.OvenContents == Item.CROISSANT) missedItems.Add(game.Oven);
        }

        if (IsMissedInHand(game, order, Item.CHOPPED_STRAWBERRIES))
        {
            missedItems.AddRange(game.Tables.Where(x => x.Item == Item.CHOPPED_STRAWBERRIES));
        }

        if (IsMissedInHand(game, order, Item.BLUEBERRIES))
        {
            missedItems.AddRange(game.Tables.Where(x => x.Item == Item.BLUEBERRIES));
            missedItems.Add(game.Blueberry);
        }

        if (IsMissedInHand(game, order, Item.ICE_CREAM))
        {
            missedItems.AddRange(game.Tables.Where(x => x.Item == Item.ICE_CREAM));
            missedItems.Add(game.IceCream);
        }

        if (missedItems.Any())
        {
            //todo sort smarter
            missedItems.Sort((x, y) => x.Position.Manhattan(game.Me.Position));
            Use(missedItems[0].Position, "take closest " + missedItems[0].Item);
            return true;
        }
        else
        {
            Use(game.Window.Position, "serve " + game.Me.Item + " for " + order.Item);
            return true;
        }

        return false;
        throw new Exception("why we here, we need to server order");
    }

    static bool IsMissed(Game game, Order order, Item item)
    {
        return !game.Me.Item.Constains(item) && order.Item.Constains(item) && !game.Tables.Find(item).Any();
    }

    private static bool CookWhatMissed(Game game, Order order)
    {

        if (IsMissed(game, order, Item.TART))
        {
            D("TART missed");
            if( CookTart(game, order) ) return true;
        }

        if (IsMissed(game, order, Item.CROISSANT))
        {
            D("C missed");
            if( CookCroissant(game) ) return true;
        }

        if (IsMissed(game, order, Item.CHOPPED_STRAWBERRIES))
        {
            D("CS missed");
            return CookChoppedStrawberries(game);
        }

        return false;
    }

    private static bool CookChoppedStrawberries(Game game)
    {
        if (game.Me.Item == Item.STRAWBERRIES)
        {
            Use(game.ChoppingBoard.Position, "chop S");
            return true;
        }

        if (game.Me.Item != Item.NONE)
        {
            Use(game.FindClosestFreeTable().Position, "put somehting for S");
            return true;
        }

        Use(game.Strawberry.Position, "get S");
        return true;
    }

    private static bool CookCroissant(Game game)
    {
        if (game.OvenContents == Item.DOUGH)
        {
            D("waiting for C in oven");
            return false;
        }

        if (game.OvenContents == Item.CROISSANT)
        {
            if (game.Me.Item == Item.NONE || !game.Me.Item.Constains(Item.DISH))
            {
                Use(game.Oven.Position, "take CROISSANT from oven");
                return true;
            }

            Use(game.FindClosestFreeTable(game.Oven.Position).Position, "leave for oven");
            return true;
        }

        if (game.Me.Item == Item.DOUGH)
        {
            Use(game.Oven.Position, "oven for CROISSANT");
            return true;
        }

        if (game.Me.Item != Item.NONE)
        {
            Use(game.FindClosestFreeTable().Position, "leave anyting for taking C from oven");
            return true;
        }

        Use(FindClosestDoughPosition(game), "take dough");
        return true;
    }

    private static Position FindClosestDoughPosition(Game game)
    {
        return game.Tables
            .Find(Item.DOUGH)
            .Concat(new[] { game.Dough })
            .OrderBy(x => x.Position.Manhattan(game.Me.Position))
            .First().Position;
    }

    private static bool CookTart(Game game, Order order)
    {
        if (game.OvenContents == Item.RAW_TART)
        {
            D("RT is in oven. waiting");
            return false;
        }

        if (game.OvenContents == Item.TART)
        {
            if (IsRedundant(order, game.Me.Item))
            {
                Use(game.FindClosestFreeTable().Position, "drop item to take tart");
                return true;
            }
            else
            {
                Use(game.Oven.Position, "take tart");
                return true;
            }
        }

        if (game.Me.Item == Item.RAW_TART)
        {
            if (game.OvenContents == Item.NONE)
            {
                Use(game.Oven.Position, "put RAW_TART to oven");
                return true;
            }
            else
            {
                return TakeItemFromOven(game);
            }
        }

        var rawTart = game.Tables.Find(Item.RAW_TART).OrderBy(x => x.Position.Manhattan(game.Me.Position)).FirstOrDefault();

        if (rawTart == null)
        {
            return CookRawTart(game);
        }
        else
        {
            Use(rawTart.Position, "take RT");
        }

        return false;
    }

    private static bool TakeItemFromOven(Game game)
    {
        if (game.Me.Item == Item.NONE)
        {
            Use(game.Oven.Position, "take from Oven");
            return true;
        }
        else
        {
            Use(game.FindClosestFreeTable(game.Oven.Position).Position, "put whatever for take thing from oven");
            return true;
        }
    }

    private static bool CookRawTart(Game game)
    {
        if (game.Me.Item.Constains(Item.CHOPPED_DOUGH))
        {
            Use(game.Blueberry.Position, "add B to CD");
            return true;
        }

        if (game.Me.Item.Constains(Item.DOUGH))
        {
            Use(game.ChoppingBoard.Position, "chop D");
            return true;
        }

        var cd = game.Tables.FindBest(Item.CHOPPED_DOUGH);

        if (cd != null)
        {
            if (game.Me.Item == Item.NONE)
            {
                Use(cd.Position, "get CD");
                return true;
            }
            else
            {
                Use(game.FindClosestFreeTable().Position, "drop for CD");
                return true;
            }
        }

        if (game.Me.Item != Item.NONE)
        {
            Use(game.FindClosestFreeTable().Position, "drop for D");
            return true;
        }
        else
        {
            Use(FindClosestDoughPosition(game), "take D");
            return true;
        }

        throw new Exception("why we here to cook raw tart?");
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
            .Where(x => x.Item == Item.NONE && x.TableFunction == TableFunction.NONE)
            .OrderBy(x => x.Position.Manhattan(p))
            .First();
    }

    public static Table FindBest(this List<Table> tables, Item item)
    {
        return tables.Find(item).FirstOrDefault();
    }

    public static IEnumerable<Table> Find(this List<Table> tables, Item item)
    {
        //return tables.Where(x => x.Item == item);
        return tables.Where(x => x.Item.Constains(item));
    }
}