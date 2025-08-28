using SkiaSharp;
using System;

namespace SkiaLizer
{
	public partial class VisualizerForm
	{
		private void DrawWaveform(SKCanvas canvas, int width, int height)
		{
			var waveformArray = waveformQueue.ToArray();
			if (waveformArray.Length == 0) return;

			// Create gradient from current palette
			SKColor[] gradientColors = new SKColor[Math.Min(4, colorPalette.Length)];
			float[] gradientPositions = new float[gradientColors.Length];
			
			for (int i = 0; i < gradientColors.Length; i++)
			{
				float t = i / (float)(gradientColors.Length - 1);
				gradientColors[i] = GetPaletteColor(t);
				gradientPositions[i] = t;
			}

			using SKPaint paint = new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				StrokeWidth = 2,
				Shader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(width, 0),
					gradientColors, gradientPositions, SKShaderTileMode.Clamp)
			};

			using var path = new SKPath();
			float step = (float)waveformArray.Length / width;
			path.MoveTo(0, height / 2);
			for (int x = 0; x < width; x++)
			{
				int index = (int)(x * step);
				if (index >= waveformArray.Length) break;
				float y = height / 2 + waveformArray[index] * (height / 4);
				path.LineTo(x, y);
			}
			canvas.DrawPath(path, paint);

			using var invertedPath = new SKPath();
			invertedPath.MoveTo(0, height / 2);
			for (int x = 0; x < width; x++)
			{
				int index = (int)(x * step);
				if (index >= waveformArray.Length) break;
				float y = height / 2 - waveformArray[index] * (height / 4);
				invertedPath.LineTo(x, y);
			}
			canvas.DrawPath(invertedPath, paint);
		}
	}
}
