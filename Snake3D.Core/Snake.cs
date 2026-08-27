namespace Snake3D.Core;

/// <summary>
/// Manages the snake's body segments, heading, growth queue, and input buffer.
/// </summary>
public sealed class Snake
{
    private readonly List<GridPoint> _segments = new();
    private readonly Queue<Direction> _inputQueue = new();
    private int _growthPending;

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
        _growthPending = 0;
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
        var lastPlanned = _inputQueue.Count > 0 ? _inputQueue.Last() : CurrentDirection;

        if (nextDirection == lastPlanned || nextDirection.IsOppositeTo(lastPlanned))
        {
            return false;
        }

        if (_inputQueue.Count < 2)
        {
            _inputQueue.Enqueue(nextDirection);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Advances the snake one grid unit in its heading direction.
    /// If growth is pending, the tail is preserved so the snake grows seamlessly without duplicate nodes.
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
        if (grow || _growthPending > 0)
        {
            if (_growthPending > 0)
                _growthPending--;
        }
        else
        {
            _segments.RemoveAt(_segments.Count - 1);
        }
    }

    /// <summary>
    /// Queues growth so the tail will be extended cleanly on the next step.
    /// </summary>
    public void Grow(int count = 1)
    {
        if (count > 0)
        {
            _growthPending += count;
        }
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

    /// <summary>
    /// Checks for self-collision against active body segments.
    /// </summary>
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
