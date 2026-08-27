using System.Numerics;
using SkiaSharp;
using Snake3D.Core;

namespace Snake3D.Rendering;

/// <summary>
/// High-fidelity 3D mesh generator for anatomically realistic snakes, sculpted 3D frogs, insects (ladybugs, grasshoppers, dragonflies), dense multi-layer pasture grass, wheat crops, and farmland props.
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
    /// Builds a rich, dense clump of natural grass blades with multi-stage curve, wind sway, and lush foliage coloration.
    /// </summary>
    public static void AddDenseGrassClump(
        List<Polygon3D> list,
        Vector3 basePos,
        float scale,
        float windTime,
        float phaseOffset)
    {
        int blades = 7;
        var grassDark = new SKColor(16, 75, 38); // Deep rich earth green
        var grassMid = new SKColor(34, 150, 68); // Lush meadow green
        var grassLight = new SKColor(74, 205, 90); // Sunlit blade crest
        var grassTip = new SKColor(163, 230, 53); // Fresh chartreuse blade tip

        for (int b = 0; b < blades; b++)
        {
            float angle = (b * MathF.PI * 2f / blades) + phaseOffset;
            float bladeHeight = (0.52f + (MathF.Sin((b * 2.3f) + phaseOffset) * 0.18f)) * scale;
            float bladeWidth = 0.095f * scale;

            float sway = MathF.Sin(windTime + phaseOffset + (b * 0.45f)) * 0.18f * scale;
            var dir = new Vector3(MathF.Cos(angle), 0, MathF.Sin(angle));
            var perp = new Vector3(-dir.Z, 0, dir.X);

            var p0 = basePos - (perp * bladeWidth * 0.5f);
            var p1 = basePos + (perp * bladeWidth * 0.5f);
            var pMid = basePos + (dir * 0.16f * scale) + new Vector3(sway * 0.4f, bladeHeight * 0.50f, sway * 0.3f);
            var tip = basePos + (dir * 0.34f * scale) + new Vector3(sway, bladeHeight, sway * 0.65f);

            var bColLower = (b % 2 == 0) ? grassDark : grassMid;
            var bColUpper = (b % 2 == 0) ? grassLight : grassTip;

            list.Add(new Polygon3D([p0, p1, pMid + (perp * bladeWidth * 0.35f), pMid - (perp * bladeWidth * 0.35f)], bColLower));
            list.Add(new Polygon3D([pMid - (perp * bladeWidth * 0.35f), pMid + (perp * bladeWidth * 0.35f), tip], bColUpper)
            {
                SpecularIntensity = 0.35f,
                Shininess = 16f
            });
        }
    }

    /// <summary>
    /// Builds a sculpted 3D Ladybug (small beetle with shiny red spotted shell, black head, antennae, and legs).
    /// </summary>
    public static void AddSculptedLadybug(
        List<Polygon3D> list,
        Vector3 center,
        float scale,
        float animTime)
    {
        var redShell = new SKColor(239, 68, 68); // Glossy scarlet red
        var blackShell = new SKColor(15, 23, 42); // Black spots / head
        var whiteDot = new SKColor(255, 255, 255);

        float twitch = MathF.Sin(animTime * 6f) * 0.04f * scale;
        Vector3 up = Vector3.UnitY;
        Vector3 fwd = new Vector3(0, 0, 1);
        Vector3 right = new Vector3(1, 0, 0);

        // Domed red wing covers (Elytra)
        var domePos = center + (up * 0.18f * scale);
        AddBox(list, domePos, new Vector3(0.55f, 0.30f, 0.65f) * scale, redShell, redShell);

        // Black central dividing line & spots
        AddBox(list, domePos + (up * 0.16f * scale), new Vector3(0.04f, 0.02f, 0.62f) * scale, blackShell, blackShell);
        AddBox(list, domePos + (up * 0.16f * scale) - (right * 0.16f * scale) + (fwd * 0.12f * scale), new Vector3(0.12f, 0.02f, 0.12f) * scale, blackShell, blackShell);
        AddBox(list, domePos + (up * 0.16f * scale) + (right * 0.16f * scale) + (fwd * 0.12f * scale), new Vector3(0.12f, 0.02f, 0.12f) * scale, blackShell, blackShell);
        AddBox(list, domePos + (up * 0.16f * scale) - (right * 0.16f * scale) - (fwd * 0.14f * scale), new Vector3(0.12f, 0.02f, 0.12f) * scale, blackShell, blackShell);
        AddBox(list, domePos + (up * 0.16f * scale) + (right * 0.16f * scale) - (fwd * 0.14f * scale), new Vector3(0.12f, 0.02f, 0.12f) * scale, blackShell, blackShell);

        // Head & Pronotum
        var headPos = center + (fwd * 0.38f * scale) + (up * 0.12f * scale);
        AddBox(list, headPos, new Vector3(0.32f, 0.20f, 0.22f) * scale, blackShell, blackShell);
        AddBox(list, headPos + (fwd * 0.10f * scale) - (right * 0.10f * scale) + (up * 0.06f * scale), new Vector3(0.06f, 0.06f, 0.06f) * scale, whiteDot, whiteDot, isEmissive: true);
        AddBox(list, headPos + (fwd * 0.10f * scale) + (right * 0.10f * scale) + (up * 0.06f * scale), new Vector3(0.06f, 0.06f, 0.06f) * scale, whiteDot, whiteDot, isEmissive: true);

        // Antennae
        var antL = headPos + (fwd * 0.20f * scale) - (right * 0.14f * scale) + (up * 0.08f * scale);
        var antR = headPos + (fwd * 0.20f * scale) + (right * 0.14f * scale) + (up * 0.08f * scale);
        AddBox(list, antL, new Vector3(0.03f, 0.08f, 0.14f) * scale, blackShell, blackShell);
        AddBox(list, antR, new Vector3(0.03f, 0.08f, 0.14f) * scale, blackShell, blackShell);

        // Crawling legs
        for (int i = -1; i <= 1; i++)
        {
            var legL = center + (fwd * i * 0.18f * scale) - (right * 0.32f * scale) + (up * 0.04f * scale);
            var legR = center + (fwd * i * 0.18f * scale) + (right * 0.32f * scale) + (up * 0.04f * scale);
            AddBox(list, legL, new Vector3(0.18f, 0.04f, 0.04f) * scale, blackShell, blackShell);
            AddBox(list, legR, new Vector3(0.18f, 0.04f, 0.04f) * scale, blackShell, blackShell);
        }
    }

    /// <summary>
    /// Builds a sculpted 3D Grasshopper / Cricket (elongated green body, raised bent hind legs, antennae, and folded wings).
    /// </summary>
    public static void AddSculptedGrasshopper(
        List<Polygon3D> list,
        Vector3 center,
        float scale,
        float animTime)
    {
        var grassCol = new SKColor(101, 163, 13); // Rich olive-green
        var grassLight = new SKColor(132, 204, 22); // Chartreuse
        var grassDark = new SKColor(63, 98, 18);
        var eyeBlack = new SKColor(15, 23, 42);

        float twitch = MathF.Sin(animTime * 5f) * 0.03f * scale;
        Vector3 up = Vector3.UnitY;
        Vector3 fwd = new Vector3(0, 0, 1);
        Vector3 right = new Vector3(1, 0, 0);

        // 1. Thorax and Elongated Abdomen
        var bodyPos = center + (up * 0.22f * scale);
        AddBox(list, bodyPos - (fwd * 0.10f * scale), new Vector3(0.24f, 0.22f, 0.72f) * scale, grassCol, grassDark);

        // Folded long wings on back
        AddBox(list, bodyPos + (up * 0.12f * scale) - (fwd * 0.18f * scale), new Vector3(0.18f, 0.06f, 0.85f) * scale, grassLight, grassCol);

        // 2. Head
        var headPos = bodyPos + (fwd * 0.38f * scale) + (up * 0.06f * scale);
        AddBox(list, headPos, new Vector3(0.22f, 0.28f, 0.22f) * scale, grassLight, grassCol);

        // Big compound eyes
        AddBox(list, headPos + (fwd * 0.08f * scale) - (right * 0.10f * scale) + (up * 0.08f * scale), new Vector3(0.08f, 0.10f, 0.08f) * scale, eyeBlack, eyeBlack);
        AddBox(list, headPos + (fwd * 0.08f * scale) + (right * 0.10f * scale) + (up * 0.08f * scale), new Vector3(0.08f, 0.10f, 0.08f) * scale, eyeBlack, eyeBlack);

        // Long Antennae
        var antL = headPos + (fwd * 0.26f * scale) - (right * 0.08f * scale) + (up * 0.22f * scale);
        var antR = headPos + (fwd * 0.26f * scale) + (right * 0.08f * scale) + (up * 0.22f * scale);
        AddBox(list, antL, new Vector3(0.03f, 0.25f, 0.18f) * scale, grassLight, grassLight);
        AddBox(list, antR, new Vector3(0.03f, 0.25f, 0.18f) * scale, grassLight, grassLight);

        // 3. Prominent Raised Hind Jumping Legs (Femur & Tibia)
        float legHop = MathF.Sin(animTime * 4f) * 0.06f * scale;
        var hindJointL = bodyPos - (fwd * 0.28f * scale) - (right * 0.22f * scale) + (up * (0.28f * scale + legHop));
        var hindJointR = bodyPos - (fwd * 0.28f * scale) + (right * 0.22f * scale) + (up * (0.28f * scale + legHop));

        // Upper femur (thick jumping muscle angled up and back)
        AddBox(list, hindJointL, new Vector3(0.08f, 0.36f, 0.32f) * scale, grassLight, grassDark);
        AddBox(list, hindJointR, new Vector3(0.08f, 0.36f, 0.32f) * scale, grassLight, grassDark);

        // Lower tibia down to ground
        var tibiaL = hindJointL - (fwd * 0.12f * scale) - (up * 0.25f * scale);
        var tibiaR = hindJointR - (fwd * 0.12f * scale) - (up * 0.25f * scale);
        AddBox(list, tibiaL, new Vector3(0.05f, 0.28f, 0.06f) * scale, grassCol, grassDark);
        AddBox(list, tibiaR, new Vector3(0.05f, 0.28f, 0.06f) * scale, grassCol, grassDark);

        // Front walking legs
        var fLegL = bodyPos + (fwd * 0.15f * scale) - (right * 0.18f * scale) - (up * 0.12f * scale);
        var fLegR = bodyPos + (fwd * 0.15f * scale) + (right * 0.18f * scale) - (up * 0.12f * scale);
        AddBox(list, fLegL, new Vector3(0.04f, 0.20f, 0.04f) * scale, grassDark, grassDark);
        AddBox(list, fLegR, new Vector3(0.04f, 0.20f, 0.04f) * scale, grassDark, grassDark);
    }

    /// <summary>
    /// Builds a sculpted 3D Dragonfly (slender azure body, 4 iridescent wings fluttering, and large compound eyes).
    /// </summary>
    public static void AddSculptedDragonfly(
        List<Polygon3D> list,
        Vector3 center,
        float scale,
        float animTime)
    {
        var bodyCyan = new SKColor(6, 182, 212); // Azure cyan
        var bodyDark = new SKColor(8, 51, 68);
        var wingCol = new SKColor(224, 242, 254); // Iridescent wings
        var eyeCol = new SKColor(14, 165, 233);

        float flutter = MathF.Sin(animTime * 28f) * 0.24f * scale;
        Vector3 up = Vector3.UnitY;
        Vector3 fwd = new Vector3(0, 0, 1);
        Vector3 right = new Vector3(1, 0, 0);

        // Slender long abdomen
        var bodyPos = center + (up * 0.40f * scale);
        AddBox(list, bodyPos - (fwd * 0.25f * scale), new Vector3(0.12f, 0.12f, 1.10f) * scale, bodyCyan, bodyDark);

        // Thorax & Head
        var thoraxPos = bodyPos + (fwd * 0.35f * scale);
        AddBox(list, thoraxPos, new Vector3(0.24f, 0.24f, 0.28f) * scale, bodyCyan, bodyCyan);
        AddBox(list, thoraxPos + (fwd * 0.18f * scale), new Vector3(0.26f, 0.22f, 0.18f) * scale, eyeCol, eyeCol, isEmissive: true);

        // 4 Large Outspread Iridescent Wings (fluttering)
        var wFwdL = thoraxPos - (right * 0.55f * scale) + (fwd * 0.08f * scale) + (up * flutter);
        var wFwdR = thoraxPos + (right * 0.55f * scale) + (fwd * 0.08f * scale) - (up * flutter);
        var wBackL = thoraxPos - (right * 0.50f * scale) - (fwd * 0.12f * scale) - (up * flutter);
        var wBackR = thoraxPos + (right * 0.50f * scale) - (fwd * 0.12f * scale) + (up * flutter);

        AddBox(list, wFwdL, new Vector3(0.85f, 0.02f, 0.22f) * scale, wingCol, wingCol, isEmissive: true);
        AddBox(list, wFwdR, new Vector3(0.85f, 0.02f, 0.22f) * scale, wingCol, wingCol, isEmissive: true);
        AddBox(list, wBackL, new Vector3(0.75f, 0.02f, 0.18f) * scale, wingCol, wingCol, isEmissive: true);
        AddBox(list, wBackR, new Vector3(0.75f, 0.02f, 0.18f) * scale, wingCol, wingCol, isEmissive: true);
    }

    /// <summary>
    /// Builds an anatomically realistic 3D frog (hunched dorsal back, wide mouth, bulging 3D eyes, folded hind thighs, and forelegs).
    /// </summary>
    public static void AddSculptedFrog(
        List<Polygon3D> list,
        Vector3 center,
        float scale,
        float breathingPhase,
        bool isGolden)
    {
        // Colors for realistic amphibian skin
        var frogDorsal = isGolden ? new SKColor(245, 158, 11) : new SKColor(34, 160, 68); // Vibrant meadow green / golden poison
        var frogDorsalDark = isGolden ? new SKColor(180, 83, 9) : new SKColor(18, 90, 38); // Dark dorsal spots
        var frogFlank = isGolden ? new SKColor(251, 191, 36) : new SKColor(52, 199, 89); // Emerald flank
        var frogBelly = isGolden ? new SKColor(254, 243, 199) : new SKColor(254, 240, 138); // Pale creamy throat/belly
        var eyeIris = isGolden ? new SKColor(17, 24, 39) : new SKColor(245, 158, 11); // Golden amphibian iris
        var eyePupil = new SKColor(8, 10, 14);

        float breath = MathF.Sin(breathingPhase * 3.5f) * 0.05f * scale;

        Vector3 forward = new Vector3(0, 0, 1);
        Vector3 right = new Vector3(1, 0, 0);
        Vector3 up = Vector3.UnitY;

        float frogLen = 1.15f * scale;
        float frogWid = 0.98f * scale;
        float frogHgt = (0.58f * scale) + breath;

        // Landmarks of the frog body
        var snoutTip = center + (forward * frogLen * 0.52f) + (up * frogHgt * 0.35f);
        var snoutBottom = center + (forward * frogLen * 0.50f) + (up * 0.06f * scale);
        var headTop = center + (forward * frogLen * 0.20f) + (up * frogHgt * 0.82f);
        var backHump = center - (forward * frogLen * 0.14f) + (up * frogHgt);
        var backRear = center - (forward * frogLen * 0.48f) + (up * frogHgt * 0.42f);
        var bellyCenter = center - (up * 0.02f);

        var flankL = center - (right * frogWid * 0.48f) + (up * frogHgt * 0.45f);
        var flankR = center + (right * frogWid * 0.48f) + (up * frogHgt * 0.45f);

        // 1. Dorsal Body & Head (Hunched back quads)
        list.Add(new Polygon3D([snoutTip, headTop, flankL], frogDorsal)
        {
            SpecularIntensity = 0.70f,
            Shininess = 28f
        });
        list.Add(new Polygon3D([snoutTip, flankR, headTop], frogDorsal)
        {
            SpecularIntensity = 0.70f,
            Shininess = 28f
        });
        list.Add(new Polygon3D([headTop, backHump, flankL], frogDorsalDark)
        {
            SpecularIntensity = 0.70f,
            Shininess = 28f
        });
        list.Add(new Polygon3D([headTop, flankR, backHump], frogDorsalDark)
        {
            SpecularIntensity = 0.70f,
            Shininess = 28f
        });
        list.Add(new Polygon3D([backHump, backRear, flankL], frogDorsal)
        {
            SpecularIntensity = 0.70f,
            Shininess = 28f
        });
        list.Add(new Polygon3D([backHump, flankR, backRear], frogDorsal)
        {
            SpecularIntensity = 0.70f,
            Shininess = 28f
        });

        // 2. Wide Amphibian Mouth & Pale Gular Throat
        list.Add(new Polygon3D([snoutTip, flankL, snoutBottom], frogFlank));
        list.Add(new Polygon3D([snoutTip, snoutBottom, flankR], frogFlank));
        list.Add(new Polygon3D([snoutBottom, flankL, bellyCenter, flankR], frogBelly)
        {
            SpecularIntensity = 0.40f
        });

        // 3. Bulging 3D Frog Eyes on top of skull
        float eyeRad = 0.16f * scale;
        var eyePosL = headTop + (forward * 0.06f * scale) - (right * 0.25f * scale) + (up * 0.12f * scale);
        var eyePosR = headTop + (forward * 0.06f * scale) + (right * 0.25f * scale) + (up * 0.12f * scale);

        AddBox(list, eyePosL, new Vector3(eyeRad * 1.35f, eyeRad * 1.35f, eyeRad * 1.35f), eyeIris, eyeIris);
        AddBox(list, eyePosR, new Vector3(eyeRad * 1.35f, eyeRad * 1.35f, eyeRad * 1.35f), eyeIris, eyeIris);

        // Horizontal slit pupils
        AddBox(list, eyePosL + (forward * 0.08f * scale), new Vector3(eyeRad * 1.05f, 0.05f * scale, 0.07f * scale), eyePupil, eyePupil);
        AddBox(list, eyePosR + (forward * 0.08f * scale), new Vector3(eyeRad * 1.05f, 0.05f * scale, 0.07f * scale), eyePupil, eyePupil);

        // Wet corneal eye glint
        AddBox(list, eyePosL + (forward * 0.07f * scale) + (up * 0.07f * scale) - (right * 0.05f * scale), new Vector3(0.04f, 0.04f, 0.04f) * scale, SKColors.White, SKColors.White, isEmissive: true);
        AddBox(list, eyePosR + (forward * 0.07f * scale) + (up * 0.07f * scale) + (right * 0.05f * scale), new Vector3(0.04f, 0.04f, 0.04f) * scale, SKColors.White, SKColors.White, isEmissive: true);

        // 4. Folded Muscular Hind Legs (Thighs pulled in tightly)
        var thighL = center - (forward * 0.35f * scale) - (right * 0.48f * scale) + (up * 0.22f * scale);
        var thighR = center - (forward * 0.35f * scale) + (right * 0.48f * scale) + (up * 0.22f * scale);
        AddBox(list, thighL, new Vector3(0.30f, 0.34f, 0.50f) * scale, frogDorsalDark, frogFlank);
        AddBox(list, thighR, new Vector3(0.30f, 0.34f, 0.50f) * scale, frogDorsalDark, frogFlank);

        // Webbed hind feet
        var footL = thighL + (forward * 0.28f * scale) - (right * 0.08f * scale) - (up * 0.15f * scale);
        var footR = thighR + (forward * 0.28f * scale) + (right * 0.08f * scale) - (up * 0.15f * scale);
        AddBox(list, footL, new Vector3(0.24f, 0.05f, 0.32f) * scale, frogFlank, frogFlank);
        AddBox(list, footR, new Vector3(0.24f, 0.05f, 0.32f) * scale, frogFlank, frogFlank);

        // 5. Front Forelegs
        var armL = center + (forward * 0.32f * scale) - (right * 0.38f * scale) + (up * 0.14f * scale);
        var armR = center + (forward * 0.32f * scale) + (right * 0.38f * scale) + (up * 0.14f * scale);
        AddBox(list, armL, new Vector3(0.14f, 0.26f, 0.14f) * scale, frogFlank, frogFlank);
        AddBox(list, armR, new Vector3(0.14f, 0.26f, 0.14f) * scale, frogFlank, frogFlank);
    }

    /// <summary>
    /// Builds a dense cluster of golden ripe wheat stalks with swaying stems, nodding ears (spikelets), and golden awn bristles.
    /// </summary>
    public static void AddWheatCluster(
        List<Polygon3D> list,
        Vector3 basePos,
        float scale,
        float windTime,
        float phaseOffset)
    {
        int stalkCount = 3;
        for (int s = 0; s < stalkCount; s++)
        {
            float angleOffset = s * 2.1f;
            float radialDist = 0.14f * s * scale;
            var stalkBase = basePos + new Vector3(MathF.Cos(angleOffset) * radialDist, 0, MathF.Sin(angleOffset) * radialDist);

            float individualPhase = phaseOffset + (s * 0.85f);
            float sway = MathF.Sin(windTime + individualPhase);
            float swayX = sway * 0.22f * scale;
            float swayZ = MathF.Cos((windTime * 0.8f) + individualPhase) * 0.15f * scale;

            float stalkHeight = (1.15f + (MathF.Sin(stalkBase.X * 5.7f + stalkBase.Z * 3.3f) * 0.25f)) * scale;
            float midY = stalkHeight * 0.52f;

            var p0 = stalkBase;
            var pMid = stalkBase + new Vector3(swayX * 0.38f, midY, swayZ * 0.38f);
            var pTop = stalkBase + new Vector3(swayX, stalkHeight, swayZ);

            var stemCol = new SKColor(205, 172, 92); // Sunlit amber-gold stem
            var stemDark = new SKColor(165, 130, 58);
            var earBright = new SKColor(254, 226, 140); // Golden ripe wheat ear
            var earCol = new SKColor(238, 198, 108);

            // Stem blade
            float stemWidth = 0.045f * scale;
            Vector3 perp = new Vector3(-swayZ, 0, swayX);
            if (perp.LengthSquared() < 0.0001f) perp = Vector3.UnitX;
            else perp = Vector3.Normalize(perp);

            var wVec = perp * stemWidth;
            list.Add(new Polygon3D([p0 - wVec, p0 + wVec, pMid + wVec, pMid - wVec], stemCol));
            list.Add(new Polygon3D([pMid - wVec, pMid + wVec, pTop + wVec, pTop - wVec], stemCol));

            // Lower wheat foliage leaf
            var leafTip = pMid + (perp * 0.35f * scale) + new Vector3(swayX * 0.5f, -0.20f * scale, swayZ * 0.5f);
            list.Add(new Polygon3D([pMid - wVec, pMid + wVec, leafTip], stemDark));

            // Wheat Head / Ear (Spikelets)
            int grains = 6;
            float earLength = 0.55f * scale;
            Vector3 earDir = Vector3.Normalize(pTop - pMid);
            Vector3 earPerp = Vector3.Normalize(Vector3.Cross(earDir, Vector3.UnitY));
            if (earPerp.LengthSquared() < 0.001f) earPerp = Vector3.UnitX;

            for (int g = 0; g < grains; g++)
            {
                float t = (float)g / grains;
                var gNode = pTop + (earDir * t * earLength);
                float grainW = (0.095f + (MathF.Sin(t * MathF.PI) * 0.055f)) * scale;

                var side = (g % 2 == 0) ? 1.0f : -1.0f;
                var g0 = gNode - (earPerp * grainW * 0.5f);
                var g1 = gNode + (earPerp * grainW * 0.5f);
                var gTip = gNode + (earDir * 0.14f * scale) + (earPerp * side * grainW * 1.35f);

                var gColor = (g % 2 == 0) ? earBright : earCol;
                list.Add(new Polygon3D([g0, g1, gTip], gColor)
                {
                    SpecularIntensity = 0.45f,
                    Shininess = 18f
                });

                // Wheat awn (golden bristle)
                var awnTip = gTip + (earDir * 0.28f * scale) + (earPerp * side * 0.12f * scale);
                list.Add(new Polygon3D([gTip, gTip + (earPerp * side * 0.02f * scale), awnTip], earBright));
            }
        }
    }

    /// <summary>
    /// Builds realistic meadow flowers (chamomile/daisies/wild field poppies) dotting the agricultural field.
    /// </summary>
    public static void AddFieldFlower(
        List<Polygon3D> list,
        Vector3 basePos,
        float scale,
        float windSway,
        bool isPoppy = false)
    {
        var stemCol = new SKColor(72, 120, 48);
        var petalCol = isPoppy ? new SKColor(239, 68, 68) : new SKColor(254, 254, 254);
        var centerCol = isPoppy ? new SKColor(30, 20, 20) : new SKColor(245, 158, 11);

        float height = 0.50f * scale;
        var top = basePos + new Vector3(windSway * 0.14f, height, windSway * 0.10f);

        // Stalk
        list.Add(new Polygon3D([
            basePos - new Vector3(0.025f, 0, 0),
            basePos + new Vector3(0.025f, 0, 0),
            top + new Vector3(0.025f, 0, 0),
            top - new Vector3(0.025f, 0, 0)
        ], stemCol));

        // Petals disc
        float discR = (isPoppy ? 0.18f : 0.15f) * scale;
        int petalCount = isPoppy ? 4 : 6;
        for (int p = 0; p < petalCount; p++)
        {
            float a = p * MathF.PI * 2f / petalCount;
            var dir = new Vector3(MathF.Cos(a), 0.18f, MathF.Sin(a));
            var pEnd = top + (dir * discR);
            var pLeft = top + (new Vector3(-dir.Z, 0, dir.X) * 0.05f * scale);
            var pRight = top + (new Vector3(dir.Z, 0, -dir.X) * 0.05f * scale);

            list.Add(new Polygon3D([pLeft, pRight, pEnd], petalCol)
            {
                SpecularIntensity = 0.35f
            });
        }

        // Center boss
        AddBox(list, top + new Vector3(0, 0.035f * scale, 0), new Vector3(0.08f, 0.06f, 0.08f) * scale, centerCol, centerCol);
    }

    /// <summary>
    /// Builds an anatomically realistic viper/python snake head with multi-faceted head scale plates, heat-sensing pit organs, 3D serpentine eyes, and flicking forked tongue.
    /// </summary>
    public static void AddSculptedSnakeHead(
        List<Polygon3D> list,
        Vector3 headPos,
        Vector3 forwardVec,
        Vector3 rightVec,
        float tongueProgress,
        SKColor dorsalColor,
        SKColor flankColor,
        SKColor bellyColor,
        SKColor diamondColor,
        SKColor lipScaleColor)
    {
        Vector3 upVec = Vector3.UnitY;
        float headLength = 1.45f;
        float headWidth = 1.22f;
        float headHeight = 0.72f;

        // Anatomical landmark nodes for real viper skull geometry
        var snoutTop = headPos + (forwardVec * headLength * 0.58f) + (upVec * headHeight * 0.16f);
        var snoutTip = headPos + (forwardVec * headLength * 0.68f) - (upVec * headHeight * 0.02f);
        var snoutBottom = headPos + (forwardVec * headLength * 0.54f) - (upVec * headHeight * 0.34f);

        // Canthus rostralis & Nasal pits (heat pits)
        var canthusLeft = headPos + (forwardVec * headLength * 0.44f) - (rightVec * headWidth * 0.28f) + (upVec * headHeight * 0.22f);
        var canthusRight = headPos + (forwardVec * headLength * 0.44f) + (rightVec * headWidth * 0.28f) + (upVec * headHeight * 0.22f);
        var pitLeft = headPos + (forwardVec * headLength * 0.46f) - (rightVec * headWidth * 0.32f) - (upVec * headHeight * 0.02f);
        var pitRight = headPos + (forwardVec * headLength * 0.46f) + (rightVec * headWidth * 0.32f) - (upVec * headHeight * 0.02f);

        // Supraocular brow crests (prominent above eye orbits)
        var browLeft = headPos + (forwardVec * headLength * 0.18f) - (rightVec * headWidth * 0.52f) + (upVec * headHeight * 0.46f);
        var browRight = headPos + (forwardVec * headLength * 0.18f) + (rightVec * headWidth * 0.52f) + (upVec * headHeight * 0.46f);

        // Venom gland temporal swellings (wide triangular rear skull)
        var glandLeft = headPos - (forwardVec * headLength * 0.22f) - (rightVec * headWidth * 0.62f) + (upVec * headHeight * 0.24f);
        var glandRight = headPos - (forwardVec * headLength * 0.22f) + (rightVec * headWidth * 0.62f) + (upVec * headHeight * 0.24f);

        // Mandible / jaw corners & supralabial lip line
        var lipMidLeft = headPos + (forwardVec * headLength * 0.12f) - (rightVec * headWidth * 0.54f) - (upVec * headHeight * 0.15f);
        var lipMidRight = headPos + (forwardVec * headLength * 0.12f) + (rightVec * headWidth * 0.54f) - (upVec * headHeight * 0.15f);
        var jawLeft = headPos - (forwardVec * headLength * 0.30f) - (rightVec * headWidth * 0.58f) - (upVec * headHeight * 0.26f);
        var jawRight = headPos - (forwardVec * headLength * 0.30f) + (rightVec * headWidth * 0.58f) - (upVec * headHeight * 0.26f);

        // Crown & nape transition
        var frontalMid = headPos + (forwardVec * headLength * 0.22f) + (upVec * headHeight * 0.40f);
        var crownMid = headPos + (upVec * headHeight * 0.45f);
        var crownBack = headPos - (forwardVec * headLength * 0.52f) + (upVec * headHeight * 0.38f);
        var throatBack = headPos - (forwardVec * headLength * 0.52f) - (upVec * headHeight * 0.30f);

        // 1. Dorsal Crown Plates (Frontal, Prefrontals, Parietals with V-markings)
        list.Add(new Polygon3D([snoutTop, canthusLeft, frontalMid], dorsalColor)
        {
            SpecularIntensity = 0.85f,
            Shininess = 44f
        });
        list.Add(new Polygon3D([snoutTop, frontalMid, canthusRight], dorsalColor)
        {
            SpecularIntensity = 0.85f,
            Shininess = 44f
        });
        list.Add(new Polygon3D([frontalMid, canthusLeft, browLeft, crownMid], diamondColor)
        {
            SpecularIntensity = 0.90f,
            Shininess = 48f
        });
        list.Add(new Polygon3D([frontalMid, crownMid, browRight, canthusRight], diamondColor)
        {
            SpecularIntensity = 0.90f,
            Shininess = 48f
        });
        list.Add(new Polygon3D([crownMid, browLeft, glandLeft, crownBack], dorsalColor)
        {
            SpecularIntensity = 0.85f,
            Shininess = 44f
        });
        list.Add(new Polygon3D([crownMid, crownBack, glandRight, browRight], dorsalColor)
        {
            SpecularIntensity = 0.85f,
            Shininess = 44f
        });

        // 2. Rostral Snout & Nostril Scales
        list.Add(new Polygon3D([snoutTop, snoutTip, canthusLeft], dorsalColor));
        list.Add(new Polygon3D([snoutTop, canthusRight, snoutTip], dorsalColor));
        list.Add(new Polygon3D([snoutTip, snoutBottom, pitLeft, canthusLeft], flankColor));
        list.Add(new Polygon3D([snoutTip, canthusRight, pitRight, snoutBottom], flankColor));

        // 3. Supralabial Lip Scales
        list.Add(new Polygon3D([canthusLeft, pitLeft, lipMidLeft, browLeft], lipScaleColor));
        list.Add(new Polygon3D([canthusRight, browRight, lipMidRight, pitRight], lipScaleColor));
        list.Add(new Polygon3D([browLeft, lipMidLeft, jawLeft, glandLeft], flankColor));
        list.Add(new Polygon3D([browRight, glandRight, jawRight, lipMidRight], flankColor));
        list.Add(new Polygon3D([glandLeft, jawLeft, throatBack, crownBack], dorsalColor));
        list.Add(new Polygon3D([glandRight, crownBack, throatBack, jawRight], dorsalColor));

        // 4. Pale Ventral Belly & Infralabials (Lower Jaw)
        list.Add(new Polygon3D([snoutBottom, pitLeft, lipMidLeft, jawLeft, throatBack, jawRight, lipMidRight, pitRight], bellyColor)
        {
            SpecularIntensity = 0.35f,
            Shininess = 16f
        });

        // 5. Realistic 3D Serpentine Eyes with Amber Iris, Vertical Pupil Slit & Wet Corneal Specularity
        float eyeRadius = 0.18f;
        var leftEyeCenter = browLeft + (forwardVec * 0.10f) - (rightVec * 0.06f) - (upVec * 0.08f);
        var rightEyeCenter = browRight + (forwardVec * 0.10f) + (rightVec * 0.06f) - (upVec * 0.08f);

        var goldAmberIris = new SKColor(250, 175, 20); // Bright golden reptile iris
        var pupilBlack = new SKColor(8, 10, 14); // Deep black vertical slit pupil
        var wetCornealGlint = new SKColor(255, 255, 255);

        // Eye Bulge Sphere
        AddBox(list, leftEyeCenter, new Vector3(eyeRadius * 1.18f, eyeRadius * 1.18f, eyeRadius * 1.18f), goldAmberIris, goldAmberIris);
        AddBox(list, rightEyeCenter, new Vector3(eyeRadius * 1.18f, eyeRadius * 1.18f, eyeRadius * 1.18f), goldAmberIris, goldAmberIris);

        // Vertical Slit Pupils
        AddBox(list, leftEyeCenter - (rightVec * eyeRadius * 0.55f), new Vector3(0.04f, eyeRadius * 1.05f, 0.085f), pupilBlack, pupilBlack);
        AddBox(list, rightEyeCenter + (rightVec * eyeRadius * 0.55f), new Vector3(0.04f, eyeRadius * 1.05f, 0.085f), pupilBlack, pupilBlack);

        // Wet Cornea Glint
        AddBox(list, leftEyeCenter - (rightVec * eyeRadius * 0.48f) + (upVec * 0.06f) + (forwardVec * 0.06f), new Vector3(0.035f, 0.035f, 0.035f), wetCornealGlint, wetCornealGlint, isEmissive: true);
        AddBox(list, rightEyeCenter + (rightVec * eyeRadius * 0.48f) + (upVec * 0.06f) + (forwardVec * 0.06f), new Vector3(0.035f, 0.035f, 0.035f), wetCornealGlint, wetCornealGlint, isEmissive: true);

        // 6. Heat-Sensing Pits & Nostril Cavities
        var pitDark = new SKColor(12, 22, 16);
        AddBox(list, pitLeft + (forwardVec * 0.02f), new Vector3(0.05f, 0.045f, 0.05f), pitDark, pitDark);
        AddBox(list, pitRight + (forwardVec * 0.02f), new Vector3(0.05f, 0.045f, 0.05f), pitDark, pitDark);

        // 7. Dynamic Flicking Forked Tongue
        float tongueLen = 1.05f * MathF.Sin(tongueProgress * MathF.PI);
        if (tongueLen > 0.04f)
        {
            float tongueCurl = MathF.Sin(tongueProgress * MathF.PI * 2f) * 0.12f;
            var tongueBase = snoutBottom + (forwardVec * 0.06f) + (upVec * 0.06f);
            var tongueFork = tongueBase + (forwardVec * tongueLen * 0.68f) + (upVec * tongueCurl);
            var tipL = tongueFork + (forwardVec * tongueLen * 0.32f) - (rightVec * 0.15f) + (upVec * tongueCurl * 1.5f);
            var tipR = tongueFork + (forwardVec * tongueLen * 0.32f) + (rightVec * 0.15f) + (upVec * tongueCurl * 1.5f);
            var deepPinkTongue = new SKColor(235, 35, 80);

            // Stalk
            list.Add(new Polygon3D([
                tongueBase - (rightVec * 0.048f),
                tongueBase + (rightVec * 0.048f),
                tongueFork + (rightVec * 0.035f),
                tongueFork - (rightVec * 0.035f)
            ], deepPinkTongue, isEmissive: true));

            // Forked tips
            list.Add(new Polygon3D([tongueFork, tongueFork + (rightVec * 0.035f), tipR], deepPinkTongue, isEmissive: true));
            list.Add(new Polygon3D([tongueFork - (rightVec * 0.035f), tongueFork, tipL], deepPinkTongue, isEmissive: true));
        }
    }
}
