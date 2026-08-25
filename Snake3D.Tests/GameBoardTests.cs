using Snake3D.Core;
using Xunit;

namespace Snake3D.Tests;

public class GameBoardTests
{
    [Fact]
    public void GameBoard_Detects_OutOfBoundsCorrectly()
    {
        var board = new GameBoard(20, 20);

        Assert.False(board.IsOutOfBounds(new GridPoint(0, 0)));
        Assert.False(board.IsOutOfBounds(new GridPoint(19, 19)));
        Assert.False(board.IsOutOfBounds(new GridPoint(10, 10)));

        Assert.True(board.IsOutOfBounds(new GridPoint(-1, 5)));
        Assert.True(board.IsOutOfBounds(new GridPoint(5, -1)));
        Assert.True(board.IsOutOfBounds(new GridPoint(20, 5)));
        Assert.True(board.IsOutOfBounds(new GridPoint(5, 20)));
    }

    [Fact]
    public void GameBoard_FindRandomFreeCell_NeverReturnsOccupiedCell()
    {
        var board = new GameBoard(5, 5);
        var occupied = new HashSet<GridPoint>();

        // Occupy all cells except (2, 2)
        for (int y = 0; y < 5; y++)
        {
            for (int x = 0; x < 5; x++)
            {
                if (x != 2 || y != 2)
                {
                    occupied.Add(new GridPoint(x, y));
                }
            }
        }

        var freeCell = board.FindRandomFreeCell(occupied);

        Assert.NotNull(freeCell);
        Assert.Equal(new GridPoint(2, 2), freeCell.Value);
    }

    [Fact]
    public void GameBoard_FindRandomFreeCell_ReturnsNullWhenFull()
    {
        var board = new GameBoard(3, 3);
        var occupied = new HashSet<GridPoint>();

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                occupied.Add(new GridPoint(x, y));
            }
        }

        var freeCell = board.FindRandomFreeCell(occupied);
        Assert.Null(freeCell);
    }
}
