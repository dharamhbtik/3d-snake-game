namespace Snake3D.Core;

/// <summary>
/// Represents a 2D coordinate on the integer grid board.
/// </summary>
public readonly record struct GridPoint(int X, int Y)
{
    public static readonly GridPoint Zero = new(0, 0);

    public GridPoint Move(Direction direction) => direction switch
    {
        Direction.Up => new GridPoint(X, Y - 1),
        Direction.Down => new GridPoint(X, Y + 1),
        Direction.Left => new GridPoint(X - 1, Y),
        Direction.Right => new GridPoint(X + 1, Y),
        _ => this
    };

    public bool IsWithinBounds(int width, int height) =>
        X >= 0 && X < width && Y >= 0 && Y < height;

    public int ManhattanDistance(GridPoint other) =>
        Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
}
