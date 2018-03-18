using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using MapPointSet = System.Collections.Generic.HashSet<MapPoint>;
using System.Diagnostics;

class LabPoints
{
    public const char WALL = '#';
    public const char HOLLOW = '.';
    public const char START_POINT = 'T';
    public const char CONTROL_ROOM = 'C';
    public const char UNKNOW = '?';
}

class Directions
{
    public const string UP = "UP";
    public const string DOWN = "DOWN";
    public const string LEFT = "LEFT";
    public const string RIGHT = "RIGHT";
}

class PathFinder
{
    bool[,] maze;
    int[,] lenMatrix;
    int maxRow;
    int maxCol;

    public PathFinder(List<string> Rows, Func<char, bool> freeCellFunc)
    {
        maxRow = Rows.Count() - 1;
        maxCol = Rows[0].Length - 1;
        maze = new bool[maxRow + 1, maxCol + 1];
        lenMatrix = new int[maxRow + 1, maxCol + 1];

        int row = 0;
        foreach (var r in Rows)
        {
            int column = 0;
            foreach (char c in r)
            {
                maze[row, column] = freeCellFunc(c);
                lenMatrix[row, column] = int.MaxValue;
                column++;
            }
            row++;
        }

        //DebugMaze();
    }

    int GetPointValue(MapPoint p)
    {
        if (!IsValid(p.Row, p.Column))
        {
            return int.MaxValue;
        }

        return lenMatrix[p.Row, p.Column];
    }

    public void DebugMaze()
    {
        Console.Error.WriteLine("DebugLenMatrix");
        for (int row = 0; row <= maxRow; row++)
        {
            for (int column = 0; column <= maxCol; column++)
            {
                if (maze[row,column])
                {
                    Console.Error.Write("1");
                }
                else
                {
                    Console.Error.Write("0");
                }
            }
            Console.Error.WriteLine();
        }
    }

    public void DebugLenMatrix()
    {
        Console.Error.WriteLine("DebugLenMatrix");
        for (int row = 0; row <= maxRow; row++)
        {
            for (int column = 0; column <= maxCol; column++)
            {
                var l = lenMatrix[row, column];
                if (l == int.MaxValue)
                {
                    Console.Error.Write("   ");
                }
                else
                {
                    Console.Error.Write("{0,3}", l);
                }
            }
            Console.Error.WriteLine();
        }
    }

    public List<MapPoint> FindPath(MapPoint start, MapPoint finish)
    {
        lenMatrix[start.Row, start.Column] = 0;

        CalcLen(start);

        if (GetPointValue(finish) == int.MaxValue)
        {
            return null;
        }

        var path = new List<MapPoint>();
        var minPoint = new MapPoint(finish);
        path.Add(finish);

        while (!start.Equals(minPoint))
        {
            var p = FindMinLenAround(minPoint);

            //Debug.Assert(!p.Equals(minPoint));

            path.Add(p);
            minPoint = p;
        }

        path.Reverse();
        path.RemoveAt(0);

        return path;
    }

    private MapPoint FindMinLenAround(MapPoint start)
    {
        var min = new MapPoint(start);

        for (int i = -1; i <= 1; i += 2)
        {
            var p = new MapPoint(start.Row + i, start.Column);
            if (GetPointValue(p) < GetPointValue(min))
            {
                min = p;
            }

            p = new MapPoint(start.Row, start.Column + i);
            if (GetPointValue(p) < GetPointValue(min))
            {
                min = p;
            }
        }

        return min;
    }

    bool ShouldCalcLen(MapPoint p, Queue<MapPointLen> q)
    {
        if (q.Any(x => x.Equals(p)))
            return false;

        return ShouldCalcLen(p.Row, p.Column);
    }

    bool ShouldCalcLen(int row, int column)
    {
        if (!IsValid(row, column))
        {
            return false;
        }

        if (!maze[row, column])
        {
            return false;
        }

        if (lenMatrix[row, column] != int.MaxValue)
        {
            return false;
        }

        return true;
    }

    void SetLenMatrix(MapPoint p, int value)
    {
        lenMatrix[p.Row, p.Column] = value;
    }

    class MapPointLen: MapPoint
    {
        public int Length;

        public MapPointLen(MapPoint p, int len)
            : base(p)
        {
            Length = len;
        }
    }

    static void Debug(string s)
    {
        Console.Error.WriteLine(s);
    }

    private void CalcLen(MapPoint p)
    {
        var queue = new Queue<MapPointLen>();

        queue.Enqueue(new MapPointLen(p, 0));

        while (queue.Count() > 0)
        {
            var ml = queue.Dequeue();
            SetLenMatrix(ml, ml.Length);

            Debug($"{ml} ({ml.Length})");

            for (int i = -1; i <= 1; i += 2)
            {
                MapPoint pp;

                pp = new MapPoint(ml);
                pp.Row += i;
                if (ShouldCalcLen(pp, queue))
                {
                    queue.Enqueue(new MapPointLen(pp, ml.Length + 1));
                }

                pp = new MapPoint(ml);
                pp.Column += i;
                if (ShouldCalcLen(pp, queue))
                {
                    queue.Enqueue(new MapPointLen(pp, ml.Length + 1));
                }
            }
        }
    }

    bool IsValid(int row, int column)
    {
        if (row < 0)
            return false;

        if (column < 0)
            return false;

        if (row >= maxRow)
            return false;

        if (column >= maxCol)
            return false;

        return true;
    }
}

class MapPoint
{
    public override bool Equals(object obj)
    {
        var p = obj as MapPoint;

        if (obj == null)
        {
            return false;
        }

        return p.Row == this.Row && p.Column == this.Column;
    }

    public void Debug()
    {
        Console.Error.WriteLine(this.ToString());
    }

    public override string ToString()
    {
        return String.Format("r: {0} c: {1}", Row, Column);
    }

    public override int GetHashCode()
    {
        return this.Row * 1000 + this.Column;
    }

    public int Row { get; set; }
    public int Column { get; set; }

    public MapPoint()
    {
    }

    public MapPoint(MapPoint p)
    {
        this.Row = p.Row;
        this.Column = p.Column;
    }

    public MapPoint(int row, int column)
    {
        Row = row;
        Column = column;
    }

    internal int CalcSqDistTo(MapPoint p1, MapPoint p2)
    {
        var dx = p1.Column - p2.Column;
        var dy = p1.Row - p2.Row;

        return dx * dx + dy * dy;
    }
}

class Labyrinth
{
    Random TurnRandom = new Random();

    int RowsCount;
    int ColumnsCount;
    int AlarmRounds;

    Queue<MapPoint> PathToCr = null;
    List<MapPoint> PathToStart = null;

    Stack<MapPoint> StepsStack = new Stack<MapPoint>();
    MapPointSet VisitedPoints = new MapPointSet();

    bool IsControlRoomIsReached = false;
    public MapPoint ControlRoomPosition;
    public MapPoint StartPosition;

    MapPoint KirkPosition;
    List<string> Rows;

    void ReadInitData()
    {
        var a = Console.ReadLine().Split(' ');

        RowsCount = int.Parse(a[0]);
        ColumnsCount = int.Parse(a[1]);
        AlarmRounds = int.Parse(a[2]);

        ReadTurnData();

        StartPosition = KirkPosition;
        VisitedPoints.Add(KirkPosition);
        StepsStack.Push(KirkPosition);
    }

    void ReadTurnData()
    {
        var a = Console.ReadLine().Split(' ');

        KirkPosition = new MapPoint
        {
            Row = int.Parse(a[0]),
            Column = int.Parse(a[1])
        };

        Rows = new List<string>(RowsCount);

        for (int i = 0; i < RowsCount; i++)
        {
            var row = Console.ReadLine();

            if (ControlRoomPosition == null)
            {
                var c = row.IndexOf(LabPoints.CONTROL_ROOM);

                //Debug(row);

                if (c >= 0)
                {
                    ControlRoomPosition = new MapPoint(i, c);
                }
            }
            Rows.Add(row);
        }
    }

    char GetPointChar(MapPoint p)
    {
        if (!IsValid(p))
        {
            return LabPoints.WALL;
        }

        return Rows[p.Row][p.Column];
    }

    MapPoint GetUpPoint()
    {
        var p = new MapPoint(KirkPosition);
        p.Row--;

        return p;
    }

    MapPoint GetDownPoint()
    {
        var p = new MapPoint(KirkPosition);
        p.Row++;

        return p;
    }

    MapPoint GetLeftPoint()
    {
        var p = new MapPoint(KirkPosition);
        p.Column--;

        return p;
    }

    MapPoint GetRightPoint()
    {
        var p = new MapPoint(KirkPosition);
        p.Column++;

        return p;
    }

    bool IsValid(MapPoint p)
    {
        if (p.Row < 0)
            return false;

        if (p.Column < 0)
            return false;

        if (p.Row >= RowsCount)
            return false;

        if (p.Column >= ColumnsCount)
            return false;

        return true;
    }

    void DoBestTurn()
    {
        var p = GetControlRoomDirectionPoint();
        if (p != null)
        {
            Debug("move to cr");
            DoTurn(p, false);
            return;
        }

        var nonVisitedFreePointsAround = GetNonVisitedPointsAround().ToList();
        if (nonVisitedFreePointsAround.Count() > 0)
        {
            var pp = nonVisitedFreePointsAround
                        .Where(x => !x.Equals(ControlRoomPosition))
                        .OrderByDescending(x => x.CalcSqDistTo(x, StartPosition)).ToList();

            if (pp.Any())
            {
                Debug($"control room: {ControlRoomPosition}");
                Debug($"goal: {pp.First()}");

                Debug("move opposite to start");
                DoTurn(pp.First());
                return;
            }
        }

        StepBack();
    }

    MapPoint GetControlRoomDirectionPoint()
    {
        if (ControlRoomPosition == null)
        {
            return null;
        }

        if (PathToStart == null)
        {
            Debug("finding cr->sp path");
            var pathFinder = new PathFinder(Rows, IsFreePoint);
            Debug("path finder initialized");
            PathToStart = pathFinder.FindPath(ControlRoomPosition, StartPosition);
            Debug($"back path len: {PathToStart?.Count}, alarm: {AlarmRounds}");
            if (PathToStart != null && PathToStart.Count <= AlarmRounds)
            {
                Debug("finding kirk->cr path");
                var pf = new PathFinder(Rows, IsFreePoint);
                var path = pf.FindPath(KirkPosition, ControlRoomPosition);
                PathToCr = new Queue<MapPoint>(path);
                return PathToCr.Dequeue();
            }
            else
            {
                PathToStart = null;
            }
        }

        return null;
    }

    void StepBack()
    {
        StepsStack.Pop(); //remove current pos from stack
        var p = StepsStack.Pop();
        Debug("move: we stuck. step back: ");
        DoTurn(p);
    }

    static void Debug(string s)
    {
        Console.Error.WriteLine(s);
    }

    void DoTurn(MapPoint p, bool stepBack = true)
    {

        if (stepBack && !IsControlRoomIsReached)
        {
            Debug("StepStack Len: " + StepsStack.Count);
            StepsStack.Push(p);
            VisitedPoints.Add(p);
        }

        var dir = GetStrDirection(p);
        Console.WriteLine(dir);
        KirkPosition = p;

        if (KirkPosition.Equals(ControlRoomPosition))
        {
            IsControlRoomIsReached = true;
        }
    }

    MapPoint GetMapPointTo(MapPoint p)
    {
        var dy = p.Row - KirkPosition.Row;
        var dx = p.Column - KirkPosition.Column;

        //Console.Error.WriteLine("cr dx {0}, dy {1}", dx, dy);

        if (dx > 0 && dx > dy)
        {
            return GetRightPoint();
        }

        if (dx < 0 && dx < dy)
        {
            return GetLeftPoint();
        }

        if (dy < 0)
        {
            return GetUpPoint();
        }

        return GetDownPoint();
    }

    MapPoint GetOppositePointTo(MapPoint p)
    {
        var dy = p.Row - KirkPosition.Row;
        var dx = p.Column - KirkPosition.Column;

        //Console.Error.WriteLine("cr dx {0}, dy {1}", dx, dy);

        if (dx > 0 && dx > dy)
        {
            return GetLeftPoint();
        }

        if (dx < 0 && dx < dy)
        {
            return GetRightPoint();
        }

        if (dy < 0)
        {
            return GetDownPoint();
        }

        return GetUpPoint();
    }

    string GetStrDirection(MapPoint mapPoint)
    {
        if (mapPoint.Equals(GetUpPoint()))
        {
            return Directions.UP;
        }

        if (mapPoint.Equals(GetDownPoint()))
        {
            return Directions.DOWN;
        }

        if (mapPoint.Equals(GetLeftPoint()))
        {
            return Directions.LEFT;
        }

        if (mapPoint.Equals(GetRightPoint()))
        {
            return Directions.RIGHT;
        }

        throw new ArgumentOutOfRangeException(mapPoint.ToString());
    }

    IEnumerable<MapPoint> GetAllDirectionsEnumerator()
    {
        yield return GetUpPoint();
        yield return GetRightPoint();
        yield return GetLeftPoint();
        yield return GetDownPoint();
    }

    public IEnumerable<MapPoint> GetNonVisitedPointsAround()
    {
        foreach (var p in GetAllDirectionsEnumerator())
        {
            if (IsFreePoint(GetPointChar(p)) && !VisitedPoints.Contains(p))
            {
                yield return p;
            }
        }
    }

    bool IsFreePoint(char p)
    {
        return p == LabPoints.HOLLOW || p == LabPoints.START_POINT || p == LabPoints.CONTROL_ROOM;
    }

    private void GoBackToStartPoint()
    {
        //var pathFinder = new PathFinder(Rows, IsFreePoint);
        //var path = pathFinder.FindPath(ControlRoomPosition, StartPosition);

        foreach (var step in PathToStart)
        {
            DoTurn(step);
        }
    }

    public void Play()
    {
        ReadInitData();

        while (!IsControlRoomIsReached)
        {
            if (PathToCr != null)
            {
                DoTurn(PathToCr.Dequeue());
            }
            else
            {
                DoBestTurn();
                ReadTurnData();
            }
        }

        GoBackToStartPoint();
    }
}

class Player
{
    static void Main(String[] args)
    {
        //var x = new List<string>();
        //x.Add("XXXXXXXXXXXXXXXXXXXXXXXX");
        //x.Add("XX                   XXX");
        //x.Add("XXXXX   XXXXXXXXXXX  XXX");
        //x.Add("XX                   XXX");
        //x.Add("XX    XXXXXXXXXXXXXXXXXX");
        //x.Add("XX                   XXX");
        //x.Add("XXХXXXXXXXXXXXX XXXXXXXX");
        //x.Add("XX                XXXXXX");
        //x.Add("XXXXXXXXXXXXXXXXXXXXXXXX");

        //var p = new PathFinder(x, xx => xx != 'X');
        //var start = new MapPoint(1, 3);
        //var finish = new MapPoint(7, 13);
        //var path = p.FindPath(start, finish);
        //p.DebugLenMatrix();

        var lab = new Labyrinth();
        lab.Play();
    }
}