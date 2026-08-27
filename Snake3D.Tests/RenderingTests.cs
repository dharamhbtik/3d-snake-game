using SkiaSharp;
using Snake3D.Core;
using Snake3D.Rendering;
using Xunit;

namespace Snake3D.Tests;

public class RenderingTests
{
    [Fact]
    public void GameRenderer3D_RendersWithoutCrashing()
    {
        var renderer = new GameRenderer3D();
        var engine = new GameEngine(boardWidth: 20, boardHeight: 20);
        engine.StartGame();

        using var bitmap = new SKBitmap(800, 600, SKColorType.Rgba8888, SKAlphaType.Premul);
        using var canvas = new SKCanvas(bitmap);

        // Step snake and render
        engine.Snake.Step(grow: true);
        renderer.Render(canvas, 800, 600, engine, 0.016f);

        Assert.True(bitmap.Width == 800);
        Assert.True(bitmap.Height == 600);
    }

    [Fact]
    public void GameRenderer3D_GeneratesStoreScreenshots()
    {
        string outputDir = Path.Combine("..", "..", "..", "..", "assets", "store");
        GameRenderer3D.GenerateStoreScreenshots(outputDir);

        Assert.True(File.Exists(Path.Combine(outputDir, "real_screenshot_1_gameplay.png")));
        Assert.True(File.Exists(Path.Combine(outputDir, "real_screenshot_2_action.png")));
        Assert.True(File.Exists(Path.Combine(outputDir, "real_screenshot_3_menu.png")));
    }
}
