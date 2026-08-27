using Snake3D.Core;
using Xunit;

namespace Snake3D.Tests;

public class GameEngineTests
{
    [Fact]
    public void GameEngine_StartGame_InitializesProperState()
    {
        var engine = new GameEngine(20, 20);
        Assert.Equal(GameState.Menu, engine.State);

        engine.StartGame();

        Assert.Equal(GameState.Playing, engine.State);
        Assert.Equal(0, engine.Score);
        Assert.Equal(3, engine.Snake.Length);
        Assert.NotNull(engine.CurrentFood);
        Assert.False(engine.Snake.Contains(engine.CurrentFood.Position));
    }

    [Fact]
    public void GameEngine_EatingFood_IncreasesScoreAndGrowsSnake()
    {
        var engine = new GameEngine(20, 20);
        engine.StartGame();

        int initialLength = engine.Snake.Length;
        int initialScore = engine.Score;

        // Force food position directly in front of snake (head is moving Right)
        var foodPos = engine.Snake.Head.Move(Direction.Right);
        typeof(GameEngine)
            .GetProperty(nameof(GameEngine.CurrentFood))!
            .SetValue(engine, new Food(foodPos, FoodType.Apple, 10));

        // Advance 1 step to consume food
        engine.Update(engine.StepIntervalSeconds + 0.01);

        Assert.Equal(initialScore + 10, engine.Score);
        Assert.NotNull(engine.CurrentFood); // New food spawned

        // Advance next step to complete growth
        engine.Update(engine.StepIntervalSeconds + 0.01);
        Assert.Equal(initialLength + 1, engine.Snake.Length);
    }

    [Fact]
    public void GameEngine_WallCollision_TriggersGameOver()
    {
        var engine = new GameEngine(10, 10);
        engine.StartGame();

        bool gameOverFired = false;
        engine.GameOver += () => gameOverFired = true;

        // Snake starts at (5, 5) heading Right. Move Right 6 times to hit wall at X=10
        for (int i = 0; i < 6; i++)
        {
            engine.Update(engine.StepIntervalSeconds + 0.01);
            if (engine.State == GameState.GameOver)
                break;
        }

        Assert.Equal(GameState.GameOver, engine.State);
        Assert.True(gameOverFired);
    }

    [Fact]
    public void GameEngine_PauseAndResume_TogglesStateCorrectly()
    {
        var engine = new GameEngine(20, 20);
        engine.StartGame();

        engine.Pause();
        Assert.Equal(GameState.Paused, engine.State);

        // While paused, updates do not move snake
        var headBefore = engine.Snake.Head;
        engine.Update(1.0);
        Assert.Equal(headBefore, engine.Snake.Head);

        engine.Resume();
        Assert.Equal(GameState.Playing, engine.State);
    }

    [Fact]
    public void GameEngine_Restart_ResetsScoreAndPosition()
    {
        var engine = new GameEngine(20, 20);
        engine.StartGame();

        // Advance and hit wall
        for (int i = 0; i < 15; i++)
        {
            engine.Update(engine.StepIntervalSeconds + 0.01);
        }
        Assert.Equal(GameState.GameOver, engine.State);

        engine.Restart();
        Assert.Equal(GameState.Playing, engine.State);
        Assert.Equal(0, engine.Score);
        Assert.Equal(3, engine.Snake.Length);
    }

    [Fact]
    public void GameEngine_SpeedProgression_AcceleratesWithFood()
    {
        var engine = new GameEngine(20, 20);
        engine.StartGame();

        double initialInterval = engine.StepIntervalSeconds;

        // Simulate eating 10 foods
        for (int i = 0; i < 10; i++)
        {
            var nextPos = engine.Snake.Head.Move(engine.Snake.CurrentDirection);
            typeof(GameEngine)
                .GetProperty(nameof(GameEngine.CurrentFood))!
                .SetValue(engine, new Food(nextPos, FoodType.Apple, 10));

            engine.Update(engine.StepIntervalSeconds + 0.01);
        }

        Assert.True(engine.StepIntervalSeconds < initialInterval, "Speed interval should decrease as more food is eaten");
        Assert.True(engine.Level >= 2);
    }

    [Fact]
    public void GameEngine_HighScore_TracksNewRecords()
    {
        var engine = new GameEngine(20, 20);
        engine.HighScore = 50;
        engine.StartGame();

        int recordEventScore = 0;
        engine.HighScoreChanged += s => recordEventScore = s;

        // Eat 6 foods (60 points > 50)
        for (int i = 0; i < 6; i++)
        {
            var nextPos = engine.Snake.Head.Move(engine.Snake.CurrentDirection);
            typeof(GameEngine)
                .GetProperty(nameof(GameEngine.CurrentFood))!
                .SetValue(engine, new Food(nextPos, FoodType.Apple, 10));

            engine.Update(engine.StepIntervalSeconds + 0.01);
        }

        Assert.Equal(60, engine.Score);
        Assert.Equal(60, engine.HighScore);
        Assert.Equal(60, recordEventScore);
    }
}
