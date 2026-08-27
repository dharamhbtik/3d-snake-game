using System.Diagnostics;
using Snake3D.Core;
using Snake3D.Rendering;
using Snake3D.Services;

namespace Snake3D.ViewModels;

public sealed partial class MainViewModel : ObservableObject
{
    private readonly GameEngine _engine;
    private readonly HighScoreService _highScoreService;
    private readonly GameAudioService _audioService;
    private readonly GameRenderer3D _renderer;
    private readonly Stopwatch _stopwatch = new();
    private double _lastElapsedSeconds;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsMenu))]
    [NotifyPropertyChangedFor(nameof(IsPlaying))]
    [NotifyPropertyChangedFor(nameof(IsPaused))]
    [NotifyPropertyChangedFor(nameof(IsGameOver))]
    [NotifyPropertyChangedFor(nameof(ShowHud))]
    private GameState _state = GameState.Menu;

    [ObservableProperty]
    private int _score;

    [ObservableProperty]
    private int _highScore;

    [ObservableProperty]
    private int _level = 1;

    [ObservableProperty]
    private string _preyLabel = "🐸 FROG (+25)";

    [ObservableProperty]
    private bool _isNewHighScore;

    [ObservableProperty]
    private bool _soundEnabled = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SpeedLabel))]
    [NotifyPropertyChangedFor(nameof(IsSpeedRelaxed))]
    [NotifyPropertyChangedFor(nameof(IsSpeedNormal))]
    [NotifyPropertyChangedFor(nameof(IsSpeedFast))]
    private GameSpeed _speed = GameSpeed.Normal;

    public string SpeedLabel => Speed switch
    {
        GameSpeed.Relaxed => "🐢 RELAXED",
        GameSpeed.Fast => "⚡ FAST",
        _ => "🎯 NORMAL"
    };

    public bool IsSpeedRelaxed => Speed == GameSpeed.Relaxed;
    public bool IsSpeedNormal => Speed == GameSpeed.Normal;
    public bool IsSpeedFast => Speed == GameSpeed.Fast;

    public bool IsMenu => State == GameState.Menu;
    public bool IsPlaying => State == GameState.Playing;
    public bool IsPaused => State == GameState.Paused;
    public bool IsGameOver => State == GameState.GameOver;
    public bool ShowHud => State is GameState.Playing or GameState.Paused or GameState.GameOver;

    public GameEngine Engine => _engine;
    public GameRenderer3D Renderer => _renderer;

    public MainViewModel()
    {
        _highScoreService = new HighScoreService();
        _audioService = new GameAudioService(_highScoreService);
        _renderer = new GameRenderer3D();

        _engine = new GameEngine(boardWidth: 24, boardHeight: 24);
        HighScore = _highScoreService.HighScore;
        _engine.HighScore = HighScore;
        SoundEnabled = _highScoreService.SoundEnabled;
        Speed = _highScoreService.Speed;
        _engine.Speed = Speed;

        // Wire Engine events
        _engine.StateChanged += OnEngineStateChanged;
        _engine.ScoreChanged += s => Score = s;
        _engine.HighScoreChanged += OnHighScoreUpdated;
        _engine.FoodConsumed += OnFoodConsumed;
        _engine.SnakeStepCompleted += () =>
        {
            _audioService.PlaySlitherSound();
            if (_engine.CurrentFood != null)
            {
                PreyLabel = _engine.SpecialFood != null ? _engine.SpecialFood.DisplayName : _engine.CurrentFood.DisplayName;
            }
        };
        _engine.GameOver += OnGameOver;

        _stopwatch.Start();
    }

    private void OnEngineStateChanged(GameState newState)
    {
        State = newState;
        if (newState == GameState.Playing && _engine.CurrentFood != null)
        {
            PreyLabel = _engine.CurrentFood.DisplayName;
        }
    }

    private void OnHighScoreUpdated(int newRecord)
    {
        HighScore = newRecord;
        IsNewHighScore = true;
        _highScoreService.SaveHighScore(newRecord);
    }

    private void OnFoodConsumed(Food food)
    {
        _audioService.PlayPreyCatchSound(food);
        _renderer.OnFoodEaten(food, _engine);
        Level = _engine.Level;
        if (_engine.CurrentFood != null)
        {
            PreyLabel = _engine.SpecialFood != null ? _engine.SpecialFood.DisplayName : _engine.CurrentFood.DisplayName;
        }
    }

    private void OnGameOver()
    {
        _audioService.PlayGameOverSound();
        _renderer.OnGameOver(_engine);
        _highScoreService.SaveHighScore(Score);
    }

    [RelayCommand]
    public void StartGame()
    {
        IsNewHighScore = false;
        _audioService.PlayButtonClick();
        _engine.StartGame();
    }

    [RelayCommand]
    public void Pause()
    {
        _audioService.PlayButtonClick();
        _engine.Pause();
    }

    [RelayCommand]
    public void Resume()
    {
        _audioService.PlayButtonClick();
        _engine.Resume();
    }

    [RelayCommand]
    public void Restart()
    {
        IsNewHighScore = false;
        _audioService.PlayButtonClick();
        _engine.Restart();
    }

    [RelayCommand]
    public void GoToMenu()
    {
        _audioService.PlayButtonClick();
        _engine.GoToMenu();
    }

    [RelayCommand]
    public void ToggleSound()
    {
        SoundEnabled = !SoundEnabled;
        _highScoreService.SoundEnabled = SoundEnabled;
    }

    [RelayCommand]
    public void SetSpeed(string speedName)
    {
        if (Enum.TryParse<GameSpeed>(speedName, out var newSpeed))
        {
            Speed = newSpeed;
            _engine.Speed = newSpeed;
            _highScoreService.Speed = newSpeed;
            _audioService.PlayButtonClick();
        }
    }

    [RelayCommand]
    public void CycleSpeed()
    {
        var nextSpeed = Speed switch
        {
            GameSpeed.Relaxed => GameSpeed.Normal,
            GameSpeed.Normal => GameSpeed.Fast,
            _ => GameSpeed.Relaxed
        };
        Speed = nextSpeed;
        _engine.Speed = nextSpeed;
        _highScoreService.Speed = nextSpeed;
        _audioService.PlayButtonClick();
    }

    private readonly HashSet<Windows.System.VirtualKey> _heldMovementKeys = new();
    private double _keyHoldDuration;

    public void HandleKeyDown(Windows.System.VirtualKey key)
    {
        switch (key)
        {
            case Windows.System.VirtualKey.W:
            case Windows.System.VirtualKey.Up:
                _heldMovementKeys.Add(key);
                _engine.EnqueueDirection(Direction.Up);
                break;

            case Windows.System.VirtualKey.S:
            case Windows.System.VirtualKey.Down:
                _heldMovementKeys.Add(key);
                _engine.EnqueueDirection(Direction.Down);
                break;

            case Windows.System.VirtualKey.A:
            case Windows.System.VirtualKey.Left:
                _heldMovementKeys.Add(key);
                _engine.EnqueueDirection(Direction.Left);
                break;

            case Windows.System.VirtualKey.D:
            case Windows.System.VirtualKey.Right:
                _heldMovementKeys.Add(key);
                _engine.EnqueueDirection(Direction.Right);
                break;

            case Windows.System.VirtualKey.Space:
                if (State == GameState.Playing)
                    Pause();
                else if (State == GameState.Paused)
                    Resume();
                else if (State is GameState.Menu or GameState.GameOver)
                    StartGame();
                break;

            case Windows.System.VirtualKey.Enter:
                if (State is GameState.Menu or GameState.GameOver)
                    StartGame();
                break;

            case Windows.System.VirtualKey.Escape:
                if (State == GameState.Playing)
                    Pause();
                else if (State == GameState.Paused)
                    GoToMenu();
                break;
        }
    }

    public void HandleKeyUp(Windows.System.VirtualKey key)
    {
        _heldMovementKeys.Remove(key);
        if (_heldMovementKeys.Count == 0)
        {
            _keyHoldDuration = 0.0;
            _engine.SpeedBoostMultiplier = 1.0;
        }
    }

    public void UpdateFrame()
    {
        double currentSeconds = _stopwatch.Elapsed.TotalSeconds;
        double dt = currentSeconds - _lastElapsedSeconds;
        _lastElapsedSeconds = currentSeconds;

        // Clamp large delta jumps (e.g. window dragging/resuming)
        dt = Math.Min(dt, 0.1);

        // Dynamic speed boost on holding movement keys (longer press = faster speed)
        if (_heldMovementKeys.Count > 0 && State == GameState.Playing)
        {
            _keyHoldDuration += dt;
            // Scales smoothly from 1.0x up to 3.5x as key is held
            _engine.SpeedBoostMultiplier = 1.0 + Math.Min(2.5, _keyHoldDuration * 2.2);
        }
        else
        {
            _keyHoldDuration = 0.0;
            _engine.SpeedBoostMultiplier = 1.0;
        }

        _engine.Update(dt);
    }
}
