using SkiaSharp;
using System.Numerics;
using System;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void Draw3DPipes(SKCanvas canvas, int width, int height)
        {
            float fov = 60f;
            float scale = (float)(height / (2 * Math.Tan(fov / 2 * Math.PI / 180)));

            foreach (var system in pipeSystems)
            {
                for (int i = 0; i < system.Count; i++)
                {
                    var segment = system[i];
                    Vector2 p1 = ProjectSafe(segment.Start, scale, width, height);
                    Vector2 p2 = ProjectSafe(segment.End, scale, width, height);

                    float depth1 = Math.Max(10f, segment.Start.Z + 120f);
                    float depth2 = Math.Max(10f, segment.End.Z + 120f);
                    float avgDepth = (depth1 + depth2) / 2f;

                    using SKPaint paint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        StrokeWidth = Math.Max(1.6f, 24f / avgDepth),
                        Color = segment.Color,
                        StrokeCap = SKStrokeCap.Round
                    };

                    canvas.DrawLine(p1.X, p1.Y, p2.X, p2.Y, paint);

                    using SKPaint jointPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Fill,
                        Color = segment.Color.WithAlpha(220)
                    };
                    float radius = Math.Max(1.2f, 12f / avgDepth);
                    canvas.DrawCircle(p2.X, p2.Y, radius, jointPaint);
                }
            }
        }
    }
}
