namespace Snake3D.Core;

/// <summary>
/// Cardinal movement directions on the 2D grid.
/// </summary>
public enum Direction
{
    Up,
    Down,
    Left,
    Right
}

public static class DirectionExtensions
{
    public static bool IsOppositeTo(this Direction current, Direction next) => (current, next) switch
    {
        (Direction.Up, Direction.Down) => true,
        (Direction.Down, Direction.Up) => true,
        (Direction.Left, Direction.Right) => true,
        (Direction.Right, Direction.Left) => true,
        _ => false
    };

    public static Direction Opposite(this Direction direction) => direction switch
    {
        Direction.Up => Direction.Down,
        Direction.Down => Direction.Up,
        Direction.Left => Direction.Right,
        Direction.Right => Direction.Left,
        _ => direction
    };
}
