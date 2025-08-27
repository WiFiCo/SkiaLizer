using SkiaSharp;
using System;
using System.Linq;

namespace SkiaLizer
{
	public partial class VisualizerForm
	{
		private void DrawRadialSpectrum(SKCanvas canvas, int width, int height)
		{
			float[] localSpectrum;
			lock (dataLock)
			{
				localSpectrum = (float[])spectrum.Clone();
			}

			// apply adaptive gain and log compression for sensitivity
			for (int i = 0; i < localSpectrum.Length; i++)
			{
				float v = localSpectrum[i] * spectrumGain;
				localSpectrum[i] = (float)System.Math.Log10(1f + v * 9f);
			}

			float avg = localSpectrum.Average();
			rotationAngle += avg / 10f;

			float centerX = width / 2f;
			float centerY = height / 2f;
			float maxRadius = Math.Min(centerX, centerY) * 0.9f;
			const int binCount = 128;

			canvas.Save();
			canvas.RotateDegrees(rotationAngle, centerX, centerY);

			using SKPaint paint = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 2,
				Shader = SKShader.CreateRadialGradient(new SKPoint(centerX, centerY), maxRadius,
					new[] { SKColors.Purple, SKColors.Blue, SKColors.Cyan },
					new float[] { 0, 0.5f, 1 }, SKShaderTileMode.Clamp)
			};

			for (int i = 0; i < binCount; i++)
			{
				float angle = (float)(i * 2 * Math.PI / binCount);
				int spectrumIndex = (int)(i * (localSpectrum.Length / (float)binCount));
				float magnitudeNorm = localSpectrum[spectrumIndex];
				float magnitude = magnitudeNorm * maxRadius * 0.98f;
				float innerRadius = maxRadius * 0.2f;
				float x1 = centerX + (float)Math.Cos(angle) * innerRadius;
				float y1 = centerY + (float)Math.Sin(angle) * innerRadius;
				float x2 = centerX + (float)Math.Cos(angle) * (innerRadius + magnitude);
				float y2 = centerY + (float)Math.Sin(angle) * (innerRadius + magnitude);
				canvas.DrawLine(x1, y1, x2, y2, paint);
			}

			canvas.Restore();
		}
	}
}
