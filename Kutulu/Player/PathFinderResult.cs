using System.Linq;

public class PathFinderResult
{
    public static readonly PathFinderResult NO_PATH = new PathFinderResult();
    public Cell[] Path;

    public int WeightedLength = -1;

    public bool HasNextCell()
    {
        return Path.Length > 1;
    }

    public Cell GetNextCell()
    {
        return Path.First();
    }

    public bool HasNoPath()
    {
        return WeightedLength == -1;
    }
}
