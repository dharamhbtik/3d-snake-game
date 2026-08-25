using System.Numerics;
using SkiaSharp;

namespace Snake3D.Rendering;

/// <summary>
/// 3D perspective camera providing view and projection transformations with screen-space mapping.
/// </summary>
public sealed class Camera3D
{
    public Vector3 Position { get; set; } = new(0, 22f, -22f);
    public Vector3 Target { get; set; } = new(0, 0, 1.5f);
    public Vector3 Up { get; set; } = Vector3.UnitY;
    public float FieldOfViewDegrees { get; set; } = 48f;
    public float NearPlane { get; set; } = 0.5f;
    public float FarPlane { get; set; } = 150f;

    public Matrix4x4 ViewMatrix { get; private set; }
    public Matrix4x4 ProjectionMatrix { get; private set; }
    public Matrix4x4 ViewProjectionMatrix { get; private set; }

    public float ViewportWidth { get; private set; }
    public float ViewportHeight { get; private set; }

    public void UpdateMatrices(float width, float height)
    {
        ViewportWidth = Math.Max(1f, width);
        ViewportHeight = Math.Max(1f, height);
        float aspectRatio = ViewportWidth / ViewportHeight;

        ViewMatrix = Matrix4x4.CreateLookAt(Position, Target, Up);
        float fovRad = FieldOfViewDegrees * (MathF.PI / 180f);
        ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(fovRad, aspectRatio, NearPlane, FarPlane);
        ViewProjectionMatrix = ViewMatrix * ProjectionMatrix;
    }

    /// <summary>
    /// Transforms a 3D world-space point into 2D screen coordinates with depth.
    /// Returns false if the point is behind the camera near plane.
    /// </summary>
    public bool WorldToScreen(Vector3 worldPoint, out SKPoint screenPoint, out float depth)
    {
        Vector4 clip = Vector4.Transform(new Vector4(worldPoint, 1.0f), ViewProjectionMatrix);

        if (clip.W <= 0.001f)
        {
            screenPoint = SKPoint.Empty;
            depth = float.MaxValue;
            return false;
        }

        float invW = 1.0f / clip.W;
        float ndcX = clip.X * invW;
        float ndcY = clip.Y * invW;
        float ndcZ = clip.Z * invW;

        // Screen mapping: NDC [-1, 1] to pixel coords [0, Width], [0, Height] (Y flipped for top-left origin)
        float sx = (ndcX + 1.0f) * 0.5f * ViewportWidth;
        float sy = (1.0f - ndcY) * 0.5f * ViewportHeight;

        screenPoint = new SKPoint(sx, sy);
        depth = clip.W; // linear depth for sorting
        return true;
    }
}
