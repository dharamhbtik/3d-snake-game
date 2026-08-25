using System.Numerics;
using SkiaSharp;
using Snake3D.Core;

namespace Snake3D.Rendering;

/// <summary>
/// Generates high-fidelity 3D meshes for realistic snake anatomy, undulating tubular body, sculpted 3D apples, grass tufts, and stone arena.
/// </summary>
public static class MeshBuilder
{
    public static void AddBox(
        List<Polygon3D> list,
        Vector3 center,
        Vector3 size,
        SKColor topColor,
        SKColor sideColor,
        SKColor? strokeColor = null,
        bool isEmissive = false)
    {
        float hx = size.X * 0.5f;
        float hy = size.Y * 0.5f;
        float hz = size.Z * 0.5f;

        var p000 = center + new Vector3(-hx, -hy, -hz);
        var p100 = center + new Vector3(hx, -hy, -hz);
        var p110 = center + new Vector3(hx, hy, -hz);
        var p010 = center + new Vector3(-hx, hy, -hz);

        var p001 = center + new Vector3(-hx, -hy, hz);
        var p101 = center + new Vector3(hx, -hy, hz);
        var p111 = center + new Vector3(hx, hy, hz);
        var p011 = center + new Vector3(-hx, hy, hz);

        // Top face (+Y)
        list.Add(new Polygon3D([p010, p110, p111, p011], topColor, strokeColor, isEmissive));
        // Front face (+Z)
        list.Add(new Polygon3D([p011, p111, p101, p001], sideColor, strokeColor, isEmissive));
        // Back face (-Z)
        list.Add(new Polygon3D([p110, p010, p000, p100], sideColor, strokeColor, isEmissive));
        // Right face (+X)
        list.Add(new Polygon3D([p111, p110, p100, p101], sideColor, strokeColor, isEmissive));
        // Left face (-X)
        list.Add(new Polygon3D([p010, p011, p001, p000], sideColor, strokeColor, isEmissive));
    }

    /// <summary>
    /// Builds prominent, dense realistic 3D grass tufts that sway gently in the wind.
    /// </summary>
    public static void AddGrassTuft(
        List<Polygon3D> list,
        Vector3 basePos,
        float scale,
        float windSway,
        SKColor grassDark,
        SKColor grassLight,
        bool hasFlower = false)
    {
        int blades = 8;
        for (int i = 0; i < blades; i++)
        {
            float angle = i * MathF.PI * 2f / blades;
            float bladeHeight = (0.75f + (MathF.Sin(i * 1.8f) * 0.22f)) * scale;
            float bladeWidth = 0.10f * scale;

            var dir = new Vector3(MathF.Cos(angle), 0, MathF.Sin(angle));
            var p0 = basePos - (dir * bladeWidth);
            var p1 = basePos + (dir * bladeWidth);
            var tip = basePos + new Vector3(
                (dir.X * 0.32f * scale) + (windSway * 0.25f),
                bladeHeight,
                (dir.Z * 0.32f * scale) + (windSway * 0.16f));

            var col = (i % 2 == 0) ? grassLight : grassDark;
            list.Add(new Polygon3D([p0, p1, tip], col, isEmissive: false)
            {
                SpecularIntensity = 0.35f,
                Shininess = 12f
            });
        }

        // Meadow daisy / flower
        if (hasFlower)
        {
            var flowerCenter = basePos + new Vector3(windSway * 0.20f, 0.65f * scale, windSway * 0.14f);
            var flowerColor = new SKColor(255, 255, 255); // White petal
            var centerColor = new SKColor(251, 191, 36); // Yellow center
            float fSize = 0.12f * scale;
            AddBox(list, flowerCenter, new Vector3(fSize, fSize * 0.6f, fSize), flowerColor, flowerColor);
            AddBox(list, flowerCenter + new Vector3(0, fSize * 0.35f, 0), new Vector3(fSize * 0.45f, fSize * 0.45f, fSize * 0.45f), centerColor, centerColor);
        }
    }

    /// <summary>
    /// Builds a realistic 3D Apple with sculpted roundness, wooden stem, and green leaf.
    /// </summary>
    public static void AddRealisticApple(
        List<Polygon3D> list,
        Vector3 center,
        float scale,
        float rotationY,
        bool isGolden)
    {
        SKColor appleBodyTop = isGolden ? new SKColor(254, 240, 138) : new SKColor(239, 68, 68);
        SKColor appleBodySide = isGolden ? new SKColor(234, 179, 8) : new SKColor(185, 28, 28);
        SKColor appleBodyDark = isGolden ? new SKColor(202, 138, 4) : new SKColor(153, 27, 27);
        SKColor stemColor = new SKColor(101, 67, 33);
        SKColor leafColor = isGolden ? new SKColor(250, 204, 21) : new SKColor(34, 197, 94);

        int ringSegments = 10;
        int verticalRings = 4;

        // Radii and Y-offsets for natural apple profile
        float[] ringY = [-0.38f, -0.15f, 0.18f, 0.35f];
        float[] ringR = [0.18f, 0.44f, 0.46f, 0.24f];

        var rings = new Vector3[verticalRings][];
        for (int r = 0; r < verticalRings; r++)
        {
            rings[r] = new Vector3[ringSegments];
            for (int s = 0; s < ringSegments; s++)
            {
                float a = rotationY + (s * MathF.PI * 2f / ringSegments);
                rings[r][s] = center + new Vector3(
                    MathF.Cos(a) * ringR[r] * scale,
                    ringY[r] * scale,
                    MathF.Sin(a) * ringR[r] * scale);
            }
        }

        // Connect rings with quads
        for (int r = 0; r < verticalRings - 1; r++)
        {
            for (int s = 0; s < ringSegments; s++)
            {
                int next = (s + 1) % ringSegments;
                var col = r switch
                {
                    0 => appleBodyDark,
                    1 => appleBodySide,
                    _ => appleBodyTop
                };

                list.Add(new Polygon3D(
                    [rings[r][s], rings[r][next], rings[r + 1][next], rings[r + 1][s]],
                    col)
                {
                    SpecularIntensity = 0.60f,
                    Shininess = 32f
                });
            }
        }

        // Bottom cap
        var bottomCenter = center + new Vector3(0, -0.42f * scale, 0);
        for (int s = 0; s < ringSegments; s++)
        {
            int next = (s + 1) % ringSegments;
            list.Add(new Polygon3D([bottomCenter, rings[0][next], rings[0][s]], appleBodyDark));
        }

        // Top dimple
        var topDimple = center + new Vector3(0, 0.28f * scale, 0);
        for (int s = 0; s < ringSegments; s++)
        {
            int next = (s + 1) % ringSegments;
            list.Add(new Polygon3D([topDimple, rings[verticalRings - 1][s], rings[verticalRings - 1][next]], appleBodyDark));
        }

        // Wooden Stem
        var stemBase = topDimple;
        var stemTip = stemBase + new Vector3(0.08f * scale, 0.32f * scale, 0.04f * scale);
        AddBox(list, (stemBase + stemTip) * 0.5f, new Vector3(0.06f, 0.28f, 0.06f) * scale, stemColor, stemColor);

        // Green Leaf
        var leafBase = stemBase + new Vector3(0.04f * scale, 0.18f * scale, 0.02f * scale);
        var leafTip = leafBase + new Vector3(0.32f * scale, 0.12f * scale, 0.18f * scale);
        var leafSide1 = leafBase + new Vector3(0.16f * scale, 0.16f * scale, -0.05f * scale);
        var leafSide2 = leafBase + new Vector3(0.20f * scale, 0.08f * scale, 0.22f * scale);

        list.Add(new Polygon3D([leafBase, leafSide1, leafTip, leafSide2], leafColor, new SKColor(22, 101, 52))
        {
            SpecularIntensity = 0.45f
        });
    }

    /// <summary>
    /// Builds a sculpted viper snake head with jaw, brow ridges, nostrils, 3D golden serpentine eyes, and flicking fork tongue.
    /// </summary>
    public static void AddSculptedSnakeHead(
        List<Polygon3D> list,
        Vector3 headPos,
        Vector3 forwardVec,
        Vector3 rightVec,
        float tongueProgress,
        SKColor dorsalColor,
        SKColor flankColor,
        SKColor bellyColor)
    {
        Vector3 upVec = Vector3.UnitY;
        float headLength = 0.95f;
        float headWidth = 0.78f;
        float headHeight = 0.52f;

        // Key landmarks of the viper head
        var snoutTop = headPos + (forwardVec * headLength * 0.55f) + (upVec * headHeight * 0.22f);
        var snoutBottom = headPos + (forwardVec * headLength * 0.52f) - (upVec * headHeight * 0.28f);
        var browLeft = headPos + (forwardVec * headLength * 0.18f) - (rightVec * headWidth * 0.46f) + (upVec * headHeight * 0.42f);
        var browRight = headPos + (forwardVec * headLength * 0.18f) + (rightVec * headWidth * 0.46f) + (upVec * headHeight * 0.42f);
        var jawLeft = headPos - (forwardVec * headLength * 0.22f) - (rightVec * headWidth * 0.50f) - (upVec * headHeight * 0.18f);
        var jawRight = headPos - (forwardVec * headLength * 0.22f) + (rightVec * headWidth * 0.50f) - (upVec * headHeight * 0.18f);
        var crownBack = headPos - (forwardVec * headLength * 0.45f) + (upVec * headHeight * 0.38f);
        var throatBack = headPos - (forwardVec * headLength * 0.45f) - (upVec * headHeight * 0.28f);

        // Dorsal Crown
        list.Add(new Polygon3D([snoutTop, browRight, crownBack, browLeft], dorsalColor)
        {
            SpecularIntensity = 0.55f,
            Shininess = 28f
        });

        // Left Flank
        list.Add(new Polygon3D([snoutTop, browLeft, jawLeft, snoutBottom], flankColor));
        list.Add(new Polygon3D([browLeft, crownBack, throatBack, jawLeft], flankColor));

        // Right Flank
        list.Add(new Polygon3D([snoutTop, snoutBottom, jawRight, browRight], flankColor));
        list.Add(new Polygon3D([browRight, jawRight, throatBack, crownBack], flankColor));

        // Pale Ventral Belly
        list.Add(new Polygon3D([snoutBottom, jawLeft, throatBack, jawRight], bellyColor)
        {
            SpecularIntensity = 0.25f
        });

        // 3D Serpentine Eyes
        float eyeRadius = 0.13f;
        var leftEyeCenter = browLeft + (forwardVec * 0.06f) - (rightVec * 0.04f);
        var rightEyeCenter = browRight + (forwardVec * 0.06f) + (rightVec * 0.04f);

        var goldIris = new SKColor(245, 158, 11);
        var slitPupil = new SKColor(15, 23, 42);

        AddBox(list, leftEyeCenter, new Vector3(eyeRadius * 1.1f, eyeRadius * 1.1f, eyeRadius * 1.1f), goldIris, goldIris);
        AddBox(list, rightEyeCenter, new Vector3(eyeRadius * 1.1f, eyeRadius * 1.1f, eyeRadius * 1.1f), goldIris, goldIris);

        // Pupil slit in direction of view
        AddBox(list, leftEyeCenter - (rightVec * eyeRadius * 0.55f), new Vector3(0.04f, eyeRadius * 0.9f, 0.08f), slitPupil, slitPupil);
        AddBox(list, rightEyeCenter + (rightVec * eyeRadius * 0.55f), new Vector3(0.04f, eyeRadius * 0.9f, 0.08f), slitPupil, slitPupil);

        // Animated Forked Tongue
        float tongueLen = 0.65f * MathF.Sin(tongueProgress * MathF.PI);
        if (tongueLen > 0.06f)
        {
            var tongueBase = snoutBottom + (forwardVec * 0.04f) + (upVec * 0.05f);
            var tongueFork = tongueBase + (forwardVec * tongueLen * 0.65f);
            var tipL = tongueFork + (forwardVec * tongueLen * 0.35f) - (rightVec * 0.09f);
            var tipR = tongueFork + (forwardVec * tongueLen * 0.35f) + (rightVec * 0.09f);
            var redTongue = new SKColor(225, 29, 72);

            // Tongue stalk
            list.Add(new Polygon3D([
                tongueBase - (rightVec * 0.035f),
                tongueBase + (rightVec * 0.035f),
                tongueFork + (rightVec * 0.025f),
                tongueFork - (rightVec * 0.025f)
            ], redTongue, isEmissive: true));

            // Forked tips
            list.Add(new Polygon3D([tongueFork, tongueFork + (rightVec * 0.025f), tipR], redTongue, isEmissive: true));
            list.Add(new Polygon3D([tongueFork - (rightVec * 0.025f), tongueFork, tipL], redTongue, isEmissive: true));
        }
    }
}
