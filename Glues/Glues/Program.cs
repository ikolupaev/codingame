using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        int n = int.Parse(Console.ReadLine());
        D(n);
        var all = new List<Point>();

        for (int i = 0; i < n; i++)
        {
            var s = Console.ReadLine();
            D(s);

            var v = s.Split(' ').Select(int.Parse).ToArray();
            var p = new Point(v[0], v[1]);
            all.Add(p);
        }

        if( all.Count == 1 )
        {
            Console.WriteLine(4);
            return;
        }

        var curPoint = all.OrderBy(x => x.Y).First();
        double curAngle = 0;
        var convex = new List<Point>() { curPoint };
        while (true)
        {
            double minAngle = 10;
            Point minAnglePoint = null;
            foreach (var p in all)
            {
                if (p == curPoint) continue;

                var a = GetAngle(curPoint, p);
                if (a >= curAngle && a < minAngle)
                {
                    minAngle = a;
                    minAnglePoint = p;
                }
            }

            if (minAnglePoint == convex[0]) break;

            convex.Add(minAnglePoint);
            curAngle = minAngle;
            curPoint = minAnglePoint;
        }

        var d = GetDistance(convex.First(), convex.Last());
        for (var i = 0; i < convex.Count - 1; i++)
        {
            d += GetDistance(convex[i], convex[i + 1]);
        }

        var r = d / (2 * Math.PI);
        d = (r + 3) * 2 * Math.PI;

        Console.WriteLine(Math.Ceiling(d / 5));
    }

    private static double GetDistance(Point p1, Point p2)
    {
        var xx = Math.Abs(p1.X - p2.X);
        var yy = Math.Abs(p1.Y - p2.Y);

        return Math.Sqrt(xx * xx + yy * yy);
    }

    private static double GetAngle(Point p2, Point p1)
    {
        var a = Math.Atan2(p1.Y - p2.Y, p1.X - p2.X);
        if (a < 0) a = Math.PI * 2 + a;
        return a;
    }

    private static void D(object o)
    {
        Console.Error.WriteLine(o);
    }

    public class Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        public override string ToString()
        {
            return $"{X} {Y}";
        }
    }
}
