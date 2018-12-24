using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

class Player
{
    const string state = @"";
    struct Vector
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

        public int Key => Y * 30 + X;

        public int X;
        public int Y;

        public override string ToString()
        {
            return $"{X}:{Y}";
        }
    }

    static class StateSerializer
    {
        static MemoryStream stream = new MemoryStream((20 * 30 + 1 + 1 + 3 * 2) * sizeof(int));
        static BinaryWriter writer = new BinaryWriter(stream);
        static BinaryReader reader = new BinaryReader(stream);

        public static void Load(string s)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var buff = Convert.FromBase64String(s);
            stream.Write(buff, 0, buff.Length);

            stream.Seek(0, SeekOrigin.Begin);

            for (var y = 0; y < 20; y++)
            {
                for (var x = 0; x < 30; x++)
                {
                    board[x, y] = reader.ReadInt32();
                }
            }

            myIndex = reader.ReadInt32();
            playersNumber = reader.ReadInt32();
            for (var i = 0; i < 3; i++)
            {
                players[i].X = reader.ReadInt32();
                players[i].Y = reader.ReadInt32();
            }
        }

        public static string Serialize()
        {
            stream.Seek(0, SeekOrigin.Begin);
            Do((x, y) => writer.Write(board[x, y]));

            writer.Write(myIndex);
            writer.Write(playersNumber);
            for (var i = 0; i < 3; i++)
            {
                writer.Write(players[i].X);
                writer.Write(players[i].Y);
            }

            writer.Flush();
            return Convert.ToBase64String(stream.ToArray());
        }
    }

    static int[,] board = new int[30, 20];
    static int[,,] distances = new int[30, 20, 4];
    static int[,] voronoy = new int[30, 20];
    static string[] turnsNames = new[] { "UP", "DOWN", "LEFT", "RIGHT" };
    static Vector[] turns = new[] { new Vector(0, -1), new Vector(0, 1), new Vector(-1, 0), new Vector(1, 0) };
    static Vector[] players = new Vector[4];
    static bool[] cleanedPlayers = new bool[4];
    static int playersNumber;
    static int myIndex;

    static int bestScore;

    static void Main(string[] args)
    {
        Do((x, y) => board[x, y] = -1);

        for (var p = 0; p < 4; p++) cleanedPlayers[p] = false;

        // game loop
        while (true)
        {
#if DEBUG
            StateSerializer.Load("/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////wEAAAABAAAAAQAAAAEAAAABAAAAAQAAAAEAAAABAAAAAQAAAAAAAAAAAAAAAAAAAAAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD///////////////////////////////8BAAAAAQAAAAAAAAD//////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD///////////////////////////////8BAAAA/////wAAAAD//////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD///////////////////////////////8BAAAA/////wAAAAD//////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD///////////////////////////////8BAAAA/////wAAAAD//////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD///////////////////////////////8BAAAA/////wAAAAD//////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAABAAAAAQAAAAEAAAD///////////////8BAAAA/////wAAAAD//////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD//////////wEAAAABAAAA//////////8BAAAA/////wAAAAD//////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD///////////////8BAAAAAQAAAAEAAAABAAAAAAAAAAAAAAD//////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD/////////////////////////////////////AAAAAP///////////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD/////////////////////////////////////AAAAAP///////////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD/////////////////////////////////////AAAAAAAAAAAAAAAAAAAAAAAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAD/////////////////////////////////////AAAAAAAAAAD//////////wAAAAD//////////////////////////////////////////////////////////////////////////////////////////wEAAAABAAAAAQAAAAEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAD/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////AAAAAAIAAAAUAAAAEgAAABMAAAASAAAAAAAAAAAAAAA=");
#else
            LoadState();
#endif
            D(players[myIndex]);
            D(StateSerializer.Serialize());

            bestScore = 0;
            var bestTurnIndex = FindBestTurnIndex();

            Console.WriteLine(turnsNames[bestTurnIndex]); // A single line with UP, DOWN, LEFT or RIGHT
        }
    }

    private static int FindBestTurnIndex()
    {
        var bestTurnIndex = 0;
        var bestScore = int.MinValue;
        for (var i = 0; i < 4; i++)
        {
            var cell = players[myIndex] + turns[i];

            if (!IsValid(cell)) continue;

            //D(board[cell.X, cell.Y]);
            //DB((x, y) => board[x, y]);

            var score = SimulateMyMove(cell);

            D(turnsNames[i], score);
            //DB((x, y) => voronoy[x, y]);

            if (score > bestScore)
            {
                bestTurnIndex = i;
                bestScore = score;
            }
        }

        //DB((x, y) => distances[x, y,2]);

        return bestTurnIndex;
    }

    private static void LoadState()
    {
        if (state.Trim(' ', '\n', '\r').Length > 0)
        {
            StateSerializer.Load(state.Trim(' ', '\n', '\r'));
            return;
        }

        var inputs = Console.ReadLine().Split(' ');
        playersNumber = int.Parse(inputs[0]); // total number of players (2 to 4).
        myIndex = int.Parse(inputs[1]); // your player number (0 to 3).

        for (int i = 0; i < playersNumber; i++)
        {
            inputs = Console.ReadLine().Split(' ');
            int x0 = int.Parse(inputs[0]); // starting X coordinate of lightcycle (or -1)
            int y0 = int.Parse(inputs[1]); // starting Y coordinate of lightcycle (or -1)
            int x1 = int.Parse(inputs[2]); // starting X coordinate of lightcycle (can be the same as X0 if you play before this player)
            int y1 = int.Parse(inputs[3]); // starting Y coordinate of lightcycle (can be the same as Y0 if you play before this player)

            players[i].X = x1;
            players[i].Y = y1;

            if (x1 < 0)
            {
                ClearDead(i);
                continue;
            }

            board[x1, y1] = i;
        }
    }

    private static void ClearDead(int i)
    {
        if (!cleanedPlayers[i])
        {
            D("dead: ", i);
            cleanedPlayers[i] = true;

            for (var y = 0; y < 20; y++)
                for (var x = 0; x < 30; x++)
                    if (board[x, y] == i)
                    {
                        board[x, y] = -1;
                        voronoy[x, y] = -1;
                        distances[x, y, i] = 99;
                    }
        }
    }

    private static int SimulateMyMove(Vector cell)
    {
        var savedMyPosition = players[myIndex];

        players[myIndex] = cell;
        board[cell.X, cell.Y] = myIndex;

        for (var i = 0; i < playersNumber; i++)
        {
            CalcDistances(i);
        }

        var score = CalcScore();

        players[myIndex] = savedMyPosition;
        board[cell.X, cell.Y] = -1;

        return score;
    }

    private static int CalcScore()
    {
        var myArea = 0;
        var enemyArea = 0;
        var enemyDists = 0;

        for (var y = 0; y < 20; y++)
        {
            for (var x = 0; x < 30; x++)
            {
                voronoy[x, y] = -1;

                if (board[x, y] >= 0) continue;

                var minIndex = myIndex;
                for (var i = 0; i < playersNumber; i++)
                {
                    if (players[i].X < 0) continue;

                    if (distances[x, y, i] < distances[x, y, minIndex])
                    {
                        minIndex = i;
                    }
                }

                if (distances[x, y, minIndex] != int.MaxValue)
                {
                    voronoy[x, y] = minIndex;

                    if (minIndex != myIndex)
                    {
                        enemyArea++;
                        enemyDists += distances[x, y, minIndex];
                    }
                    else
                    {
                        myArea++;
                    }
                }
            }
        }
        return myArea * 10000000 + enemyArea * -100000;// + enemyDists;
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

    private static void D(params object[] o)
    {
        Console.Error.WriteLine(string.Join(" ", o.Select(x => x.ToString())));
    }

    private static bool IsValid(Vector cell)
    {
        return cell.X >= 0 && cell.X < 30 && cell.Y >= 0 && cell.Y < 20 && board[cell.X, cell.Y] < 0;
    }

    private static void CalcDistances(int playerIndex)
    {
        Do((x, y) => distances[x, y, playerIndex] = int.MaxValue);

        if (players[playerIndex].X < 0) return;

        var cell = players[playerIndex];
        distances[cell.X, cell.Y, playerIndex] = 0;

        var toVisit = new Queue<Vector>();
        toVisit.Enqueue(cell);
        while (toVisit.Any())
        {
            cell = toVisit.Dequeue();
            var distance = distances[cell.X, cell.Y, playerIndex] + 1;
            foreach (var t in turns.Select(x => x + cell).Where(IsValid))
            {
                var d = distances[t.X, t.Y, playerIndex];

                if (d == int.MaxValue) toVisit.Enqueue(t);

                if (d > distance) distances[t.X, t.Y, playerIndex] = distance;
            }
        }
    }

    private static void Do(Action<int, int> f)
    {
        for (var y = 0; y < 20; y++) for (var x = 0; x < 30; x++) f(x, y);
    }
}
