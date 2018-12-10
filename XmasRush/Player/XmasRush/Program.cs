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
        var p = Last.Move(dir, false);
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

    public IEnumerable<string> GetPathDirecitons()
    {
        return Path.Reverse().Take(20).Select(x => x.ToString().ToUpperInvariant());
    }

    public override string ToString()
    {
        return string.Join(" ", GetPathDirecitons());
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

    public bool Equals(int x, int y)
    {
        return this.X == x && this.Y == y;
    }

    public bool Equals(Vector other)
    {
        return Equals(other.X, other.Y);
    }

    internal Vector Move(Direction dir, bool wrap)
    {
        var x = X;
        var y = Y;

        switch (dir)
        {
            case Direction.Up:
                y--;
                break;
            case Direction.Right:
                x++;
                break;
            case Direction.Down:
                y++;
                break;
            case Direction.Left:
                x--;
                break;
            default:
                throw new ArgumentException();
        }

        if (wrap)
        {
            if (x < 0) x = 6;
            if (x > 6) x = 0;
            if (y < 0) y = 6;
            if (y > 6) y = 0;
        }

        return new Vector(x, y);
    }

    public bool IsValid()
    {
        return (X >= 0 && X < 7 && Y >= 0 && Y < 7);
    }

    public int GetDistance(Vector x)
    {
        return Math.Abs(x.X - X) + Math.Abs(x.Y - Y);
    }

    public override string ToString()
    {
        return $"{X} {Y}";
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
        SwapMyTile(6, index);

        foreach (var item in Items.Where(x => x.Vector.Y == index))
        {
            item.Vector = item.Vector.Move(Direction.Left, true);
        }
        foreach (var player in Players.Where(x => x.Vector.Y == index))
        {
            player.Vector = player.Vector.Move(Direction.Left, true);
        }
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
        SwapMyTile(0, index);

        foreach (var item in Items.Where(x => x.Vector.Y == index))
        {
            item.Vector = item.Vector.Move(Direction.Right, true);
        }
        foreach (var player in Players.Where(x => x.Vector.Y == index))
        {
            player.Vector = player.Vector.Move(Direction.Right, true);
        }
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
        SwapMyTile(index, 6);

        foreach (var item in Items.Where(x => x.Vector.X == index))
        {
            item.Vector = item.Vector.Move(Direction.Up, true);
        }
        foreach (var player in Players.Where(x => x.Vector.X == index))
        {
            player.Vector = player.Vector.Move(Direction.Up, true);
        }
    }

    private void SwapMyTile(int x, int y)
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
        SwapMyTile(index, 0);

        foreach (var item in Items.Where(x => x.Vector.X == index))
        {
            item.Vector = item.Vector.Move(Direction.Down, true);
        }
        foreach (var player in Players.Where(x => x.Vector.X == index))
        {
            player.Vector = player.Vector.Move(Direction.Down, true);
        }
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

    internal IEnumerable<Vector> GetAdjacentCells(Vector cell)
    {
        foreach (var dir in Cells[cell.X, cell.Y].GetDirections())
        {
            var v = cell.Move(dir, false);

            if (!v.IsValid()) continue;
            if (!Cells[v.X, v.Y].Has(dir.Opposite())) continue;

            yield return v;
        }
    }

    public Dictionary<Vector, int> CreateAdjacentMap(Vector start)
    {
        var toVisit = new Queue<Vector>();
        var distanceMap = new Dictionary<Vector, int>();

        toVisit.Enqueue(start);
        distanceMap[start] = 0;

        while (toVisit.Any())
        {
            var v = toVisit.Dequeue();
            var dist = distanceMap[v] + 1;
            foreach (var adj in GetAdjacentCells(v))
            {
                if (distanceMap.TryGetValue(adj, out int adjDist))
                {
                    if (adjDist > dist) distanceMap[adj] = dist;
                }
                else
                {
                    toVisit.Enqueue(adj);
                    distanceMap[adj] = dist;
                }
            }
        }
        return distanceMap;
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

        var prevPush = "";
        var samePrevPushCount = 0;

        while (true)
        {
#if DEBUG
            board.Deserialize("AQkKCQYGCg0GCwMOCg4KCgcHBQ0NBw8LAw8MDAsODQcFDQYPBgoKCgsKCgcGCQkGCQ4EAAAABgAAAAwAAAAJBAAAAAEAAAAJAAAACRUAAAAEAAAATUFTSwIAAAAGAAAAAAAAAAMAAABLRVkBAAAAAQAAAAAAAAAFAAAAQ0FORFkFAAAABAAAAAEAAAAEAAAAQk9PSwYAAAABAAAAAQAAAAQAAABDQU5FBgAAAAAAAAAAAAAABAAAAENBTkUAAAAABgAAAAEAAAAFAAAAU1dPUkQEAAAABQAAAAEAAAAEAAAAQk9PSwIAAAAFAAAAAAAAAAUAAABDQU5EWQIAAAAAAAAAAAAAAAYAAABTSElFTEQBAAAAAAAAAAAAAAAFAAAAQVJST1cGAAAAAgAAAAAAAAAFAAAAQVJST1cBAAAABAAAAAEAAAAGAAAAU0hJRUxEBQAAAAUAAAABAAAABAAAAE1BU0sEAAAAAAAAAAEAAAAEAAAARklTSAAAAAACAAAAAAAAAAcAAABESUFNT05EBAAAAAYAAAABAAAABgAAAFBPVElPTgEAAAACAAAAAAAAAAYAAABTQ1JPTEwGAAAAAwAAAAEAAAAFAAAAU1dPUkQDAAAAAQAAAAAAAAAGAAAAU0NST0xMAAAAAAQAAAAAAAAABwAAAERJQU1PTkQFAAAABgAAAAAAAAAGAAAAAwAAAEtFWQAAAAAGAAAAUE9USU9OAAAAAAQAAABNQVNLAAAAAAQAAABNQVNLAQAAAAUAAABTV09SRAEAAAAFAAAAQ0FORFkBAAAA");
#else
            board.Load();
            timer.Restart();
            Console.Error.WriteLine(board.Serialize());
#endif
            ticks = 0;

            D(board.Players[0].NumPlayerCards, board.Players[1].NumPlayerCards);
            if (board.MoveTurn)
            {
                var items = board.GetBoardQuestsItems(0).ToArray();

                if (items.Length == 0)
                {
                    D($"no board items: {items.Length}, numPlayerCards: {board.Players[0].NumPlayerCards} items: {board.Items.Length}, quests: {board.Quests.Length}");
                    Console.WriteLine("PASS");
                    continue;
                }

                var adjMap = board.CreateAdjacentMap(board.Players[0].Vector);
                var adjItems = items
                    .Where(x => adjMap.ContainsKey(x.Vector))
                    .OrderBy(x => adjMap[x.Vector]).ToArray();

                D("found adj items: ", adjItems.Length);

                var targets = new List<Vector>();
                if (adjItems.Any())
                {
                    targets.AddRange(adjItems.Select(x => x.Vector));
                }

                if (targets.Count == 0)
                {
                    var closest = adjMap.Keys
                        .OrderBy(x => items.Min(i => i.Vector.GetDistance(x)))
                        .First();

                    D($"no targets. closest: {closest}");

                    if (closest.Equals(board.Players[0].Vector))
                    {
                        Console.WriteLine("PASS");
                    }
                    else
                    {
                        var p = FindPath(board, board.Players[0].Vector, closest);
                        Console.WriteLine("MOVE " + string.Join(" ", p.Take(20)));
                    }
                }
                else
                {
                    var startCell = board.Players[0].Vector;
                    var path = new List<string>();
                    foreach (var t in targets)
                    {
                        var p = FindPath(board, startCell, t);
                        path.AddRange(p);
                        startCell = t;
                    }

                    Console.WriteLine("MOVE " + string.Join(" ", path.Take(20))); // PUSH <id> <direction> | MOVE <direction> | PASS
                }
            }
            else
            {
                var push = FindBestPush(board).ToString();
                if (prevPush == push)
                {
                    samePrevPushCount++;
                    D("same push:", samePrevPushCount);

                    if (samePrevPushCount > 5 && board.Players[0].NumPlayerCards > board.Players[1].NumPlayerCards)
                    {
                        D("break push");
                        push = new Push(rnd.Next(6), ((Direction)rnd.Next(4))).ToString();
                    }
                }
                else
                {
                    samePrevPushCount = 0;
                    prevPush = push;
                }

                Console.WriteLine(push);
            }
        }
    }

    public static IEnumerable<string> FindPath(Board board, Vector start, Vector target)
    {
        var path = new Stack<string>();
        var adjMap = board.CreateAdjacentMap(start);
        var curCell = target;
        while (!curCell.Equals(start))
        {
            var minDir = Direction.Up;
            var minDist = int.MaxValue;
            var minCell = curCell;
            foreach (var dir in board.Cells[curCell.X, curCell.Y].GetDirections())
            {
                var c = curCell.Move(dir, false);
                var opDir = dir.Opposite();
                if (!c.IsValid() || !adjMap.ContainsKey(c) || !board.Cells[c.X, c.Y].Has(opDir)) continue;

                var dist = adjMap[c];
                if (dist < minDist)
                {
                    minDist = dist;
                    minDir = opDir;
                    minCell = c;
                }
            }

            path.Push(minDir.ToString().ToUpperInvariant());
            curCell = minCell;
        }

        return path;
    }

    //public static MyPath FindPath(Board board, Vector start, Vector target)
    //{
    //    var path = new MyPath(start, target);
    //    FindRestOfPath(board, path);

    //    return path;
    //}

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
        if (timer.ElapsedMilliseconds > 45)
        {
            Console.Error.WriteLine($"timeout {ticks}, {timer.ElapsedMilliseconds}");
            throw new TimeoutException();
        }
    }

    private static Push FindBestPush(Board board)
    {
        var maxScore = int.MinValue;
        Push maxPush = null;

        try
        {
            AssertTimeout();

            for (var dir = 0; dir < 4; dir++)
            {
                var direction = (Direction)dir;
                for (var index = 0; index < 7; index++)
                {
                    var pos = board.Players[0].Vector;
                    var push = new Push(index, direction);
                    board.Push(push);

                    //D($"{direction} {index}");
                    var score = CalcScore(board);
                    if (score > maxScore)
                    {
                        maxScore = score;
                        maxPush = push;
                    }
                    board.Push(new Push(index, direction.Opposite()));

                    //Debug.Assert(pos.Equals(board.Players[0].Vector));
                }
            }
        }
        catch (TimeoutException) { }

        return maxPush;
    }

    private static int CalcScore(Board board)
    {
        var items = board.GetBoardQuestsItems(0).ToArray();

        if (items.Length == 0)
        {
            //D($"no board items: {items.Length}, numPlayerCards: {board.Players[0].NumPlayerCards} items: {board.Items.Length}, quests: {board.Quests.Length}");
            return 0;
        }

        var adjMap = board.CreateAdjacentMap(board.Players[0].Vector);
        var adjItems = items.Count(x => adjMap.ContainsKey(x.Vector));
        var minDist = adjMap.Keys.Min(x => items.Min(j => j.Vector.GetDistance(x)));

        //D($"adj items: {adjItems}, adj cells: {adjMap.Count}, min dist: {minDist}");

        if (adjItems > 0) return adjItems * 1000;

        return minDist * -1;
    }

    public static void D(params object[] p)
    {
        Console.Error.WriteLine(string.Join(" ", p.Select(x => x?.ToString())));
    }
}