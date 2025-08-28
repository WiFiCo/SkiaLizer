using SkiaSharp;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private void DrawCrtGlitch(SKCanvas canvas, int width, int height)
        {
            float sensNow = reactiveLevel * 0.6f + lowBandLevel * 0.4f + highBandLevel * 0.5f + (beatPulse > 0.8f ? 0.3f : 0f);
            if (sensNow > 2f) sensNow = 2f;
            // Smooth sens for stability
            static float Lerp(float a, float b, float t) => a + (b - a) * t;
            sensNow = Lerp(0, sensNow, 0.9f) + Lerp(0, sensNow, 0.1f); // simple smoothing stub

            using SKPaint bg = new SKPaint { Color = SKColors.Black.WithAlpha((byte)System.Math.Clamp(180 + sensNow * 30, 100, 240)) };
            canvas.DrawRect(0, 0, width, height, bg);

            int spacing = (int)System.Math.Clamp(3 - sensNow * 1.0f, 1, 4);
            using SKPaint scan = new SKPaint { Color = SKColors.White.WithAlpha((byte)System.Math.Clamp(8 + sensNow * 20, 8, 60)) };
            for (int y = 0; y < height; y += spacing)
            {
                canvas.DrawLine(0, y, width, y, scan);
            }

            using SKPaint vignette = new SKPaint { Shader = SKShader.CreateRadialGradient(new SKPoint(width / 2f, height / 2f), System.Math.Max(width, height) / (1.2f - sensNow * 0.08f), new[] { SKColors.Transparent, SKColors.Black.WithAlpha((byte)System.Math.Clamp(150 + sensNow * 40, 100, 255)) }, new float[] { 0.7f, 1f }, SKShaderTileMode.Clamp) };
            canvas.DrawRect(0, 0, width, height, vignette);

            float shift = 1.5f + sensNow * 5f;
            using SKPaint red = new SKPaint { Color = SKColors.Red.WithAlpha((byte)System.Math.Clamp(30 + sensNow * 50, 20, 120)) };
            using SKPaint green = new SKPaint { Color = SKColors.Green.WithAlpha((byte)System.Math.Clamp(30 + sensNow * 50, 20, 120)) };
            using SKPaint blue = new SKPaint { Color = SKColors.Blue.WithAlpha((byte)System.Math.Clamp(30 + sensNow * 50, 20, 120)) };

            canvas.DrawRect(-shift, 0, width, height, red);
            canvas.DrawRect(shift, 0, width, height, blue);
            canvas.DrawRect(0, -shift, width, height, green);

            if (sensNow > 0.3f)
            {
                int glitches = (int)System.Math.Clamp(1 + sensNow * 8, 1, 18);
                float glitchColorPos = ((colorHueBase + sensNow * 90f) / 360f) % 1.0f;
                using SKPaint glitchPaint = new SKPaint { BlendMode = SKBlendMode.Difference, Color = GetPaletteColor(glitchColorPos).WithAlpha((byte)System.Math.Clamp(100 + sensNow * 60, 80, 255)) };
                for (int i = 0; i < glitches; i++)
                {
                    int gw = (int)System.Math.Clamp(24 + sensNow * 160, 20, 220);
                    int gh = (int)System.Math.Clamp(6 + sensNow * 28, 6, 40);
                    int gx = random.Next(0, System.Math.Max(1, width - gw));
                    int gy = random.Next(0, System.Math.Max(1, height - gh));
                    canvas.DrawRect(gx, gy, gw, gh, glitchPaint);
                }
            }
        }
    }
}
