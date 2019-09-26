using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Diagnostics;

class TronBattle
{
    private static string GetDirectionName(Vector orig, Vector dest)
    {
        if (orig.X < dest.X) return "RIGHT";
        if (orig.X > dest.X) return "LEFT";
        if (orig.Y < dest.Y) return "DOWN";
        return "UP";
    }

    public static readonly Vector[] Turns = new[] { new Vector(1, 0), new Vector(-1, 0), new Vector(0, 1), new Vector(0, -1) };
    public static Stopwatch timer = new Stopwatch();

#if DEBUG
    const string state = @"
//////////////////8AAAAAAAAAAAAAAAAAAAAAAgICAgICAgICAgICAgIAAAAAAAAAAAAAAAAAAAAAAgICAgICAgIC/////wIAAAAAAAAAAAAAAAAAAAAAAQEBAgICAgIC////AgIA//8AAAAAAAAAAAAAAAAAAQEBAgICAv//////Av8AAAAAAAAAAAAAAAAAAAAA/wEB////AgICAgL/AgICAgD//wAAAAAAAAAAAAAA/wEB/wICAgICAgL//wICAgAA/////////////////wEB/wICAgICAgICAgICAv8A/////////////////wEBAgICAgICAv//AgICAv8A/////////////////wEBAgICAgICAgICAgICAv8A/////////////////wEBAgIBAQEBAQEBAf8CAv8A/////////////////wEBAgL///////8BAQICAQEA/////////////////wEBAQEBAQEB//8BAgIBAQEA/////////////////wEBAQEBAQEBAQEBAgIB/wEA/////////////////////////wEBAQEBAQEB/wEA/////////////////////////wEBAQH/AQEBAQEA/////////////////////////////wEBAQEBAQEA/////////////////////////////wEBAQEBAQEA////////////////////////////AQEBAQEBAQEA////////////////////////////AQEBAQEBAQEA////////////////AgAAAAQAAAATAAAABQAAAAkAAAASAAAACQAAAAgAAAD//////////w==
";
#else
    const string state = null;
#endif

    static Vector bestTurn;
    private static int ticks;

    static void Main(string[] args)
    {
        var board = new Board();
        // game loop
        while (true)
        {
            board.Load(state);
            Log.D(board.Serialize());
            //Log.D(board.Me);

#if !DEBUG
            ticks = 0;
            timer.Restart();
#endif
            try
            {
                FindBestTurn(board, board.MyIndex, 0, true);
            }
            catch (TimeoutException) { }

            Console.WriteLine(GetDirectionName(board.Me, bestTurn)); // A single line with UP, DOWN, LEFT or RIGHT
        }
    }

    private static double FindBestTurn(Board board, int playerId, int depth, bool isMax)
    {
        AssertTimeout();

        if (depth == 4)
        {
            return CalcScore(board, playerId);
        }

        var bestScore = isMax ? double.MinValue : double.MaxValue;
        var neighbours = board.GetFreeNeighbours(playerId).ToArray();

        foreach (var cell in neighbours)
        {
            var testBoard = new Board(board);
            testBoard.Players[playerId] = cell;
            testBoard.Cells[cell.X, cell.Y] = playerId;

            if( neighbours.Length == 1)
            {
                return CalcScore(testBoard, playerId);
            }

            var nextPlayer = testBoard.MyIndex;
            if (testBoard.MyIndex == playerId)
            {
                nextPlayer = testBoard.FindClosestPlayerId();
            }

            var score = FindBestTurn(testBoard, nextPlayer, depth + 1, nextPlayer == testBoard.MyIndex);

            if (isMax)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    if (depth == 0) bestTurn = cell;
                }
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                }
            }
        }

        return bestScore;
    }

    private static void AssertTimeout()
    {
        ticks++;
        if (timer.ElapsedMilliseconds > 95)
        {
            Log.D("timeout:", ticks);
            throw new TimeoutException();
        }
    }

    private static void Do(Action<int, int> f)
    {
        for (var y = 0; y < 20; y++) for (var x = 0; x < 30; x++) f(x, y);
    }

    private static double CalcScore(Board board, int playerIndex)
    {
        var playerPositions = board.Players.Select(x => new List<List<Vector>>() { new List<Vector>() { x } }).ToArray();
        var visited = (int[,])board.Cells.Clone();
        var full = false;
        var myCells = 0;
        var otherCells = 0;
        var otherCellsDistancesSum = 0;

        while (!full)
        {
            full = true;
            foreach (var pi in board.GetOrderedPlayers(playerIndex))
            {
                var nextPositions = new List<Vector>();
                foreach (var turn in Turns)
                {
                    foreach (var p in playerPositions[pi].Last())
                    {
                        var cell = p + turn;
                        if (!cell.Valid || visited[cell.X, cell.Y] >= 0) continue;

                        nextPositions.Add(cell);
                        visited[cell.X, cell.Y] = pi;
                    }
                }

                if (nextPositions.Any())
                {
                    playerPositions[pi].Add(nextPositions);
                    full = false;

                    if (pi == board.MyIndex)
                    {
                        myCells += nextPositions.Count;
                    }
                    else
                    {
                        otherCells += nextPositions.Count;
                        otherCellsDistancesSum += nextPositions.Count * playerPositions[pi].Count;
                    }
                }
            }
        }

        if (myCells == 0) return double.MinValue;

        return myCells * 10000000.0 + otherCells * -100000.0 + otherCellsDistancesSum;
    }
}

public struct Vector : IEqualityComparer<Vector>
{
    public static Vector operator +(Vector v1, Vector v2)
    {
        return new Vector(v1.X + v2.X, v1.Y + v2.Y);
    }

    public Vector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public int X;
    public int Y;

    public bool Valid => X >= 0 && X < 30 && Y >= 0 && Y < 20;

    public override string ToString()
    {
        return $"{X}:{Y}";
    }

    public bool Equals(Vector a, Vector b)
    {
        return a.X == b.X && a.Y == b.Y;
    }

    public int GetHashCode(Vector obj)
    {
        return Y * 30 + X;
    }

    public string GetName()
    {
        if (X > 0) return "RIGHT";
        if (Y > 0) return "DOWN ";
        if (X < 0) return "LEFT ";
        return "UP   ";
    }
}

public class Board
{
    public int[,] Cells;
    public Vector[] Players = new Vector[4];
    public int PlayersNumber;
    public int MyIndex;

    public Board(Board board) : this()
    {
        Cells = (int[,])board.Cells.Clone();
        Players = (Vector[])board.Players.Clone();
        PlayersNumber = board.PlayersNumber;
        MyIndex = board.MyIndex;
    }

    public Board()
    {
        Cells = new int[30, 20];
        Do((x, y) => Cells[x, y] = -1);
    }

    public Vector Me
    {
        get
        {
            return Players[MyIndex];
        }
        set
        {
            Players[MyIndex].X = value.X;
            Players[MyIndex].Y = value.Y;
        }
    }

    public IEnumerable<Vector> GetFreeNeighbours(int playerIndex)
    {
        foreach (var t in TronBattle.Turns)
        {
            var cell = Players[playerIndex] + t;
            if (cell.Valid && IsFree(cell))
            {
                yield return cell;
            }
        }
    }

    public IEnumerable<int> GetOrderedPlayers(int playerIndex)
    {
        for (var playerOrder = 0; playerOrder < PlayersNumber; playerOrder++)
        {
            yield return (playerOrder + playerIndex) % PlayersNumber;
        }
    }

    public string Serialize()
    {
        return StateSerializer.Serialize(this);
    }

    public void Load(string state)
    {
        if (state != null)
        {
            StateSerializer.Load(this, state.Trim(' ', '\n', '\r'));
            return;
        }

        var inputs = Console.ReadLine().Split(' ');
        PlayersNumber = int.Parse(inputs[0]); // total number of players (2 to 4).
        MyIndex = int.Parse(inputs[1]); // your player number (0 to 3).

        for (int i = 0; i < PlayersNumber; i++)
        {
            var s = Console.ReadLine();
            inputs = s.Split(' ');
            int x0 = int.Parse(inputs[0]); // starting X coordinate of lightcycle (or -1)
            int y0 = int.Parse(inputs[1]); // starting Y coordinate of lightcycle (or -1)
            int x1 = int.Parse(inputs[2]); // starting X coordinate of lightcycle (can be the same as X0 if you play before this player)
            int y1 = int.Parse(inputs[3]); // starting Y coordinate of lightcycle (can be the same as Y0 if you play before this player)

            if (x1 >= 0)
            {
                Cells[x1, y1] = i;
                Cells[x0, y0] = i;
            }
            else if (Players[i].X >= 0)
            {
                Log.D("dead:", i);
                ClearPlayer(i);
            }

            Players[i].X = x1;
            Players[i].Y = y1;
        }
    }

    public void ClearPlayer(int playerId)
    {
        Do((x, y) => { if (Cells[x, y] == playerId) Cells[x, y] = -1; });
    }

    private static void DB(Func<int, int, int> f)
    {
        for (var y = 0; y < 20; y++)
        {
            for (var x = 0; x < 30; x++)
            {
                var d = f(x, y);
                if (d > 99) d = 99;
                Console.Error.Write($"{d,3}");
            }
            Console.Error.WriteLine();
        }
    }

    private static void Do(Action<int, int> f)
    {
        for (var y = 0; y < 20; y++) for (var x = 0; x < 30; x++) f(x, y);
    }

    internal bool IsFree(Vector cell)
    {
        return Cells[cell.X, cell.Y] < 0;
    }

    internal int FindClosestPlayerId()
    {
        var index = -1;
        var minDist = int.MaxValue;
        for (var i = 0; i < PlayersNumber; i++)
        {
            if (Players[i].X >= 0 && i != MyIndex)
            {
                var d = Math.Abs(Me.X - Players[i].X) + Math.Abs(Me.Y - Players[i].Y);
                if (d < minDist)
                {
                    minDist = d;
                    index = i;
                }
            }
        }
        return index;
    }

    static class StateSerializer
    {
        static MemoryStream stream = new MemoryStream((20 * 30) * sizeof(sbyte) + (1 + 1 + 4 * 2) * sizeof(int));
        static BinaryWriter writer = new BinaryWriter(stream);
        static BinaryReader reader = new BinaryReader(stream);

        public static void Load(Board board, string s)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var buff = Convert.FromBase64String(s);
            stream.Write(buff, 0, buff.Length);

            stream.Seek(0, SeekOrigin.Begin);

            Do((x, y) => board.Cells[x, y] = (sbyte)reader.ReadByte());

            board.MyIndex = reader.ReadInt32();
            board.PlayersNumber = reader.ReadInt32();
            for (var i = 0; i < 4; i++)
            {
                board.Players[i].X = reader.ReadInt32();
                board.Players[i].Y = reader.ReadInt32();
            }
        }

        public static string Serialize(Board board)
        {
            stream.Seek(0, SeekOrigin.Begin);
            Do((x, y) => writer.Write((sbyte)board.Cells[x, y]));

            writer.Write(board.MyIndex);
            writer.Write(board.PlayersNumber);
            for (var i = 0; i < 4; i++)
            {
                writer.Write(board.Players[i].X);
                writer.Write(board.Players[i].Y);
            }

            writer.Flush();
            return Convert.ToBase64String(stream.ToArray());
        }
    }
}

class TurnsPermutator : IEnumerable<int[]>
{
    private int len;
    private int[] turns;

    public TurnsPermutator(int len)
    {
        this.len = len;
        this.turns = new int[len];

        for (int i = 0; i < len; i++)
        {
            turns[i] = 0;
        }
    }

    public IEnumerator<int[]> GetEnumerator()
    {
        while (IncTurn())
        {
            yield return turns;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private bool IncTurn()
    {
        turns[0]++;
        for (int i = 0; i < len - 1; i++)
        {
            if (turns[i] > 3)
            {
                turns[i + 1]++;
                turns[i] = 0;
            }
            else
            {
                break;
            }
        }

        return turns[len - 1] < 4;
    }
}

public static class Log
{
    public static void D(params object[] o)
    {
        Console.Error.WriteLine(string.Join(" ", o.Select(x => x.ToString())));
    }
}
