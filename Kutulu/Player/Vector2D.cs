using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Vector2D : IEquatable<Vector2D>
{
    public static readonly Vector2D UP = new Vector2D(0, -1);
    public static readonly Vector2D RIGHT = new Vector2D(1, 0);
    public static readonly Vector2D DOWN = new Vector2D(0, 1);
    public static readonly Vector2D LEFT = new Vector2D(-1, 0);

    public const int MAX_DIRS = 4;
    public static readonly Vector2D[] Directions = new[] { Vector2D.UP, Vector2D.RIGHT, Vector2D.DOWN, Vector2D.LEFT };
    public static readonly Vector2D[] DirectionsAndMe = new[] { new Vector2D(), Vector2D.UP, Vector2D.RIGHT, Vector2D.DOWN, Vector2D.LEFT };

    public int X;
    public int Y;

    public Vector2D() : this(0)
    {
    }

    public Vector2D(int x) : this(x, x)
    {
    }

    public Vector2D(int x, int y)
    {
        this.X = x;
        this.Y = y;
    }

    public Vector2D(Vector2D vect)
    {
        this.X = vect.X;
        this.Y = vect.Y;
    }

    public bool Equals(Vector2D a)
    {
        return this.X == a.X && this.Y == a.Y;
    }

    public bool IsNull()
    {
        return (this.X | this.Y) == 0;
    }

    public Vector2D Set(int x, int y)
    {
        this.X = x;
        this.Y = y;
        return this;
    }

    public Vector2D Set(Vector2D a)
    {
        this.X = a.X;
        this.Y = a.Y;
        return this;
    }

    public Vector2D Add(Vector2D a)
    {
        this.X += a.X;
        this.Y += a.Y;
        return this;
    }

    public Vector2D Sub(Vector2D a)
    {
        this.X -= a.X;
        this.Y -= a.Y;
        return this;
    }

    public Vector2D Mult(int a)
    {
        this.X *= a;
        this.Y *= a;
        return this;
    }

    public Vector2D Div(int a)
    {
        this.X /= a;
        this.Y /= a;
        return this;
    }

    public Vector2D Negate()
    {
        this.X = -this.X;
        this.Y = -this.Y;
        return this;
    }

    public Vector2D Normalize()
    {
        if (IsNull())
            return this;

        int absx = Math.Abs(this.X);
        int absy = Math.Abs(this.Y);
        if (absx > absy)
        {
            this.X /= absx;
            this.Y = 0;
        }
        else if (absx < absy)
        {
            this.X = 0;
            this.Y /= absy;
        }
        else
        {
            this.X /= absx;
            this.Y /= absy;
        }
        return this;
    }

    public int ManhattanDistance()
    {
        return Math.Abs(X) + Math.Abs(Y);
    }

    public int ManhattanDistance(Vector2D a)
    {
        return Math.Abs(this.X - a.X) + Math.Abs(this.Y - a.Y);
    }

    public int TchebychevDistance()
    {
        return Math.Max(X, Y);
    }

    public int TchebychevDistance(Vector2D a)
    {
        return Math.Max(Math.Abs(this.X - a.X), Math.Abs(this.Y - a.Y));
    }

    public double EuclidianDistance2()
    {
        return X * X + Y * Y;
    }

    public double EuclidianDistance2(Vector2D a)
    {
        return Math.Pow(this.X - a.X, 2) + Math.Pow(this.Y - a.Y, 2);
    }

    public double EuclidianDistance()
    {
        return Math.Sqrt(EuclidianDistance());
    }

    public double EuclidianDistance(Vector2D a)
    {
        return Math.Sqrt(EuclidianDistance2(a));
    }

    public static Vector2D Add(Vector2D a, Vector2D b)
    {
        return new Vector2D(a).Add(b);
    }

    public static Vector2D Sub(Vector2D a, Vector2D b)
    {
        return new Vector2D(a).Sub(b);
    }

    public static Vector2D Mult(Vector2D a, int b)
    {
        return new Vector2D(a).Mult(b);
    }

    public static Vector2D Div(Vector2D a, int b)
    {
        return new Vector2D(a).Div(b);
    }

    public bool IsInlineToAny(IEnumerable<Vector2D> vectors)
    {
        return vectors.Any(x => this.IsInlineTo(x));
    }

    public bool IsInlineTo(Vector2D v)
    {
        return v.X == this.X || v.Y == this.Y;
    }

    public override String ToString()
    {
        return "[" + X + ":" + Y + "]";
    }
}
