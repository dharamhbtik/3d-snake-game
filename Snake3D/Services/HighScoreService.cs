using System.Text.Json;

namespace Snake3D.Services;

public sealed class PlayerPreferences
{
    public int HighScore { get; set; }
    public bool SoundEnabled { get; set; } = true;
    public Snake3D.Core.GameSpeed Speed { get; set; } = Snake3D.Core.GameSpeed.Normal;
}

/// <summary>
/// Handles local high score and user preference persistence across macOS, Windows, and Linux.
/// </summary>
public sealed class HighScoreService
{
    private readonly string _filePath;
    private PlayerPreferences _preferences = new();

    public int HighScore => _preferences.HighScore;
    public Snake3D.Core.GameSpeed Speed
    {
        get => _preferences.Speed;
        set
        {
            _preferences.Speed = value;
            Save();
        }
    }

    public bool SoundEnabled
    {
        get => _preferences.SoundEnabled;
        set
        {
            _preferences.SoundEnabled = value;
            Save();
        }
    }

    public HighScoreService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var dir = Path.Combine(appData, "Snake3D");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "preferences.json");
        Load();
    }

    public void SaveHighScore(int newScore)
    {
        if (newScore > _preferences.HighScore)
        {
            _preferences.HighScore = newScore;
            Save();
        }
    }

    private void Load()
    {
        try
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _preferences = JsonSerializer.Deserialize<PlayerPreferences>(json) ?? new PlayerPreferences();
            }
        }
        catch
        {
            _preferences = new PlayerPreferences();
        }
    }

    private void Save()
    {
        try
        {
            var json = JsonSerializer.Serialize(_preferences, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }
        catch
        {
            // Ignore persistence errors
        }
    }
}
