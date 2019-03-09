using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

public class Game
{
    public Player[] Players = new Player[2];
    public Table Dishwasher;
    public Table Window;
    public Table Blueberry;
    public Table Strawberry;
    public Table IceCream;
    public Table Dough;
    public Table ChoppingBoard;
    public Table Oven;
    public List<Table> Tables = new List<Table>();
}

public class Table
{
    public Position Position;
    public bool HasFunction;
    public Item Item;
}

public class Order
{
    public Item Item;
    public int Award;
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
                if (kitchenLine[x] == 'W') game.Window = new Table { Position = new Position(x, i), HasFunction = true };
                if (kitchenLine[x] == 'D') game.Dishwasher = new Table { Position = new Position(x, i), HasFunction = true };
                if (kitchenLine[x] == 'I') game.IceCream = new Table { Position = new Position(x, i), HasFunction = true };
                if (kitchenLine[x] == 'B') game.Blueberry = new Table { Position = new Position(x, i), HasFunction = true };
                if (kitchenLine[x] == 'S') game.Strawberry = new Table { Position = new Position(x, i), HasFunction = true };
                if (kitchenLine[x] == 'H') game.Dough = new Table { Position = new Position(x, i), HasFunction = true };
                if (kitchenLine[x] == 'C') game.ChoppingBoard = new Table { Position = new Position(x, i), HasFunction = true };
                if (kitchenLine[x] == 'O') game.Oven = new Table { Position = new Position(x, i), HasFunction = true };
                if (kitchenLine[x] == '#') game.Tables.Add(new Table { Position = new Position(x, i) });
            }
        }

        return game;
    }

    private static void Move(Position p) => Console.WriteLine("MOVE " + p);

    private static void Use(Position p, string message)
    {
        Console.WriteLine("USE " + p + ";" + message);
    }

    private static string ReadLine()
    {
        var s = Console.ReadLine();
        if (Debug) Console.Error.WriteLine(s);
        return s;
    }

    static void Main()
    {
        string[] inputs;

        // ALL CUSTOMERS INPUT: to ignore until Bronze
        int numAllCustomers = int.Parse(ReadLine());
        for (int i = 0; i < numAllCustomers; i++)
        {
            inputs = ReadLine().Split(' ');
            string customerItem = inputs[0]; // the food the customer is waiting for
            int customerAward = int.Parse(inputs[1]); // the number of points awarded for delivering the food
        }

        // KITCHEN INPUT
        var game = ReadGame();

        while (true)
        {
            int turnsRemaining = int.Parse(ReadLine());

            // PLAYERS INPUT
            inputs = ReadLine().Split(' ');
            game.Players[0].Update(new Position(int.Parse(inputs[0]), int.Parse(inputs[1])), CreateItem(inputs[2]));
            inputs = ReadLine().Split(' ');
            game.Players[1].Update(new Position(int.Parse(inputs[0]), int.Parse(inputs[1])), CreateItem(inputs[2]));

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
            var ovenContents = CreateItem(inputs[0]); // ignore until bronze league
            int ovenTimer = int.Parse(inputs[1]);
            int numCustomers = int.Parse(ReadLine()); // the number of customers currently waiting for food

            var orders = new Order[numCustomers];
            for (int i = 0; i < numCustomers; i++)
            {
                inputs = ReadLine().Split(' ');
                orders[i] = new Order { Item = CreateItem(inputs[0]), Award = int.Parse(inputs[1]) };
            }

            var order = orders.Where(x => (x.Item & Item.TART) == 0).FirstOrDefault();

            if (order == null)
            {
                Console.WriteLine("WAIT");
                continue;
            }

            var me = game.Players[0];


            if (order.Item.Constains(Item.CROISSANT) &&
                !me.Item.Constains(Item.CROISSANT) &&
                !game.Tables.Find(Item.CROISSANT).Any() &&
                ovenContents != Item.DOUGH)
            {
                if (ovenContents == Item.CROISSANT)
                {
                    if (me.Item != Item.NONE)
                        Use(game.FindClosestFreeTable().Position, "leave for oven");
                    else
                        Use(game.Oven.Position, "take CROISSANT");
                }
                else if (me.Item == Item.DOUGH)
                {
                    Use(game.Oven.Position, "oven");
                }
                else if (me.Item != Item.NONE)
                {
                    Use(game.FindClosestFreeTable().Position, "leave");
                }
                else
                {
                    Use(game.Dough.Position, "dough");
                }
            }
            else if (order.Item.Constains(Item.CHOPPED_STRAWBERRIES) &&
                !me.Item.Constains(Item.CHOPPED_STRAWBERRIES) &&
                !game.Tables.Find(Item.CHOPPED_STRAWBERRIES).Any())
            {
                if (me.Item == Item.STRAWBERRIES)
                {
                    Use(game.ChoppingBoard.Position, "chopp");
                }
                else if (me.Item != Item.NONE)
                {
                    Use(game.FindClosestFreeTable().Position, "drop for straw");
                }
                else
                {
                    var straw = game.Tables
                        .Find(Item.STRAWBERRIES)
                        .Concat(new[] { game.Strawberry })
                        .OrderBy(x => x.Position.Manhattan(me.Position))
                        .First();

                    Use(straw.Position, "take straw");
                }
            }
            else
            {
                if (!me.Item.Constains(Item.DISH))
                {
                    if (me.Item != Item.NONE)
                        Use(game.FindClosestFreeTable().Position, "leave for dish");
                    else
                    {
                        var d = game.Tables.Find(Item.DISH).Concat(new[] { game.Dishwasher }).First();
                        Use(d.Position, "take dish");
                    }
                }
                else if (order.Item.Constains(Item.CROISSANT) && !me.Item.Constains(Item.CROISSANT) && game.Tables.Find(Item.CROISSANT).Any())
                {
                    Use(game.Tables.FindBest(Item.CROISSANT).Position, "take CROISSANT");
                }
                else if (order.Item.Constains(Item.CHOPPED_STRAWBERRIES) && !me.Item.Constains(Item.CHOPPED_STRAWBERRIES) && game.Tables.Find(Item.CHOPPED_STRAWBERRIES).Any())
                {
                    Use(game.Tables.FindBest(Item.CHOPPED_STRAWBERRIES).Position, "take CHOPPED_STRAWBERRIES");
                }
                else if (order.Item.Constains(Item.BLUEBERRIES) && !me.Item.Constains(Item.BLUEBERRIES))
                {
                    Use(game.Blueberry.Position, "take Blueberry");
                }
                else if (order.Item.Constains(Item.ICE_CREAM) && !me.Item.Constains(Item.ICE_CREAM))
                {
                    Use(game.IceCream.Position, "take ice cream");
                }
                else
                {
                    Use(game.Window.Position, "place to window");
                }
            }
        }
    }

    private static Item CreateItem(string s)
    {
        var item = Item.NONE;

        if (s.Contains("BLUEBERRIES")) item |= Item.BLUEBERRIES;
        if (s.Contains("ICE_CREAM")) item |= Item.ICE_CREAM;
        if (s.Contains("CHOPPED_STRAWBERRIES")) item |= Item.CHOPPED_STRAWBERRIES;
        if (s.Contains("CROISSANT")) item |= Item.CROISSANT;
        if (s.Contains("DOUGH")) item |= Item.DOUGH;
        if (s.Contains("STRAWBERRIES")) item |= Item.STRAWBERRIES;
        if (s.Contains("DISH")) item |= Item.DISH;
        if (s.Contains("TART")) item |= Item.TART;
        if (s.Contains("CHOPPED_DOUGH")) item |= Item.CHOPPED_DOUGH;

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
        return game.Tables
            .Where(x => x.Item == Item.NONE)
            .OrderBy(x => x.Position.Manhattan(game.Players[0].Position))
            .First();
    }

    public static Table FindBest(this List<Table> tables, Item item)
    {
        return tables.Find(item).First();
    }

    public static IEnumerable<Table> Find(this List<Table> tables, Item item)
    {
        return tables.Where(x => x.Item.Constains(item));
    }
}