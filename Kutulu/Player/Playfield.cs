using System;
using System.Text;

public class Playfield
{
    public Vector2D Dimentions;
    readonly Cell[,] maze;

    public const char WALL = '#';
    public const char EMPTY = '.';
    public const char WANDERER_SPAWN = 'w';
    public const char OTHER_SPAWN = 's';
    public const char PLAYER_SPAWN = 'S';
    public const char SHELTER = 'U';

    public Playfield(int width, int height)
    {
        Dimentions = new Vector2D(width, height);
        maze = new Cell[width, height];
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                maze[x, y] = new Cell(x, y);
            }
        }
    }

    public Playfield(Vector2D dimentions) : this(dimentions.X, dimentions.Y)
    {
    }

    public void CopyCellTypesFrom(Playfield field)
    {
        for (var y = 0; y < Dimentions.Y; y++)
        {
            for (var x = 0; x < Dimentions.X; x++)
            {
                maze[x, y].CellType = field[x, y].CellType;
            }
        }
    }

    public void SetCell(int x, int y, char ch)
    {
        var type = GetCellType(ch);
        maze[x, y] = new Cell(x, y, type);
    }

    private CellType GetCellType(char ch)
    {
        switch (ch)
        {
            case WALL:
                return CellType.WALL;
            case EMPTY:
                return CellType.EMPTY;
            case SHELTER:
                return CellType.SHELTER;
            default:
                return CellType.EMPTY;
        }
    }

    public Cell this[Vector2D p]
    {
        get => this[p.X, p.Y];
    }
    public Cell this[int x, int y]
    {
        get => GetCell(x, y);
    }

    public Cell GetCell(int x, int y)
    {
        return IsInBoundaries(x, y) ? maze[x, y] : Cell.INVALID_CELL;
    }

    public Cell GetCell(Vector2D a)
    {
        return GetCell(a.X, a.Y);
    }

    public bool IsWalkable(Vector2D a)
    {
        var w = IsWalkable(GetCell(a));
        //D(a, w);
        return w;
    }

    private static void D(params object[] oo)
    {
        if (oo == null)
        {
            Console.WriteLine("(null)");
            return;
        }

        foreach (var o in oo)
        {
            Console.Error.Write(o);
            Console.Error.Write(" ");
        }

        Console.Error.WriteLine();
    }

    public bool IsWalkable(Cell cell)
    {
        return cell.CellType != CellType.WALL && IsInBoundaries(cell);
    }

    public bool IsInBoundaries(Cell cell)
    {
        return IsInBoundaries(cell.Pos.X, cell.Pos.Y);
    }

    public bool IsInBoundaries(int x, int y)
    {
        return !(x < 0 || x >= Dimentions.X || y < 0 || y >= Dimentions.Y);
    }

    public override string ToString()
    {
        var o = new StringBuilder();

        for (var y = 0; y < Dimentions.Y; y++)
        {
            for (var x = 0; x < Dimentions.X; x++)
            {
                o.Append(GetCellChar(maze[x, y].CellType));
            }
            o.Append(Environment.NewLine);
        }
        return o.ToString();
    }

    private char GetCellChar(CellType c)
    {
        switch (c)
        {
            case CellType.WALL:
                return Playfield.WALL;
            case CellType.EMPTY:
                return Playfield.EMPTY;
            case CellType.SHELTER:
                return Playfield.EMPTY;
            default:
                throw new ArgumentException(c.ToString());
        }
    }
}
