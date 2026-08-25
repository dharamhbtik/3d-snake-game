namespace Snake3D.Core;

/// <summary>
/// Manages the snake's body segments, heading, and input buffer.
/// </summary>
public sealed class Snake
{
    private readonly List<GridPoint> _segments = new();
    private readonly Queue<Direction> _inputQueue = new();

    public IReadOnlyList<GridPoint> Segments => _segments;
    public GridPoint Head => _segments.Count > 0 ? _segments[0] : GridPoint.Zero;
    public GridPoint Tail => _segments.Count > 0 ? _segments[^1] : GridPoint.Zero;
    public GridPoint PreviousTail { get; private set; } = GridPoint.Zero;
    public Direction CurrentDirection { get; private set; } = Direction.Right;
    public int Length => _segments.Count;

    public Snake(GridPoint startHead, Direction initialDirection = Direction.Right, int initialLength = 3)
    {
        Reset(startHead, initialDirection, initialLength);
    }

    public void Reset(GridPoint startHead, Direction initialDirection = Direction.Right, int initialLength = 3)
    {
        _segments.Clear();
        _inputQueue.Clear();
        CurrentDirection = initialDirection;

        _segments.Add(startHead);
        var opposite = initialDirection.Opposite();
        var current = startHead;

        for (int i = 1; i < initialLength; i++)
        {
            current = current.Move(opposite);
            _segments.Add(current);
        }

        PreviousTail = _segments[^1];
    }

    /// <summary>
    /// Buffers a direction request, preventing 180-degree immediate reversal.
    /// </summary>
    public bool EnqueueDirection(Direction nextDirection)
    {
        // Direction to compare against is the last enqueued direction or current direction
        var lastPlanned = _inputQueue.Count > 0 ? _inputQueue.Last() : CurrentDirection;

        if (nextDirection == lastPlanned || nextDirection.IsOppositeTo(lastPlanned))
        {
            return false;
        }

        // Keep buffer limited to 2 pending inputs to prevent stale commands
        if (_inputQueue.Count < 2)
        {
            _inputQueue.Enqueue(nextDirection);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Advances the snake one grid unit in its heading direction.
    /// </summary>
    public void Step(bool grow = false)
    {
        if (_inputQueue.Count > 0)
        {
            CurrentDirection = _inputQueue.Dequeue();
        }

        var newHead = Head.Move(CurrentDirection);
        _segments.Insert(0, newHead);

        PreviousTail = _segments[^1];
        if (!grow)
        {
            _segments.RemoveAt(_segments.Count - 1);
        }
    }

    /// <summary>
    /// Grows the snake by keeping/restoring the previous tail segment.
    /// </summary>
    public void Grow()
    {
        _segments.Add(PreviousTail);
    }

    public bool Contains(GridPoint point, bool excludeHead = false)
    {
        int startIndex = excludeHead ? 1 : 0;
        for (int i = startIndex; i < _segments.Count; i++)
        {
            if (_segments[i] == point)
                return true;
        }
        return false;
    }

    public bool HasSelfCollision()
    {
        if (_segments.Count <= 4)
            return false;

        var head = Head;
        for (int i = 1; i < _segments.Count; i++)
        {
            if (_segments[i] == head)
                return true;
        }
        return false;
    }
}
