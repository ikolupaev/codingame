using System;

public static class MathExtentions
{
    public static int GetDistance(this Point p1, Point p2)
    {
        var xx = Math.Abs(p1.X - p2.X);
        var yy = Math.Abs(p1.Y - p2.Y);

        return (int)Math.Sqrt(xx * xx + yy * yy);
    }

    public static double AngleTo(this Point p1, Point p2)
    {
        var a = Math.Atan2(p1.Y - p2.Y, p1.X - p2.X);
        if (a < 0) a = Math.PI * 2 + a;
        return a;
    }

    public static bool IsInside(this Point[] polygon, Point p)
    {
        // http://www.ecse.rpi.edu/Homepages/wrf/Research/Short_Notes/pnpoly.html

        var x = p.X;
        var y = p.Y;
        var inside = false;
        var j = polygon.Length - 1;
        for (var i = 0; i < polygon.Length; j = i++)
        {
            var xi = polygon[i].X;
            var yi = polygon[i].Y;
            var xj = polygon[j].X;
            var yj = polygon[j].Y;

            var intersect = ((yi > y) != (yj > y)) && (x < (xj - xi) * (y - yi) / (yj - yi) + xi);

            if (intersect) inside = !inside;
        }

        return inside;
    }
}
