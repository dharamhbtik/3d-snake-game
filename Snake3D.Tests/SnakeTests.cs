using Snake3D.Core;
using Xunit;

namespace Snake3D.Tests;

public class SnakeTests
{
    [Fact]
    public void Snake_Initializes_WithCorrectLengthAndSegments()
    {
        var head = new GridPoint(10, 10);
        var snake = new Snake(head, Direction.Right, initialLength: 3);

        Assert.Equal(3, snake.Length);
        Assert.Equal(head, snake.Head);
        Assert.Equal(new GridPoint(9, 10), snake.Segments[1]);
        Assert.Equal(new GridPoint(8, 10), snake.Segments[2]);
        Assert.Equal(Direction.Right, snake.CurrentDirection);
    }

    [Theory]
    [InlineData(Direction.Up, 10, 9)]
    [InlineData(Direction.Down, 10, 11)]
    [InlineData(Direction.Left, 9, 10)]
    [InlineData(Direction.Right, 11, 10)]
    public void Snake_StepsForward_InSpecifiedDirection(Direction direction, int expectedHeadX, int expectedHeadY)
    {
        var head = new GridPoint(10, 10);
        var snake = new Snake(head, direction, initialLength: 3);

        snake.Step(grow: false);

        Assert.Equal(new GridPoint(expectedHeadX, expectedHeadY), snake.Head);
        Assert.Equal(3, snake.Length);
    }

    [Fact]
    public void Snake_Prevents_DirectReversal()
    {
        var snake = new Snake(new GridPoint(10, 10), Direction.Right, initialLength: 3);

        // Trying to move Left when going Right should be rejected
        bool accepted = snake.EnqueueDirection(Direction.Left);
        Assert.False(accepted);

        snake.Step(grow: false);
        Assert.Equal(Direction.Right, snake.CurrentDirection);
    }

    [Fact]
    public void Snake_Allows_ValidOrthogonalTurns()
    {
        var snake = new Snake(new GridPoint(10, 10), Direction.Right, initialLength: 3);

        bool upAccepted = snake.EnqueueDirection(Direction.Up);
        Assert.True(upAccepted);

        snake.Step(grow: false);
        Assert.Equal(Direction.Up, snake.CurrentDirection);
        Assert.Equal(new GridPoint(10, 9), snake.Head);
    }

    [Fact]
    public void Snake_Buffers_RapidConsecutiveKeypresses()
    {
        // Facing Right: user presses Up then Left before a step occurs
        var snake = new Snake(new GridPoint(10, 10), Direction.Right, initialLength: 3);

        Assert.True(snake.EnqueueDirection(Direction.Up));
        Assert.True(snake.EnqueueDirection(Direction.Left));

        // Step 1 executes Up
        snake.Step(grow: false);
        Assert.Equal(Direction.Up, snake.CurrentDirection);
        Assert.Equal(new GridPoint(10, 9), snake.Head);

        // Step 2 executes Left
        snake.Step(grow: false);
        Assert.Equal(Direction.Left, snake.CurrentDirection);
        Assert.Equal(new GridPoint(9, 9), snake.Head);
    }

    [Fact]
    public void Snake_Grows_WhenGrowIsTrue()
    {
        var snake = new Snake(new GridPoint(10, 10), Direction.Right, initialLength: 3);
        int initialCount = snake.Length;

        snake.Step(grow: true);

        Assert.Equal(initialCount + 1, snake.Length);
        Assert.Equal(new GridPoint(11, 10), snake.Head);
    }

    [Fact]
    public void Snake_Detects_SelfCollision()
    {
        var snake = new Snake(new GridPoint(10, 10), Direction.Right, initialLength: 5);

        // Move in a tight loop: Up, Left, Down
        snake.EnqueueDirection(Direction.Up);
        snake.Step(grow: false); // (10, 9)

        snake.EnqueueDirection(Direction.Left);
        snake.Step(grow: false); // (9, 9)

        snake.EnqueueDirection(Direction.Down);
        snake.Step(grow: false); // (9, 10) - collides with body!

        Assert.True(snake.HasSelfCollision());
    }
}
