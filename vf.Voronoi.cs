using SkiaSharp;
using System.Numerics;
using System.Collections.Generic;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawVoronoi(SKCanvas canvas, int width, int height)
        {
            int sites = 64;
            while (voronoiSites.Count < sites)
            {
                voronoiSites.Add(new Vector2((float)random.NextDouble() * width, (float)random.NextDouble() * height));
            }

            float[] localSpectrum;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); }

            for (int i = 0; i < voronoiSites.Count; i++)
            {
                int idx = (int)(i / (float)voronoiSites.Count * (localSpectrum.Length - 1));
                float e = System.MathF.Min(1.5f, localSpectrum[idx] * 8f);
                Vector2 jitter = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * (2f + e * 8f + beatPulse * 6f);
                var p = voronoiSites[i] + jitter;
                if (p.X < 0) p.X = width + p.X; if (p.X > width) p.X -= width;
                if (p.Y < 0) p.Y = height + p.Y; if (p.Y > height) p.Y -= height;
                voronoiSites[i] = p;
            }

            int k = 3;
            using SKPaint edge = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.White.WithAlpha((byte)(60 + beatPulse * 120)), BlendMode = SKBlendMode.Screen };
            using SKPaint fill = new SKPaint { Style = SKPaintStyle.Fill, BlendMode = SKBlendMode.Plus };

            for (int i = 0; i < voronoiSites.Count; i++)
            {
                var a = voronoiSites[i];
                List<(float d, int j)> neigh = new List<(float, int)>();
                for (int j = 0; j < voronoiSites.Count; j++) if (j != i)
                {
                    float dx = voronoiSites[j].X - a.X; float dy = voronoiSites[j].Y - a.Y; float d = dx * dx + dy * dy;
                    neigh.Add((d, j));
                }
                neigh.Sort((x, y) => x.d.CompareTo(y.d));
                float hue = (colorHueBase + i * 5f) % 360f;
                fill.Color = SKColor.FromHsv(hue, 70, 40).WithAlpha(60);
                canvas.DrawCircle(a.X, a.Y, 6f + beatPulse * 8f, fill);
                for (int n = 0; n < System.Math.Min(k, neigh.Count); n++)
                {
                    var b = voronoiSites[neigh[n].j];
                    canvas.DrawLine(a.X, a.Y, b.X, b.Y, edge);
                }
            }
        }
    }
}
