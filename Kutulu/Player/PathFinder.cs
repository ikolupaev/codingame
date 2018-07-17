using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class PathFinder
{
    Cell from = null;
    Cell to = null;
    int directionOffest = 0;
    private Func<Cell, int> WeightFunction = (x) => 1;
    private Playfield map;

    public PathFinder(Playfield map)
    {
        this.map = map;
    }

    public PathFinder From(Vector2D p)
    {
        return From(map.GetCell(p));
    }

    public PathFinder From(Cell cell)
    {
        from = cell;
        return this;
    }

    public PathFinder To(Vector2D p)
    {
        return To(map.GetCell(p));
    }

    public PathFinder To(Cell cell)
    {
        to = cell;
        return this;
    }

    public PathFinder WithOffset(int offset)
    {
        this.directionOffest = offset;
        return this;
    }

    public PathFinder WithWeightFunction(Func<Cell, int> weightFunction)
    {
        this.WeightFunction = weightFunction;
        return this;
    }

    public PathFinderResult FindPath()
    {
        if (from == null || to == null)
        {
            return new PathFinderResult();
        }

        //D(map);
        //D("from:", from);
        //D("to:", to);

        var pathItems = new AStar(map, from, to, directionOffest, WeightFunction).Find();

        if (!pathItems.Any())
        {
            D("NO_PATH");
            return PathFinderResult.NO_PATH;
        }

        var p = new PathFinderResult
        {
            Path = pathItems.Select(x => x.Pos).ToArray(),
            WeightedLength = pathItems.Last().CumulativeLength
        };

        //D("found path:", p.WeightedLength);

        return p;
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
}
