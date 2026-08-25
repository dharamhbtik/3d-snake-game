namespace Snake3D.Core;

/// <summary>
/// Represents the 2D grid dimensions and free cell calculation.
/// </summary>
public sealed class GameBoard
{
    public int Width { get; }
    public int Height { get; }

    public GameBoard(int width = 20, int height = 20)
    {
        if (width < 3 || height < 3)
            throw new ArgumentOutOfRangeException(nameof(width), "Board dimensions must be at least 3x3");

        Width = width;
        Height = height;
    }

    public bool IsOutOfBounds(GridPoint point) => !point.IsWithinBounds(Width, Height);

    public GridPoint? FindRandomFreeCell(IEnumerable<GridPoint> occupiedCells, Random? rng = null)
    {
        rng ??= Random.Shared;
        var occupiedSet = occupiedCells as HashSet<GridPoint> ?? occupiedCells.ToHashSet();
        int totalCells = Width * Height;
        int freeCount = totalCells - occupiedSet.Count;

        if (freeCount <= 0)
            return null;

        // If occupancy is low (< 70%), fast random sampling
        if (occupiedSet.Count < totalCells * 0.7)
        {
            for (int attempt = 0; attempt < 64; attempt++)
            {
                var candidate = new GridPoint(rng.Next(Width), rng.Next(Height));
                if (!occupiedSet.Contains(candidate))
                    return candidate;
            }
        }

        // Exact fallback for high occupancy: collect free cells and pick one
        var freeCells = new List<GridPoint>(freeCount);
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                var p = new GridPoint(x, y);
                if (!occupiedSet.Contains(p))
                {
                    freeCells.Add(p);
                }
            }
        }

        return freeCells.Count > 0 ? freeCells[rng.Next(freeCells.Count)] : null;
    }
}
