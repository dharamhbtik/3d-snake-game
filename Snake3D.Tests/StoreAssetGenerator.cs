using System;
using System.IO;
using System.Numerics;
using SkiaSharp;
using Snake3D.Core;
using Snake3D.Rendering;
using Xunit;

namespace Snake3D.Tests;

public class StoreAssetGenerator
{
    private static readonly SKColor DarkBgStart = new(15, 23, 42); // #0F172A
    private static readonly SKColor DarkBgEnd = new(2, 6, 23);     // #020617
    private static readonly SKColor EmeraldAccent = new(16, 185, 129); // #10B981
    private static readonly SKColor EmeraldLight = new(52, 211, 153);  // #34D399
    private static readonly SKColor GoldAccent = new(251, 191, 36);   // #FBBF24
    private static readonly SKColor AppleRed = new(239, 68, 68);      // #EF4444

    [Fact]
    public void GenerateAllMicrosoftStoreAssets()
    {
        string baseDir = Path.Combine("..", "..", "..", "..", "assets", "store");
        string msDir = Path.Combine(baseDir, "microsoft");
        Directory.CreateDirectory(msDir);

        // 1. Screenshots (1920x1080 and 3840x2160)
        GenerateScreenshotGameplay(Path.Combine(msDir, "screenshot_1_gameplay_1920x1080.png"), 1920, 1080);
        GenerateScreenshotAction(Path.Combine(msDir, "screenshot_2_action_1920x1080.png"), 1920, 1080);
        GenerateScreenshotMenu(Path.Combine(msDir, "screenshot_3_menu_1920x1080.png"), 1920, 1080);
        GenerateScreenshotFarmland(Path.Combine(msDir, "screenshot_4_farmland_1920x1080.png"), 1920, 1080);

        // 2. 16:9 Super Hero Art (No title text) - 1920x1080 & 3840x2160
        GenerateSuperHeroArt(Path.Combine(msDir, "super_hero_art_1920x1080.png"), 1920, 1080, withTitle: false);
        GenerateSuperHeroArt(Path.Combine(msDir, "super_hero_art_3840x2160.png"), 3840, 2160, withTitle: false);

        // 3. 16:9 Titled Hero Art (Xbox & Store) - 1920x1080 (Title in top 3/4)
        GenerateTitledHeroArt(Path.Combine(msDir, "titled_hero_art_1920x1080.png"), 1920, 1080);

        // 4. 1:1 Box Art - 1080x1080 & 2160x2160
        GenerateBoxArt(Path.Combine(msDir, "box_art_1080x1080.png"), 1080, 1080, withTitle: true);
        GenerateBoxArt(Path.Combine(msDir, "box_art_2160x2160.png"), 2160, 2160, withTitle: true);

        // 5. Featured Promotional Square Art (1080x1080 - No title text)
        GenerateBoxArt(Path.Combine(msDir, "featured_promo_square_1080x1080.png"), 1080, 1080, withTitle: false);

        // 6. 9:16 Poster Art (1080x1920 & 2160x3840 true 9:16, plus 720x1080 & 1440x2160)
        GeneratePosterArt(Path.Combine(msDir, "poster_art_9_16_1080x1920.png"), 1080, 1920);
        GeneratePosterArt(Path.Combine(msDir, "poster_art_9_16_2160x3840.png"), 2160, 3840);
        GeneratePosterArt(Path.Combine(msDir, "poster_art_720x1080.png"), 720, 1080);
        GeneratePosterArt(Path.Combine(msDir, "poster_art_1440x2160.png"), 1440, 2160);

        // 7. Branded Key Art (Xbox - 584x800 - Title in top 3/4)
        GenerateBrandedKeyArt(Path.Combine(msDir, "branded_key_art_584x800.png"), 584, 800);

        // 8. App Tile Icon (300x300)
        GenerateStoreIcon(Path.Combine(msDir, "app_tile_icon_300x300.png"), 300, 300);

        // 9. Store Display Icon Medium (150x150)
        GenerateStoreIcon(Path.Combine(msDir, "store_display_icon_150x150.png"), 150, 150);

        // 10. Store Display Icon Small (71x71)
        GenerateStoreIcon(Path.Combine(msDir, "store_display_icon_71x71.png"), 71, 71);

        // Dedicated 'optional_images' folder for direct drag-and-drop into Partner Center
        string optDir = Path.Combine(msDir, "optional_images");
        Directory.CreateDirectory(optDir);
        File.Copy(Path.Combine(msDir, "poster_art_9_16_1080x1920.png"), Path.Combine(optDir, "1_9x16_Poster_Art_1080x1920.png"), true);
        File.Copy(Path.Combine(msDir, "poster_art_720x1080.png"), Path.Combine(optDir, "1_9x16_Poster_Art_720x1080.png"), true);
        File.Copy(Path.Combine(msDir, "box_art_1080x1080.png"), Path.Combine(optDir, "2_1x1_Box_Art_1080x1080.png"), true);
        File.Copy(Path.Combine(msDir, "box_art_2160x2160.png"), Path.Combine(optDir, "2_1x1_Box_Art_2160x2160.png"), true);
        File.Copy(Path.Combine(msDir, "super_hero_art_1920x1080.png"), Path.Combine(optDir, "3_16x9_Super_Hero_Art_1920x1080.png"), true);
        File.Copy(Path.Combine(msDir, "titled_hero_art_1920x1080.png"), Path.Combine(optDir, "4_16x9_Titled_Hero_Art_1920x1080.png"), true);
        File.Copy(Path.Combine(msDir, "featured_promo_square_1080x1080.png"), Path.Combine(optDir, "5_Featured_Promo_Square_1080x1080.png"), true);

        // Also update standard store root files for backward compatibility
        File.Copy(Path.Combine(msDir, "screenshot_1_gameplay_1920x1080.png"), Path.Combine(baseDir, "real_screenshot_1_gameplay.png"), true);
        File.Copy(Path.Combine(msDir, "screenshot_2_action_1920x1080.png"), Path.Combine(baseDir, "real_screenshot_2_action.png"), true);
        File.Copy(Path.Combine(msDir, "screenshot_3_menu_1920x1080.png"), Path.Combine(baseDir, "real_screenshot_3_menu.png"), true);
        File.Copy(Path.Combine(msDir, "titled_hero_art_1920x1080.png"), Path.Combine(baseDir, "store_hero_banner.png"), true);

        // Assert all files exist
        Assert.True(File.Exists(Path.Combine(msDir, "screenshot_1_gameplay_1920x1080.png")));
        Assert.True(File.Exists(Path.Combine(msDir, "super_hero_art_1920x1080.png")));
        Assert.True(File.Exists(Path.Combine(msDir, "titled_hero_art_1920x1080.png")));
        Assert.True(File.Exists(Path.Combine(msDir, "box_art_1080x1080.png")));
        Assert.True(File.Exists(Path.Combine(msDir, "featured_promo_square_1080x1080.png")));
        Assert.True(File.Exists(Path.Combine(msDir, "poster_art_720x1080.png")));
        Assert.True(File.Exists(Path.Combine(msDir, "branded_key_art_584x800.png")));
        Assert.True(File.Exists(Path.Combine(msDir, "app_tile_icon_300x300.png")));
        Assert.True(File.Exists(Path.Combine(msDir, "store_display_icon_150x150.png")));
        Assert.True(File.Exists(Path.Combine(msDir, "store_display_icon_71x71.png")));
    }

    private static void GenerateScreenshotGameplay(string path, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        var engine = new GameEngine(24, 24);
        var renderer = new GameRenderer3D();

        engine.StartGame();
        engine.Score = 240;
        engine.HighScore = 580;

        for (int i = 0; i < 9; i++)
        {
            engine.Snake.Step(grow: true);
        }

        renderer.Render(canvas, width, height, engine, 0.033f);
        DrawHUDOverlay(canvas, width, height, "SCORE: 240", "BEST: 580", "SPEED: NORMAL");
        SaveBitmap(bitmap, path);
    }

    private static void GenerateScreenshotAction(string path, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        var engine = new GameEngine(24, 24);
        var renderer = new GameRenderer3D();

        engine.StartGame();
        engine.Score = 460;
        engine.HighScore = 580;

        for (int i = 0; i < 14; i++)
        {
            engine.Snake.Step(grow: true);
        }

        if (engine.SpecialFood != null)
        {
            renderer.OnFoodEaten(engine.SpecialFood, engine);
        }

        renderer.Render(canvas, width, height, engine, 0.033f);
        DrawHUDOverlay(canvas, width, height, "SCORE: 460", "BEST: 580", "GOLDEN BONUS! +30");
        SaveBitmap(bitmap, path);
    }

    private static void GenerateScreenshotMenu(string path, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        var engine = new GameEngine(24, 24);
        var renderer = new GameRenderer3D();

        engine.HighScore = 580;
        renderer.Render(canvas, width, height, engine, 0.033f);
        DrawMenuOverlay(canvas, width, height);
        SaveBitmap(bitmap, path);
    }

    private static void GenerateScreenshotFarmland(string path, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        var engine = new GameEngine(24, 24);
        var renderer = new GameRenderer3D();

        engine.StartGame();
        engine.Score = 310;
        engine.HighScore = 580;

        for (int i = 0; i < 11; i++)
        {
            engine.Snake.Step(grow: true);
        }

        renderer.Render(canvas, width, height, engine, 0.050f);
        DrawHUDOverlay(canvas, width, height, "SCORE: 310", "BEST: 580", "3D NATURE ARENA");
        SaveBitmap(bitmap, path);
    }

    private static void GenerateSuperHeroArt(string path, int width, int height, bool withTitle)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        var engine = new GameEngine(24, 24);
        var renderer = new GameRenderer3D();

        engine.StartGame();
        for (int i = 0; i < 12; i++)
        {
            engine.Snake.Step(grow: true);
        }

        renderer.Render(canvas, width, height, engine, 0.033f);

        // Apply cinematic vignette and lighting
        using var vignettePaint = new SKPaint
        {
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(width * 0.5f, height * 0.5f),
                width * 0.65f,
                [SKColors.Transparent, new SKColor(2, 6, 23, 200)],
                [0.4f, 1.0f],
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, width, height, vignettePaint);

        if (withTitle)
        {
            DrawTitleHeader(canvas, width, height, 0.28f);
        }

        SaveBitmap(bitmap, path);
    }

    private static void GenerateTitledHeroArt(string path, int width, int height)
    {
        GenerateSuperHeroArt(path, width, height, withTitle: true);
    }

    private static void GenerateBoxArt(string path, int width, int height, bool withTitle)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        var engine = new GameEngine(20, 20);
        var renderer = new GameRenderer3D();

        engine.StartGame();
        for (int i = 0; i < 10; i++)
        {
            engine.Snake.Step(grow: true);
        }

        renderer.Render(canvas, width, height, engine, 0.033f);

        // Box art border & gradient
        using var vignettePaint = new SKPaint
        {
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(width * 0.5f, height * 0.5f),
                width * 0.68f,
                [SKColors.Transparent, new SKColor(2, 6, 23, 220)],
                [0.35f, 1.0f],
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, width, height, vignettePaint);

        if (withTitle)
        {
            DrawTitleHeader(canvas, width, height, 0.22f);
            DrawBottomPill(canvas, width, height, "CLASSIC 3D REIMAGINED");
        }

        SaveBitmap(bitmap, path);
    }

    private static void GeneratePosterArt(string path, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        var engine = new GameEngine(20, 20);
        var renderer = new GameRenderer3D();

        engine.StartGame();
        for (int i = 0; i < 10; i++)
        {
            engine.Snake.Step(grow: true);
        }

        renderer.Render(canvas, width, height, engine, 0.033f);

        // Vertical dramatic gradient
        using var gradPaint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, height),
                [new SKColor(2, 6, 23, 190), SKColors.Transparent, new SKColor(2, 6, 23, 230)],
                [0.0f, 0.45f, 1.0f],
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, width, height, gradPaint);

        DrawTitleHeader(canvas, width, height, 0.18f);
        DrawBottomPill(canvas, width, height, "HIGH-OCTANE 3D SLITHER");

        SaveBitmap(bitmap, path);
    }

    private static void GenerateBrandedKeyArt(string path, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);
        var engine = new GameEngine(20, 20);
        var renderer = new GameRenderer3D();

        engine.StartGame();
        for (int i = 0; i < 8; i++)
        {
            engine.Snake.Step(grow: true);
        }

        renderer.Render(canvas, width, height, engine, 0.033f);

        using var gradPaint = new SKPaint
        {
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(0, height),
                [new SKColor(2, 6, 23, 180), SKColors.Transparent, new SKColor(2, 6, 23, 220)],
                [0.0f, 0.4f, 1.0f],
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRect(0, 0, width, height, gradPaint);

        // Branded title in top 3/4
        DrawTitleHeader(canvas, width, height, 0.22f);
        DrawBottomPill(canvas, width, height, "XBOX & WINDOWS 11");

        SaveBitmap(bitmap, path);
    }

    private static void GenerateStoreIcon(string path, int width, int height)
    {
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);

        float cornerRadius = width * 0.22f;

        // Squircle background
        using var bgPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, 0),
                new SKPoint(width, height),
                [new SKColor(15, 23, 42), new SKColor(2, 6, 23)],
                null,
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRoundRect(new SKRect(0, 0, width, height), cornerRadius, cornerRadius, bgPaint);

        // Emerald Outer Ring
        using var ringPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = Math.Max(2f, width * 0.025f),
            Color = new SKColor(16, 185, 129, 120)
        };
        canvas.DrawRoundRect(new SKRect(ringPaint.StrokeWidth, ringPaint.StrokeWidth, width - ringPaint.StrokeWidth, height - ringPaint.StrokeWidth), cornerRadius, cornerRadius, ringPaint);

        // Ambient radial glow
        using var glowPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(width * 0.5f, height * 0.45f),
                width * 0.45f,
                [new SKColor(16, 185, 129, 90), SKColors.Transparent],
                null,
                SKShaderTileMode.Clamp)
        };
        canvas.DrawCircle(width * 0.5f, height * 0.45f, width * 0.45f, glowPaint);

        // Apple
        float appleCx = width * 0.52f;
        float appleCy = height * 0.38f;
        float appleR = width * 0.20f;

        using var applePaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateRadialGradient(
                new SKPoint(appleCx - (appleR * 0.3f), appleCy - (appleR * 0.3f)),
                appleR * 1.2f,
                [new SKColor(248, 113, 113), new SKColor(220, 38, 38), new SKColor(153, 27, 27)],
                [0f, 0.6f, 1f],
                SKShaderTileMode.Clamp)
        };
        canvas.DrawCircle(appleCx, appleCy, appleR, applePaint);

        // Apple leaf & stem
        using var stemPaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = Math.Max(2f, width * 0.03f),
            StrokeCap = SKStrokeCap.Round,
            Color = new SKColor(120, 53, 15)
        };
        canvas.DrawLine(appleCx, appleCy - appleR + 2, appleCx + (appleR * 0.2f), appleCy - appleR - (appleR * 0.35f), stemPaint);

        using var leafPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(34, 197, 94)
        };
        using var leafPath = new SKPath();
        leafPath.MoveTo(appleCx + (appleR * 0.1f), appleCy - appleR - (appleR * 0.15f));
        leafPath.QuadTo(appleCx + (appleR * 0.6f), appleCy - appleR - (appleR * 0.45f), appleCx + (appleR * 0.8f), appleCy - appleR - (appleR * 0.1f));
        leafPath.QuadTo(appleCx + (appleR * 0.4f), appleCy - appleR, appleCx + (appleR * 0.1f), appleCy - appleR - (appleR * 0.15f));
        leafPath.Close();
        canvas.DrawPath(leafPath, leafPaint);

        // Serpentine S-Curve
        using var snakePaint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = Math.Max(4f, width * 0.14f),
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, height * 0.2f),
                new SKPoint(width, height * 0.8f),
                [new SKColor(52, 211, 153), new SKColor(16, 185, 129), new SKColor(5, 150, 105)],
                null,
                SKShaderTileMode.Clamp)
        };

        using var snakePath = new SKPath();
        snakePath.MoveTo(width * 0.78f, height * 0.25f);
        snakePath.CubicTo(width * 0.45f, height * 0.15f, width * 0.22f, height * 0.38f, width * 0.40f, height * 0.58f);
        snakePath.CubicTo(width * 0.62f, height * 0.78f, width * 0.70f, height * 0.85f, width * 0.35f, height * 0.82f);
        canvas.DrawPath(snakePath, snakePaint);

        // Snake Eye
        using var eyePaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(251, 191, 36)
        };
        canvas.DrawCircle(width * 0.74f, height * 0.28f, Math.Max(1.5f, width * 0.022f), eyePaint);

        SaveBitmap(bitmap, path);
    }

    private static void DrawTitleHeader(SKCanvas canvas, int width, int height, float yRatio)
    {
        float centerY = height * yRatio;
        float titleSize = Math.Clamp(width * 0.052f, 28f, 100f);
        float subSize = titleSize * 0.36f;

        // Frosted Pill Container
        float pillW = width * 0.72f;
        float pillH = titleSize * 2.2f;
        float pillX = (width - pillW) * 0.5f;
        float pillY = centerY - (pillH * 0.5f);

        using var pillBg = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(15, 23, 42, 210)
        };
        using var pillBorder = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2.5f,
            Color = new SKColor(16, 185, 129, 160)
        };

        canvas.DrawRoundRect(new SKRect(pillX, pillY, pillX + pillW, pillY + pillH), pillH * 0.3f, pillH * 0.3f, pillBg);
        canvas.DrawRoundRect(new SKRect(pillX, pillY, pillX + pillW, pillY + pillH), pillH * 0.3f, pillH * 0.3f, pillBorder);

        // Title text with shadow
        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            TextSize = titleSize,
            FakeBoldText = true,
            TextAlign = SKTextAlign.Center,
            Color = new SKColor(0, 0, 0, 200)
        };
        canvas.DrawText("SNAKE 3D : NEW ERA", width * 0.5f + 2f, centerY + (titleSize * 0.15f) + 2f, shadowPaint);

        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            TextSize = titleSize,
            FakeBoldText = true,
            TextAlign = SKTextAlign.Center,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(0, centerY - (titleSize * 0.5f)),
                new SKPoint(0, centerY + (titleSize * 0.5f)),
                [new SKColor(248, 250, 252), new SKColor(203, 213, 225)],
                null,
                SKShaderTileMode.Clamp)
        };
        canvas.DrawText("SNAKE 3D : NEW ERA", width * 0.5f, centerY + (titleSize * 0.15f), textPaint);

        // Subtitle
        using var subPaint = new SKPaint
        {
            IsAntialias = true,
            TextSize = subSize,
            FakeBoldText = true,
            TextAlign = SKTextAlign.Center,
            Color = new SKColor(52, 211, 153)
        };
        canvas.DrawText("THE MODERN 3D SLITHER ARCADE", width * 0.5f, centerY + (titleSize * 0.65f), subPaint);
    }

    private static void DrawBottomPill(SKCanvas canvas, int width, int height, string text)
    {
        float fontSize = Math.Clamp(width * 0.032f, 18f, 54f);
        float pillW = width * 0.60f;
        float pillH = fontSize * 2.0f;
        float pillX = (width - pillW) * 0.5f;
        float pillY = height * 0.88f - (pillH * 0.5f);

        using var pillBg = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(15, 23, 42, 220)
        };
        using var pillBorder = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeWidth = 2f,
            Color = new SKColor(251, 191, 36, 180)
        };

        canvas.DrawRoundRect(new SKRect(pillX, pillY, pillX + pillW, pillY + pillH), pillH * 0.5f, pillH * 0.5f, pillBg);
        canvas.DrawRoundRect(new SKRect(pillX, pillY, pillX + pillW, pillY + pillH), pillH * 0.5f, pillH * 0.5f, pillBorder);

        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            TextSize = fontSize,
            FakeBoldText = true,
            TextAlign = SKTextAlign.Center,
            Color = new SKColor(251, 191, 36)
        };
        canvas.DrawText(text, width * 0.5f, pillY + (pillH * 0.62f), textPaint);
    }

    private static void DrawHUDOverlay(SKCanvas canvas, int width, int height, string leftText, string rightText, string badgeText)
    {
        float textSize = Math.Clamp(width * 0.024f, 16f, 44f);
        float pad = width * 0.035f;

        // Top Left Score Pill
        DrawBadge(canvas, pad, pad, leftText, textSize, EmeraldLight);

        // Top Right Best Pill
        DrawBadge(canvas, width - pad - (textSize * 7.5f), pad, rightText, textSize, GoldAccent);

        // Bottom Center Status Pill
        float bottomW = textSize * 12f;
        DrawBadge(canvas, (width - bottomW) * 0.5f, height - pad - (textSize * 2f), badgeText, textSize, new SKColor(248, 250, 252));
    }

    private static void DrawMenuOverlay(SKCanvas canvas, int width, int height)
    {
        float titleSize = Math.Clamp(width * 0.055f, 32f, 110f);

        // Title
        DrawTitleHeader(canvas, width, height, 0.28f);

        // Start Button Pill
        float btnW = width * 0.32f;
        float btnH = titleSize * 1.4f;
        float btnX = (width - btnW) * 0.5f;
        float btnY = height * 0.58f;

        using var btnPaint = new SKPaint
        {
            IsAntialias = true,
            Shader = SKShader.CreateLinearGradient(
                new SKPoint(btnX, btnY),
                new SKPoint(btnX + btnW, btnY + btnH),
                [new SKColor(16, 185, 129), new SKColor(5, 150, 105)],
                null,
                SKShaderTileMode.Clamp)
        };
        canvas.DrawRoundRect(new SKRect(btnX, btnY, btnX + btnW, btnY + btnH), btnH * 0.5f, btnH * 0.5f, btnPaint);

        using var btnText = new SKPaint
        {
            IsAntialias = true,
            TextSize = titleSize * 0.45f,
            FakeBoldText = true,
            TextAlign = SKTextAlign.Center,
            Color = SKColors.White
        };
        canvas.DrawText("▶ PLAY NOW", width * 0.5f, btnY + (btnH * 0.62f), btnText);

        // Speed Mode Selector row
        float rowY = height * 0.74f;
        float modeW = width * 0.16f;
        float modeH = titleSize * 0.8f;
        float gap = width * 0.02f;
        float startX = (width - (modeW * 3 + gap * 2)) * 0.5f;

        string[] modes = ["🐢 RELAXED", "🎯 NORMAL", "⚡ FAST"];
        for (int i = 0; i < 3; i++)
        {
            float mx = startX + (i * (modeW + gap));
            bool isSelected = i == 1;

            using var modeBg = new SKPaint
            {
                IsAntialias = true,
                Color = isSelected ? new SKColor(16, 185, 129, 230) : new SKColor(15, 23, 42, 180)
            };
            canvas.DrawRoundRect(new SKRect(mx, rowY, mx + modeW, rowY + modeH), modeH * 0.4f, modeH * 0.4f, modeBg);

            using var modeText = new SKPaint
            {
                IsAntialias = true,
                TextSize = titleSize * 0.28f,
                FakeBoldText = true,
                TextAlign = SKTextAlign.Center,
                Color = isSelected ? SKColors.White : new SKColor(148, 163, 184)
            };
            canvas.DrawText(modes[i], mx + (modeW * 0.5f), rowY + (modeH * 0.62f), modeText);
        }
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
            Color = new SKColor(textColor.Red, textColor.Green, textColor.Blue, 140)
        };

        canvas.DrawRoundRect(new SKRect(x, y, x + badgeW, y + badgeH), badgeH * 0.35f, badgeH * 0.35f, bgPaint);
        canvas.DrawRoundRect(new SKRect(x, y, x + badgeW, y + badgeH), badgeH * 0.35f, badgeH * 0.35f, borderPaint);

        using var textPaint = new SKPaint
        {
            IsAntialias = true,
            TextSize = textSize,
            FakeBoldText = true,
            Color = textColor
        };
        canvas.DrawText(text, x + (textSize * 0.9f), y + (badgeH * 0.64f), textPaint);
    }

    private static void SaveBitmap(SKBitmap bitmap, string path)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, 100);
        File.WriteAllBytes(path, data.ToArray());
    }
}
