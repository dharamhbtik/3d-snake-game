using System.Numerics;
using SkiaSharp;

namespace Snake3D.Rendering;

/// <summary>
/// Directional sun and dual-hemisphere ambient 3D lighting calculator for realistic outdoor farmland environments.
/// </summary>
public sealed class Lighting3D
{
    public Vector3 SunDirection { get; set; } = Vector3.Normalize(new Vector3(-0.55f, 0.78f, -0.42f));
    public SKColor SunColor { get; set; } = new(255, 245, 220); // Warm sunlight
    public SKColor SkyAmbientColor { get; set; } = new(175, 210, 245); // Cool blue sky ambient
    public SKColor GroundAmbientColor { get; set; } = new(70, 50, 32); // Warm loamy earth bounce
    public float AmbientIntensity { get; set; } = 0.42f;
    public float SunIntensity { get; set; } = 0.68f;

    public SKColor CalculateShading(Polygon3D polygon, Vector3 cameraPosition)
    {
        if (polygon.IsEmissive)
            return polygon.BaseColor;

        var center = polygon.CalculateCenter();
        var normal = polygon.Normal;

        // Hemispherical ambient based on surface normal Y (facing sky vs ground)
        float hemiFactor = (normal.Y + 1.0f) * 0.5f; // 0 = facing down (earth), 1 = facing up (sky)
        float ambR = (SkyAmbientColor.Red * hemiFactor) + (GroundAmbientColor.Red * (1f - hemiFactor));
        float ambG = (SkyAmbientColor.Green * hemiFactor) + (GroundAmbientColor.Green * (1f - hemiFactor));
        float ambB = (SkyAmbientColor.Blue * hemiFactor) + (GroundAmbientColor.Blue * (1f - hemiFactor));

        // Direct sunlight (Lambertian diffuse)
        float ndotl = Math.Max(0f, Vector3.Dot(normal, SunDirection));
        float directR = SunColor.Red * ndotl * SunIntensity;
        float directG = SunColor.Green * ndotl * SunIntensity;
        float directB = SunColor.Blue * ndotl * SunIntensity;

        float lightR = (ambR * AmbientIntensity / 255f) + (directR / 255f);
        float lightG = (ambG * AmbientIntensity / 255f) + (directG / 255f);
        float lightB = (ambB * AmbientIntensity / 255f) + (directB / 255f);

        // Specular highlight: Blinn-Phong (glossy scales / apples / crops)
        float specular = 0f;
        if (ndotl > 0f && polygon.SpecularIntensity > 0.01f)
        {
            var viewDir = Vector3.Normalize(cameraPosition - center);
            var halfVec = Vector3.Normalize(SunDirection + viewDir);
            float ndoth = Math.Max(0f, Vector3.Dot(normal, halfVec));
            specular = polygon.SpecularIntensity * MathF.Pow(ndoth, polygon.Shininess);
        }

        byte r = (byte)Math.Clamp((polygon.BaseColor.Red * lightR) + (SunColor.Red * specular), 0, 255);
        byte g = (byte)Math.Clamp((polygon.BaseColor.Green * lightG) + (SunColor.Green * specular), 0, 255);
        byte b = (byte)Math.Clamp((polygon.BaseColor.Blue * lightB) + (SunColor.Blue * specular), 0, 255);

        return new SKColor(r, g, b, polygon.BaseColor.Alpha);
    }
}
