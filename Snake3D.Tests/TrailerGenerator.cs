using System;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using SkiaSharp;
using Snake3D.Core;
using Snake3D.Rendering;
using Xunit;

namespace Snake3D.Tests;

public class TrailerGenerator
{
    [Fact]
    public void GenerateStoreTrailerAndCaptions()
    {
        string baseDir = Path.Combine("..", "..", "..", "..", "assets", "store");
        string trailerDir = Path.Combine(baseDir, "microsoft", "trailers");
        Directory.CreateDirectory(trailerDir);

        string tempFramesDir = Path.Combine(Path.GetTempPath(), "Snake3D_Trailer_Frames");
        if (Directory.Exists(tempFramesDir))
        {
            Directory.Delete(tempFramesDir, true);
        }
        Directory.CreateDirectory(tempFramesDir);

        int width = 1920;
        int height = 1080;
        int fps = 30;
        int totalSeconds = 15;
        int totalFrames = totalSeconds * fps;

        var engine = new GameEngine(24, 24);
        var renderer = new GameRenderer3D();
        engine.StartGame();
        engine.Score = 120;
        engine.HighScore = 580;

        for (int i = 0; i < 6; i++)
        {
            engine.Snake.Step(grow: true);
        }

        // Render 15 seconds of high-fidelity 3D gameplay animation frames
        for (int frame = 0; frame < totalFrames; frame++)
        {
            float time = (float)frame / fps;

            using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using var canvas = new SKCanvas(bitmap);

            // Simulation updates
            if (frame > 90 && frame % 15 == 0 && frame < 330)
            {
                engine.Snake.Step(grow: frame % 30 == 0);
                engine.Score += 10;
            }

            if (frame == 210)
            {
                engine.Score += 30; // Golden Apple eaten
                if (engine.SpecialFood != null)
                {
                    renderer.OnFoodEaten(engine.SpecialFood, engine);
                }
            }

            // Render 3D scene
            renderer.Render(canvas, width, height, engine, 0.033f);

            // Overlay trailer titles & motion graphics based on timestamp
            if (time < 3.5f)
            {
                // Intro scene: Title banner
                float alpha = Math.Clamp(time < 1.0f ? time : (3.5f - time) * 2f, 0f, 1f);
                DrawTrailerIntroCard(canvas, width, height, alpha);
            }
            else if (time >= 3.5f && time < 7.5f)
            {
                // Gameplay HUD
                DrawTrailerHUD(canvas, width, height, $"SCORE: {engine.Score}", "BEST: 580", "🌿 3D NATURE MEADOW");
            }
            else if (time >= 7.5f && time < 11.5f)
            {
                // Golden Apple Bonus HUD
                DrawTrailerHUD(canvas, width, height, $"SCORE: {engine.Score}", "BEST: 580", "✨ GOLDEN APPLE BONUS! +30 PTS");
            }
            else if (time >= 11.5f && time < 13.5f)
            {
                // Speed options HUD
                DrawTrailerHUD(canvas, width, height, $"SCORE: {engine.Score}", "BEST: 580", "⚡ 3 CUSTOMIZABLE SPEED MODES");
            }
            else
            {
                // Outro End Card
                float alpha = Math.Clamp((time - 13.5f) * 2f, 0f, 1f);
                DrawTrailerOutroCard(canvas, width, height, alpha);
            }

            string framePath = Path.Combine(tempFramesDir, $"frame_{frame:D5}.png");
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 95);
            File.WriteAllBytes(framePath, data.ToArray());
        }

        // Generate Soundtrack WAV
        string audioWavPath = Path.Combine(tempFramesDir, "soundtrack.wav");
        GenerateTrailerAudioTrack(audioWavPath, totalSeconds);

        // Thumbnail
        string thumbnailPath = Path.Combine(trailerDir, "trailer_thumbnail_1920x1080.png");
        File.Copy(Path.Combine(tempFramesDir, "frame_00075.png"), thumbnailPath, true);

        // Compile MP4 with FFmpeg
        string outputMp4 = Path.Combine(trailerDir, "trailer_1080p.mp4");
        if (File.Exists(outputMp4)) File.Delete(outputMp4);

        RunFfmpeg($"-y -framerate {fps} -i \"{Path.Combine(tempFramesDir, "frame_%05d.png")}\" -i \"{audioWavPath}\" -c:v libx264 -pix_fmt yuv420p -c:a aac -b:a 192k -shortest -movflags +faststart \"{outputMp4}\"");

        // Generate Closed Captions (WebVTT and SRT)
        GenerateClosedCaptions(trailerDir);

        // Generate Audio Descriptions (WebVTT and SRT)
        GenerateAudioDescriptions(trailerDir);

        // Copy trailer files to root assets for convenience
        string rootTrailerDir = Path.Combine(baseDir, "trailers");
        Directory.CreateDirectory(rootTrailerDir);
        File.Copy(outputMp4, Path.Combine(rootTrailerDir, "trailer_1080p.mp4"), true);
        File.Copy(thumbnailPath, Path.Combine(rootTrailerDir, "trailer_thumbnail_1920x1080.png"), true);
        File.Copy(Path.Combine(trailerDir, "trailer_closed_captions.vtt"), Path.Combine(rootTrailerDir, "trailer_closed_captions.vtt"), true);
        File.Copy(Path.Combine(trailerDir, "trailer_audio_description.vtt"), Path.Combine(rootTrailerDir, "trailer_audio_description.vtt"), true);

        Assert.True(File.Exists(outputMp4));
        Assert.True(File.Exists(thumbnailPath));
        Assert.True(File.Exists(Path.Combine(trailerDir, "trailer_closed_captions.vtt")));
        Assert.True(File.Exists(Path.Combine(trailerDir, "trailer_audio_description.vtt")));
    }

    private static void DrawTrailerIntroCard(SKCanvas canvas, int width, int height, float alpha)
    {
        byte a = (byte)(alpha * 240);
        using var scrim = new SKPaint { Color = new SKColor(2, 6, 23, a) };
        canvas.DrawRect(0, 0, width, height, scrim);

        float titleSize = 76f;
        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(248, 250, 252, (byte)(alpha * 255)),
            FakeBoldText = true,
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText("SNAKE 3D : NEW ERA", width * 0.5f, (height * 0.45f), SKTextAlign.Center, new SKFont { Size = titleSize, Embolden = true }, textPaint);

        using var subPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(52, 211, 153, (byte)(alpha * 255)),
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText("THE MODERN 3D RETRO REMAKE", width * 0.5f, (height * 0.54f), SKTextAlign.Center, new SKFont { Size = 30f, Embolden = true }, subPaint);
    }

    private static void DrawTrailerOutroCard(SKCanvas canvas, int width, int height, float alpha)
    {
        byte a = (byte)(alpha * 245);
        using var scrim = new SKPaint { Color = new SKColor(2, 6, 23, a) };
        canvas.DrawRect(0, 0, width, height, scrim);

        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(248, 250, 252, (byte)(alpha * 255)),
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText("SNAKE 3D : NEW ERA", width * 0.5f, height * 0.42f, SKTextAlign.Center, new SKFont { Size = 72f, Embolden = true }, textPaint);

        using var goldPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(251, 191, 36, (byte)(alpha * 255)),
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText("AVAILABLE NOW ON WINDOWS 10/11 & XBOX", width * 0.5f, height * 0.52f, SKTextAlign.Center, new SKFont { Size = 28f, Embolden = true }, goldPaint);

        using var freeBadge = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(16, 185, 129, (byte)(alpha * 255)),
            TextAlign = SKTextAlign.Center
        };
        canvas.DrawText("★ 100% FREE TO PLAY ★", width * 0.5f, height * 0.60f, SKTextAlign.Center, new SKFont { Size = 24f, Embolden = true }, freeBadge);
    }

    private static void DrawTrailerHUD(SKCanvas canvas, int width, int height, string score, string best, string status)
    {
        float pad = 40f;
        float fontSz = 26f;

        // Top Left Score
        DrawBadge(canvas, pad, pad, score, fontSz, new SKColor(52, 211, 153));

        // Top Right Best
        DrawBadge(canvas, width - pad - 260f, pad, best, fontSz, new SKColor(251, 191, 36));

        // Bottom Status Banner
        DrawBadge(canvas, (width - (status.Length * 20f)) * 0.5f, height - pad - 50f, status, fontSz, new SKColor(248, 250, 252));
    }

    private static void DrawBadge(SKCanvas canvas, float x, float y, string text, float textSize, SKColor textColor)
    {
        float badgeW = textSize * (text.Length * 0.65f + 1.8f);
        float badgeH = textSize * 1.9f;

        using var bgPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(15, 23, 42, 220)
        };
        using var borderPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            Color = new SKColor(textColor.Red, textColor.Green, textColor.Blue, 150)
        };

        canvas.DrawRoundRect(new SKRect(x, y, x + badgeW, y + badgeH), badgeH * 0.35f, badgeH * 0.35f, bgPaint);
        canvas.DrawRoundRect(new SKRect(x, y, x + badgeW, y + badgeH), badgeH * 0.35f, badgeH * 0.35f, borderPaint);

        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            Color = textColor
        };
        canvas.DrawText(text, x + (textSize * 0.9f), y + (badgeH * 0.64f), SKTextAlign.Left, new SKFont { Size = textSize, Embolden = true }, textPaint);
    }

    private static void GenerateTrailerAudioTrack(string path, int seconds)
    {
        int sampleRate = 44100;
        int totalSamples = sampleRate * seconds;
        var samples = new short[totalSamples];
        var rng = new Random(42);

        // Chord progression: Am -> F -> C -> G (Synth arpeggios + slither + chimes)
        float[] notes = [220.0f, 261.63f, 329.63f, 440.0f, 349.23f, 392.0f, 523.25f];

        for (int i = 0; i < totalSamples; i++)
        {
            float t = (float)i / sampleRate;
            float val = 0f;

            // 1. Synth Arpeggio Bass & Melody
            int noteIndex = (int)(t * 4.0f) % notes.Length;
            float freq = notes[noteIndex];
            float noteT = (t * 4.0f) - MathF.Floor(t * 4.0f);
            float env = MathF.Pow(1.0f - noteT, 1.5f);
            float melodicTone = MathF.Sin(2f * MathF.PI * freq * t) * env * 0.30f;
            val += melodicTone;

            // 2. Continuous sub-bass warmth (55Hz / 110Hz)
            float bass = MathF.Sin(2f * MathF.PI * 55f * t) * 0.15f;
            val += bass;

            // 3. Apple Eating Chimes at 3.0s, 5.0s, 7.0s, 9.0s
            if (t is >= 7.0f and <= 7.4f)
            {
                float chimeT = (t - 7.0f) / 0.4f;
                float chime = MathF.Sin(2f * MathF.PI * 880f * t) * MathF.Pow(1f - chimeT, 2f) * 0.45f;
                val += chime;
            }

            // 4. Golden Apple Sparkle at 7.0s
            if (t is >= 7.0f and <= 8.5f)
            {
                float sparkle = ((rng.NextSingle() * 2f) - 1f) * MathF.Sin(t * 30f) * 0.18f;
                val += sparkle;
            }

            // Fade in and out
            float masterFade = Math.Clamp(t < 1.0f ? t : (seconds - t), 0f, 1f);
            samples[i] = (short)(Math.Clamp(val * masterFade, -1f, 1f) * 32767);
        }

        File.WriteAllBytes(path, EncodePcmToWav(samples, sampleRate));
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

    private static void GenerateClosedCaptions(string outputDir)
    {
        // WebVTT format
        string vttContent = @"WEBVTT
Kind: captions
Language: en

00:00:00.500 --> 00:00:03.500
[Upbeat electronic synth music plays]
SNAKE 3D : NEW ERA - The Modern 3D Retro Remake.

00:00:03.600 --> 00:00:07.400
Slither across a rich 24 by 24 3D nature meadow with realistic serpentine physics.

00:00:07.500 --> 00:00:11.400
[Chime rings and particles burst]
Hunt juicy apples and capture the rare glowing Golden Apple for massive +30 bonus points!

00:00:11.500 --> 00:00:13.500
Choose from 3 custom speed modes: Relaxed, Normal, or Fast!

00:00:13.600 --> 00:00:15.000
Snake 3D : New Era. Available now on Windows 10, Windows 11, and Xbox. 100% Free to Play!
";
        File.WriteAllText(Path.Combine(outputDir, "trailer_closed_captions.vtt"), vttContent);

        // SRT format
        string srtContent = @"1
00:00:00,500 --> 00:00:03,500
[Upbeat electronic synth music plays]
SNAKE 3D : NEW ERA - The Modern 3D Retro Remake.

2
00:00:03,600 --> 00:00:07,400
Slither across a rich 24 by 24 3D nature meadow with realistic serpentine physics.

3
00:00:07,500 --> 00:00:11,400
[Chime rings and particles burst]
Hunt juicy apples and capture the rare glowing Golden Apple for massive +30 bonus points!

4
00:00:11,500 --> 00:00:13,500
Choose from 3 custom speed modes: Relaxed, Normal, or Fast!

5
00:00:13,600 --> 00:00:15,000
Snake 3D : New Era. Available now on Windows 10, Windows 11, and Xbox. 100% Free to Play!
";
        File.WriteAllText(Path.Combine(outputDir, "trailer_closed_captions.srt"), srtContent);
    }

    private static void GenerateAudioDescriptions(string outputDir)
    {
        // WebVTT format for Audio Description
        string vttContent = @"WEBVTT
Kind: descriptions
Language: en

00:00:00.500 --> 00:00:03.500
A cinematic title card appears on a deep midnight blue background reading: Snake 3D : New Era, The Modern 3D Retro Remake.

00:00:03.600 --> 00:00:07.400
The camera pans over an expansive 3D nature meadow where a green viper serpent slithers smoothly between swaying grass and blooming daisies.

00:00:07.500 --> 00:00:11.400
The snake consumes a shiny red apple, causing a visible digestion lump to ripple down its vertebrae. A rare golden apple appears and bursts into sparkling golden particles.

00:00:11.500 --> 00:00:13.500
HUD indicators highlight Relaxed, Normal, and Fast speed presets alongside high score tracking.

00:00:13.600 --> 00:00:15.000
The final screen displays Snake 3D : New Era, Available on Windows 10, Windows 11, and Xbox.
";
        File.WriteAllText(Path.Combine(outputDir, "trailer_audio_description.vtt"), vttContent);

        // SRT format for Audio Description
        string srtContent = @"1
00:00:00,500 --> 00:00:03,500
A cinematic title card appears on a deep midnight blue background reading: Snake 3D : New Era, The Modern 3D Retro Remake.

2
00:00:03,600 --> 00:00:07,400
The camera pans over an expansive 3D nature meadow where a green viper serpent slithers smoothly between swaying grass and blooming daisies.

3
00:00:07,500 --> 00:00:11,400
The snake consumes a shiny red apple, causing a visible digestion lump to ripple down its vertebrae. A rare golden apple appears and bursts into sparkling golden particles.

4
00:00:11,500 --> 00:00:13,500
HUD indicators highlight Relaxed, Normal, and Fast speed presets alongside high score tracking.

5
00:00:13,600 --> 00:00:15,000
The final screen displays Snake 3D : New Era, Available on Windows 10, Windows 11, and Xbox.
";
        File.WriteAllText(Path.Combine(outputDir, "trailer_audio_description.srt"), srtContent);
    }

    private static void RunFfmpeg(string arguments)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proc = Process.Start(psi);
        proc?.WaitForExit();
    }
}
