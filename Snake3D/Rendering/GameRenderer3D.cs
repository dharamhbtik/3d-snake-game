using System.Numerics;
using SkiaSharp;
using Snake3D.Core;

namespace Snake3D.Rendering;

#pragma warning disable CS0618 // SkiaSharp SKPath methods marked obsolete in preview builds

/// <summary>
/// High-fidelity realistic 3D rendering pipeline for the borderless agricultural crop field environment, dense pasture grass, organic serpentine anatomy, varied prey (frogs, ladybugs, grasshoppers, dragonflies), outdoor sunlight, and farmland atmosphere.
/// </summary>
public sealed class GameRenderer3D
{
    private readonly Camera3D _camera = new();
    private readonly Lighting3D _lighting = new();
    private readonly ParticleSystem3D _particles = new();
    private readonly List<Polygon3D> _polygons = new(10240);

    private float _totalGameTime;
    private float _shakeIntensity;
    private float _tongueTimer;
    private float _digestionPulse = -1f;

    // Atmospheric golden pollen motes / floating field spores
    private readonly Vector3[] _ambientPollen = new Vector3[36];

    // Farmland & Crop Color Palette
    private static readonly SKColor SkyZenith = new(65, 130, 205); // Vivid clear farm sky
    private static readonly SKColor SkyHorizon = new(210, 228, 245); // Golden haze horizon
    private static readonly SKColor FarFieldHorizon = new(135, 155, 75); // Distant pasture horizon

    // Rich Agricultural Loamy Soil Furrows
    private static readonly SKColor LoamTrench = new(38, 22, 12); // Deep rich moist furrow trench
    private static readonly SKColor LoamRidgeDark = new(70, 44, 24); // Tilled earth furrow ridge
    private static readonly SKColor LoamRidgeLight = new(92, 62, 36); // Sunlit soil crest
    private static readonly SKColor LoamSubtleGreen = new(65, 70, 28); // Soil with crop/moss tint

    // Real Snake Anatomical Scale Colors (Emerald Viper / Diamondback Serpent pattern)
    private static readonly SKColor SnakeDorsalDark = new(15, 64, 32); // Deep forest green / olive dorsal spine
    private static readonly SKColor SnakeDorsalBlack = new(8, 36, 18); // Dark saddle boundary
    private static readonly SKColor SnakeDorsalDiamond = new(163, 230, 53); // Golden-lime diamond scale crest
    private static readonly SKColor SnakeDorsalAccent = new(234, 179, 8); // Golden scale highlight
    private static readonly SKColor SnakeFlank = new(34, 197, 94); // Emerald scale flank
    private static readonly SKColor SnakeFlankDark = new(21, 128, 61); // Lateral scale shadow
    private static readonly SKColor SnakeBelly = new(254, 240, 138); // Pale creamy ventral belly scutes
    private static readonly SKColor SnakeBellyEdge = new(202, 138, 4); // Ventral scale junction
    private static readonly SKColor SnakeLipScale = new(253, 230, 138); // Pale supralabial lip scales

    public ParticleSystem3D Particles => _particles;

    public GameRenderer3D()
    {
        for (int i = 0; i < _ambientPollen.Length; i++)
        {
            _ambientPollen[i] = new Vector3(
                (Random.Shared.NextSingle() - 0.5f) * 44f,
                0.3f + (Random.Shared.NextSingle() * 5.5f),
                (Random.Shared.NextSingle() - 0.5f) * 44f);
        }
    }

    public void TriggerShake(float intensity = 1.0f)
    {
        _shakeIntensity = intensity;
    }

    public void OnFoodEaten(Food food, GameEngine engine)
    {
        _digestionPulse = 0f; // Start dynamic digestion lump traveling down the serpent
        var worldPos = GridToWorld(food.Position, engine.Board);
        var burstColor = food.Type switch
        {
            FoodType.Ladybug => new SKColor(239, 68, 68),
            FoodType.Grasshopper => new SKColor(132, 204, 22),
            FoodType.Dragonfly => new SKColor(6, 182, 212),
            FoodType.GoldenFrog => new SKColor(255, 215, 0),
            _ => new SKColor(52, 199, 89)
        };
        _particles.SpawnBurst(worldPos + new Vector3(0, 0.35f, 0), burstColor, count: food.IsSpecial ? 50 : 35);
    }

    public void OnGameOver(GameEngine engine)
    {
        TriggerShake(1.6f);
        var headWorld = GridToWorld(engine.Snake.Head, engine.Board);
        _particles.SpawnBurst(headWorld + new Vector3(0, 0.5f, 0), new SKColor(239, 68, 68), count: 55, speed: 6.5f);
    }

    public void Render(SKCanvas canvas, int width, int height, GameEngine engine, float deltaSeconds)
    {
        _totalGameTime += deltaSeconds;
        _tongueTimer += deltaSeconds * 3.4f;

        if (_digestionPulse >= 0f)
        {
            _digestionPulse += deltaSeconds * 5.5f;
            if (_digestionPulse > engine.Snake.Length + 4)
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

        // 1. Draw atmospheric farm sky & horizon
        DrawFarmlandSky(canvas, width, height);

        // 2. Setup 3D Perspective Camera framing the crop field border-to-border with no bottom cut-off
        UpdateCamera(width, height, engine);

        // 3. Build 3D Farmland Terrain, Dense Grass Carpet, Crop Rows, Prey & Realistic Snake Geometry
        _polygons.Clear();
        BuildBorderlessFarmland(engine.Board);
        BuildDenseGrassCarpetAndCrops(engine.Board);
        BuildFoodGeometry(engine);
        BuildRealisticSnakeGeometry(engine, out var spineNodesForShadow);

        // 4. Calculate depths & Sort Polygons (Painter's Algorithm)
        var camPos = _camera.Position;
        for (int i = 0; i < _polygons.Count; i++)
        {
            var poly = _polygons[i];
            poly.Depth = Vector3.DistanceSquared(camPos, poly.CalculateCenter());
        }

        _polygons.Sort((a, b) => b.Depth.CompareTo(a.Depth));

        // 5. Draw realistic smooth ground contact drop shadows
        DrawGroundContactShadows(canvas, engine, spineNodesForShadow);

        // 6. Rasterize 3D Polygons with Blinn-Phong & Hemispherical Sun Lighting
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
                        path.MoveTo(pt.X, pt.Y);
                    else
                        path.LineTo(pt.X, pt.Y);
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

            // Shading
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

        // 7. Render 3D Ambient Golden Pollen Motes
        DrawAmbientPollen(canvas);

        // 8. Render 3D Particle Bursts
        _particles.Render(canvas, _camera);
    }

    private void UpdateCamera(int width, int height, GameEngine engine)
    {
        float aspect = (float)width / Math.Max(1, height);

        if (engine.State == GameState.Menu)
        {
            // Sweeping cinematic orbit across the crop field
            float orbitAngle = _totalGameTime * 0.20f;
            float orbitRadius = 28f;
            _camera.Position = new Vector3(
                MathF.Sin(orbitAngle) * orbitRadius,
                21f,
                -MathF.Cos(orbitAngle) * orbitRadius
            );
            _camera.Target = new Vector3(0, 0, 0);
        }
        else
        {
            // Perspective camera framed so the entire field is centered in view with ample bottom and top margins
            float boardSpan = MathF.Max(engine.Board.Width, engine.Board.Height);
            float dist = boardSpan * (aspect < 1.0f ? 1.08f : 0.90f);

            float shakeX = _shakeIntensity * (Random.Shared.NextSingle() - 0.5f) * 1.2f;
            float shakeY = _shakeIntensity * (Random.Shared.NextSingle() - 0.5f) * 1.2f;

            _camera.Position = new Vector3(shakeX, (dist * 1.38f) + shakeY, -dist * 0.80f);
            _camera.Target = new Vector3(0, 0, 0.0f);
        }

        _camera.FieldOfViewDegrees = aspect < 1.0f ? 50f : 43f;
        _camera.UpdateMatrices(width, height);
    }

    private void DrawFarmlandSky(SKCanvas canvas, int width, int height)
    {
        using var skyPaint = new SKPaint();
        using var shader = SKShader.CreateLinearGradient(
            new SKPoint(width * 0.5f, 0),
            new SKPoint(width * 0.5f, height * 0.60f),
            [SkyZenith, SkyHorizon, FarFieldHorizon],
            [0.0f, 0.70f, 1.0f],
            SKShaderTileMode.Clamp);

        skyPaint.Shader = shader;
        canvas.DrawRect(0, 0, width, height, skyPaint);

        // Sun disc glow in the sky
        using var sunGlowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(255, 250, 220, 95),
            Style = SKPaintStyle.Fill
        };
        canvas.DrawCircle(width * 0.25f, height * 0.16f, MathF.Min(width, height) * 0.16f, sunGlowPaint);
    }

    private void DrawGroundContactShadows(SKCanvas canvas, GameEngine engine, Vector3[]? spineNodes)
    {
        using var shadowPaint = new SKPaint
        {
            IsAntialias = true,
            Color = new SKColor(18, 12, 6, 140), // Rich dark earthy contact shadow
            Style = SKPaintStyle.Fill
        };

        // Food / Insect / Frog prey drop shadows
        if (engine.CurrentFood != null)
        {
            var foodPos = GridToWorld(engine.CurrentFood.Position, engine.Board);
            if (_camera.WorldToScreen(new Vector3(foodPos.X, 0.02f, foodPos.Z), out var screenPos, out float depth))
            {
                float baseR = engine.CurrentFood.Type switch
                {
                    FoodType.Ladybug => 14f,
                    FoodType.Grasshopper => 18f,
                    _ => 22f
                };
                float radius = Math.Max(3f, (baseR * _camera.ViewportHeight * 0.035f) / depth);
                canvas.DrawOval(screenPos.X, screenPos.Y, radius * 1.35f, radius * 1.10f, shadowPaint);
            }
        }

        if (engine.SpecialFood != null)
        {
            var foodPos = GridToWorld(engine.SpecialFood.Position, engine.Board);
            if (_camera.WorldToScreen(new Vector3(foodPos.X, 0.02f, foodPos.Z), out var screenPos, out float depth))
            {
                float radius = Math.Max(4f, (26f * _camera.ViewportHeight * 0.035f) / depth);
                canvas.DrawOval(screenPos.X, screenPos.Y, radius * 1.50f, radius * 1.25f, shadowPaint);
            }
        }

        // Smooth continuous ribbon shadow under the entire snake spine
        if (spineNodes != null && spineNodes.Length > 1)
        {
            using var shadowStrokePaint = new SKPaint
            {
                IsAntialias = true,
                Color = new SKColor(18, 12, 6, 125),
                Style = SKPaintStyle.Stroke,
                StrokeCap = SKStrokeCap.Round,
                StrokeJoin = SKStrokeJoin.Round
            };

            using var shadowPath = new SKPath();
            bool started = false;

            for (int i = 0; i < spineNodes.Length; i++)
            {
                var groundNode = new Vector3(spineNodes[i].X, 0.02f, spineNodes[i].Z);
                if (_camera.WorldToScreen(groundNode, out var pt, out float depth))
                {
                    if (!started)
                    {
                        shadowPath.MoveTo(pt.X, pt.Y);
                        started = true;
                    }
                    else
                    {
                        shadowPath.LineTo(pt.X, pt.Y);
                    }

                    if (i == 0)
                    {
                        float headRadius = Math.Max(4f, (19f * _camera.ViewportHeight * 0.035f) / depth);
                        shadowStrokePaint.StrokeWidth = headRadius * 1.6f;
                    }
                }
            }

            if (started)
            {
                canvas.DrawPath(shadowPath, shadowStrokePaint);
            }
        }
    }

    private void DrawAmbientPollen(SKCanvas canvas)
    {
        using var pollenPaint = new SKPaint { IsAntialias = true, Style = SKPaintStyle.Fill };

        for (int i = 0; i < _ambientPollen.Length; i++)
        {
            float t = _totalGameTime + (i * 1.55f);
            var pos = _ambientPollen[i] + new Vector3(
                MathF.Sin(t * 0.7f) * 0.6f + (_totalGameTime * 0.4f),
                MathF.Sin(t * 1.1f) * 0.25f,
                MathF.Cos(t * 0.5f) * 0.5f);

            // Wrap around horizontal bounds
            if (pos.X > 25f) pos.X -= 50f;

            if (_camera.WorldToScreen(pos, out var pt, out float depth))
            {
                float alphaNorm = 0.50f + (MathF.Sin(t * 2.8f) * 0.35f);
                pollenPaint.Color = new SKColor(254, 240, 138, (byte)(alphaNorm * 255));
                float r = Math.Max(1.5f, (3.2f * _camera.ViewportHeight * 0.02f) / depth);
                canvas.DrawCircle(pt, r, pollenPaint);
            }
        }
    }

    /// <summary>
    /// Builds a borderless agricultural farmland terrain with tilled loamy soil furrows and crop beds stretching across the screen.
    /// </summary>
    private void BuildBorderlessFarmland(GameBoard board)
    {
        float halfW = board.Width * 0.5f;
        float halfH = board.Height * 0.5f;

        // Generous margin expanding the tilled field seamlessly to the app borders
        int marginX = 18;
        int marginY = 18;
        int minX = -marginX;
        int maxX = board.Width + marginX;
        int minY = -marginY;
        int maxY = board.Height + marginY;

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                float wx = halfW - x - 0.5f;
                float wz = halfH - y - 0.5f;
                var center = new Vector3(wx, 0f, wz);

                // Alternating cultivation furrows and ridges
                bool isRidge = y % 2 == 0;
                var soilColor = isRidge
                    ? ((x + y) % 3 == 0 ? LoamRidgeLight : LoamRidgeDark)
                    : ((x + y) % 4 == 0 ? LoamSubtleGreen : LoamTrench);

                float ridgeElevation = isRidge ? 0.05f : -0.05f;

                var p0 = center + new Vector3(-0.50f, ridgeElevation, -0.50f);
                var p1 = center + new Vector3(0.50f, ridgeElevation, -0.50f);
                var p2 = center + new Vector3(0.50f, ridgeElevation, 0.50f);
                var p3 = center + new Vector3(-0.50f, ridgeElevation, 0.50f);

                _polygons.Add(new Polygon3D([p0, p1, p2, p3], soilColor)
                {
                    SpecularIntensity = 0.15f,
                    Shininess = 10f
                });
            }
        }
    }

    /// <summary>
    /// Builds dense pasture grass carpet across all tiles and swaying golden wheat rows across the field.
    /// </summary>
    private void BuildDenseGrassCarpetAndCrops(GameBoard board)
    {
        float halfW = board.Width * 0.5f;
        float halfH = board.Height * 0.5f;
        float windTime = _totalGameTime * 2.8f;

        // 1. Dense grass carpet extending across the entire screen from corner to corner and beyond
        int margin = 8;
        for (int y = -margin; y < board.Height + margin; y++)
        {
            for (int x = -margin; x < board.Width + margin; x++)
            {
                float jx = MathF.Sin((x * 19.3f) + (y * 7.7f)) * 0.25f;
                float jz = MathF.Cos((y * 17.1f) + (x * 11.3f)) * 0.25f;
                float gx = halfW - x - 0.5f + jx;
                float gz = halfH - y - 0.5f + jz;
                var grassPos = new Vector3(gx, 0.0f, gz);

                // Primary grass clump
                float grassScale = 0.95f + (MathF.Abs(MathF.Sin(x * 4.1f + y * 2.3f)) * 0.25f);
                MeshBuilder.AddDenseGrassClump(_polygons, grassPos, grassScale, windTime, (x * 0.6f + y * 0.4f));

                // Secondary offset grass tuft for continuous lush lawn coverage
                var grassPos2 = new Vector3(gx + 0.32f, 0.0f, gz - 0.28f);
                MeshBuilder.AddDenseGrassClump(_polygons, grassPos2, grassScale * 0.85f, windTime, (x * 1.1f + y * 0.8f));

                // Wild field chamomile / poppy
                if ((x * 7 + y * 5) % 9 == 0)
                {
                    var flowerPos = grassPos + new Vector3(0.20f, 0, 0.20f);
                    bool isPoppy = (x + y) % 3 == 0;
                    MeshBuilder.AddFieldFlower(_polygons, flowerPos, scale: 0.88f, MathF.Sin(windTime + x), isPoppy);
                }
            }
        }

        // 2. Wheat crop clusters along alternating ridges
        for (int y = -margin; y < board.Height + margin; y += 2)
        {
            for (int x = -margin; x < board.Width + margin; x += 3)
            {
                float wx = halfW - x - 0.5f + (MathF.Sin(x + y) * 0.2f);
                float wz = halfH - y - 0.5f + (MathF.Cos(x + y) * 0.2f);
                var cropPos = new Vector3(wx, 0.0f, wz);
                float cropScale = 0.95f + (MathF.Abs(MathF.Sin(x * 3f + y)) * 0.25f);
                MeshBuilder.AddWheatCluster(_polygons, cropPos, cropScale, windTime, (x * 0.5f + y * 0.3f));
            }
        }

        // 3. Dense radial crop field extending in all 360-degree directions around the boundary
        float boardRadius = MathF.Max(halfW, halfH);
        for (float r = boardRadius + 1.2f; r <= boardRadius + 16f; r += 2.2f)
        {
            int countOnRing = (int)(r * 3.6f);
            for (int i = 0; i < countOnRing; i++)
            {
                float angle = (i * MathF.PI * 2f / countOnRing) + (r * 0.35f);
                float jitter = MathF.Sin(i * 13.7f + r * 5.1f) * 0.4f;
                float dist = r + jitter;
                var pos = new Vector3(MathF.Cos(angle) * dist, 0.0f, MathF.Sin(angle) * dist);

                float scale = 1.15f + (MathF.Abs(MathF.Sin(i * 2.3f + r)) * 0.25f);
                MeshBuilder.AddWheatCluster(_polygons, pos, scale, windTime, (angle * 2f + r));
            }
        }
    }

    private void BuildFoodGeometry(GameEngine engine)
    {
        if (engine.CurrentFood != null)
        {
            var food = engine.CurrentFood;
            var pos = GridToWorld(food.Position, engine.Board);

            switch (food.Type)
            {
                case FoodType.Ladybug:
                    pos.Y = 0.04f + (MathF.Sin(_totalGameTime * 5f) * 0.015f);
                    MeshBuilder.AddSculptedLadybug(_polygons, pos, scale: 0.95f, _totalGameTime);
                    break;

                case FoodType.Grasshopper:
                    pos.Y = 0.06f + (MathF.Sin(_totalGameTime * 4f) * 0.03f);
                    MeshBuilder.AddSculptedGrasshopper(_polygons, pos, scale: 1.15f, _totalGameTime);
                    break;

                case FoodType.Dragonfly:
                    pos.Y = 0.45f + (MathF.Sin(_totalGameTime * 7f) * 0.10f);
                    MeshBuilder.AddSculptedDragonfly(_polygons, pos, scale: 1.10f, _totalGameTime);
                    break;

                case FoodType.GoldenFrog:
                    pos.Y = 0.06f + (MathF.Sin(_totalGameTime * 5.5f) * 0.04f);
                    MeshBuilder.AddSculptedFrog(_polygons, pos, scale: 1.45f, _totalGameTime * 4f, isGolden: true);
                    break;

                default: // Frog
                    pos.Y = 0.04f + (MathF.Sin(_totalGameTime * 3.5f) * 0.02f);
                    MeshBuilder.AddSculptedFrog(_polygons, pos, scale: 1.35f, _totalGameTime * 2.5f, isGolden: false);
                    break;
            }
        }

        if (engine.SpecialFood != null)
        {
            var food = engine.SpecialFood;
            var pos = GridToWorld(food.Position, engine.Board);

            if (food.Type == FoodType.Dragonfly)
            {
                pos.Y = 0.55f + (MathF.Sin(_totalGameTime * 8f) * 0.12f);
                MeshBuilder.AddSculptedDragonfly(_polygons, pos, scale: 1.35f, _totalGameTime);
            }
            else
            {
                pos.Y = 0.08f + (MathF.Sin(_totalGameTime * 6.0f) * 0.06f);
                MeshBuilder.AddSculptedFrog(_polygons, pos, scale: 1.50f, _totalGameTime * 4.0f, isGolden: true);
            }
        }
    }

    /// <summary>
    /// Builds an anatomically realistic serpent with smooth spline curves, lateral undulation, 16-vertex reptilian scale patterning, and flicking forked tongue.
    /// </summary>
    private void BuildRealisticSnakeGeometry(GameEngine engine, out Vector3[] spineNodesOut)
    {
        var segments = engine.Snake.Segments;
        if (segments.Count == 0)
        {
            spineNodesOut = Array.Empty<Vector3>();
            return;
        }

        float subTick = (float)engine.SubTickProgress;
        var board = engine.Board;
        var headingDir = engine.Snake.CurrentDirection;

        int count = segments.Count;
        var rawNodes = new Vector3[count];

        for (int i = 0; i < count; i++)
        {
            var currWorld = GridToWorld(segments[i], board);
            Vector3 prevWorld;

            if (i == 0)
            {
                var prevHeadGrid = segments.Count > 1 ? segments[1] : segments[0].Move(headingDir.Opposite());
                prevWorld = GridToWorld(prevHeadGrid, board);
            }
            else
            {
                prevWorld = GridToWorld(segments[i], board);
                currWorld = GridToWorld(segments[i - 1], board);
            }

            rawNodes[i] = Vector3.Lerp(prevWorld, currWorld, engine.State == GameState.Playing ? subTick : 0.0f);
        }

        // Subdivide segments with smooth Catmull-Rom spline nodes (6 sub-steps per segment)
        int subSteps = 6;
        int totalSplineCount = ((count - 1) * subSteps) + 1;
        var spineNodes = new Vector3[totalSplineCount];

        int idx = 0;
        for (int i = 0; i < count - 1; i++)
        {
            var p0 = i > 0 ? rawNodes[i - 1] : rawNodes[0] + (rawNodes[0] - rawNodes[1]);
            var p1 = rawNodes[i];
            var p2 = rawNodes[i + 1];
            var p3 = (i + 2 < count) ? rawNodes[i + 2] : rawNodes[i + 1] + (rawNodes[i + 1] - rawNodes[i]);

            for (int s = 0; s < subSteps; s++)
            {
                float t = (float)s / subSteps;
                spineNodes[idx++] = CatmullRom(p0, p1, p2, p3, t);
            }
        }
        spineNodes[idx] = rawNodes[^1];

        // Apply natural lateral serpentine traveling wave through the spline nodes
        for (int i = 0; i < totalSplineCount; i++)
        {
            float norm = (float)i / totalSplineCount;
            float wavePhase = (_totalGameTime * 14f) - (i * 0.18f);
            float waveAmp = (i == 0 ? 0.03f : 0.16f) * (engine.State == GameState.Playing ? 1.0f : 0.3f);
            float wave = MathF.Sin(wavePhase) * waveAmp;

            Vector3 forward;
            if (i == 0)
                forward = DirectionToVector(headingDir);
            else if (i == totalSplineCount - 1)
                forward = Vector3.Normalize(spineNodes[i - 1] - spineNodes[i] + new Vector3(0.0001f, 0, 0));
            else
                forward = Vector3.Normalize(spineNodes[i - 1] - spineNodes[i + 1] + new Vector3(0.0001f, 0, 0));

            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            spineNodes[i] += right * wave;
            spineNodes[i].Y = 0.32f; // Height resting over dense grass
        }

        spineNodesOut = spineNodes;

        // Head orientation
        Vector3 headForward = DirectionToVector(headingDir);
        Vector3 headRight = new Vector3(-headForward.Z, 0, headForward.X);

        // Build Sculpted Anatomical Snake Head
        MeshBuilder.AddSculptedSnakeHead(
            _polygons,
            spineNodes[0],
            headForward,
            headRight,
            MathF.Abs(MathF.Sin(_tongueTimer)),
            SnakeDorsalDark,
            SnakeFlank,
            SnakeBelly,
            SnakeDorsalDiamond,
            SnakeLipScale);

        // Build Continuous Tubular Body Mesh with 16-sided cross-sectional rings
        int ringSegments = 16;
        var rings = new Vector3[totalSplineCount][];

        for (int i = 0; i < totalSplineCount; i++)
        {
            rings[i] = new Vector3[ringSegments];

            // Natural snake body girth profile: narrow neck behind head, muscular midbody, tapering to a slender tail
            float norm = (float)i / Math.Max(1, totalSplineCount - 1);
            float neckGirth = (i < subSteps * 2) ? (0.75f + (0.25f * (float)i / (subSteps * 2))) : 1.0f;
            float taperFactor = MathF.Sin(MathF.PI * 0.5f * (1.0f - (norm * 0.88f))) * neckGirth;
            float radiusX = 0.52f * taperFactor;
            float radiusY = 0.38f * taperFactor; // Anatomically flattened ventral belly

            // Dynamic food digestion lump travelling down spine
            if (_digestionPulse >= 0f)
            {
                float splinePulse = _digestionPulse * subSteps;
                float distToPulse = MathF.Abs(i - splinePulse);
                if (distToPulse < 3.0f * subSteps)
                {
                    float lumpFactor = 1.0f + (0.45f * MathF.Cos(distToPulse * (MathF.PI / (3.0f * subSteps)) * 0.5f));
                    radiusX *= lumpFactor;
                    radiusY *= lumpFactor;
                }
            }

            Vector3 fwd;
            if (i == 0)
                fwd = headForward;
            else if (i == totalSplineCount - 1)
                fwd = Vector3.Normalize(spineNodes[i - 1] - spineNodes[i]);
            else
                fwd = Vector3.Normalize(spineNodes[i - 1] - spineNodes[i + 1]);

            Vector3 rgt = new Vector3(-fwd.Z, 0, fwd.X);
            Vector3 up = Vector3.UnitY;

            for (int s = 0; s < ringSegments; s++)
            {
                float angle = s * MathF.PI * 2f / ringSegments;
                float ca = MathF.Cos(angle);
                float sa = MathF.Sin(angle);

                rings[i][s] = spineNodes[i] + (rgt * ca * radiusX) + (up * sa * radiusY);
            }
        }

        // Connect consecutive rings with realistic reptilian scale quads
        for (int i = 0; i < totalSplineCount - 1; i++)
        {
            for (int s = 0; s < ringSegments; s++)
            {
                int next = (s + 1) % ringSegments;

                // Scale coloration based on anatomical zone (dorsal diamond crest, lateral flank, pale ventral belly scutes)
                SKColor quadColor;
                float specIntensity = 0.75f;
                float shininess = 36f;

                int segmentIndex = i / subSteps;
                int subRing = i % subSteps;

                if (s == 3 || s == 4 || s == 5)
                {
                    bool isDiamondCenter = (segmentIndex % 3 == 0) && (subRing >= 2 && subRing <= 4);
                    bool isDiamondBorder = (segmentIndex % 3 == 0) && (subRing == 1 || subRing == 5);

                    if (isDiamondCenter)
                    {
                        quadColor = SnakeDorsalAccent;
                        specIntensity = 0.95f;
                        shininess = 48f;
                    }
                    else if (isDiamondBorder)
                    {
                        quadColor = SnakeDorsalDiamond;
                        specIntensity = 0.90f;
                        shininess = 44f;
                    }
                    else
                    {
                        quadColor = ((i + s) % 2 == 0) ? SnakeDorsalDark : SnakeDorsalBlack;
                        specIntensity = 0.80f;
                        shininess = 38f;
                    }
                }
                else if (s >= 10 && s <= 14) // Pale ventral belly plates (horizontal belly scutes)
                {
                    quadColor = (segmentIndex % 2 == 0) ? SnakeBelly : SnakeBellyEdge;
                    specIntensity = 0.35f;
                    shininess = 16f;
                }
                else if (s == 1 || s == 2 || s == 6 || s == 7) // Upper flanks
                {
                    quadColor = ((i + s) % 3 == 0) ? SnakeFlankDark : SnakeFlank;
                    specIntensity = 0.75f;
                    shininess = 34f;
                }
                else // Lower lateral flanks
                {
                    quadColor = SnakeFlankDark;
                    specIntensity = 0.65f;
                    shininess = 28f;
                }

                _polygons.Add(new Polygon3D(
                    [rings[i][s], rings[i][next], rings[i + 1][next], rings[i + 1][s]],
                    quadColor)
                {
                    SpecularIntensity = specIntensity,
                    Shininess = shininess
                });
            }
        }

        // Tapered tail tip closure
        var tailTip = spineNodes[^1] - ((spineNodes.Length > 1 ? Vector3.Normalize(spineNodes[^2] - spineNodes[^1]) : headForward) * 0.24f);
        for (int s = 0; s < ringSegments; s++)
        {
            int next = (s + 1) % ringSegments;
            _polygons.Add(new Polygon3D([tailTip, rings[^1][next], rings[^1][s]], SnakeDorsalDark));
        }
    }

    private static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float t2 = t * t;
        float t3 = t2 * t;

        return 0.5f * (
            (2.0f * p1) +
            (-p0 + p2) * t +
            ((2.0f * p0) - (5.0f * p1) + (4.0f * p2) - p3) * t2 +
            (-p0 + (3.0f * p1) - (3.0f * p2) + p3) * t3
        );
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

    public static void GenerateStoreScreenshots(string outputDir)
    {
        try
        {
            Directory.CreateDirectory(outputDir);

            // Screenshot 1: Gameplay in 24x24 crop field with insects and frogs
            {
                using var bitmap = new SKBitmap(1920, 1080, SKColorType.Rgba8888, SKAlphaType.Premul);
                using var canvas = new SKCanvas(bitmap);
                var engine = new GameEngine(boardWidth: 24, boardHeight: 24);
                var renderer = new GameRenderer3D();

                engine.StartGame();
                engine.Score = 180;
                engine.HighScore = 350;

                for (int i = 0; i < 8; i++)
                {
                    engine.Snake.Step(grow: true);
                }

                renderer.Render(canvas, 1920, 1080, engine, 0.033f);

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                File.WriteAllBytes(Path.Combine(outputDir, "real_screenshot_1_gameplay.png"), data.ToArray());
            }

            // Screenshot 2: Action moment with Grasshopper & Ladybug in crop field
            {
                using var bitmap = new SKBitmap(1920, 1080, SKColorType.Rgba8888, SKAlphaType.Premul);
                using var canvas = new SKCanvas(bitmap);
                var engine = new GameEngine(boardWidth: 24, boardHeight: 24);
                var renderer = new GameRenderer3D();

                engine.StartGame();
                engine.Score = 320;
                engine.HighScore = 350;
                for (int i = 0; i < 12; i++)
                {
                    engine.Snake.Step(grow: true);
                }

                if (engine.SpecialFood != null)
                {
                    renderer.OnFoodEaten(engine.SpecialFood, engine);
                }

                renderer.Render(canvas, 1920, 1080, engine, 0.033f);

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                File.WriteAllBytes(Path.Combine(outputDir, "real_screenshot_2_action.png"), data.ToArray());
            }

            // Screenshot 3: Main Menu Orbiting Farmland Showcase
            {
                using var bitmap = new SKBitmap(1920, 1080, SKColorType.Rgba8888, SKAlphaType.Premul);
                using var canvas = new SKCanvas(bitmap);
                var engine = new GameEngine(boardWidth: 24, boardHeight: 24);
                var renderer = new GameRenderer3D();

                engine.HighScore = 350;
                renderer.Render(canvas, 1920, 1080, engine, 0.033f);

                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                File.WriteAllBytes(Path.Combine(outputDir, "real_screenshot_3_menu.png"), data.ToArray());
            }
        }
        catch
        {
            // Ignore capture exceptions
        }
    }
}
