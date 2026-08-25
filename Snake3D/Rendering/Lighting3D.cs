using System.Numerics;
using SkiaSharp;

namespace Snake3D.Rendering;

/// <summary>
/// Directional and ambient 3D lighting calculator.
/// </summary>
public sealed class Lighting3D
{
    public Vector3 LightDirection { get; set; } = Vector3.Normalize(new Vector3(-0.45f, 0.85f, -0.6f));
    public SKColor LightColor { get; set; } = new(255, 250, 240);
    public float AmbientIntensity { get; set; } = 0.38f;
    public float DiffuseIntensity { get; set; } = 0.62f;

    public SKColor CalculateShading(Polygon3D polygon, Vector3 cameraPosition)
    {
        if (polygon.IsEmissive)
            return polygon.BaseColor;

        var center = polygon.CalculateCenter();
        var normal = polygon.Normal;

        // Diffuse lighting: N dot L
        float ndotl = Math.Max(0f, Vector3.Dot(normal, LightDirection));
        float totalDiffuse = AmbientIntensity + (DiffuseIntensity * ndotl);

        // Specular highlight: Blinn-Phong
        var viewDir = Vector3.Normalize(cameraPosition - center);
        var halfVec = Vector3.Normalize(LightDirection + viewDir);
        float ndoth = Math.Max(0f, Vector3.Dot(normal, halfVec));
        float specular = polygon.SpecularIntensity * MathF.Pow(ndoth, polygon.Shininess);

        byte r = (byte)Math.Clamp((polygon.BaseColor.Red * totalDiffuse) + (LightColor.Red * specular), 0, 255);
        byte g = (byte)Math.Clamp((polygon.BaseColor.Green * totalDiffuse) + (LightColor.Green * specular), 0, 255);
        byte b = (byte)Math.Clamp((polygon.BaseColor.Blue * totalDiffuse) + (LightColor.Blue * specular), 0, 255);

        return new SKColor(r, g, b, polygon.BaseColor.Alpha);
    }
}
