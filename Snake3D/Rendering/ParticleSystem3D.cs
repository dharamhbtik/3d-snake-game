using System.Numerics;
using SkiaSharp;

namespace Snake3D.Rendering;

public sealed class Particle3D
{
    public Vector3 Position;
    public Vector3 Velocity;
    public SKColor Color;
    public float Size;
    public float Age;
    public float MaxAge;

    public bool IsAlive => Age < MaxAge;

    public void Update(float dt)
    {
        Age += dt;
        Position += Velocity * dt;
        Velocity.Y -= 9.8f * dt; // Gravity
        Velocity.X *= MathF.Pow(0.92f, dt * 60f); // Air resistance
        Velocity.Z *= MathF.Pow(0.92f, dt * 60f);

        // Floor bounce
        if (Position.Y < 0.05f)
        {
            Position.Y = 0.05f;
            Velocity.Y = -Velocity.Y * 0.45f;
        }
    }
}

public sealed class ParticleSystem3D
{
    private readonly List<Particle3D> _particles = new(256);
    private readonly Random _rng = Random.Shared;

    public void SpawnBurst(Vector3 origin, SKColor color, int count = 28, float speed = 4.5f)
    {
        for (int i = 0; i < count; i++)
        {
            float theta = _rng.NextSingle() * MathF.PI * 2f;
            float phi = _rng.NextSingle() * MathF.PI * 0.5f; // upwards hemisphere
            float s = speed * (0.6f + (_rng.NextSingle() * 0.8f));

            var velocity = new Vector3(
                MathF.Cos(theta) * MathF.Cos(phi) * s,
                MathF.Sin(phi) * s + (_rng.NextSingle() * 2f),
                MathF.Sin(theta) * MathF.Cos(phi) * s
            );

            _particles.Add(new Particle3D
            {
                Position = origin + new Vector3(
                    (_rng.NextSingle() - 0.5f) * 0.4f,
                    _rng.NextSingle() * 0.4f,
                    (_rng.NextSingle() - 0.5f) * 0.4f),
                Velocity = velocity,
                Color = color,
                Size = 0.28f + (_rng.NextSingle() * 0.22f),
                Age = 0f,
                MaxAge = 0.7f + (_rng.NextSingle() * 0.6f)
            });
        }
    }

    public void Update(float dt)
    {
        for (int i = _particles.Count - 1; i >= 0; i--)
        {
            var p = _particles[i];
            p.Update(dt);
            if (!p.IsAlive)
            {
                _particles.RemoveAt(i);
            }
        }
    }

    public void Render(SKCanvas canvas, Camera3D camera)
    {
        if (_particles.Count == 0)
            return;

        using var paint = new SKPaint
        {
            IsAntialias = true,
            Style = SKPaintStyle.Fill
        };

        foreach (var p in _particles)
        {
            if (camera.WorldToScreen(p.Position, out var screenPt, out float depth))
            {
                float lifeNorm = 1.0f - (p.Age / p.MaxAge);
                byte alpha = (byte)(lifeNorm * 255);
                paint.Color = p.Color.WithAlpha(alpha);

                float screenRadius = Math.Max(1.5f, (p.Size * camera.ViewportHeight * 0.35f) / depth);
                canvas.DrawCircle(screenPt, screenRadius, paint);
            }
        }
    }

    public void Clear() => _particles.Clear();
}
