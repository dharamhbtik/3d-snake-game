using System.Numerics;
using SkiaSharp;

namespace Snake3D.Rendering;

/// <summary>
/// Represents a flat 3D polygon (triangle or quad) with lighting properties and depth-sorting support.
/// </summary>
public sealed class Polygon3D
{
    public Vector3[] Vertices { get; set; }
    public Vector3 Normal { get; set; }
    public SKColor BaseColor { get; set; }
    public SKColor? StrokeColor { get; set; }
    public float StrokeWidth { get; set; } = 1.0f;
    public float SpecularIntensity { get; set; } = 0.35f;
    public float Shininess { get; set; } = 16.0f;
    public bool IsEmissive { get; set; }
    public float Depth { get; set; }

    public Polygon3D(Vector3[] vertices, SKColor baseColor, SKColor? strokeColor = null, bool isEmissive = false)
    {
        Vertices = vertices;
        BaseColor = baseColor;
        StrokeColor = strokeColor;
        IsEmissive = isEmissive;

        if (vertices.Length >= 3)
        {
            var v0 = vertices[0];
            var v1 = vertices[1];
            var v2 = vertices[2];
            var edge1 = v1 - v0;
            var edge2 = v2 - v0;
            Normal = Vector3.Normalize(Vector3.Cross(edge1, edge2));
        }
    }

    public Vector3 CalculateCenter()
    {
        var sum = Vector3.Zero;
        for (int i = 0; i < Vertices.Length; i++)
        {
            sum += Vertices[i];
        }
        return sum / Vertices.Length;
    }
}
