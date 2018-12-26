using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

class Player
{
    static readonly Vector[] Turns = new[] { new Vector(1, 0), new Vector(-1, 0), new Vector(0, 1), new Vector(0, -1) };

#if DEBUG
    const string state = @"
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////AgAAAAIAAAACAAAAAgAAAAIAAAACAAAA//////////////////////////////////////////////////////////////////////////////////////////////////////////8CAAAAAgAAAAIAAAACAAAAAgAAAP////////////////////8CAAAA/////////////////////////////////////////////////////////////////////////////////////////////////////wIAAAACAAAA/////////////////////////////////////////////////////wEAAAD/////////////////////////////////////////////////////////////////////////////////////AgAAAAIAAAD//////////////////////////////////////////////////////////wEAAAD/////////////////////////////////////////////////////////////////////////////////////AgAAAP///////////////////////////////////////////////////////////////wEAAAD///////////////8BAAAAAQAAAAEAAAABAAAAAQAAAAEAAAD/////////////////////////////////////AgAAAP///////////////////////////////////////////////////////////////wEAAAABAAAAAQAAAAEAAAABAAAA//////////8BAAAAAQAAAAEAAAD/////////////////////////////////////AgAAAAIAAAACAAAAAgAAAAIAAAACAAAAAgAAAAIAAAACAAAAAgAAAAIAAAACAAAAAgAAAAIAAAACAAAAAgAAAAIAAAACAAAA//////////8BAAAA/////wEAAAD///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////8BAAAA/////wEAAAD///////////////////////////////8DAAAAAwAAAAMAAAADAAAAAwAAAAMAAAADAAAAAwAAAAMAAAADAAAAAwAAAAMAAAD///////////////////////////////////////////////8BAAAA/////wEAAAD///////////////////////////////8DAAAA////////////////////////////////AwAAAAMAAAADAAAAAwAAAAMAAAD///////////////////////////////////////////////8BAAAA/////wEAAAD///////////////////////////////8DAAAAAwAAAAMAAAD///////////////////////////////////////////////////////////////////////////////////////////////8BAAAA/////wEAAAABAAAA/////////////////////////////////////wMAAAADAAAA//////////////////////////////////////////8DAAAA//////////////////////////////////////////8BAAAA//////////8BAAAA//////////////////////////////////////////8DAAAAAwAAAAMAAAADAAAAAwAAAP///////////////wMAAAADAAAA//////////////////////////////////////////8BAAAA//////////8BAAAA//////////////////////////////////////////////////////////8AAAAAAwAAAAMAAAADAAAAAwAAAAMAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD///////////////8BAAAAAQAAAAEAAAABAAAA//////////////////////////////////////////////////////////8AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD//////////////////////////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP///////////////////////////////////////////////////////////////////////////////////////////////wAAAAD/////////////////////////////////////////////////////AAAAAP///////////////////////////////////////////////////////////////////////////////////////////////wAAAAD/////////////////////////////////////////////////////AAAAAP///////////////////////////////////////////////////////////////////////////////////////////////wAAAAD/////////////////////////////////////////////////////AAAAAP///////////////////////////////////////////////////////////////////////////////////////////////wAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAP//////////////////////////AQAAAAQAAAAIAAAADgAAABAAAAADAAAADgAAAAIAAAA=
";
#else
    const string state = null;
#endif

    static void Main(string[] args)
    {
        var board = new Board();
        // game loop
        while (true)
        {
            board.Load(state);
            Log.D(board.Serialize());
            Log.D(board.Me);

            var bestScore = double.MinValue;
            Vector bestTurn = Turns[0];

            foreach (var t in Turns)
            {
                var cell = board.Me + t;
                if (!cell.Valid || !board.IsFree(cell)) continue;

                var saveCell = board.Me;
                board.Me = cell;
                board.Cells[cell.X, cell.Y] = board.MyIndex;

                var score = CalcScore(board);
                Log.D(cell, t.GetName(), score);

                board.Me = saveCell;
                board.Cells[cell.X, cell.Y] = -1;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTurn = t;
                }
            }

            Console.WriteLine(bestTurn.GetName()); // A single line with UP, DOWN, LEFT or RIGHT
        }
    }

    private static void Do(Action<int, int> f)
    {
        for (var y = 0; y < 20; y++) for (var x = 0; x < 30; x++) f(x, y);
    }

    private static double CalcScore(Board board)
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
            foreach (var playerIndex in board.GetOrderedPlayers())
            {
                var nextPositions = new List<Vector>();
                foreach (var turn in Turns)
                {
                    foreach (var p in playerPositions[playerIndex].Last())
                    {
                        var cell = p + turn;
                        if (!cell.Valid || visited[cell.X, cell.Y] >= 0) continue;

                        nextPositions.Add(cell);
                        visited[cell.X, cell.Y] = playerIndex;
                    }
                }

                if (nextPositions.Any())
                {
                    playerPositions[playerIndex].Add(nextPositions);
                    full = false;

                    if (playerIndex == board.MyIndex)
                    {
                        myCells += nextPositions.Count;
                    }
                    else
                    {
                        otherCells += nextPositions.Count;
                        otherCellsDistancesSum += nextPositions.Count * playerPositions[playerIndex].Count;
                    }
                }
            }
        }

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
        if (Y > 0) return "DOWN";
        if (X < 0) return "LEFT";
        return "UP";
    }
}

public class Board
{
    public int[,] Cells = new int[30, 20];
    public Vector[] Players = new Vector[4];
    public int PlayersNumber;
    public int MyIndex;

    public Board()
    {
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

    public IEnumerable<int> GetOrderedPlayers()
    {
        for (var playerOrder = 0; playerOrder < PlayersNumber; playerOrder++)
        {
            yield return (playerOrder + MyIndex) % PlayersNumber;
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
                Do((x, y) => { if (Cells[x, y] == i) Cells[x, y] = -1; });
            }

            Players[i].X = x1;
            Players[i].Y = y1;
        }
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

    static class StateSerializer
    {
        static MemoryStream stream = new MemoryStream((20 * 30 + 1 + 1 + 3 * 2) * sizeof(int));
        static BinaryWriter writer = new BinaryWriter(stream);
        static BinaryReader reader = new BinaryReader(stream);

        public static void Load(Board board, string s)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var buff = Convert.FromBase64String(s);
            stream.Write(buff, 0, buff.Length);

            stream.Seek(0, SeekOrigin.Begin);

            Do((x, y) => board.Cells[x, y] = reader.ReadInt32());

            board.MyIndex = reader.ReadInt32();
            board.PlayersNumber = reader.ReadInt32();
            for (var i = 0; i < 3; i++)
            {
                board.Players[i].X = reader.ReadInt32();
                board.Players[i].Y = reader.ReadInt32();
            }
        }

        public static string Serialize(Board board)
        {
            stream.Seek(0, SeekOrigin.Begin);
            Do((x, y) => writer.Write(board.Cells[x, y]));

            writer.Write(board.MyIndex);
            writer.Write(board.PlayersNumber);
            for (var i = 0; i < 3; i++)
            {
                writer.Write(board.Players[i].X);
                writer.Write(board.Players[i].Y);
            }

            writer.Flush();
            return Convert.ToBase64String(stream.ToArray());
        }
    }
}

public static class Log
{
    public static void D(params object[] o)
    {
        Console.Error.WriteLine(string.Join(" ", o.Select(x => x.ToString())));
    }
}
