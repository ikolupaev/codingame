using System;
using System.Linq;
using System.Collections.Generic;

class Stop
{
    public string Code;
    public string Name;
    public double Lg;
    public double Lt;
    public bool MinDist = false;
    public double DistFromStart = int.MaxValue;
    public Dictionary<Stop, double> Routes = new Dictionary<Stop, double>();
    public Dictionary<Stop, double> ReverseRoutes = new Dictionary<Stop, double>();

    public override string ToString()
    {
        return Name;
    }
}

class Solution
{
    static void Main(string[] args)
    {
        string startPoint = Console.ReadLine();
        string endPoint = Console.ReadLine();
        Dictionary<string, Stop> stops = new Dictionary<string, Stop>();

        int N = int.Parse(Console.ReadLine());
        for (int i = 0; i < N; i++)
        {
            var s = Console.ReadLine().Split(',');
            var stop = new Stop
            {
                Code = s[0],
                Name = s[1],
                Lt = double.Parse(s[3]) * Math.PI / 180.0,
                Lg = double.Parse(s[4]) * Math.PI / 180.0
            };
            stops.Add(stop.Code, stop);
        }

        int M = int.Parse(Console.ReadLine());
        for (int i = 0; i < M; i++)
        {
            var route = Console.ReadLine().Split();

            if (route[0] == route[1]) continue;

            var a = stops[route[0]];
            var b = stops[route[1]];

            var x = (b.Lg - a.Lg) * Math.Cos((b.Lt + a.Lt) / 2.0);
            var y = b.Lt - a.Lt;

            var d = Math.Sqrt(x * x + y * y) * 6371;

            a.Routes.Add(b, d);
            b.ReverseRoutes.Add(a, d);
        }

        stops[startPoint].DistFromStart = 0;

        for (var i = 0; i < stops.Count; i++)
        {
            var p = stops.Values.Where(x => !x.MinDist).OrderBy(x => x.DistFromStart).First();
            p.MinDist = true;

            if (p.Code == endPoint) break;

            foreach (var x in p.Routes)
            {
                if( !x.Key.MinDist && p.DistFromStart != double.MaxValue && p.DistFromStart + x.Value < x.Key.DistFromStart )
                {
                    x.Key.DistFromStart = p.DistFromStart + x.Value;
                }
            }
        }

        var path = new Stack<Stop>();
        var z = stops[endPoint];
        path.Push(z);
        while (z.Code != startPoint)
        {
            z = z.ReverseRoutes.Keys.OrderBy(x => x.DistFromStart).First();
            path.Push(z);
        }

        foreach (var x in path)
        {
            Console.WriteLine(x.Name.Replace("\"", ""));
        }
    }
}