//move to the tile where 1 shift to goal

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

public class XmaxRush
{
    public static Random Rnd = new Random();
    static Stopwatch timer = new Stopwatch();
    static int ticks = 0;

    static void Main(string[] args)
    {
        var board = new Board();
        var lastPush = "";
        var samePushes = 0;

        while (true)
        {
//#if DEBUG
//            board.Deserialize("AAYDCgUHBQYOCgkJCgsKBQoHBw0JDQoDCQoGDAoHBgcNDQoFCg4KBgYKCwkFDQUKDAkAAAAAAAAAAAwAAAAGBgAAAAYAAAAMAAAACRgAAAAEAAAAQk9PSwEAAAABAAAAAAAAAAEHAAAARElBTU9ORAIAAAAAAAAAAAAAAAAGAAAAU0NST0xMAQAAAAAAAAABAAAAAAQAAABGSVNIAgAAAAQAAAABAAAAAAUAAABTV09SRAQAAAAEAAAAAQAAAAAEAAAATUFTSwQAAAABAAAAAAAAAAAEAAAAQ0FORQAAAAABAAAAAAAAAAEFAAAAQ0FORFkFAAAAAAAAAAEAAAAABQAAAEFSUk9XAAAAAAQAAAAAAAAAAQQAAABCT09LBQAAAAUAAAABAAAAAQYAAABQT1RJT04AAAAABQAAAAEAAAAABwAAAERJQU1PTkQEAAAABgAAAAEAAAAABgAAAFNISUVMRAAAAAAGAAAAAQAAAAADAAAAS0VZBgAAAAQAAAAAAAAAAAUAAABTV09SRAIAAAACAAAAAAAAAAAGAAAAU0hJRUxEBgAAAAAAAAAAAAAAAAQAAABNQVNLAgAAAAUAAAABAAAAAAQAAABDQU5FBgAAAAUAAAABAAAAAQYAAABTQ1JPTEwFAAAABgAAAAAAAAAABQAAAEFSUk9XBgAAAAIAAAABAAAAAQMAAABLRVkAAAAAAgAAAAEAAAAABgAAAFBPVElPTgYAAAABAAAAAAAAAAAFAAAAQ0FORFkBAAAABgAAAAAAAAAABAAAAEZJU0gEAAAAAgAAAAAAAAAA");
//#else
            board.Load();
            timer.Restart();
            Console.Error.WriteLine(board.Serialize());
//#endif
            ticks = 0;

            D(board.Players[0].NumPlayerCards, board.Players[1].NumPlayerCards);

            var solver = new Solver(board);
            var turn = solver.FindBestTurn();

            if (!board.MoveTurn)
            {
                if (turn == lastPush)
                {
                    samePushes++;
                    D("same push", samePushes);

                    if (samePushes > 3 && board.Players[0].NumPlayerCards >= board.Players[1].NumPlayerCards)
                    {
                        D("break clinch");
                        turn = Turn.RandomPush().ToString();
                        lastPush = turn;
                        samePushes = 0;
                    }
                }
                else
                {
                    lastPush = turn;
                    samePushes = 0;
                }
            }

            Console.WriteLine(turn);
        }
    }

    public static void AssertTimeout()
    {
        ticks++;
        if (timer.ElapsedMilliseconds > 45)
        {
            Console.Error.WriteLine($"timeout {ticks}, {timer.ElapsedMilliseconds}");
            throw new TimeoutException();
        }
    }

    public static void D(params object[] p)
    {
        Console.Error.WriteLine(string.Join(" ", p.Select(x => x?.ToString())));
    }
}

[Flags]
public enum Direction
{
    Up = 0b1000,
    Right = 0b0100,
    Down = 0b0010,
    Left = 0b0001
}

static class DirectionExt
{
    public static Direction Opposite(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return Direction.Down;
            case Direction.Right:
                return Direction.Left;
            case Direction.Down:
                return Direction.Up;
            case Direction.Left:
                return Direction.Right;
            default:
                throw new ArgumentException();
        }
    }

    public static string AsStr(this Direction dir)
    {
        switch (dir)
        {
            case Direction.Up:
                return "UP";
            case Direction.Right:
                return "RIGHT";
            case Direction.Down:
                return "DOWN";
            case Direction.Left:
                return "LEFT";
            default:
                throw new ArgumentException();
        }
    }
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

public struct Tile
{
    public readonly byte Value;

    public Tile(byte v)
    {
        Value = v;
    }

    public Tile(string input)
    {
        Value = 0;
        if (input[0] == '1')
        {
            Value |= (byte)Direction.Up;
        }

        if (input[1] == '1')
        {
            Value |= (byte)Direction.Right;
        }

        if (input[2] == '1')
        {
            Value |= (byte)Direction.Down;
        }

        if (input[3] == '1')
        {
            Value |= (byte)Direction.Left;
        }
    }

    public IEnumerable<Direction> GetDirections()
    {
        if ((Value & (byte)Direction.Up) > 0) yield return Direction.Up;
        if ((Value & (byte)Direction.Down) > 0) yield return Direction.Down;
        if ((Value & (byte)Direction.Left) > 0) yield return Direction.Left;
        if ((Value & (byte)Direction.Right) > 0) yield return Direction.Right;
    }

    internal bool Has(Direction dir)
    {
        return (Value & (byte)dir) > 0;
    }

    public override string ToString()
    {
        return ToChar().ToString();
    }

    public char ToChar()
    {
        switch (Value)
        {
            case 0b0011:
                return '┐';
            case 0b1001:
                return '┘';
            case 0b0101:
                return '─';
            case 0b0110:
                return '┌';
            case 0b1100:
                return '└';
            case 0b1010:
                return '│';
            case 0b0111:
                return '┬';
            case 0b1101:
                return '┴';
            case 0b1110:
                return '├';
            case 0b1011:
                return '┤';
            case 0b1111:
                return '┼';
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

    public Player()
    {
    }

    public Player(Player p)
    {
        Vector = p.Vector;
        NumPlayerCards = p.NumPlayerCards;
        Tile = p.Tile;
    }
}

public class Item
{
    public string Name;
    public Vector Vector;
    public int PlayerId;
    public bool Quest;

    public Item()
    {
    }

    public Item(Item other)
    {
        Name = other.Name;
        Vector = other.Vector;
        PlayerId = other.PlayerId;
        Quest = other.Quest;
    }
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
    public List<Item> Items;

    public Board Clone()
    {
        var clone = new Board();
        clone.MoveTurn = MoveTurn;

        for (var y = 0; y < 7; y++)
            for (var x = 0; x < 7; x++)
                clone.Cells[x, y] = Cells[x, y];

        clone.Players = new[] { new Player(Players[0]), new Player(Players[1]) };
        clone.Items = Items.Select(x => new Item(x)).ToList();

        return clone;
    }

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
        Items = new List<Item>();
        for (int i = 0; i < numItems; i++)
        {
            var inputs = ReadLine().Split(' ');
            Items.Add(new Item
            {
                Name = inputs[0],
                Vector = new Vector(int.Parse(inputs[1]), int.Parse(inputs[2])),
                PlayerId = int.Parse(inputs[3])
            });
        }

        int numQuests = int.Parse(ReadLine()); // the total number of revealed quests for both players
        for (int i = 0; i < numQuests; i++)
        {
            var inputs = ReadLine().Split(' ');

            var playerId = int.Parse(inputs[1]);
            Items.First(x => x.Name == inputs[0] && x.PlayerId == playerId).Quest = true;
        }
    }

    public IEnumerable<Item> GetBoardQuestsItems(int playerId)
    {
        return Items.Where(x => x.Quest && x.PlayerId == 0 && x.Vector.X >= 0);
    }

    public void Move(int playerIndex, Direction dir)
    {
        var v = Players[playerIndex].Vector.Move(dir, false);

        if (!v.IsValid()) throw new InvalidOperationException();

        Players[playerIndex].Vector = v;
        var item = GetBoardQuestsItems(playerIndex).FirstOrDefault(x => x.Vector.Equals(v));
        if (item != null)
        {
            item.Quest = false;
            Players[playerIndex].NumPlayerCards--;
            //todo: add new quest
        }
    }

    public void Push(int index, Direction direction)
    {
        switch (direction)
        {
            case Direction.Up:
                PushUp(index);
                break;
            case Direction.Right:
                PushRight(index);
                break;
            case Direction.Down:
                PushDown(index);
                break;
            case Direction.Left:
                PushLeft(index);
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

            writer.Write(Items.Count);
            for (var i = 0; i < Items.Count; i++)
            {
                writer.Write(Items[i].Name.Length);
                writer.Write(Items[i].Name.ToCharArray());
                writer.Write(Items[i].Vector.X);
                writer.Write(Items[i].Vector.Y);
                writer.Write(Items[i].PlayerId);
                writer.Write(Items[i].Quest);
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

            Items = new List<Item>();
            var itemsCount = reader.ReadInt32();
            for (var i = 0; i < itemsCount; i++)
            {
                var len = reader.ReadInt32();
                var name = new string(reader.ReadChars(len));

                var x = reader.ReadInt32();
                var y = reader.ReadInt32();
                Items.Add(new Item
                {
                    Name = name,
                    Vector = new Vector(x, y),
                    PlayerId = reader.ReadInt32(),
                    Quest = reader.ReadBoolean()
                });
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

    internal IEnumerable<(Vector, Direction)> GetAdjacentDirections(Vector cell)
    {
        foreach (var dir in Cells[cell.X, cell.Y].GetDirections())
        {
            var v = cell.Move(dir, false);

            if (!v.IsValid()) continue;
            if (!Cells[v.X, v.Y].Has(dir.Opposite())) continue;

            yield return (v, dir);
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

    public override string ToString()
    {
        var s = new StringBuilder();

        for (var y = 0; y < 7; y++)
        {
            for (var x = 0; x < 7; x++)
            {
                s.Append(Cells[x, y].ToChar());
            }
            s.Append(Environment.NewLine);
        }

        return s.ToString();
    }
}

//move to the tile where 1 shift to goal

public enum TurnType
{
    Pass,
    Move,
    Push
}

//move to the tile where 1 shift to goal

public class Solver
{
    private readonly Board originBoard;
    Turn[] maxTurns;
    public Turn[] NextTurns;
    double maxScore = double.MinValue;

    public Solver(Board board)
    {
        this.originBoard = board;
    }

    public string FindBestTurn()
    {
        var turns = new Stack<Turn>();

        try
        {
            SimulateMoves(originBoard, turns, 2);
        }
        catch (TimeoutException) { }

        XmaxRush.D(maxTurns);

        return maxTurns.Last().ToString();
    }

    private static int CalcScore(Board board, int playerId)
    {
        var adjItems = GetAdjacentItems(board, playerId);

        return
            (12 - board.Players[0].NumPlayerCards) * 100
            + adjItems * 90
            //+ adjMap.Count
            ;
    }

    static int GetAdjacentItems(Board board, int playerId)
    {
        var adjMap = board.CreateAdjacentMap(board.Players[playerId].Vector);
        var items = board.GetBoardQuestsItems(playerId).ToArray();
        return items.Count(x => adjMap.ContainsKey(x.Vector));
    }

    public void SimulateMoves(Board board, Stack<Turn> turns, int deep)
    {
        if (turns.Any())
        {
            var score = CalcScore(board, 0);

            if (score > maxScore)
            {
                maxScore = score;
                NextTurns = maxTurns;
                maxTurns = turns.ToArray();
                //XmaxRush.D(maxTurns);
                //XmaxRush.D(maxScore);
            }
        }

        if (deep == 0) return;

        XmaxRush.AssertTimeout();

        IEnumerable<Turn> turnsToEvaluate;
        if (board.MoveTurn)
        {
            turnsToEvaluate = EnumMoves(board).ToArray();
        }
        else
        {
            turnsToEvaluate = EnumPushes(board);
        }

        foreach (var t in turnsToEvaluate)
        {
            var newBoard = board.Clone();
            t.Apply(newBoard);
            turns.Push(t);
            SimulateMoves(newBoard, turns, deep - 1);
            turns.Pop();
        }
    }

    private IEnumerable<Turn> EnumPushes(Board board)
    {
        for (var index = 0; index < 7; index++)
        {
            yield return Turn.Push(index, Direction.Up);
            yield return Turn.Push(index, Direction.Right);
            yield return Turn.Push(index, Direction.Down);
            yield return Turn.Push(index, Direction.Left);
        }
    }

    private IEnumerable<Turn> EnumMoves(Board board)
    {
        var items = board.GetBoardQuestsItems(0).ToArray();

        var adjMap = board.CreateAdjacentMap(board.Players[0].Vector);
        var adjItems = items
            .Where(x => adjMap.ContainsKey(x.Vector))
            .OrderBy(x => adjMap[x.Vector]).ToArray();

        var targets = new List<Vector>();
        if (adjItems.Any())
        {
            var start = board.Players[0].Vector;
            var adjPath = new List<Direction>();
            foreach (var t in adjItems)
            {
                var p = FindPath(board, start, t.Vector);
                adjPath.AddRange(p);
                start = t.Vector;
            }

            yield return Turn.Move(adjPath.Take(20));

            targets.AddRange(adjItems.Select(x => x.Vector));
        }

        targets.AddRange(adjMap.Keys.Except(targets));

        var startCell = board.Players[0].Vector;
        foreach (var t in targets)
        {
            var p = FindPath(board, startCell, t);
            if (p.Count() > 0)
                yield return Turn.Move(p.Take(20));
        }

        yield return Turn.Pass();
    }

    public static IEnumerable<Direction> FindPath(Board board, Vector start, Vector target)
    {
        var path = new Stack<Direction>();
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
            path.Push(minDir);
            curCell = minCell;
        }

        return path;
    }
}

public class Turn
{
    TurnType turnType;
    int index;
    IEnumerable<Direction> direcitons;
    Direction direction;

    internal static Turn Pass()
    {
        return new Turn { turnType = TurnType.Pass };
    }

    internal static Turn Move(IEnumerable<Direction> steps)
    {
        return new Turn { turnType = TurnType.Move, direcitons = steps };
    }

    internal static Turn Push(int index, Direction direction)
    {
        return new Turn { turnType = TurnType.Push, index = index, direction = direction };
    }

    public void Apply(Board board)
    {
        board.MoveTurn = !board.MoveTurn;

        if (turnType == TurnType.Move)
        {
            foreach (var d in direcitons) board.Move(0, d);
            return;
        }

        if (turnType == TurnType.Push)
        {
            board.Push(index, direction);
            return;
        }
    }

    public override string ToString()
    {
        switch (turnType)
        {
            case TurnType.Pass:
                return "PASS";
            case TurnType.Move:
                return "MOVE " + string.Join(" ", direcitons.Select(x => x.AsStr()));
            case TurnType.Push:
                return $"PUSH {index} {direction.AsStr()}";
            default:
                throw new ArgumentException();
        }
    }

    internal static Turn RandomPush()
    {
        return new Turn
        {
            turnType = TurnType.Push,
            direction = (Direction)(1 << XmaxRush.Rnd.Next(4)),
            index = XmaxRush.Rnd.Next(7)
        };
    }
}