using SkiaSharp;
using System.Numerics;
using System;
using System.Linq;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawNeonTunnel(SKCanvas canvas, int width, int height)
        {
            float[] localSpectrum;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); }
            for (int i = 0; i < localSpectrum.Length; i++)
            {
                float v = localSpectrum[i] * spectrumGain;
                localSpectrum[i] = (float)Math.Log10(1f + v * 9f);
            }
            float level = localSpectrum.Average();
            float centroid = 0f, total = 0f; for (int i = 0; i < localSpectrum.Length; i++) { centroid += localSpectrum[i] * i; total += localSpectrum[i]; }
            float centroidNorm = (total > 0) ? centroid / total / Math.Max(1, localSpectrum.Length) : 0.5f;

            // Fixed FOV + adaptive speed
            float fov = 60f;
            float scale = (float)(height / (2 * Math.Tan(fov / 2 * Math.PI / 180)));
            float speed = 0.8f + level * 2.6f + beatPulse * 4.2f;
            tunnelPhase += speed * 2.0f;

            // Color and paints from palette
            float colorPosition = (centroidNorm + tunnelPhase * 0.01f) % 1.0f;
            SKColor tunnelColor = GetPaletteColor(colorPosition);
            using SKPaint glow = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 3f, Color = tunnelColor.WithAlpha(90), MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f) };
            using SKPaint line = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1.4f, Color = tunnelColor };

            // Vertex wormhole: polar grid deformed in Z with twist
            int radial = 22;   // around circle
            int depthSegs = 28; // along tunnel
            float radiusBase = 140f;
            float twist = 0.1f + level * 0.6f;
            float ringSpacing = 28f;

            // Draw depth lines (rings) using small polylines
            for (int j = 0; j < depthSegs; j++)
            {
                float z = j * ringSpacing - (tunnelPhase % ringSpacing) * ringSpacing;
                float radius = radiusBase + j * 10f + (float)Math.Sin(j * 0.3f + tunnelPhase * 0.05f) * 10f;
                SKPath ring = new SKPath();
                for (int i = 0; i <= radial; i++)
                {
                    float t = i / (float)radial;
                    float ang = (float)(t * 2 * Math.PI + j * twist);
                    Vector2 p = ProjectSafe(new Vector3((float)Math.Cos(ang) * radius, (float)Math.Sin(ang) * radius, z), scale, width, height);
                    if (i == 0) ring.MoveTo(p.X, p.Y); else ring.LineTo(p.X, p.Y);
                }
                canvas.DrawPath(ring, glow);
                canvas.DrawPath(ring, line);
            }

            // Draw radial lines
            int spokes = 24;
            for (int i = 0; i < spokes; i++)
            {
                float t = i / (float)spokes;
                float ang0 = (float)(t * 2 * Math.PI);
                SKPath spoke = new SKPath();
                for (int j = 0; j < depthSegs; j++)
                {
                    float z = j * ringSpacing - (tunnelPhase % ringSpacing) * ringSpacing;
                    float radius = radiusBase + j * 10f + (float)Math.Sin(j * 0.3f + tunnelPhase * 0.05f) * 10f;
                    float ang = ang0 + j * twist;
                    Vector2 p = ProjectSafe(new Vector3((float)Math.Cos(ang) * radius, (float)Math.Sin(ang) * radius, z), scale, width, height);
                    if (j == 0) spoke.MoveTo(p.X, p.Y); else spoke.LineTo(p.X, p.Y);
                }
                canvas.DrawPath(spoke, line);
            }
        }
    }
}
