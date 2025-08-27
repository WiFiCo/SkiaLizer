using SkiaSharp;
using System.Numerics;
using System;
using System.Linq;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawAudioTerrain(SKCanvas canvas, int width, int height)
        {
            float[] localSpectrum;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); }

            for (int i = 0; i < localSpectrum.Length; i++)
            {
                float v = localSpectrum[i] * spectrumGain;
                localSpectrum[i] = (float)System.Math.Log10(1f + v * 9f);
            }

            float level = localSpectrum.Average();
            float centroid = 0f, total = 0f;
            for (int i = 0; i < localSpectrum.Length; i++) { centroid += localSpectrum[i] * i; total += localSpectrum[i]; }
            float centroidNorm = (total > 0) ? centroid / total / System.Math.Max(1, localSpectrum.Length) : 0.5f;

            float heightScale = (0.6f + level * 1.8f + beatPulse * 2.2f);
            int cols = TerrainCols;
            for (int c = 0; c < TerrainCols; c++)
            {
                float f = c / (float)System.Math.Max(1, cols - 1);
                float logf = f * f;
                int idx = (int)(logf * (localSpectrum.Length - 1));
                idx = System.Math.Clamp(idx, 0, localSpectrum.Length - 1);
                float v = localSpectrum[idx] * heightScale;
                terrainHeights[0, c] = v;
            }
            for (int c = 1; c < TerrainCols - 1; c++)
            {
                terrainHeights[0, c] = (terrainHeights[0, c - 1] + terrainHeights[0, c] * 2f + terrainHeights[0, c + 1]) / 4f;
            }

            for (int r = TerrainRows - 1; r > 0; r--)
                for (int c = 0; c < TerrainCols; c++)
                    terrainHeights[r, c] = terrainHeights[r - 1, c] * 0.992f;

            float fov = 60f;
            float scale = (float)(height / (2 * System.Math.Tan(fov / 2 * System.Math.PI / 180)));
            Vector3 cam = new Vector3(0f, 90f, 180f);

            float sx = 10f, sz = 10f, sy = 80f;

            // skybox mountains overlay at the top
            DrawSkyboxMountains(canvas, width, height, centroidNorm, level);

            float hue = (colorHueBase + centroidNorm * 240f) % 360f;
            byte val = (byte)System.Math.Clamp(70 + (int)(level * 30) + (int)(beatPulse * 40), 0, 100);
            using SKPaint glow = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 3.5f, Color = SKColor.FromHsv(hue, 80, val).WithAlpha(60) };
            using SKPaint paint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1.6f, Color = SKColor.FromHsv(hue, 80, val) };

            for (int r = 0; r < TerrainRows - 1; r++)
            {
                SKPath path = new SKPath();
                for (int c = 0; c < TerrainCols; c++)
                {
                    Vector3 world = new Vector3((c - TerrainCols / 2) * sx - cam.X, (-terrainHeights[r, c] * sy) - cam.Y, (r * sz) - cam.Z);
                    Vector2 p = ProjectSafe(world, scale, width, height);
                    if (c == 0) path.MoveTo(p.X, p.Y); else path.LineTo(p.X, p.Y);
                }
                canvas.DrawPath(path, glow);
                canvas.DrawPath(path, paint);
            }
            for (int c = 0; c < TerrainCols; c += 2)
            {
                SKPath path = new SKPath();
                for (int r = 0; r < TerrainRows; r++)
                {
                    Vector3 world = new Vector3((c - TerrainCols / 2) * sx - cam.X, (-terrainHeights[r, c] * sy) - cam.Y, (r * sz) - cam.Z);
                    Vector2 p = ProjectSafe(world, scale, width, height);
                    if (r == 0) path.MoveTo(p.X, p.Y); else path.LineTo(p.X, p.Y);
                }
                canvas.DrawPath(path, glow);
                canvas.DrawPath(path, paint);
            }
        }

        private void DrawSkyboxMountains(SKCanvas canvas, int width, int height, float centroidNorm, float level)
        {
            int points = 64;
            float baseline = height * 0.38f; // horizon height
            float ampBase = height * 0.18f;   // spiky baseline
            float ampAudio = height * (0.05f + level * 0.12f + beatPulse * 0.18f);
            float amp = ampBase + ampAudio;

            SKPath path = new SKPath();
            float x0 = 0;
            float y0 = baseline - SpikeNoise(0, level) * amp;
            if (y0 > baseline) y0 = baseline;
            path.MoveTo(x0, y0);
            for (int i = 1; i < points; i++)
            {
                float t = i / (float)(points - 1);
                float x = t * width;
                float spike = SpikeNoise(i, level);
                float y = baseline - spike * amp;
                if (y > baseline) y = baseline;
                path.LineTo(x, y);
            }
            path.LineTo(width, 0);
            path.LineTo(0, 0);
            path.Close();

            SKColor c1 = SKColor.FromHsv((colorHueBase + centroidNorm * 200f) % 360f, 60, 70).WithAlpha(120);
            SKColor c0 = SKColor.FromHsv((colorHueBase + centroidNorm * 200f + 20f) % 360f, 40, 30).WithAlpha(0);
            using SKPaint fill = new SKPaint
            {
                Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, baseline), new[] { c0, c1 }, new float[] { 0, 1 }, SKShaderTileMode.Clamp)
            };
            canvas.DrawPath(path, fill);
        }

        private float SpikeNoise(int i, float level)
        {
            // deterministic pseudo-noise spikes with audio wobble
            int n = (i * 1103515245 + 12345) & 0x7fffffff;
            float r1 = ((n % 1000) / 1000f);
            float r2 = (((n / 1000) % 1000) / 1000f);
            float baseSpike = MathF.Pow(r1, 0.35f);
            float wobble = MathF.Sin((i + treePhase * 0.15f) * 0.4f) * 0.2f + (r2 - 0.5f) * 0.1f;
            float audio = level * 0.5f + beatPulse * 0.8f;
            float spike = baseSpike + wobble + audio;
            if (spike < 0f) spike = 0f;
            if (spike > 1.4f) spike = 1.4f;
            return spike;
        }
    }
}
