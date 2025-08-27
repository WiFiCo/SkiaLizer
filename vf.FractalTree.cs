using SkiaSharp;
using System;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawFractalTree(SKCanvas canvas, int width, int height)
        {
            float level = reactiveLevel; // 0..1
            int dynDepth = (int)(7 + level * 6 + beatPulse * 4);
            if (dynDepth < 6) dynDepth = 6; if (dynDepth > 16) dynDepth = 16;
            currentTreeDepth = dynDepth;

            canvas.Translate(width / 2, height);
            float amplitude = isSilent ? 0f : (5f * level + 12f * beatPulse + 8f * level * beatPulse);
            float sway = (float)Math.Sin(treePhase * 0.05f) * amplitude;
            DrawBranch(canvas, 0, -height / 3f, -90 + sway, dynDepth, level, 0);
        }

        private void DrawBranch(SKCanvas canvas, float x, float y, float angle, int depth, float intensity, int branchId)
        {
            if (depth <= 0) return;

            float depthRatio = depth / (float)currentTreeDepth; // 0..1
            float baseLen = (0.02f + intensity * 0.08f) * canvas.DeviceClipBounds.Height;
            float growthBurst = 1f + beatPulse * 2.0f + intensity * 0.5f; // more length on beats
            float length = baseLen * (0.5f + 0.5f * depthRatio) * growthBurst + (float)random.Next(-10, 10);

            float branchAngle = 20f + intensity * 40f + (float)random.Next(-15, 15);
            branchAngle += (float)Math.Sin(treePhase * 0.1f + branchId) * intensity * 12f;

            float hue = (colorHueBase + branchId * 12 + random.Next(0, 20) + intensity * 60f) % 360f;
            byte sat = (byte)Math.Clamp(70 + (int)(intensity * 30) + (int)(beatPulse * 30), 0, 100);
            byte val = (byte)Math.Clamp(75 + (int)(intensity * 25) + (int)(beatPulse * 40), 0, 100); // brighter on beats

            float x2 = x + (float)(Math.Cos(angle * Math.PI / 180) * length);
            float y2 = y + (float)(Math.Sin(angle * Math.PI / 180) * length);

            using SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = Math.Max(1f, depth * 1.2f * (0.7f + intensity * 0.4f + beatPulse * 0.6f)), // thicker on beats
                Color = SKColor.FromHsv(hue, sat, val)
            };

            canvas.DrawLine(x, y, x2, y2, paint);

            DrawBranch(canvas, x2, y2, angle - branchAngle, depth - 1, intensity * 0.98f, branchId + 1);
            DrawBranch(canvas, x2, y2, angle + branchAngle, depth - 1, intensity * 0.98f, branchId + 1);

            // Extra branch with stronger beat influence
            double extraProb = 0.08 + 0.30 * intensity + 0.40 * Math.Min(1.0, beatPulse);
            if (depth > 2 && !isSilent && random.NextDouble() < extraProb)
            {
                float extraAngle = angle + (float)random.Next(-45, 45);
                DrawBranch(canvas, x2, y2, extraAngle, depth - 2, intensity * 0.95f, branchId + 2);
            }
        }
    }
}
