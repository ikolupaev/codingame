using System.Diagnostics;

public enum CellType
{
    //NONE,
    EMPTY,
    WALL,
    SHELTER
}

public class Cell
{
    public static readonly Cell INVALID_CELL = new Cell(-1, -1);

    public readonly Vector2D Pos;
    public CellType CellType;

    public Cell(int x, int y, CellType type) :this(x,y)
    {
        this.CellType = type;
    }

    public Cell(int x, int y)
    {
        this.Pos = new Vector2D(x, y);
    }

    public override string ToString()
    {
        return $"{Pos} {CellType}";
    }
}
