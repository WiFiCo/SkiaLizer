using SkiaSharp;
using System.Numerics;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawStarfield(SKCanvas canvas, int width, int height)
        {
            int target = 800;
            while (stars.Count < target)
            {
                stars.Add(new Star
                {
                    Pos = new Vector3((float)(random.NextDouble() * 2 - 1) * 200f, (float)(random.NextDouble() * 2 - 1) * 200f, (float)(random.NextDouble()) * 400f),
                    Speed = 0.5f + (float)random.NextDouble() * 1.5f,
                    Hue = (float)random.NextDouble() * 360f,
                    Phase = (float)random.NextDouble() * 6.28318f
                });
            }

            // Smoothed speed based on audio
            float loud = reactiveLevel * 0.6f + lowBandLevel * 0.6f + (beatPulse > 0.6f ? 0.3f : 0f);
            float targetSpeed = 1f + loud * 8f;
            starSpeedSmooth = starSpeedSmooth * 0.85f + targetSpeed * 0.15f;

            float lr = 0f; int count = 0;
            var wave = waveformQueue.ToArray();
            for (int i = 0; i + 1 < wave.Length; i += 2) { lr += wave[i] - wave[i + 1]; count++; }
            float stereo = count > 0 ? ClampF(lr / count, -1f, 1f) : 0f;

            float fov = 60f; float scale = (float)(height / (2 * System.Math.Tan(fov / 2 * System.Math.PI / 180)));

            using SKPaint paint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };

            for (int i = 0; i < stars.Count; i++)
            {
                var s = stars[i];
                s.Pos.Z -= s.Speed * starSpeedSmooth;
                s.Pos.X += stereo * 0.8f * s.Speed;
                s.Phase += 0.08f + starSpeedSmooth * 0.05f;
                if (s.Pos.Z < 10f)
                {
                    s.Pos.Z = 400f;
                    s.Pos.X = (float)(random.NextDouble() * 2 - 1) * 200f;
                    s.Pos.Y = (float)(random.NextDouble() * 2 - 1) * 200f;
                }
                stars[i] = s;

                Vector2 p = ProjectSafe(s.Pos, scale, width, height);
                float depth = System.Math.Max(10f, s.Pos.Z);
                byte alpha = (byte)System.Math.Clamp(80 + starSpeedSmooth * 20f + System.MathF.Sin(s.Phase) * 60f, 30, 255);
                // Use palette color based on star's position and movement
                float colorPosition = ((s.Hue / 360f) + (colorHueBase / 360f) + (s.Pos.Z / 400f)) % 1.0f;
                SKColor starColor = GetPaletteColor(colorPosition);
                paint.Color = starColor.WithAlpha(alpha);
                paint.StrokeWidth = System.Math.Max(1f, 6f / depth);
                Vector2 tail = ProjectSafe(new Vector3(s.Pos.X, s.Pos.Y, s.Pos.Z + 18f * starSpeedSmooth), scale, width, height);
                canvas.DrawLine(p.X, p.Y, tail.X, tail.Y, paint);
            }
        }
    }
}
