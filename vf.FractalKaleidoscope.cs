using SkiaSharp;
using System;
using System.Collections.Generic;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private float fractalCx = 0f;
        private float fractalCy = 0f;
        private float fractalZoomSmooth = 1f;

        private void DrawFractalKaleidoscope(SKCanvas canvas, int width, int height)
        {
            int s = System.Math.Min(width, height);
            int tex = System.Math.Max(384, s / 2);
            using var bmp = new SKBitmap(tex, tex, true);
            using var off = new SKCanvas(bmp);

            // audio-smoothed parameters driving color and motion
            float level = reactiveLevel;
            float centroidSum = 0f, total = 0f; for (int i = 0; i < spectrum.Length; i++) { centroidSum += spectrum[i] * i; total += spectrum[i]; }
            float centroid = (total > 0) ? centroidSum / total / System.Math.Max(1, spectrum.Length) : 0.5f;
            float hueBase = (colorHueBase + centroid * 360f) % 360f;

            // soft radial background so tiling never shows black seams
            var bgTop = SKColor.FromHsv((hueBase + 20f) % 360f, 30, 20).WithAlpha(255);
            var bgBot = SKColor.FromHsv((hueBase + 200f) % 360f, 30, 10).WithAlpha(255);
            using (var bg = new SKPaint { Shader = SKShader.CreateRadialGradient(new SKPoint(tex / 2f, tex / 2f), tex * 0.7f, new[] { bgTop, bgBot }, new float[] { 0f, 1f }, SKShaderTileMode.Clamp) })
            {
                off.DrawRect(new SKRect(0, 0, tex, tex), bg);
            }

            // layer 1: branching fractals with adaptive branching and length
            int seeds = 5 + (int)(level * 6f);
            float baseLen = tex * (0.08f + level * 0.1f + beatPulse * 0.14f);
            var rnd = random;
            for (int i = 0; i < seeds; i++)
            {
                float ang = (float)(i * (System.Math.PI * 2) / seeds) + treePhase * 0.01f;
                float cx = tex / 2f + (float)System.Math.Cos(ang) * tex * 0.08f + fractalCx * tex * 0.06f;
                float cy = tex / 2f + (float)System.Math.Sin(ang) * tex * 0.08f + fractalCy * tex * 0.06f;
                float startAngle = ang + (float)(rnd.NextDouble() - 0.5) * 0.8f;
                int depth = 5 + (int)(level * 4f);
                DrawBranchFractal(off, cx, cy, startAngle, depth, baseLen, hueBase, 0);
            }

            // layer 2: superformula rosettes with audio-driven symmetry
            int shapes = 3 + (int)(level * 4f);
            for (int i = 0; i < shapes; i++)
            {
                float rot = (float)(i * 2 * System.Math.PI / shapes) + treePhase * 0.005f + fractalCx * 0.5f;
                float radius = tex * (0.18f + i * 0.06f);
                float m = 4f + level * 8f + (float)(rnd.NextDouble() * 2 - 1) * 1.5f;
                float n1 = 0.3f + (float)rnd.NextDouble() * 0.8f;
                float n2 = 0.2f + level * 0.6f;
                float n3 = 0.2f + highBandLevel * 0.6f;
                var hue = (hueBase + i * 22f + beatPulse * 50f) % 360f;
                using var p = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1.6f, Color = SKColor.FromHsv(hue, 70, 100).WithAlpha(180) };
                using var glow = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 4f, Color = SKColor.FromHsv(hue, 60, 80).WithAlpha(50), MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f) };
                using var path = SuperformulaPath(tex / 2f, tex / 2f, radius, m, n1, n2, n3, rot, 360);
                off.DrawPath(path, glow);
                off.DrawPath(path, p);
            }

            // kaleidoscope: wedge clipping with mirrored copies filling the viewport
            canvas.Save();
            canvas.Translate(width / 2f, height / 2f);
            int wedges = 16;
            float angleStep = 360f / wedges;
            float rotW = (treePhase * 0.1f + beatPulse * 8f) % 360f;
            float zoom = 1.0f + fractalZoomSmooth * 0.35f;
            canvas.Scale(zoom, zoom);

            float R = System.Math.Max(width, height);
            using var wedge = new SKPath();
            wedge.MoveTo(0, 0);
            wedge.LineTo(R, 0);
            wedge.ArcTo(new SKRect(-R, -R, R, R), 0, angleStep, false);
            wedge.Close();

            var srcRect = new SKRect(0, 0, tex, tex);
            var dstRect = new SKRect(-tex / 2f, -tex / 2f, tex / 2f, tex / 2f);

            for (int i = 0; i < wedges; i++)
            {
                canvas.Save();
                canvas.RotateDegrees(rotW + i * angleStep);
                canvas.ClipPath(wedge, SKClipOperation.Intersect, true);
                if ((i & 1) == 1) canvas.Scale(-1, 1);
                canvas.DrawBitmap(bmp, srcRect, dstRect);
                canvas.Restore();
            }
            canvas.Restore();
        }

        private void DrawBranchFractal(SKCanvas c, float x, float y, float angle, int depth, float length, float hueBase, int id)
        {
            if (depth <= 0 || length < 2f) return;
            float x2 = x + (float)System.Math.Cos(angle) * length;
            float y2 = y + (float)System.Math.Sin(angle) * length;
            var hue = (hueBase + id * 11f + length * 0.2f) % 360f;
            using var glow = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = depth * 1.6f, Color = SKColor.FromHsv(hue, 70, 80).WithAlpha(50), MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 4f) };
            using var p = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = depth * 0.9f, Color = SKColor.FromHsv(hue, 80, 100).WithAlpha(180) };
            c.DrawLine(x, y, x2, y2, glow);
            c.DrawLine(x, y, x2, y2, p);

            // two main branches plus an occasional third, with audio-driven spread
            float jitter = 0.3f + highBandLevel * 0.7f;
            DrawBranchFractal(c, x2, y2, angle - (0.6f + jitter * 0.4f), depth - 1, length * (0.68f + reactiveLevel * 0.05f), hueBase, id + 1);
            DrawBranchFractal(c, x2, y2, angle + (0.6f + jitter * 0.4f), depth - 1, length * (0.68f + reactiveLevel * 0.05f), hueBase, id + 2);
            if (random.NextDouble() < 0.25 + beatPulse * 0.3f)
            {
                DrawBranchFractal(c, x2, y2, angle + (float)(random.NextDouble() - 0.5) * 1.2f, depth - 2, length * 0.55f, hueBase, id + 3);
            }
        }

        private SKPath SuperformulaPath(float cx, float cy, float radius, float m, float n1, float n2, float n3, float rotation, int steps)
        {
            SKPath path = new SKPath();
            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;
                float ang = t * (float)(System.Math.PI * 2) + rotation;
                float r = SuperformulaR(ang, m, n1, n2, n3);
                float x = cx + (float)System.Math.Cos(ang) * r * radius;
                float y = cy + (float)System.Math.Sin(ang) * r * radius;
                if (i == 0) path.MoveTo(x, y); else path.LineTo(x, y);
            }
            path.Close();
            return path;
        }

        private float SuperformulaR(float ang, float m, float n1, float n2, float n3)
        {
            // johan gielis' superformula implementation for rosette shapes
            float a = 1f, b = 1f;
            float term1 = (float)System.Math.Pow(System.Math.Abs((1f / a) * System.Math.Cos(m * ang / 4f)), n2);
            float term2 = (float)System.Math.Pow(System.Math.Abs((1f / b) * System.Math.Sin(m * ang / 4f)), n3);
            float r = (float)System.Math.Pow(term1 + term2, -1f / n1);
            if (float.IsInfinity(r) || float.IsNaN(r)) r = 0f;
            return r;
        }
    }
}
