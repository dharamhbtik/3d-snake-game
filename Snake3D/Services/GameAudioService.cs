using System.Diagnostics;
using Snake3D.Core;

namespace Snake3D.Services;

/// <summary>
/// Cross-platform game audio feedback service generating synthesized PCM waveforms for snake slithering, snake hissing, frog croaking, insect eating, and game events.
/// </summary>
public sealed class GameAudioService
{
    private readonly HighScoreService _highScoreService;
    private readonly string _soundDir;
    private long _lastSlitherTick;

    public GameAudioService(HighScoreService highScoreService)
    {
        _highScoreService = highScoreService;
        _soundDir = Path.Combine(Path.GetTempPath(), "Snake3D_Audio");

        try
        {
            Directory.CreateDirectory(_soundDir);
            EnsureWavFilesGenerated();
        }
        catch
        {
            // Ignore directory creation errors
        }
    }

    /// <summary>
    /// Plays organic serpentine grass rustling / slithering audio when the snake moves.
    /// </summary>
    public void PlaySlitherSound()
    {
        if (!_highScoreService.SoundEnabled)
            return;

        long now = Environment.TickCount64;
        if (now - _lastSlitherTick < 140)
            return;

        _lastSlitherTick = now;
        PlayWavAsync("slither.wav");
    }

    /// <summary>
    /// Plays snake hiss + frog croak / insect crunch sound when the snake consumes prey.
    /// </summary>
    public void PlayPreyCatchSound(Food food)
    {
        if (!_highScoreService.SoundEnabled)
            return;

        if (food.IsSpecial)
        {
            PlayWavAsync("special_eat.wav");
        }
        else if (food.Type == FoodType.Frog)
        {
            PlayWavAsync("frog_eat_hiss.wav");
        }
        else
        {
            PlayWavAsync("insect_eat_hiss.wav");
        }
    }

    public void PlayEatSound(bool isSpecial = false)
    {
        if (!_highScoreService.SoundEnabled)
            return;

        PlayWavAsync(isSpecial ? "special_eat.wav" : "frog_eat_hiss.wav");
    }

    public void PlayGameOverSound()
    {
        if (!_highScoreService.SoundEnabled)
            return;

        PlayWavAsync("gameover.wav");
    }

    public void PlayButtonClick()
    {
        if (!_highScoreService.SoundEnabled)
            return;

        PlayWavAsync("click.wav");
    }

    private void PlayWavAsync(string filename)
    {
        string path = Path.Combine(_soundDir, filename);
        if (!File.Exists(path))
            return;

        Task.Run(() =>
        {
            try
            {
                if (OperatingSystem.IsMacOS())
                {
                    using var proc = Process.Start(new ProcessStartInfo
                    {
                        FileName = "afplay",
                        Arguments = $"\"{path}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    });
                }
                else if (OperatingSystem.IsWindows())
                {
                    using var proc = Process.Start(new ProcessStartInfo
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-c (New-Object Media.SoundPlayer '{path}').PlaySync()",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
                else if (OperatingSystem.IsLinux())
                {
                    using var proc = Process.Start(new ProcessStartInfo
                    {
                        FileName = "aplay",
                        Arguments = $"-q \"{path}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    });
                }
            }
            catch
            {
                // Ignore platform playback errors
            }
        });
    }

    private void EnsureWavFilesGenerated()
    {
        try
        {
            // 1. Serpentine grass rustle / slither
            string slitherPath = Path.Combine(_soundDir, "slither.wav");
            var slitherSamples = GenerateSlitherSamples(sampleRate: 22050, durationSeconds: 0.09f);
            File.WriteAllBytes(slitherPath, EncodePcmToWav(slitherSamples, 22050));

            // 2. Frog eating + Snake Hiss + Frog Croak
            string frogEatPath = Path.Combine(_soundDir, "frog_eat_hiss.wav");
            var frogEatSamples = GenerateFrogEatHissSamples(sampleRate: 22050, durationSeconds: 0.28f);
            File.WriteAllBytes(frogEatPath, EncodePcmToWav(frogEatSamples, 22050));

            // 3. Insect eating + Snake Hiss + snap crunch
            string insectEatPath = Path.Combine(_soundDir, "insect_eat_hiss.wav");
            var insectEatSamples = GenerateInsectEatHissSamples(sampleRate: 22050, durationSeconds: 0.22f);
            File.WriteAllBytes(insectEatPath, EncodePcmToWav(insectEatSamples, 22050));

            // 4. Special golden frog / dragonfly chime + hiss
            string specialEatPath = Path.Combine(_soundDir, "special_eat.wav");
            var specialSamples = GenerateSpecialEatSamples(sampleRate: 22050, durationSeconds: 0.32f);
            File.WriteAllBytes(specialEatPath, EncodePcmToWav(specialSamples, 22050));

            // 5. Game Over
            string gameoverPath = Path.Combine(_soundDir, "gameover.wav");
            var gameoverSamples = GenerateGameOverSamples(sampleRate: 22050, durationSeconds: 0.35f);
            File.WriteAllBytes(gameoverPath, EncodePcmToWav(gameoverSamples, 22050));

            // 6. Click
            string clickPath = Path.Combine(_soundDir, "click.wav");
            var clickSamples = GenerateClickSamples(sampleRate: 22050, durationSeconds: 0.04f);
            File.WriteAllBytes(clickPath, EncodePcmToWav(clickSamples, 22050));
        }
        catch
        {
            // Ignore generation errors
        }
    }

    private static short[] GenerateSlitherSamples(int sampleRate, float durationSeconds)
    {
        int totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];
        var rng = new Random(1234);

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            float envelope = MathF.Sin(t * MathF.PI);

            float noise = ((rng.NextSingle() * 2f) - 1f) * 0.45f;
            float tone = MathF.Sin(2f * MathF.PI * (140f + (t * 80f)) * ((float)i / sampleRate)) * 0.55f;

            float val = (noise + tone) * envelope * 0.40f;
            samples[i] = (short)(Math.Clamp(val, -1f, 1f) * 32767);
        }

        return samples;
    }

    /// <summary>
    /// Generates realistic Frog Croak / Ribbit followed by a sharp gulp and serpent HISS ("Hisssss!").
    /// </summary>
    private static short[] GenerateFrogEatHissSamples(int sampleRate, float durationSeconds)
    {
        int totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];
        var rng = new Random(5678);

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            float val = 0f;

            // 1. Initial Frog Ribbit / Croak (first 35% of sound): 180Hz guttural croak with rapid pulse modulation
            if (t < 0.38f)
            {
                float frogT = t / 0.38f;
                float frogEnv = MathF.Sin(frogT * MathF.PI);
                float frogPulse = MathF.Sin(2f * MathF.PI * 45f * ((float)i / sampleRate));
                float frogPitch = 190f + (frogPulse * 50f);
                float frogTone = MathF.Sin(2f * MathF.PI * frogPitch * ((float)i / sampleRate)) * 0.7f;
                float frogHarmonic = MathF.Sin(2f * MathF.PI * frogPitch * 2.2f * ((float)i / sampleRate)) * 0.35f;
                val += (frogTone + frogHarmonic) * frogEnv * 0.65f;
            }

            // 2. Gulp snap crunch in the middle (20% to 50%)
            if (t is >= 0.20f and <= 0.55f)
            {
                float gulpT = (t - 0.20f) / 0.35f;
                float gulpEnv = MathF.Sin(gulpT * MathF.PI);
                float gulpPitch = 480f * (1.0f - (gulpT * 0.4f));
                float gulpTone = MathF.Sin(2f * MathF.PI * gulpPitch * ((float)i / sampleRate));
                val += gulpTone * gulpEnv * 0.50f;
            }

            // 3. Serpent HISS ("Hissssss!") (from 30% to 100%): high-frequency filtered pink noise
            if (t >= 0.28f)
            {
                float hissT = (t - 0.28f) / 0.72f;
                float hissEnv = MathF.Pow(1.0f - hissT, 0.75f) * MathF.Sin(MathF.Min(1f, hissT * 4f) * MathF.PI * 0.5f);
                float hissNoise = ((rng.NextSingle() * 2f) - 1f);
                // High frequency sibilance hiss around 3200Hz
                float hissTone = MathF.Sin(2f * MathF.PI * 3400f * ((float)i / sampleRate)) * 0.3f;
                val += (hissNoise + hissTone) * hissEnv * 0.65f;
            }

            samples[i] = (short)(Math.Clamp(val, -1f, 1f) * 32767);
        }

        return samples;
    }

    /// <summary>
    /// Generates crisp insect snap crunch followed by a sharp serpent HISS.
    /// </summary>
    private static short[] GenerateInsectEatHissSamples(int sampleRate, float durationSeconds)
    {
        int totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];
        var rng = new Random(9012);

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            float val = 0f;

            // Initial cricket chirp / snap
            if (t < 0.25f)
            {
                float snapT = t / 0.25f;
                float snapEnv = MathF.Pow(1.0f - snapT, 0.6f);
                float chirp = MathF.Sin(2f * MathF.PI * (1600f + (snapT * 400f)) * ((float)i / sampleRate));
                val += chirp * snapEnv * 0.65f;
            }

            // Serpent Hiss
            if (t >= 0.15f)
            {
                float hissT = (t - 0.15f) / 0.85f;
                float hissEnv = MathF.Pow(1.0f - hissT, 0.8f);
                float hissNoise = ((rng.NextSingle() * 2f) - 1f);
                val += hissNoise * hissEnv * 0.55f;
            }

            samples[i] = (short)(Math.Clamp(val, -1f, 1f) * 32767);
        }

        return samples;
    }

    private static short[] GenerateSpecialEatSamples(int sampleRate, float durationSeconds)
    {
        int totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];
        var rng = new Random(3456);

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            float bellEnv = MathF.Pow(1.0f - t, 0.9f);
            float bell = MathF.Sin(2f * MathF.PI * 784f * ((float)i / sampleRate)) * 0.6f
                       + MathF.Sin(2f * MathF.PI * 1568f * ((float)i / sampleRate)) * 0.35f;

            float hissNoise = (t > 0.2f) ? (((rng.NextSingle() * 2f) - 1f) * (1f - t) * 0.35f) : 0f;

            float val = (bell * bellEnv) + hissNoise;
            samples[i] = (short)(Math.Clamp(val, -1f, 1f) * 32767);
        }

        return samples;
    }

    private static short[] GenerateGameOverSamples(int sampleRate, float durationSeconds)
    {
        int totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            float envelope = MathF.Pow(1.0f - t, 0.75f);
            float freq = 260f * (1.0f - (t * 0.65f));

            float val = MathF.Sin(2f * MathF.PI * freq * ((float)i / sampleRate)) * envelope * 0.65f;
            samples[i] = (short)(Math.Clamp(val, -1f, 1f) * 32767);
        }

        return samples;
    }

    private static short[] GenerateClickSamples(int sampleRate, float durationSeconds)
    {
        int totalSamples = (int)(sampleRate * durationSeconds);
        var samples = new short[totalSamples];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / totalSamples;
            float envelope = MathF.Pow(1.0f - t, 2.0f);
            float val = MathF.Sin(2f * MathF.PI * 880f * ((float)i / sampleRate)) * envelope * 0.45f;
            samples[i] = (short)(Math.Clamp(val, -1f, 1f) * 32767);
        }

        return samples;
    }

    private static byte[] EncodePcmToWav(short[] samples, int sampleRate)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        int byteRate = sampleRate * 2;
        int dataSize = samples.Length * 2;

        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + dataSize);
        writer.Write("WAVE"u8.ToArray());

        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(sampleRate);
        writer.Write(byteRate);
        writer.Write((short)2);
        writer.Write((short)16);

        writer.Write("data"u8.ToArray());
        writer.Write(dataSize);

        for (int i = 0; i < samples.Length; i++)
        {
            writer.Write(samples[i]);
        }

        return ms.ToArray();
    }
}
