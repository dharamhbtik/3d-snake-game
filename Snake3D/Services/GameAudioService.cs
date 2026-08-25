namespace Snake3D.Services;

/// <summary>
/// Lightweight game audio feedback service.
/// </summary>
public sealed class GameAudioService
{
    private readonly HighScoreService _highScoreService;

    public GameAudioService(HighScoreService highScoreService)
    {
        _highScoreService = highScoreService;
    }

    public void PlayEatSound(bool isSpecial = false)
    {
        if (!_highScoreService.SoundEnabled)
            return;

        // Visual and procedural feedback is primary.
        // System beep or sound triggering can be safely invoked here.
        TryPlayBeep(isSpecial ? 880 : 587, 60);
    }

    public void PlayGameOverSound()
    {
        if (!_highScoreService.SoundEnabled)
            return;

        TryPlayBeep(220, 200);
    }

    public void PlayButtonClick()
    {
        if (!_highScoreService.SoundEnabled)
            return;

        TryPlayBeep(440, 30);
    }

    private static void TryPlayBeep(int frequency, int durationMs)
    {
        try
        {
            if (OperatingSystem.IsWindows())
            {
                Task.Run(() =>
                {
#pragma warning disable CA1416 // Validate platform compatibility
                    try { Console.Beep(frequency, durationMs); } catch { }
#pragma warning restore CA1416
                });
            }
        }
        catch
        {
            // Ignore sound output errors
        }
    }
}
