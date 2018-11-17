using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

class Player
{
    static void Main(string[] args)
    {
        int[] inputs;
        var pods = Enumerable.Range(0, 4).Select(x => new Pod()).ToArray();
        var checkPoints = new CheckPoints();

        var laps = ReadInts()[0];
        var checkPointCount = ReadInts()[0];
        for (var i = 0; i < checkPointCount; i++)
        {
            inputs = ReadInts();
            checkPoints.Add(inputs[0], inputs[1]);
        }

        while (true)
        {
            for (var i = 0; i < 4; i++)
            {
                inputs = ReadInts();

                pods[i].Point = new Vector(inputs[0], inputs[1]);
                pods[i].Speed = new Vector(inputs[2], inputs[3]);
                pods[i].Angle = inputs[4];
                var nextCheckPoint = checkPoints.Get(inputs[5]);
                if( nextCheckPoint != pods[i].NextCheckPoint)
                {
                    pods[i].NextCheckPoint = nextCheckPoint;
                    pods[i].Checked++;
                }
            }

            for (var i = 0; i < 2; i++)
            {
                var v = pods[i].NextCheckPoint - pods[i].Point;
                var speedAngle = pods[i].Speed.DiffAngle(v);

                var thrust = "100";

                var target = new Vector(pods[i].NextCheckPoint);
                if (Math.Abs(speedAngle) < 90)
                {
                    RotatePoint(target, pods[i].Point, speedAngle);
                }
                else
                {
                    var nextCheckpointAngle = pods[i].DiffSteerAngle(pods[i].NextCheckPoint);
                    var dist = pods[i].Point.GetDistance(pods[i].NextCheckPoint);
                    D(nextCheckpointAngle, dist);

                    if (dist < 1000 && Math.Abs(nextCheckpointAngle) > 45)
                    {
                        thrust = "0";
                    }
                }

                if (!pods[0].BoostUsed)
                {
                    pods[0].BoostUsed = true;
                    thrust = "BOOST";
                }

                Console.WriteLine($"{target.X} {target.Y} {thrust}");
            }
        }
    }

    private static int[] ReadInts()
    {
        return Console.ReadLine().Split(' ').Select(int.Parse).ToArray();
    }

    public static void D(params object[] args)
    {
        Console.Error.WriteLine(string.Join(" ", args.Select(x => x == null ? "null" : x.ToString())));
    }

    public static void RotatePoint(Vector p, Vector c, double angle)
    {
        double x = p.X - c.X;
        double y = p.Y - c.Y;

        double a = angle * 3.14 / 180;

        var xx = x * Math.Cos(a) - y * Math.Sin(a) + c.X;
        var yy = y * Math.Cos(a) + x * Math.Sin(a) + c.Y;

        p.X = (int)xx;
        p.Y = (int)yy;
    }
}

public class Vector
{
    public int X;
    public int Y;

    public Vector()
    {
    }

    public Vector(Vector p)
    {
        X = p.X;
        Y = p.Y;
    }

    public Vector(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override string ToString()
    {
        return X.ToString() + ":" + Y.ToString();
    }

    public double GetDistance(Vector p)
    {
        var dx = p.X - X;
        var dy = p.Y - Y;

        return Math.Sqrt(dx * dx + dy * dy);
    }

    public double GetAngle()
    {
        return new Vector(0, 0).GetAngle(this);
    }

    public double GetAngle(Vector v)
    {
        var d = GetDistance(v);
        var dx = (v.X - X) / d;
        var dy = (v.Y - Y) / d;

        var a = Math.Acos(dx) * 180.0 / Math.PI;

        if (dy < 0)
        {
            a = 360.0 - a;
        }

        return a;
    }

    public double DiffAngle(Vector v)
    {
        var a = GetAngle(v);
        var me = GetAngle();

        var right = me <= a ? a - me : 360.0 - me + a;
        var left = me >= a ? me - a : me + 360.0 - a;

        if (right < left)
        {
            return right;
        }
        else
        {
            return -left;
        }
    }

    public static Vector operator -(Vector v1, Vector v2)
    {
        return new Vector(v1.X - v2.X, v1.Y - v2.Y);
    }
}

public class Pod
{
    public Pod()
    {
    }

    public bool BoostUsed = false;
    public Vector Point;
    public Vector Speed;
    public int Angle;
    public Vector NextCheckPoint;
    public int Checked;

    public double GetScore()
    {
        return Checked * 50000 - Point.GetDistance(NextCheckPoint);
    }

    public double DiffSteerAngle(Vector v)
    {
        var a = Point.GetAngle(v);
        var me = Angle;

        var right = me <= a ? a - me : 360.0 - me + a;
        var left = me >= a ? me - a : me + 360.0 - a;

        if (right < left)
        {
            return right;
        }
        else
        {
            return -left;
        }
    }
}

class CheckPoints
{
    List<Vector> checkPoints = new List<Vector>();
    int current = -1;

    public CheckPoints()
    {
    }

    public void Add(int x, int y)
    {
        checkPoints.Add(new Vector(x, y));
    }

    public Vector Get(int index)
    {
        return checkPoints[index];
    }

    public void SetCurrent(int index)
    {
        current = index;
    }

    internal Vector GetCurrent()
    {
        return checkPoints[current];
    }

    internal void SetCurrent(int x, int y)
    {
        if (current < 0)
        {
            checkPoints.Add(new Vector(x, y));
            current = 0;
            return;
        }

        current = (current + 1) % checkPoints.Count;
        if (checkPoints[current].X != x && checkPoints[current].Y != y)
        {
            checkPoints.Add(new Vector(x, y));
            current = checkPoints.Count - 1;
        }
    }
}