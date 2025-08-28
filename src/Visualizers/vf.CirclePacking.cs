using SkiaSharp;
using System.Numerics;
using System;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawCirclePacking(SKCanvas canvas, int width, int height)
        {
            float sens = reactiveLevel * 0.5f + lowBandLevel * 0.4f + highBandLevel * 0.6f + Math.Min(1f, beatPulse) * 0.6f;
            if (sens > 1.5f) sens = 1.5f;

            int bands = Math.Min(64, spectrum.Length);
            while (circles.Count < bands)
            {
                Vector2 cpos = new Vector2(width * (float)random.NextDouble(), height * (float)random.NextDouble());
                var dest = RandomDestination(width, height);
                circles.Add(new Circle {
                    Center = cpos,
                    Radius = 6f,
                    Target = 10f,
                    Color = GetPaletteColor(circles.Count / (float)bands),
                    Velocity = RandomVel(0.5f),
                    Alpha = 200f,
                    Life = 180f,
                    MaxLife = 180f,
                    Destination = dest,
                    Arrived = false,
                    AliveFrames = 0,
                    MinAliveFrames = 30 + random.Next(0, 90),
                    PopBias = (float)random.NextDouble() * 0.8f + 0.2f
                });
            }

            float[] localSpectrum; float[] localPeaks;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); localPeaks = (float[])peakSpectrum.Clone(); }

            for (int i = 0; i < circles.Count; i++)
            {
                int idx = (int)(i / (float)circles.Count * (localSpectrum.Length - 1));
                float energy = localSpectrum[idx];
                float peak = localPeaks[idx];
                var c = circles[i];

                float bump = 1f + (c.Arrived && ShouldPop(peak, energy, sens, c) ? 1.2f : 0f) + Math.Min(1f, beatPulse) * 0.4f;
                float target = 10f + energy * 260f * bump;
                c.Target = target;
                c.Radius = c.Radius * 0.86f + target * 0.14f;

                // Move toward destination until arrived
                Vector2 toDest = c.Destination - c.Center;
                float dist = toDest.Length();
                if (dist > 2f)
                {
                    Vector2 dir = toDest / MathF.Max(1f, dist);
                    c.Velocity += dir * (0.05f + sens * 0.08f);
                }
                else
                {
                    c.Arrived = true;
                }

                c.Center += c.Velocity * (0.6f + sens * 1.1f);
                c.Velocity *= 0.985f;

                if (c.Center.X < -30) c.Center.X = width + 30; if (c.Center.X > width + 30) c.Center.X = -30;
                if (c.Center.Y < -30) c.Center.Y = height + 30; if (c.Center.Y > height + 30) c.Center.Y = -30;

                // Age and burnout
                c.AliveFrames++;
                c.Life -= 1f + energy * 6f + Math.Min(1f, beatPulse) * 4f;
                bool allowPop = c.Arrived && c.AliveFrames >= c.MinAliveFrames;
                if ((allowPop && ShouldPop(peak, energy, sens, c)) || c.Life <= 0)
                {
                    RespawnCircle(ref c, width, height);
                }

                float colorPos = (i / (float)circles.Count + colorHueBase / 360f) % 1.0f;
                c.Color = GetPaletteColor(colorPos);
                c.Alpha = Math.Max(40f, 255f * (c.Life / Math.Max(1f, c.MaxLife)));

                circles[i] = c;
            }

            // separation
            for (int iter = 0; iter < 1; iter++)
            {
                for (int i = 0; i < circles.Count; i++)
                {
                    for (int j = i + 1; j < circles.Count; j++)
                    {
                        var a = circles[i]; var b = circles[j];
                        Vector2 d = b.Center - a.Center;
                        float dist = d.Length();
                        float minDist = a.Radius + b.Radius + 4f;
                        if (dist > 0 && dist < minDist)
                        {
                            Vector2 dir = d / dist;
                            float push = (minDist - dist) * 0.4f;
                            a.Center -= dir * push;
                            b.Center += dir * push;
                            circles[i] = a; circles[j] = b;
                        }
                    }
                }
            }

            using SKPaint fill = new SKPaint { Style = SKPaintStyle.Fill, BlendMode = SKBlendMode.Screen, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 8f) };
            using SKPaint stroke = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1.6f, Color = SKColors.White.WithAlpha(60) };
            foreach (var c in circles)
            {
                fill.Color = c.Color.WithAlpha((byte)Math.Min(255, c.Alpha));
                canvas.DrawCircle(c.Center.X, c.Center.Y, c.Radius, fill);
                canvas.DrawCircle(c.Center.X, c.Center.Y, c.Radius, stroke);
            }
        }

        private bool ShouldPop(float peak, float energy, float sens, Circle c)
        {
            // Randomized threshold with personal bias and audio influence
            float audioFactor = (peak - energy) * 4f + sens * 0.6f + (float)random.NextDouble() * 0.6f;
            float threshold = 1.2f * (1f - c.PopBias * 0.5f);
            return audioFactor > threshold;
        }

        private Vector2 RandomVel(float s)
        {
            float ang = (float)(random.NextDouble() * Math.PI * 2);
            return new Vector2(MathF.Cos(ang), MathF.Sin(ang)) * (s + (float)random.NextDouble() * s);
        }

        private Vector2 RandomDestination(int width, int height)
        {
            return new Vector2((float)random.NextDouble() * width, (float)random.NextDouble() * height);
        }

        private void RespawnCircle(ref Circle c, int width, int height)
        {
            int edge = random.Next(0, 4);
            switch (edge)
            {
                case 0: c.Center = new Vector2(-20, (float)random.NextDouble() * height); break;
                case 1: c.Center = new Vector2(width + 20, (float)random.NextDouble() * height); break;
                case 2: c.Center = new Vector2((float)random.NextDouble() * width, -20); break;
                case 3: c.Center = new Vector2((float)random.NextDouble() * width, height + 20); break;
            }
            c.Velocity = RandomVel(1.0f);
            c.Radius = 8f + (float)random.NextDouble() * 14f;
            c.Life = 180f + (float)random.NextDouble() * 240f;
            c.MaxLife = c.Life;
            c.Alpha = 220f;
            c.Destination = RandomDestination(width, height);
            c.Arrived = false;
            c.AliveFrames = 0;
            c.MinAliveFrames = 30 + random.Next(0, 90);
            c.PopBias = (float)random.NextDouble() * 0.8f + 0.2f;
        }
    }
}
