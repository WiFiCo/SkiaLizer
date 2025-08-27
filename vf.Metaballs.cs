using SkiaSharp;
using System.Numerics;
using System;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawMetaballs(SKCanvas canvas, int width, int height)
        {
            // Derive a stronger sensitivity factor from available audio metrics
            float glow = reactiveLevel * 0.5f + lowBandLevel * 0.7f + highBandLevel * 0.3f + Math.Min(1f, beatPulse) * 0.6f;
            if (glow > 1.5f) glow = 1.5f;

            // Adaptive, but restrained count driven by glow
            int target = 14 + (int)(glow * 30f);
            if (target < 10) target = 10;
            if (target > 60) target = 60;
            while (metaballs.Count < target)
            {
                metaballs.Add(new Ball
                {
                    Position = new Vector2(random.Next(0, width), random.Next(0, height)),
                    Velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * 0.9f,
                    Radius = 14f + (float)random.NextDouble() * 18f,
                    Color = SKColor.FromHsv((colorHueBase + random.Next(0, 40)) % 360f, 60, 100)
                });
            }
            if (metaballs.Count > target) metaballs.RemoveRange(0, metaballs.Count - target);

            // Soft vignette background for glow focus
            using (var vignette = new SKPaint
            {
                Shader = SKShader.CreateRadialGradient(new SKPoint(width / 2f, height / 2f), Math.Max(width, height) / 1.2f,
                    new[] { SKColors.Black.WithAlpha(0), SKColors.Black.WithAlpha(180) }, new float[] { 0.6f, 1f }, SKShaderTileMode.Clamp)
            })
            {
                canvas.DrawRect(0, 0, width, height, vignette);
            }

            // Update motion (slower base, but responsive to glow)
            float speedBoost = 0.6f + glow * 1.8f;
            for (int i = 0; i < metaballs.Count; i++)
            {
                var b = metaballs[i];
                b.Position += b.Velocity * speedBoost;
                Vector2 center = new Vector2(width / 2f, height / 2f);
                Vector2 toCenter = center - b.Position;
                b.Velocity += Vector2.Normalize(toCenter) * 0.0025f;
                b.Velocity *= 0.994f;

                if (b.Position.X < -60) b.Position.X = width + 60;
                if (b.Position.X > width + 60) b.Position.X = -60;
                if (b.Position.Y < -60) b.Position.Y = height + 60;
                if (b.Position.Y > height + 60) b.Position.Y = -60;

                metaballs[i] = b;
            }

            // Render as soft glows (screen blend), adaptive alpha and radius
            using SKPaint glowPaint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                BlendMode = SKBlendMode.Screen,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 12f)
            };

            float alphaBase = 70f + glow * 150f; // up to ~220
            if (alphaBase > 220f) alphaBase = 220f;

            // Cool-toned palette bias (cyan/blue/purple)
            float hueOffset = (colorHueBase * 0.3f) % 360f;

            for (int i = 0; i < metaballs.Count; i++)
            {
                var b = metaballs[i];
                float drawRadius = MathF.Min(MathF.Max(10f, b.Radius * (0.9f + glow * 1.2f)), MathF.Max(width, height) * 0.2f);

                float coolHue = (hueOffset + (i * 12f)) % 360f;
                SKColor baseColor = SKColor.FromHsv(coolHue, (byte)Math.Min(100, 50 + glow * 30f), 100);
                glowPaint.Color = baseColor.WithAlpha((byte)alphaBase);

                canvas.DrawCircle(b.Position.X, b.Position.Y, drawRadius, glowPaint);
            }

            using SKPaint outline = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.White.WithAlpha((byte)Math.Min(60, 20 + glow * 30f)), StrokeWidth = 1f };
            for (int i = 0; i < metaballs.Count; i++)
            {
                var b = metaballs[i];
                float drawRadius = MathF.Min(MathF.Max(10f, b.Radius * (0.9f + glow * 1.2f)), MathF.Max(width, height) * 0.2f);
                canvas.DrawCircle(b.Position.X, b.Position.Y, drawRadius, outline);
            }
        }
    }
}
