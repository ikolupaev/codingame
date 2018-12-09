using System;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public enum Direction
{
    Up = 0,
    Right = 1,
    Down = 2,
    Left = 3
}

static class DirectionExt
{
    public static Direction Opposite(this Direction dir) => (Direction)(((int)dir + 2) % 4);
}

public class MyPath
{
    public int Passed => Path.Count;
    public int ToGo => Math.Abs(Last.X - Target.X) + Math.Abs(Last.Y - Target.Y);
    public Vector Target;
    public Vector Last => Visited.Peek();
    public Vector Origin;
    public Stack<Direction> Path;
    public Stack<Vector> Visited;

    public MyPath(Vector origin, Vector target)
    {
        this.Origin = origin;
        this.Target = target;
        this.Path = new Stack<Direction>();
        this.Visited = new Stack<Vector>(new[] { origin });
    }

    public bool Add(Direction dir)
    {
        var p = Last.Move(dir);
        if (p.X < 0 || p.X > 6 || p.Y < 0 || p.Y > 6) return false;
        if (Visited.Contains(p)) return false;
        Path.Push(dir);
        Visited.Push(p);
        return true;
    }

    public void RemoveLast()
    {
        Path.Pop();
        Visited.Pop();
    }

    public override string ToString()
    {
        return string.Join(" ", Path.Reverse().Take(20).Select(x => x.ToString().ToUpperInvariant()));
    }
}

public class Tile
{
    public readonly bool Up;
    public readonly bool Right;
    public readonly bool Down;
    public readonly bool Left;

    public readonly byte Value;

    public Tile(byte v)
    {
        Value = v;
        Up = (v & (1 << 3)) != 0;
        Right = (v & (1 << 2)) != 0;
        Down = (v & (1 << 1)) != 0;
        Left = (v & 1) != 0;
    }

    public Tile(string input)
    {
        Value = 0;
        if (input[0] == '1')
        {
            Up = true;
            Value |= 1 << 3;
        }

        if (input[1] == '1')
        {
            Right = true;
            Value |= 1 << 2;
        }

        if (input[2] == '1')
        {
            Down = true;
            Value |= 1 << 1;
        }

        if (input[3] == '1')
        {
            Left = true;
            Value |= 1;
        }
    }

    public IEnumerable<Direction> GetDirections()
    {
        if (Up) yield return Direction.Up;
        if (Down) yield return Direction.Down;
        if (Left) yield return Direction.Left;
        if (Right) yield return Direction.Right;
    }

    internal bool Has(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return Up;
            case Direction.Right:
                return Right;
            case Direction.Down:
                return Down;
            case Direction.Left:
                return Left;
            default:
                throw new ArgumentException();
        }
    }
}

public struct Vector : IEquatable<Vector>
{
    public readonly int X;
    public readonly int Y;

    public Vector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vector Add(int dx, int dy)
    {
        return new Vector(X + dx, Y + dy);
    }

    public bool Equals(Vector other)
    {
        return this.X == other.X && this.Y == other.Y;
    }

    internal Vector Move(Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return new Vector(X, Y - 1);
            case Direction.Right:
                return new Vector(X + 1, Y);
            case Direction.Down:
                return new Vector(X, Y + 1);
            case Direction.Left:
                return new Vector(X - 1, Y);
            default:
                throw new ArgumentException();
        }
    }
}

public class Player
{
    public Vector Vector;
    public int NumPlayerCards;
    public Tile Tile;
}

public class Item
{
    public string Name;
    public Vector Vector;
    public int PlayerId;
}

public class Quest
{
    public string Name;
    public int QuestPlayerId;
}

public class Push
{
    public int Index;
    public Direction Direction;

    public Push(int index, Direction direction)
    {
        this.Index = index;
        this.Direction = direction;
    }

    public override string ToString()
    {
        return $"PUSH {Index} {Direction.ToString().ToUpperInvariant()}";
    }
}

public class Board
{
    public bool MoveTurn;
    public Tile[,] Cells = new Tile[7, 7];
    public Player[] Players = new Player[2];
    public Item[] Items;
    public Quest[] Quests;

    private string ReadLine()
    {
        var s = Console.ReadLine();
        return s;
    }

    public void Load()
    {
        MoveTurn = int.Parse(ReadLine()) == 1;
        for (int y = 0; y < 7; y++)
        {
            var inputs = ReadLine().Split(' ');
            for (int x = 0; x < 7; x++)
            {
                Cells[x, y] = new Tile(inputs[x]);
            }
        }

        for (int i = 0; i < 2; i++)
        {
            var inputs = ReadLine().Split(' ');
            Players[i] = new Player
            {
                NumPlayerCards = int.Parse(inputs[0]), // the total number of quests for a player (hidden and revealed)
                Vector = new Vector(int.Parse(inputs[1]), int.Parse(inputs[2])),
                Tile = new Tile(inputs[3])
            };
        };

        int numItems = int.Parse(ReadLine()); // the total number of items available on board and on player tiles
        Items = new Item[numItems];
        for (int i = 0; i < numItems; i++)
        {
            var inputs = ReadLine().Split(' ');
            Items[i] = new Item
            {
                Name = inputs[0],
                Vector = new Vector(int.Parse(inputs[1]), int.Parse(inputs[2])),
                PlayerId = int.Parse(inputs[3])
            };
        }

        int numQuests = int.Parse(ReadLine()); // the total number of revealed quests for both players
        Quests = new Quest[numQuests];
        for (int i = 0; i < numQuests; i++)
        {
            var inputs = ReadLine().Split(' ');
            Quests[i] = new Quest
            {
                Name = inputs[0],
                QuestPlayerId = int.Parse(inputs[1])
            };
        }
    }

    public IEnumerable<Item> GetBoardQuestsItems(int playerId)
    {
        foreach (var quest in Quests.Where(x => x.QuestPlayerId == 0))
        {
            var f = Items.FirstOrDefault(x => x.PlayerId == 0 && x.Name == quest.Name && x.Vector.X >= 0);
            if (f != null) yield return f;
        }
    }
    public void Push(Push push)
    {
        switch (push.Direction)
        {
            case Direction.Up:
                PushUp(push.Index);
                break;
            case Direction.Right:
                PushRight(push.Index);
                break;
            case Direction.Down:
                PushDown(push.Index);
                break;
            case Direction.Left:
                PushLeft(push.Index);
                break;
            default:
                break;
        }
    }

    private void PushLeft(int index)
    {
        var t = Players[0].Tile;
        Players[0].Tile = Cells[0, index];
        for (var x = 0; x < 6; x++)
        {
            Cells[x, index] = Cells[x + 1, index];
        }
        Cells[6, index] = t;
        SwapMyTitle(6, index);
    }

    private void PushRight(int index)
    {
        var t = Players[0].Tile;
        Players[0].Tile = Cells[6, index];
        for (var x = 6; x > 0; x--)
        {
            Cells[x, index] = Cells[x - 1, index];
        }
        Cells[0, index] = t;
        SwapMyTitle(0, index);
    }

    private void PushUp(int index)
    {
        var t = Players[0].Tile;
        Players[0].Tile = Cells[index, 0];
        for (var y = 0; y < 6; y++)
        {
            Cells[index, y] = Cells[index, y + 1];
        }
        Cells[index, 6] = t;
        SwapMyTitle(index, 6);
    }

    private void SwapMyTitle(int x, int y)
    {
        var boardTitle = Items.FirstOrDefault(i => i.Vector.X == x && i.Vector.Y == y);
        var myTitle = Items.FirstOrDefault(i => i.Vector.X == -1);
        if (boardTitle != null)
        {
            boardTitle.Vector = new Vector(-1, -1);
        }

        if (myTitle != null)
        {
            myTitle.Vector = new Vector(x, y);
        }
    }

    private void PushDown(int index)
    {
        var t = Players[0].Tile;
        Players[0].Tile = Cells[index, 6];
        for (var y = 6; y > 0; y--)
        {
            Cells[index, y] = Cells[index, y - 1];
        }
        Cells[index, 0] = t;
        SwapMyTitle(index, 0);
    }

    internal string Serialize()
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(MoveTurn);

            for (var y = 0; y < 7; y++)
            {
                for (var x = 0; x < 7; x++)
                {
                    writer.Write(Cells[x, y].Value);
                }
            }

            for (var i = 0; i < 2; i++)
            {
                writer.Write(Players[i].Vector.X);
                writer.Write(Players[i].Vector.Y);
                writer.Write(Players[i].NumPlayerCards);
                writer.Write(Players[i].Tile.Value);
            }

            writer.Write(Items.Length);
            for (var i = 0; i < Items.Length; i++)
            {
                writer.Write(Items[i].Name.Length);
                writer.Write(Items[i].Name.ToCharArray());
                writer.Write(Items[i].Vector.X);
                writer.Write(Items[i].Vector.Y);
                writer.Write(Items[i].PlayerId);
            }

            writer.Write(Quests.Length);
            for (var i = 0; i < Quests.Length; i++)
            {
                writer.Write(Quests[i].Name.Length);
                writer.Write(Quests[i].Name.ToCharArray());
                writer.Write(Quests[i].QuestPlayerId);
            }

            return Convert.ToBase64String(stream.ToArray());
        }
    }

    public void Deserialize(string base64)
    {
        using (var stream = new MemoryStream(Convert.FromBase64String(base64)))
        using (var reader = new BinaryReader(stream))
        {
            MoveTurn = reader.ReadBoolean();

            for (var y = 0; y < 7; y++)
            {
                for (var x = 0; x < 7; x++)
                {
                    Cells[x, y] = new Tile(reader.ReadByte());
                }
            }

            for (var i = 0; i < 2; i++)
            {
                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                Players[i] = new Player
                {
                    Vector = new Vector(x, y),
                    NumPlayerCards = reader.ReadInt32(),
                    Tile = new Tile(reader.ReadByte())
                };
            }

            Items = new Item[reader.ReadInt32()];
            for (var i = 0; i < Items.Length; i++)
            {
                var len = reader.ReadInt32();
                var name = new string(reader.ReadChars(len));

                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                Items[i] = new Item
                {
                    Name = name,
                    Vector = new Vector(x, y),
                    PlayerId = reader.ReadInt32()
                };
            }

            Quests = new Quest[reader.ReadInt32()];
            for (var i = 0; i < Quests.Length; i++)
            {
                var len = reader.ReadInt32();
                Quests[i] = new Quest
                {
                    Name = new string(reader.ReadChars(len)),
                    QuestPlayerId = reader.ReadInt32()
                };
            }
        }
    }
}

public class XmaxRush
{
    static Random rnd = new Random();
    static Stopwatch timer = new Stopwatch();
    static int ticks = 0;

    static void Main(string[] args)
    {
        var board = new Board();

        while (true)
        {
            board.Load();

            Console.Error.WriteLine(board.Serialize());
            //board.Deserialize("AAkMCwoHDgcGDQsDCgkFDgMKCg8NBg0KCgcJCQYHDQcNBwMGCgoKCQwMCgwKDwMHDQoBAAAAAwAAAAEAAAAGAwAAAAAAAAACAAAADQMAAAAEAAAAQ0FORQYAAAADAAAAAQAAAAQAAABNQVNL//////////8AAAAABQAAAEFSUk9XBQAAAAUAAAABAAAAAwAAAAQAAABNQVNLAAAAAAQAAABDQU5FAQAAAAUAAABBUlJPVwEAAAA=");

            timer.Restart();
            ticks = 0;

            if (board.MoveTurn)
            {
                var found = false;

                foreach (var target in board.GetBoardQuestsItems(0))
                {
                    var path = FindPath(board, target.Vector);
                    if (path.ToGo == 0)
                    {
                        found = true;
                        Console.WriteLine("MOVE " + path.ToString()); // PUSH <id> <direction> | MOVE <direction> | PASS
                        break;
                    }
                }

                if (!found) Console.WriteLine("PASS");
            }
            else
            {
                var push = FindBestPush(board);
                Console.WriteLine(push);
            }
        }
    }

    public static MyPath FindPath(Board board, Vector target)
    {
        var path = new MyPath(board.Players[0].Vector, target);
        FindRestOfPath(board, path);

        return path;
    }

    private static bool FindRestOfPath(Board board, MyPath path)
    {
        AssertTimeout();

        var p = path.Last;
        var t = path.Target;

        if (p.X == t.X && p.Y == t.Y) return true;

        foreach (var dir in board.Cells[p.X, p.Y].GetDirections())
        {
            if (path.Add(dir))
            {
                p = path.Last;
                if (board.Cells[p.X, p.Y].Has(dir.Opposite()))
                {
                    if (FindRestOfPath(board, path)) return true;
                }
                path.RemoveLast();
            }
        }

        return false;
    }

    private static void AssertTimeout()
    {
        ticks++;
        if (timer.ElapsedMilliseconds > 48)
        {
            Console.Error.WriteLine($"timeout {ticks}, {timer.ElapsedMilliseconds}");
            throw new TimeoutException();
        }
    }

    private static Push FindBestPush(Board board)
    {
        var quest = board.Quests.First(x => x.QuestPlayerId == 0);
        var target = board.Items.First(x => x.PlayerId == 0 && x.Name == quest.Name);

        var dx = Math.Sign(target.Vector.X - board.Players[0].Vector.X);
        var dy = Math.Sign(target.Vector.Y - board.Players[0].Vector.Y);

        var maxScore = int.MinValue;
        Push maxPush = null;

        try
        {
            while (true)
            {
                AssertTimeout();

                var direction = (Direction)rnd.Next(4);
                var index = rnd.Next(6);

                var push = new Push(index, direction);
                board.Push(push);

                var score = CalcScore(board);
                if (score > maxScore)
                {
                    maxScore = score;
                    maxPush = push;
                }

                board.Push(new Push(index, direction.Opposite()));
            }
        }
        catch (TimeoutException) { }

        return maxPush;
    }

    private static int CalcScore(Board board)
    {
        var maxScore = int.MinValue;

        foreach (var target in board.GetBoardQuestsItems(0))
        {
            var path = FindPath(board, target.Vector);
            if (path.ToGo == 0)
            {
                return 1000 + path.Passed - path.ToGo;
            }
            maxScore = Math.Max(maxScore, path.Passed - path.ToGo);
        }

        return maxScore;
    }

    public static void D(params object[] p)
    {
        Console.Error.WriteLine(string.Join(" ", p.Select(x => x?.ToString())));
    }
}