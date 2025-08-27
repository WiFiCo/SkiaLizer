using SkiaSharp;
using System.Numerics;
using System;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawBoids(SKCanvas canvas, int width, int height)
        {
            // Sensitivity factor from levels
            float sens = reactiveLevel * 0.4f + lowBandLevel * 0.7f + highBandLevel * 0.3f + Math.Min(1f, beatPulse) * 0.6f;
            if (sens > 1.5f) sens = 1.5f;

            // Baseline confetti count
            int baseline = 120 + (int)(sens * 300f);
            if (baseline > 900) baseline = 900;

            // Spawn bursts on beat
            if (beatPulse > 0.7f)
            {
                int burst = 30 + (int)(sens * 60f);
                SpawnConfettiBurst(width, height, burst);
            }

            // Maintain pool size around baseline
            if (boids.Count < baseline)
            {
                SpawnConfettiBurst(width, height, baseline - boids.Count);
            }
            else if (boids.Count > baseline)
            {
                boids.RemoveRange(0, boids.Count - baseline);
            }

            Vector2 gravity = new Vector2(0, 0.12f + sens * 0.6f);
            Vector2 center = new Vector2(width / 2f, height / 2f);

            // Update
            for (int i = 0; i < boids.Count; i++)
            {
                var p = boids[i];

                // Attraction to bass point (circular motion)
                float t = treePhase * 0.01f;
                Vector2 bassPoint = new Vector2(width / 2f + MathF.Cos(t) * 220f, height / 2f + MathF.Sin(t * 1.4f) * 140f);
                Vector2 toBass = bassPoint - p.Position;
                float dist = MathF.Max(20f, toBass.Length());
                Vector2 dir = toBass / dist;

                Vector2 accel = dir * (0.4f + lowBandLevel * 7f + Math.Min(1f, beatPulse) * 10f);
                accel += new Vector2((float)(random.NextDouble() - 0.5), (float)(random.NextDouble() - 0.5)) * (0.6f + highBandLevel * 4.5f);
                accel += gravity * 0.5f;

                p.Velocity += accel * 0.08f;
                float maxSpeed = 1.8f + sens * 8f;
                float spd = p.Velocity.Length();
                if (spd > maxSpeed) p.Velocity *= (maxSpeed / spd);

                p.Position += p.Velocity;

                // Lifetime and fade
                p.Life -= 1f;
                if (p.Life <= 0)
                {
                    // respawn softly near center
                    RespawnParticle(ref p, width, height, center);
                }

                // wrap softly
                if (p.Position.X < -20) p.Position.X = width + 20;
                if (p.Position.X > width + 20) p.Position.X = -20;
                if (p.Position.Y < -20) p.Position.Y = height + 20;
                if (p.Position.Y > height + 20) p.Position.Y = -20;

                // Color shift
                p.Hue = (p.Hue + highBandLevel * 8f + Math.Min(1f, beatPulse) * 16f) % 360f;
                boids[i] = p;
            }

            // Render confetti as glowing quads/lines
            using SKPaint paint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round, BlendMode = SKBlendMode.Screen };
            int stride = Math.Max(1, boids.Count / 1000);
            for (int i = 0; i < boids.Count; i += stride)
            {
                var p = boids[i];
                float lifeT = p.Life / Math.Max(1f, p.MaxLife);
                byte alpha = (byte)Math.Clamp(40 + lifeT * 160 + sens * 60, 0, 255);
                paint.Color = SKColor.FromHsv(p.Hue, (byte)Math.Min(100, 70 + sens * 20f), 100).WithAlpha(alpha);
                float thickness = 1.0f + p.Size * (0.6f + sens * 0.7f);
                paint.StrokeWidth = thickness;
                Vector2 tail = p.Position - Vector2.Normalize(p.Velocity == Vector2.Zero ? new Vector2(1, 0) : p.Velocity) * (6f + sens * 28f);
                canvas.DrawLine(p.Position.X, p.Position.Y, tail.X, tail.Y, paint);
            }
        }

        private void SpawnConfettiBurst(int width, int height, int count)
        {
            Vector2 center = new Vector2(width / 2f, height / 2f);
            for (int k = 0; k < count; k++)
            {
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float speed = 0.8f + (float)random.NextDouble() * 3.0f;
                var p = new Particle
                {
                    Position = center + new Vector2((float)(random.NextDouble() - 0.5) * 60f, (float)(random.NextDouble() - 0.5) * 60f),
                    Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed,
                    Size = 0.8f + (float)random.NextDouble() * 1.6f,
                    Hue = (colorHueBase + random.Next(0, 360)) % 360f,
                    Life = 120f + (float)random.NextDouble() * 180f, // 2-5 seconds at 60 fps
                    MaxLife = 180f
                };
                boids.Add(p);
            }
        }

        private void RespawnParticle(ref Particle p, int width, int height, Vector2 center)
        {
            float angle = (float)(random.NextDouble() * Math.PI * 2);
            float speed = 0.8f + (float)random.NextDouble() * 3.0f;
            p.Position = center + new Vector2((float)(random.NextDouble() - 0.5) * 60f, (float)(random.NextDouble() - 0.5) * 60f);
            p.Velocity = new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * speed;
            p.Size = 0.8f + (float)random.NextDouble() * 1.6f;
            p.Hue = (colorHueBase + random.Next(0, 360)) % 360f;
            p.Life = 120f + (float)random.NextDouble() * 180f;
            p.MaxLife = 180f;
        }
    }
}
