using System;
using System.Diagnostics;

public class PathItem
{
    public int CumulativeLength = 0;
    public int TotalPrevisionalLength = 0;
    public PathItem Precedent = null;
    public Cell Pos;

    public int GetLength()
    {
        var i = this;
        var count = 0;
        while (i != null)
        {
            count++;
            i = i.Precedent;
        }
        return count;
    }

    public override string ToString()
    {
        return $"{Pos} {TotalPrevisionalLength}";
    }
}