using SkiaSharp;
using System;

namespace SkiaLizer
{
    public partial class VisualizerForm
    {
        private float plasmaTime = 0f;
        private float smoothedBass = 0f;
        private float smoothedTreble = 0f;
        private float smoothedLevel = 0f;
        private float plasmaHueBase = 0f;
        private float plasmaBrightness = 0.5f;
        
        private void DrawPlasma(SKCanvas canvas, int width, int height)
        {
            // Smooth audio parameters to reduce flickering
            float targetBass = lowBandLevel;
            float targetTreble = highBandLevel;
            float targetLevel = reactiveLevel;
            
            // Apply heavy smoothing for stable visuals
            smoothedBass = smoothedBass * 0.85f + targetBass * 0.15f;
            smoothedTreble = smoothedTreble * 0.80f + targetTreble * 0.20f;
            smoothedLevel = smoothedLevel * 0.90f + targetLevel * 0.10f;
            
            // Gentle pulse response for beats
            float pulse = beatPulse;
            float smoothPulse = Math.Min(1f, pulse * 0.6f);
            
            // Slower, more musical time progression
            float timeSpeed = 0.008f + smoothedLevel * 0.02f + smoothPulse * 0.05f;
            plasmaTime += timeSpeed;
            
            // Smooth brightness changes
            float targetBrightness = 0.3f + smoothedBass * 0.4f + smoothPulse * 0.3f;
            plasmaBrightness = plasmaBrightness * 0.92f + targetBrightness * 0.08f;
            plasmaBrightness = Math.Min(1.0f, plasmaBrightness);
            
            // Gradual hue evolution
            float hueSpeed = smoothedTreble * 0.5f + smoothPulse * 1.0f + 0.1f;
            plasmaHueBase = (plasmaHueBase + hueSpeed) % 360f;
            
            // Optimize texture size for performance
            int textureWidth = Math.Min(256, width / 2);
            int textureHeight = Math.Min(256, height / 2);
            using var plasmaBitmap = new SKBitmap(textureWidth, textureHeight);
            
            // Smooth wave parameters for fluid motion
            float wave1Speed = plasmaTime * 0.8f;
            float wave2Speed = plasmaTime * 0.6f;
            float wave3Speed = plasmaTime * 1.2f;
            float wave4Speed = plasmaTime * 0.4f;
            
            // Gentle audio-reactive wave amplitudes
            float amp1 = 0.8f + smoothedLevel * 0.4f;
            float amp2 = 1.0f + smoothedBass * 0.6f;
            float amp3 = 0.6f + smoothedTreble * 0.8f;
            float amp4 = 0.9f + smoothPulse * 0.3f;
            
            for (int y = 0; y < textureHeight; y++)
            {
                for (int x = 0; x < textureWidth; x++)
                {
                    // Normalize coordinates
                    float nx = (x - textureWidth * 0.5f) / textureWidth;
                    float ny = (y - textureHeight * 0.5f) / textureHeight;
                    
                    // Multiple overlapping sine waves for plasma effect
                    float wave1 = (float)Math.Sin(nx * 8f * amp1 + wave1Speed) * 
                                 (float)Math.Sin(ny * 6f * amp1 + wave1Speed * 1.3f);
                    
                    float wave2 = (float)Math.Sin((nx + ny) * 5f * amp2 + wave2Speed) * 
                                 (float)Math.Sin((nx - ny) * 7f * amp2 + wave2Speed * 0.8f);
                    
                    float wave3 = (float)Math.Sin(Math.Sqrt(nx * nx + ny * ny) * 12f * amp3 + wave3Speed) *
                                 (float)Math.Sin(Math.Atan2(ny, nx) * 4f + wave3Speed * 1.5f);
                    
                    float wave4 = (float)Math.Sin(nx * ny * 15f * amp4 + wave4Speed) *
                                 (float)Math.Sin((nx + ny) * 9f * amp4 + wave4Speed * 0.6f);
                    
                    // Combine waves
                    float plasma = (wave1 + wave2 + wave3 + wave4) * 0.25f;
                    
                    // Normalize to 0-1 range
                    plasma = (plasma + 1f) * 0.5f;
                    
                    // Apply smooth brightness
                    float brightness = plasma * plasmaBrightness;
                    brightness = Math.Min(1f, brightness);
                    
                    // Gentle edge enhancement for neon look
                    float edgeSharpness = 0.5f + smoothedTreble * 0.3f;
                    brightness = (float)Math.Pow(brightness, edgeSharpness);
                    
                    // Smooth color layers that evolve gradually
                    float hue1 = (plasmaHueBase + plasma * 30f) % 360f;
                    float hue2 = (plasmaHueBase + 120f + plasma * 40f) % 360f;
                    float hue3 = (plasmaHueBase + 240f + plasma * 20f) % 360f;
                    
                    // Smooth color mixing with lower saturation for pleasant viewing
                    // Use palette colors with plasma positioning
                    float pos1 = (plasma + 0.0f) % 1.0f;
                    float pos2 = (plasma + 0.33f) % 1.0f;
                    float pos3 = (plasma + 0.67f) % 1.0f;
                    
                    SKColor color1 = GetPaletteColor(pos1);
                    SKColor color2 = GetPaletteColor(pos2);
                    SKColor color3 = GetPaletteColor(pos3);
                    
                    // Apply brightness modulation
                    color1 = new SKColor((byte)(color1.Red * brightness), (byte)(color1.Green * brightness), (byte)(color1.Blue * brightness));
                    color2 = new SKColor((byte)(color2.Red * brightness), (byte)(color2.Green * brightness), (byte)(color2.Blue * brightness));
                    color3 = new SKColor((byte)(color3.Red * brightness), (byte)(color3.Green * brightness), (byte)(color3.Blue * brightness));
                    
                    // Smoother blending for fluid lava lamp transitions
                    float blend1 = 0.5f + 0.5f * (float)Math.Sin(plasma * Math.PI * 1.5f + wave1Speed * 0.5f);
                    float blend2 = 0.5f + 0.5f * (float)Math.Sin(plasma * Math.PI * 2.0f + wave2Speed * 0.5f);
                    float blend3 = 0.5f + 0.5f * (float)Math.Sin(plasma * Math.PI * 1.2f + wave3Speed * 0.5f);
                    
                    float totalBlend = blend1 + blend2 + blend3;
                    if (totalBlend > 0f)
                    {
                        blend1 /= totalBlend;
                        blend2 /= totalBlend;
                        blend3 /= totalBlend;
                    }
                    else
                    {
                        blend1 = blend2 = blend3 = 0.33f;
                    }
                    
                    byte r = (byte)(color1.Red * blend1 + color2.Red * blend2 + color3.Red * blend3);
                    byte g = (byte)(color1.Green * blend1 + color2.Green * blend2 + color3.Green * blend3);
                    byte b = (byte)(color1.Blue * blend1 + color2.Blue * blend2 + color3.Blue * blend3);
                    
                    // Gentle glow enhancement
                    float glow = 1f + smoothPulse * 0.2f;
                    r = (byte)Math.Min(255, r * glow);
                    g = (byte)Math.Min(255, g * glow);
                    b = (byte)Math.Min(255, b * glow);
                    
                    plasmaBitmap.SetPixel(x, y, new SKColor(r, g, b, 255));
                }
            }
            
            // Draw base plasma with smooth scaling
            using var paint = new SKPaint
            {
                FilterQuality = SKFilterQuality.Medium,
                BlendMode = SKBlendMode.SrcOver
            };
            
            var srcRect = new SKRect(0, 0, textureWidth, textureHeight);
            var destRect = new SKRect(0, 0, width, height);
            canvas.DrawBitmap(plasmaBitmap, srcRect, destRect, paint);
            
            // Subtle glow layer for depth
            using var glowPaint = new SKPaint
            {
                BlendMode = SKBlendMode.Plus,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 6f + smoothPulse * 8f),
                Color = GetPaletteColor((plasmaHueBase / 360f) % 1.0f).WithAlpha((byte)(40 + smoothPulse * 80))
            };
            
            canvas.DrawBitmap(plasmaBitmap, srcRect, destRect, glowPaint);
            
            // Gentle edge enhancement when treble is present
            if (smoothedTreble > 0.2f)
            {
                using var edgePaint = new SKPaint
                {
                    BlendMode = SKBlendMode.Screen,
                    MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 3f),
                    Color = GetPaletteColor(((plasmaHueBase + 180f) / 360f) % 1.0f).WithAlpha((byte)(smoothedTreble * 100))
                };
                
                canvas.DrawBitmap(plasmaBitmap, srcRect, destRect, edgePaint);
            }
        }
    }
}
