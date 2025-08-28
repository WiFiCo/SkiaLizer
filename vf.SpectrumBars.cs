using SkiaSharp;
using System;
using System.Linq;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawSpectrumBars(SKCanvas canvas, int width, int height)
        {
            float[] localSpectrum;
            float[] localPeaks;
            lock (dataLock)
            {
                localSpectrum = (float[])spectrum.Clone();
                localPeaks = (float[])peakSpectrum.Clone();
            }

            // apply adaptive gain and log compression for sensitivity
            for (int i = 0; i < localSpectrum.Length; i++)
            {
                float v = localSpectrum[i] * spectrumGain;
                localSpectrum[i] = (float)System.Math.Log10(1f + v * 9f);
            }
            for (int i = 0; i < localPeaks.Length; i++)
            {
                float v = localPeaks[i] * spectrumGain;
                localPeaks[i] = (float)System.Math.Log10(1f + v * 9f);
            }

            const int barCount = 64;
            float barWidth = width / (float)barCount;
            // Create gradient from current palette
            SKColor[] gradientColors = new SKColor[Math.Min(8, colorPalette.Length)];
            float[] gradientPositions = new float[gradientColors.Length];
            
            for (int i = 0; i < gradientColors.Length; i++)
            {
                float t = i / (float)(gradientColors.Length - 1);
                gradientColors[i] = GetPaletteColor(t);
                gradientPositions[i] = t;
            }
            
            using SKPaint paint = new SKPaint
            {
                Shader = SKShader.CreateLinearGradient(new SKPoint(0, height), new SKPoint(0, 0),
                    gradientColors, gradientPositions, SKShaderTileMode.Clamp)
            };

            using SKPaint peakPaint = new SKPaint { Color = SKColors.White, StrokeWidth = 2 };

            for (int i = 0; i < barCount; i++)
            {
                int spectrumIndex = (int)(i * (localSpectrum.Length / (float)barCount));
                float barHeight = localSpectrum[spectrumIndex] * height * 0.95f;
                barHeight = System.Math.Min(barHeight, height);
                canvas.DrawRect(i * barWidth, height - barHeight, barWidth - 2, barHeight, paint);

                float peakHeight = localPeaks[spectrumIndex] * height * 0.95f;
                canvas.DrawLine(i * barWidth, height - peakHeight, (i + 1) * barWidth - 2, height - peakHeight, peakPaint);
            }
        }
    }
}
