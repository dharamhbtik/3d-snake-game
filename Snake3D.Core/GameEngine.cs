namespace Snake3D.Core;

/// <summary>
/// Core game engine managing game state, game loop updates, collisions, scoring, and food lifecycle.
/// </summary>
public sealed class GameEngine
{
    private double _timeAccumulator;
    private double _specialFoodTimer;
    private readonly Random _rng;

    public GameBoard Board { get; }
    public Snake Snake { get; }
    public Food? CurrentFood { get; private set; }
    public Food? SpecialFood { get; private set; }
    public GameState State { get; private set; } = GameState.Menu;

    public int Score { get; private set; }
    public int HighScore { get; set; }
    public int FoodEatenCount { get; private set; }
    public int Level => 1 + (FoodEatenCount / 5);

    public GameSpeed Speed { get; set; } = GameSpeed.Normal;

    /// <summary>
    /// Current duration in seconds between snake grid steps based on selected difficulty/speed.
    /// </summary>
    public double StepIntervalSeconds
    {
        get
        {
            double baseInterval = Speed switch
            {
                GameSpeed.Relaxed => 0.250, // Relaxed & comfortable (4 moves/sec)
                GameSpeed.Fast => 0.110,    // Fast challenge
                _ => 0.170                 // Normal
            };

            double interval = baseInterval - (FoodEatenCount * 0.0022) - ((Level - 1) * 0.005);
            double minInterval = Speed switch
            {
                GameSpeed.Relaxed => 0.120,
                GameSpeed.Fast => 0.045,
                _ => 0.075
            };

            return Math.Max(minInterval, interval);
        }
    }

    /// <summary>
    /// Normalized interpolation factor [0.0, 1.0] representing time between the last step and next step.
    /// Used by the 3D renderer for silky smooth 60fps movement.
    /// </summary>
    public double SubTickProgress { get; private set; }

    // Events
    public event Action<Food>? FoodConsumed;
    public event Action? SnakeStepCompleted;
    public event Action? GameOver;
    public event Action<GameState>? StateChanged;
    public event Action<int>? ScoreChanged;
    public event Action<int>? HighScoreChanged;
    public event Action? GameRestarted;

    public GameEngine(int boardWidth = 20, int boardHeight = 20, Random? rng = null)
    {
        _rng = rng ?? Random.Shared;
        Board = new GameBoard(boardWidth, boardHeight);
        var startHead = new GridPoint(boardWidth / 2, boardHeight / 2);
        Snake = new Snake(startHead, Direction.Right, initialLength: 3);
    }

    public void StartGame()
    {
        Score = 0;
        FoodEatenCount = 0;
        _timeAccumulator = 0;
        _specialFoodTimer = 0;
        SpecialFood = null;

        var startHead = new GridPoint(Board.Width / 2, Board.Height / 2);
        Snake.Reset(startHead, Direction.Right, initialLength: 3);

        SpawnFood();
        SetState(GameState.Playing);
        ScoreChanged?.Invoke(Score);
    }

    public void Pause()
    {
        if (State == GameState.Playing)
        {
            SetState(GameState.Paused);
        }
    }

    public void Resume()
    {
        if (State == GameState.Paused)
        {
            SetState(GameState.Playing);
        }
    }

    public void TogglePause()
    {
        if (State == GameState.Playing)
            Pause();
        else if (State == GameState.Paused)
            Resume();
    }

    public void Restart()
    {
        StartGame();
        GameRestarted?.Invoke();
    }

    public void GoToMenu()
    {
        SetState(GameState.Menu);
    }

    public bool EnqueueDirection(Direction direction)
    {
        if (State != GameState.Playing)
            return false;

        return Snake.EnqueueDirection(direction);
    }

    /// <summary>
    /// Updates the game state by delta time in seconds.
    /// </summary>
    public void Update(double deltaSeconds)
    {
        if (State != GameState.Playing)
        {
            SubTickProgress = 0.0;
            return;
        }

        // Handle special food countdown
        if (SpecialFood != null)
        {
            _specialFoodTimer -= deltaSeconds;
            if (_specialFoodTimer <= 0)
            {
                SpecialFood = null;
            }
        }

        _timeAccumulator += deltaSeconds;
        double interval = StepIntervalSeconds;

        while (_timeAccumulator >= interval && State == GameState.Playing)
        {
            _timeAccumulator -= interval;
            PerformStep();
        }

        SubTickProgress = State == GameState.Playing
            ? Math.Clamp(_timeAccumulator / interval, 0.0, 1.0)
            : 0.0;
    }

    private void PerformStep()
    {
        // Advance snake by 1 grid step
        Snake.Step(grow: false);
        SnakeStepCompleted?.Invoke();

        // Check boundary collision
        if (Board.IsOutOfBounds(Snake.Head))
        {
            TriggerGameOver();
            return;
        }

        // Check self collision
        if (Snake.HasSelfCollision())
        {
            TriggerGameOver();
            return;
        }

        // Process food consumption when head reaches prey
        if (CurrentFood != null && Snake.Head == CurrentFood.Position)
        {
            var food = CurrentFood;
            Score += food.Points;
            FoodEatenCount++;
            CurrentFood = null;
            Snake.Grow();

            CheckHighScore();
            ScoreChanged?.Invoke(Score);
            FoodConsumed?.Invoke(food);

            SpawnFood();
            MaybeSpawnSpecialFood();
        }
        else if (SpecialFood != null && Snake.Head == SpecialFood.Position)
        {
            var food = SpecialFood;
            Score += food.Points;
            SpecialFood = null;
            Snake.Grow();

            CheckHighScore();
            ScoreChanged?.Invoke(Score);
            FoodConsumed?.Invoke(food);
        }
    }

    private void SpawnFood()
    {
        var occupied = new List<GridPoint>(Snake.Segments);
        if (SpecialFood != null)
        {
            occupied.Add(SpecialFood.Position);
        }

        var freeCell = Board.FindRandomFreeCell(occupied, _rng);
        if (freeCell.HasValue)
        {
            CurrentFood = new Food(freeCell.Value, FoodType.Apple, Points: 10);
        }
    }

    private void MaybeSpawnSpecialFood()
    {
        // Every 4th food eaten, spawn a Golden Apple with 10s lifetime
        if (FoodEatenCount > 0 && FoodEatenCount % 4 == 0 && SpecialFood == null)
        {
            var occupied = new List<GridPoint>(Snake.Segments);
            if (CurrentFood != null)
                occupied.Add(CurrentFood.Position);

            var freeCell = Board.FindRandomFreeCell(occupied, _rng);
            if (freeCell.HasValue)
            {
                _specialFoodTimer = 10.0;
                SpecialFood = new Food(freeCell.Value, FoodType.GoldenApple, Points: 30, LifetimeSeconds: 10.0);
            }
        }
    }

    private void CheckHighScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
            HighScoreChanged?.Invoke(HighScore);
        }
    }

    private void TriggerGameOver()
    {
        SetState(GameState.GameOver);
        GameOver?.Invoke();
    }

    private void SetState(GameState newState)
    {
        if (State != newState)
        {
            State = newState;
            StateChanged?.Invoke(State);
        }
    }
}
