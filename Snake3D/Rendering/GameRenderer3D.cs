using System.Numerics;
using SkiaSharp;
using Snake3D.Core;

namespace Snake3D.Rendering;

/// <summary>
/// High-fidelity realistic 3D rendering pipeline for the 3D Snake game arena, organic serpentine body, lighting, and natural environment.
/// </summary>
public sealed class GameRenderer3D
{
    private readonly Camera3D _camera = new();
    private readonly Lighting3D _lighting = new();
    private readonly ParticleSystem3D _particles = new();
    private readonly List<Polygon3D> _polygons = new(3072);

    private float _totalGameTime;
    private float _shakeIntensity;
    private float _tongueTimer;
    private float _digestionPulse = -1f;

    // Ambient floating fireflies / pollen spores
    private readonly Vector3[] _ambientDust = new Vector3[24];

    // Colors
    private static readonly SKColor BgSkyTop = new(12, 22, 38);
    private static readonly SKColor BgSkyBottom = new(3, 8, 16);
    private static readonly SKColor GrassDark = new(20, 83, 45); // Deep Forest Green
    private static readonly SKColor GrassLight = new(34, 120, 68); // Meadow Green
    private static readonly SKColor GrassTuftLight = new(74, 222, 128); // Vibrant blade highlight
    private static readonly SKColor StoneWallTop = new(71, 85, 105); // Slate stone
    private static readonly SKColor StoneWallSide = new(51, 65, 85);
    private static readonly SKColor StoneWallTrim = new(34, 197, 94); // Moss accents

    // Snake Skin Colors
    private static readonly SKColor SnakeDorsal = new(21, 128, 61); // Rich forest green spine
    private static readonly SKColor SnakeFlank = new(34, 197, 94); // Emerald scale flank
    private static readonly SKColor SnakeDorsalDiamond = new(132, 204, 22); // Golden-lime diamond pattern
    private static readonly SKColor SnakeBelly = new(254, 240, 138); // Pale creamy ventral scale

    public ParticleSystem3D Particles => _particles;

    public GameRenderer3D()
    {
        for (int i = 0; i < _ambientDust.Length; i++)
        {
            _ambientDust[i] = new Vector3(
                (Random.Shared.NextSingle() - 0.5f) * 22f,
                0.5f + (Random.Shared.NextSingle() * 4.5f),
                (Random.Shared.NextSingle() - 0.5f) * 22f);
        }
    }

    public void TriggerShake(float intensity = 1.0f)
    {
        _shakeIntensity = intensity;
    }

    public void OnFoodEaten(Food food, GameEngine engine)
    {
        _digestionPulse = 0f; // Start digestion lump traveling down the spine
        var worldPos = GridToWorld(food.Position, engine.Board);
        var burstColor = food.Type == FoodType.GoldenApple ? new SKColor(255, 215, 0) : new SKColor(239, 68, 68);
        _particles.SpawnBurst(worldPos + new Vector3(0, 0.4f, 0), burstColor, count: food.Type == FoodType.GoldenApple ? 45 : 30);
    }

    public void OnGameOver(GameEngine engine)
    {
        TriggerShake(1.5f);
        var headWorld = GridToWorld(engine.Snake.Head, engine.Board);
        _particles.SpawnBurst(headWorld + new Vector3(0, 0.5f, 0), new SKColor(239, 68, 68), count: 50, speed: 6.0f);
    }

    public void Render(SKCanvas canvas, int width, int height, GameEngine engine, float deltaSeconds)
    {
        _totalGameTime += deltaSeconds;
        _tongueTimer += deltaSeconds * 3.0f;

        if (_digestionPulse >= 0f)
        {
            _digestionPulse += deltaSeconds * 5.0f;
            if (_digestionPulse > engine.Snake.Length)
                _digestionPulse = -1f;
        }

        if (_shakeIntensity > 0.01f)
        {
            _shakeIntensity *= MathF.Pow(0.05f, deltaSeconds);
        }
        else
        {
            _shakeIntensity = 0f;
        }

        _particles.Update(deltaSeconds);

        // 1. Draw atmospheric sky gradient
        DrawBackground(canvas, width, height);

        // 2. Setup 3D Camera
        UpdateCamera(width, height, engine);

        // 3. Build 3D world geometry
        _polygons.Clear();
        BuildBoardGeometry(engine.Board);
        BuildGrassTufts(engine.Board);
        BuildFoodGeometry(engine);
        BuildRealisticSnakeGeometry(engine);

        // 4. Calculate depths & Sort Polygons (Painter's Algorithm)
        var camPos = _camera.Position;
        for (int i = 0; i < _polygons.Count; i++)
        {
            var poly = _polygons[i];
            poly.Depth = Vector3.DistanceSquared(camPos, poly.CalculateCenter());
        }

        _polygons.Sort((a, b) => b.Depth.CompareTo(a.Depth));

        // 5. Draw floor drop shadows
        DrawFloorShadows(canvas, engine);

        // 6. Rasterize 3D Polygons
        using var fillPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };
        using var strokePaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Stroke };

        foreach (var poly in _polygons)
        {
            // Backface culling
            var center = poly.CalculateCenter();
            var toCam = camPos - center;
            if (Vector3.Dot(poly.Normal, toCam) <= 0.0001f && !poly.IsEmissive)
            {
                continue;
            }

            using var path = new SKPath();
            bool allVisible = true;

            for (int vi = 0; vi < poly.Vertices.Length; vi++)
            {
                if (_camera.WorldToScreen(poly.Vertices[vi], out var pt, out _))
                {
                    if (vi == 0)
                        path.MoveTo(pt);
                    else
                        path.LineTo(pt);
                }
                else
                {
                    allVisible = false;
                    break;
                }
            }

            if (!allVisible)
                continue;

            path.Close();

            // Blinn-Phong lighting calculation
            var shadedColor = _lighting.CalculateShading(poly, camPos);
            fillPaint.Color = shadedColor;
            canvas.DrawPath(path, fillPaint);

            if (poly.StrokeColor.HasValue)
            {
                strokePaint.Color = poly.StrokeColor.Value;
                strokePaint.StrokeWidth = poly.StrokeWidth;
                canvas.DrawPath(path, strokePaint);
            }
        }

        // 7. Render 3D Ambient Dust / Fireflies
        DrawAmbientParticles(canvas);

        // 8. Render 3D Particle effects
        _particles.Render(canvas, _camera);
    }

    private void UpdateCamera(int width, int height, GameEngine engine)
    {
        float aspect = (float)width / Math.Max(1, height);

        if (engine.State == GameState.Menu)
        {
            // Orbiting cinematic camera showcase
            float orbitAngle = _totalGameTime * 0.22f;
            float orbitRadius = 26f;
            _camera.Position = new Vector3(
                MathF.Sin(orbitAngle) * orbitRadius,
                20f,
                -MathF.Cos(orbitAngle) * orbitRadius
            );
            _camera.Target = new Vector3(0, 0, 0);
        }
        else
        {
            // Elevated perspective camera giving generous visual clearance for the bottom area
            float boardSpan = MathF.Max(engine.Board.Width, engine.Board.Height);
            float dist = boardSpan * (aspect < 1.0f ? 1.08f : 0.88f);

            float shakeX = _shakeIntensity * (Random.Shared.NextSingle() - 0.5f) * 1.2f;
            float shakeY = _shakeIntensity * (Random.Shared.NextSingle() - 0.5f) * 1.2f;

            _camera.Position = new Vector3(shakeX, (dist * 1.18f) + shakeY, -dist * 1.02f);
            _camera.Target = new Vector3(0, 0, -1.0f);
        }

        _camera.FieldOfViewDegrees = aspect < 1.0f ? 50f : 40f;
        _camera.UpdateMatrices(width, height);
    }

    private void DrawBackground(SKCanvas canvas, int width, int height)
    {
        using var bgPaint = new SKPaint();
        using var shader = SKShader.CreateRadialGradient(
            new SKPoint(width * 0.5f, height * 0.42f),
            MathF.Max(width, height) * 0.85f,
            [BgSkyTop, BgSkyBottom],
            [0.0f, 1.0f],
            SKShaderTileMode.Clamp);

        bgPaint.Shader = shader;
        canvas.DrawRect(0, 0, width, height, bgPaint);
    }

    private void DrawFloorShadows(SKCanvas canvas, GameEngine engine)
    {
        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(0, 0, 0, 95),
            Style = SKPaintStyle.Fill
        };

        // Shadow for food
        if (engine.CurrentFood != null)
        {
            var foodPos = GridToWorld(engine.CurrentFood.Position, engine.Board);
            if (_camera.WorldToScreen(new Vector3(foodPos.X, 0.02f, foodPos.Z), out var screenPos, out float depth))
            {
                float radius = Math.Max(2f, (16f * _camera.ViewportHeight * 0.035f) / depth);
                canvas.DrawOval(screenPos.X, screenPos.Y, radius * 1.3f, radius * 0.85f, shadowPaint);
            }
        }

        if (engine.SpecialFood != null)
        {
            var foodPos = GridToWorld(engine.SpecialFood.Position, engine.Board);
            if (_camera.WorldToScreen(new Vector3(foodPos.X, 0.02f, foodPos.Z), out var screenPos, out float depth))
            {
                float radius = Math.Max(2f, (18f * _camera.ViewportHeight * 0.035f) / depth);
                canvas.DrawOval(screenPos.X, screenPos.Y, radius * 1.4f, radius * 0.9f, shadowPaint);
            }
        }
    }

    private void DrawAmbientParticles(SKCanvas canvas)
    {
        using var dustPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };

        for (int i = 0; i < _ambientDust.Length; i++)
        {
            float t = _totalGameTime + (i * 1.7f);
            var pos = _ambientDust[i] + new Vector3(
                MathF.Sin(t * 0.8f) * 0.4f,
                MathF.Sin(t * 1.2f) * 0.3f,
                MathF.Cos(t * 0.6f) * 0.4f);

            if (_camera.WorldToScreen(pos, out var pt, out float depth))
            {
                float alphaNorm = 0.4f + (MathF.Sin(t * 2.5f) * 0.3f);
                dustPaint.Color = new SKColor(254, 240, 138, (byte)(alphaNorm * 255));
                float r = Math.Max(1.2f, (2.5f * _camera.ViewportHeight * 0.02f) / depth);
                canvas.DrawCircle(pt, r, dustPaint);
            }
        }
    }

    private void BuildBoardGeometry(GameBoard board)
    {
        float halfW = board.Width * 0.5f;
        float halfH = board.Height * 0.5f;

        // Base Pedestal / Earth ground
        MeshBuilder.AddBox(
            _polygons,
            new Vector3(0, -0.65f, 0),
            new Vector3(board.Width + 1.6f, 1.3f, board.Height + 1.6f),
            new SKColor(30, 22, 14), // Dark rich soil
            new SKColor(20, 14, 8),
            new SKColor(45, 34, 22));

        // Checkerboard Grass Tiles
        for (int y = 0; y < board.Height; y++)
        {
            for (int x = 0; x < board.Width; x++)
            {
                var tileColor = (x + y) % 2 == 0 ? GrassDark : GrassLight;
                var center = new Vector3(halfW - x - 0.5f, 0f, (halfH - y - 0.5f));

                var p0 = center + new Vector3(-0.49f, 0, -0.49f);
                var p1 = center + new Vector3(0.49f, 0, -0.49f);
                var p2 = center + new Vector3(0.49f, 0, 0.49f);
                var p3 = center + new Vector3(-0.49f, 0, 0.49f);

                _polygons.Add(new Polygon3D([p0, p1, p2, p3], tileColor)
                {
                    SpecularIntensity = 0.14f
                });
            }
        }

        // Stone Perimeter Boundary Walls
        float wallHeight = 0.55f;
        float wallThickness = 0.5f;

        // North wall (+Z)
        MeshBuilder.AddBox(
            _polygons,
            new Vector3(0, wallHeight * 0.5f, halfH + (wallThickness * 0.5f)),
            new Vector3(board.Width + (wallThickness * 2f), wallHeight, wallThickness),
            StoneWallTop,
            StoneWallSide,
            StoneWallTrim);

        // South wall (-Z)
        MeshBuilder.AddBox(
            _polygons,
            new Vector3(0, wallHeight * 0.5f, -halfH - (wallThickness * 0.5f)),
            new Vector3(board.Width + (wallThickness * 2f), wallHeight, wallThickness),
            StoneWallTop,
            StoneWallSide,
            StoneWallTrim);

        // West wall (+X in camera space -> Right)
        MeshBuilder.AddBox(
            _polygons,
            new Vector3(-halfW - (wallThickness * 0.5f), wallHeight * 0.5f, 0),
            new Vector3(wallThickness, wallHeight, board.Height),
            StoneWallTop,
            StoneWallSide,
            StoneWallTrim);

        // East wall (-X in camera space -> Left)
        MeshBuilder.AddBox(
            _polygons,
            new Vector3(halfW + (wallThickness * 0.5f), wallHeight * 0.5f, 0),
            new Vector3(wallThickness, wallHeight, board.Height),
            StoneWallTop,
            StoneWallSide,
            StoneWallTrim);
    }

    private void BuildGrassTufts(GameBoard board)
    {
        float halfW = board.Width * 0.5f;
        float halfH = board.Height * 0.5f;
        float windSway = MathF.Sin(_totalGameTime * 3.2f);

        // 1. Perimeter grass along all 4 stone walls
        for (int i = 0; i < board.Width; i += 2)
        {
            float wx = halfW - i - 0.5f;
            var posN = new Vector3(wx, 0.0f, halfH - 0.35f);
            var posS = new Vector3(wx, 0.0f, -halfH + 0.35f);
            MeshBuilder.AddGrassTuft(_polygons, posN, 1.0f, windSway, GrassDark, GrassTuftLight, hasFlower: i % 4 == 0);
            MeshBuilder.AddGrassTuft(_polygons, posS, 1.0f, windSway, GrassDark, GrassTuftLight, hasFlower: (i + 2) % 4 == 0);
        }

        for (int i = 0; i < board.Height; i += 2)
        {
            float wz = halfH - i - 0.5f;
            var posW = new Vector3(-halfW + 0.35f, 0.0f, wz);
            var posE = new Vector3(halfW - 0.35f, 0.0f, wz);
            MeshBuilder.AddGrassTuft(_polygons, posW, 1.0f, windSway, GrassDark, GrassTuftLight, hasFlower: i % 4 == 0);
            MeshBuilder.AddGrassTuft(_polygons, posE, 1.0f, windSway, GrassDark, GrassTuftLight, hasFlower: (i + 2) % 4 == 0);
        }

        // 2. Dense field grass tufts across ground tiles
        for (int y = 1; y < board.Height - 1; y += 2)
        {
            for (int x = 1; x < board.Width - 1; x += 2)
            {
                float jitterX = (MathF.Sin((x * 13f) + (y * 7f)) * 0.28f);
                float jitterZ = (MathF.Cos((y * 11f) + (x * 5f)) * 0.28f);
                float gx = halfW - x - 0.5f + jitterX;
                float gz = halfH - y - 0.5f + jitterZ;
                var innerPos = new Vector3(gx, 0.0f, gz);
                bool hasFlower = (x * 3 + y * 7) % 7 == 0;
                float scale = 0.85f + (MathF.Abs(MathF.Sin(x + y)) * 0.25f);
                MeshBuilder.AddGrassTuft(_polygons, innerPos, scale, windSway, GrassDark, GrassTuftLight, hasFlower: hasFlower);
            }
        }
    }

    private void BuildFoodGeometry(GameEngine engine)
    {
        if (engine.CurrentFood != null)
        {
            var pos = GridToWorld(engine.CurrentFood.Position, engine.Board);
            float bobbing = 0.42f + (MathF.Sin(_totalGameTime * 4.5f) * 0.08f);
            pos.Y = bobbing;

            MeshBuilder.AddRealisticApple(
                _polygons,
                pos,
                scale: 1.08f,
                rotationY: _totalGameTime * 2.4f,
                isGolden: false);
        }

        if (engine.SpecialFood != null)
        {
            var pos = GridToWorld(engine.SpecialFood.Position, engine.Board);
            float bobbing = 0.50f + (MathF.Sin(_totalGameTime * 6f) * 0.12f);
            pos.Y = bobbing;

            MeshBuilder.AddRealisticApple(
                _polygons,
                pos,
                scale: 1.28f,
                rotationY: _totalGameTime * 3.6f,
                isGolden: true);
        }
    }

    private void BuildRealisticSnakeGeometry(GameEngine engine)
    {
        var segments = engine.Snake.Segments;
        if (segments.Count == 0)
            return;

        float subTick = (float)engine.SubTickProgress;
        var board = engine.Board;
        var headingDir = engine.Snake.CurrentDirection;

        // Calculate smooth spine nodes
        int count = segments.Count;
        var spine = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            var currWorld = GridToWorld(segments[i], board);
            Vector3 prevWorld;

            if (i == 0)
            {
                // Head interpolation from previous grid position
                var prevHeadGrid = segments.Count > 1 ? segments[1] : segments[0].Move(headingDir.Opposite());
                prevWorld = GridToWorld(prevHeadGrid, board);
            }
            else
            {
                // Segment i moves toward segment i-1
                prevWorld = GridToWorld(segments[i], board);
                currWorld = GridToWorld(segments[i - 1], board);
            }

            var pos = Vector3.Lerp(prevWorld, currWorld, engine.State == GameState.Playing ? subTick : 0.0f);

            // Natural lateral serpentine undulation wave through spine
            float wavePhase = (_totalGameTime * 14f) - (i * 0.65f);
            float waveAmp = (i == 0 ? 0.04f : 0.12f) * (engine.State == GameState.Playing ? 1.0f : 0.25f);
            float wave = MathF.Sin(wavePhase) * waveAmp;

            // Lateral perpendicular vector
            Vector3 forward = (i == 0) ? DirectionToVector(headingDir) : (i > 0 ? Vector3.Normalize(spine[i - 1] - pos + new Vector3(0.001f, 0, 0)) : Vector3.UnitZ);
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            pos += right * wave;
            pos.Y = 0.28f; // Height above grass
            spine[i] = pos;
        }

        // Head vectors
        Vector3 headForward = DirectionToVector(headingDir);
        Vector3 headRight = new Vector3(-headForward.Z, 0, headForward.X);

        // Build Sculpted Snake Head
        MeshBuilder.AddSculptedSnakeHead(
            _polygons,
            spine[0],
            headForward,
            headRight,
            MathF.Abs(MathF.Sin(_tongueTimer)),
            SnakeDorsal,
            SnakeFlank,
            SnakeBelly);

        // Build Continuous Tubular Body Mesh with 10-sided cross-sectional rings
        int ringSegments = 10;
        var rings = new Vector3[count][];

        for (int i = 0; i < count; i++)
        {
            rings[i] = new Vector3[ringSegments];

            // Taper profile: starts wide behind head, thickens slightly at midbody, tapers smoothly to tail tip
            float norm = (float)i / Math.Max(1, count - 1);
            float radiusX = 0.38f * (1.0f - (norm * 0.72f));
            float radiusY = 0.28f * (1.0f - (norm * 0.70f));

            // Food digestion lump travelling down spine
            if (_digestionPulse >= 0f)
            {
                float distToPulse = MathF.Abs(i - _digestionPulse);
                if (distToPulse < 2.0f)
                {
                    float lumpFactor = 1.0f + (0.35f * MathF.Cos(distToPulse * MathF.PI * 0.5f));
                    radiusX *= lumpFactor;
                    radiusY *= lumpFactor;
                }
            }

            Vector3 fwd;
            if (i == 0)
                fwd = headForward;
            else if (i == count - 1)
                fwd = Vector3.Normalize(spine[i - 1] - spine[i]);
            else
                fwd = Vector3.Normalize(spine[i - 1] - spine[i + 1]);

            Vector3 rgt = new Vector3(-fwd.Z, 0, fwd.X);
            Vector3 up = Vector3.UnitY;

            for (int s = 0; s < ringSegments; s++)
            {
                float angle = s * MathF.PI * 2f / ringSegments;
                float ca = MathF.Cos(angle);
                float sa = MathF.Sin(angle);

                rings[i][s] = spine[i] + (rgt * ca * radiusX) + (up * sa * radiusY);
            }
        }

        // Connect consecutive rings with smooth reptilian skin quads
        for (int i = 0; i < count - 1; i++)
        {
            for (int s = 0; s < ringSegments; s++)
            {
                int next = (s + 1) % ringSegments;

                // Color selection based on dorsal, flank, or belly section of ring
                SKColor quadColor;
                if (s == 2 || s == 3) // Top spine
                {
                    quadColor = (i % 2 == 0) ? SnakeDorsalDiamond : SnakeDorsal;
                }
                else if (s >= 7 && s <= 8) // Bottom belly
                {
                    quadColor = SnakeBelly;
                }
                else // Flanks
                {
                    quadColor = SnakeFlank;
                }

                _polygons.Add(new Polygon3D(
                    [rings[i][s], rings[i][next], rings[i + 1][next], rings[i + 1][s]],
                    quadColor)
                {
                    SpecularIntensity = 0.55f,
                    Shininess = 24f
                });
            }
        }

        // Tail tip closure
        var tailTip = spine[^1] - ((spine.Length > 1 ? Vector3.Normalize(spine[^2] - spine[^1]) : headForward) * 0.18f);
        for (int s = 0; s < ringSegments; s++)
        {
            int next = (s + 1) % ringSegments;
            _polygons.Add(new Polygon3D([tailTip, rings[^1][next], rings[^1][s]], SnakeDorsal));
        }
    }

    private static Vector3 DirectionToVector(Direction dir) => dir switch
    {
        Direction.Up => new Vector3(0, 0, 1),
        Direction.Down => new Vector3(0, 0, -1),
        Direction.Left => new Vector3(1, 0, 0),
        Direction.Right => new Vector3(-1, 0, 0),
        _ => Vector3.UnitZ
    };

    public static Vector3 GridToWorld(GridPoint grid, GameBoard board)
    {
        float x = (board.Width * 0.5f) - grid.X - 0.5f;
        float z = (board.Height * 0.5f) - grid.Y - 0.5f;
        return new Vector3(x, 0f, z);
    }
}
