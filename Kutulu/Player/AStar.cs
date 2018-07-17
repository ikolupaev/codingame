using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class AStar
{
    Dictionary<Cell, PathItem> closedList = new Dictionary<Cell, PathItem>();
    List<PathItem> openList = new List<PathItem>();
    List<PathItem> path = new List<PathItem>();

    Playfield playfield;
    Cell from;
    Cell target;
    int dirOffset;
    private Func<Cell, int> weightFunction;

    public AStar(Playfield playfield, Cell from, Cell target, int dirOffset, Func<Cell, int> weightFunction)
    {
        this.playfield = playfield;
        this.from = from;
        this.target = target;
        this.dirOffset = dirOffset;
        this.weightFunction = weightFunction;
    }

    public List<PathItem> Find()
    {
        PathItem item = GetPathItemLinkedList();
        path.Clear();
        if (item != null)
        {
            CalculatePath(item);
        }
        return path;
    }

    void CalculatePath(PathItem item)
    {
        PathItem i = item;
        while (i != null)
        {
            path.Insert(0, i);
            i = i.Precedent;
        }
    }

    PathItem GetPathItemLinkedList()
    {
        var root = new PathItem
        {
            Pos = this.from
        };

        openList.Add(root);

        while (openList.Count > 0)
        {
            var visiting = openList.First();
            openList.Remove(visiting);

            var cell = visiting.Pos;

            if (cell == target)
            {
                return visiting;
            }

            if (closedList.ContainsKey(cell))
            {
                continue;
            }

            closedList[cell] = visiting;

            for (int i = 0; i < Vector2D.MAX_DIRS; i++)
            {
                int index = (i + dirOffset) % Vector2D.MAX_DIRS;
                var nextCell = playfield.GetCell(Vector2D.Add(Vector2D.Directions[index], cell.Pos));
                if (playfield.IsWalkable(nextCell))
                {
                    AddToOpenList(visiting, cell, nextCell);
                }
            }

            openList.Sort((a, b) => a.TotalPrevisionalLength - b.TotalPrevisionalLength);
        }
        return null; 
    }

    void AddToOpenList(PathItem visiting, Cell fromCell, Cell toCell)
    {
        if (closedList.ContainsKey(toCell))
        {
            return;
        }

        PathItem pi = new PathItem();
        pi.Pos = toCell;
        pi.CumulativeLength = visiting.CumulativeLength + weightFunction(toCell);
        int manh = fromCell.Pos.ManhattanDistance(toCell.Pos);
        pi.TotalPrevisionalLength = pi.CumulativeLength + manh;
        pi.Precedent = visiting;

        openList.Add(pi);
    }
}
