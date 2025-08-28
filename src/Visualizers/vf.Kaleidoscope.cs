using SkiaSharp;
using System;
using System.Linq;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawKaleidoscope(SKCanvas canvas, int width, int height)
        {
            int s = System.Math.Min(width, height);
            int tex = System.Math.Max(256, s / 2);
            using var srcBmp = new SKBitmap(tex, tex, true);
            using var src = new SKCanvas(srcBmp);

            // Process spectrum with adaptive gain and compression for sensitivity
            float[] localSpectrum;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); }
            for (int i = 0; i < localSpectrum.Length; i++)
            {
                float v = localSpectrum[i] * spectrumGain;
                localSpectrum[i] = (float)System.Math.Log10(1f + v * 9f);
            }
            float level = localSpectrum.Average();
            float centroid = 0f, total = 0f; for (int i = 0; i < localSpectrum.Length; i++) { centroid += localSpectrum[i] * i; total += localSpectrum[i]; }
            float centroidNorm = (total > 0) ? centroid / total / System.Math.Max(1, localSpectrum.Length) : 0.5f;

            // Build source texture (radial arcs + dots) reacting to audio
            src.Clear(SKColors.Black);
            float cx = tex / 2f, cy = tex / 2f;
            float maxR = tex * 0.48f;
            float hueBase = (colorHueBase + centroidNorm * 360f) % 360f;

            // Radial spokes
            using (var spokePaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
            {
                int spokes = 96;
                for (int i = 0; i < spokes; i++)
                {
                    float t = i / (float)spokes;
                    int idx = (int)(t * (localSpectrum.Length - 1));
                    float mag = localSpectrum[idx];
                    float r = maxR * (0.2f + mag * 0.8f);
                    float ang = (t + treePhase * 0.002f) * System.MathF.PI * 2f;
                    float x2 = cx + System.MathF.Cos(ang) * r;
                    float y2 = cy + System.MathF.Sin(ang) * r;
                    float colorPos = ((hueBase + t * 360f) / 360f) % 1.0f;
                    SKColor spokeColor = GetPaletteColor(colorPos);
                    // Apply magnitude-based brightness
                    float brightnessMult = (50 + mag * 200) / 100f;
                    spokePaint.Color = new SKColor((byte)(spokeColor.Red * brightnessMult), (byte)(spokeColor.Green * brightnessMult), (byte)(spokeColor.Blue * brightnessMult));
                    src.DrawLine(cx, cy, x2, y2, spokePaint);
                }
            }
            // Concentric rings
            using (var ringPaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = SKColors.White.WithAlpha(40) })
            {
                int rings = 10;
                for (int i = 0; i < rings; i++)
                {
                    float t = i / (float)(rings - 1);
                    float r = maxR * (0.1f + 0.9f * t * (0.6f + 0.4f * level));
                    src.DrawCircle(cx, cy, r, ringPaint);
                }
            }
            // Dots on peaks
            using (var dotPaint = new SKPaint { Style = SKPaintStyle.Fill })
            {
                int dots = 80;
                for (int i = 0; i < dots; i++)
                {
                    float t = i / (float)dots;
                    int idx = (int)(t * (localSpectrum.Length - 1));
                    float mag = localSpectrum[idx];
                    if (mag < 0.05f) continue;
                    float r = maxR * (0.2f + mag * 0.8f);
                    float ang = (t * 2f + treePhase * 0.004f) * System.MathF.PI * 2f;
                    float x = cx + System.MathF.Cos(ang) * r;
                    float y = cy + System.MathF.Sin(ang) * r;
                    float dotColorPos = ((hueBase + 180f * mag) / 360f) % 1.0f;
                    dotPaint.Color = GetPaletteColor(dotColorPos).WithAlpha(160);
                    src.DrawCircle(x, y, 2f + mag * 6f, dotPaint);
                }
            }

            // Classic kaleidoscope: wedge mirroring
            int segments = 16; // number of mirrored wedges
            float wedgeAngle = 360f / segments;
            float innerZoom = 1f + level * 0.3f + beatPulse * 0.6f;
            float rotate = (treePhase * (0.05f + level * 0.2f) + beatPulse * 10f) % 360f;

            canvas.Save();
            canvas.Translate(width / 2f, height / 2f);
            canvas.Scale(innerZoom, innerZoom);

            // Pre-build a wedge clip path (sector 0..wedgeAngle)
            float R = System.Math.Max(width, height);
            using var wedge = new SKPath();
            wedge.MoveTo(0, 0);
            wedge.LineTo(R, 0);
            wedge.ArcTo(new SKRect(-R, -R, R, R), 0, wedgeAngle, false);
            wedge.Close();

            for (int i = 0; i < segments; i++)
            {
                canvas.Save();
                canvas.RotateDegrees(rotate + i * wedgeAngle);
                canvas.ClipPath(wedge, SKClipOperation.Intersect, true);
                if ((i & 1) == 1)
                {
                    canvas.Scale(-1, 1);
                }
                var srcRect = new SKRect(0, 0, tex, tex);
                var dstRect = new SKRect(-tex / 2f, -tex / 2f, tex / 2f, tex / 2f);
                canvas.DrawBitmap(srcBmp, srcRect, dstRect);
                canvas.Restore();
            }
            canvas.Restore();
        }
    }
}
