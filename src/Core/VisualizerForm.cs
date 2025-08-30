using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using SkiaSharp;
using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.Dsp;
using System.Collections.Concurrent;
using System.Linq;
using System.Drawing.Imaging;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SkiaLizer
{
    public partial class VisualizerForm : Form
    {
        // Windows API for layered window transparency
        [DllImport("user32.dll")]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        
        [DllImport("user32.dll")]
        private static extern bool UpdateLayeredWindow(IntPtr hWnd, IntPtr hdcDst, ref POINT pptDst, ref SIZE psize, IntPtr hdcSrc, ref POINT pptSrc, uint crKey, ref BLENDFUNCTION pblend, uint dwFlags);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetDC(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        
        [DllImport("gdi32.dll")]
        private static extern IntPtr CreateCompatibleDC(IntPtr hdc);
        
        [DllImport("gdi32.dll")]
        private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);
        
        [DllImport("gdi32.dll")]
        private static extern bool DeleteDC(IntPtr hdc);
        
        [DllImport("gdi32.dll")]
        private static extern bool DeleteObject(IntPtr hObject);
        
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int ULW_ALPHA = 0x00000002;
        
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct SIZE
        {
            public int cx;
            public int cy;
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct BLENDFUNCTION
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        // Helper classes used by partial visualization methods
        private class PipeSegment
        {
            public Vector3 Start { get; set; }
            public Vector3 End { get; set; }
            public SKColor Color { get; set; }
        }

        private class Ball
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Radius;
            public SKColor Color;
        }

        private class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float Hue;
            public float Life;
            public float MaxLife;
        }

        private class Circle
        {
            public Vector2 Center;
            public float Radius;
            public float Target;
            public SKColor Color;
            public Vector2 Velocity;
            public float Alpha;
            public float Life;
            public float MaxLife;
            public Vector2 Destination;
            public bool Arrived;
            public int AliveFrames;
            public int MinAliveFrames;
            public float PopBias;
        }

        private class Star
        {
            public Vector3 Pos;
            public float Speed;
            public float Hue;
            public float Phase;
        }

        // Core fields used by partial visualization methods
        private static float ClampF(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        private readonly WasapiLoopbackCapture capture;
        private readonly int selectedVisual;

        private const int FftLength = 1024;
        private readonly NAudio.Dsp.Complex[] fftBuffer = new NAudio.Dsp.Complex[FftLength];
        private readonly float[] spectrum = new float[FftLength / 2];
        private readonly ConcurrentQueue<float> waveformQueue = new ConcurrentQueue<float>();
        private bool isFullScreen = false;
        private readonly object dataLock = new object();
        private Bitmap? bitmap;

        private readonly float[] smoothedSpectrum = new float[FftLength / 2];
        private readonly float[] peakSpectrum = new float[FftLength / 2];
        private float rotationAngle = 0;

        private readonly Random random = new Random();

        private float reactiveLevel = 0f;
        private float beatPulse = 0f;
        private float colorHueBase = 0f;
        private int currentTreeDepth = 10;
        private float treePhase = 0f;
        private float treeSpeed = 1f;
        private float previousLowLevel = 0f;
        private bool isSilent = true;
        private float lowBandLevel = 0f;
        private float highBandLevel = 0f;
        private float spectrumGain = 10f;
        private const float SpectrumTargetLevel = 0.7f;
        private const float Epsilon = 1e-6f;

        private float starSpeedSmooth = 1f;

        private readonly List<Ball> metaballs = new List<Ball>();
        private readonly List<Particle> boids = new List<Particle>();
        private readonly List<PipeSegment> pipes = new List<PipeSegment>();
        private Vector3 currentPosition = new Vector3(0, 0, 0);
        private Vector3 currentDirection = new Vector3(0, 1, 0);
        private int frameCount = 0;

        private List<List<PipeSegment>> pipeSystems = new List<List<PipeSegment>>();
        private List<Vector3> currentPositions = new List<Vector3>();
        private List<Vector3> currentDirections = new List<Vector3>();

        private const int TerrainCols = 64;
        private const int TerrainRows = 64;
        private readonly float[,] terrainHeights = new float[TerrainRows, TerrainCols];

        private float tunnelPhase = 0f;

        private readonly List<Circle> circles = new List<Circle>();
        private readonly List<Star> stars = new List<Star>();
        private readonly List<Vector2> voronoiSites = new List<Vector2>();

        private bool isTransparent;
        private bool isAlwaysOnTop;
        private System.Windows.Forms.Timer renderTimer;
        private SKColor[] colorPalette;

        public VisualizerForm(int visual, MMDevice device, bool enableTransparency, bool alwaysOnTop = false, int windowWidth = 800, int windowHeight = 600, bool startFullScreen = false, SKColor[]? palette = null)
        {
            selectedVisual = visual;
            isTransparent = enableTransparency;
            isAlwaysOnTop = alwaysOnTop;
            
            // Set color palette (fallback to rainbow if null)
            colorPalette = palette ?? PaletteManager.GetCurrentPalette();
            
            // Set initial title with current state
            string title = "SkiaLizer Visualizer";
            if (enableTransparency) title += " (Transparent)";
            if (alwaysOnTop) title += " (Always on Top)";
            this.Text = title;
            this.ClientSize = new Size(windowWidth, windowHeight);
            this.KeyPreview = true;
            this.DoubleBuffered = true;

            // Set always on top based on setting
            this.TopMost = alwaysOnTop;

            // Restore window position if the feature is enabled and valid position is saved
            if (SettingsManager.ToggleRememberPosition && 
                SettingsManager.WindowPositionX >= 0 && 
                SettingsManager.WindowPositionY >= 0)
            {
                // Check if the saved position is still valid (within screen bounds)
                if (IsPositionOnScreen(SettingsManager.WindowPositionX, SettingsManager.WindowPositionY))
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.Location = new Point(SettingsManager.WindowPositionX, SettingsManager.WindowPositionY);
                }
            }

            // Enable transparency for desktop and OBS
            if (enableTransparency)
            {
                // Enable per-pixel alpha transparency
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.FormBorderStyle = FormBorderStyle.None;
                this.ShowInTaskbar = true; // Keep in taskbar for easy access
                this.BackColor = Color.Transparent;
                this.TransparencyKey = Color.Empty; // No color key - use per-pixel alpha
            }

            this.Paint += OnPaint;
            this.Resize += OnResize;
            this.Move += OnMove;

            capture = new WasapiLoopbackCapture(device);
            capture.DataAvailable += OnDataAvailable;
            capture.StartRecording();

            this.KeyDown += OnKeyDown;
            this.FormClosing += (s, e) => { 
                // Save window position if the feature is enabled
                SaveWindowPosition();
                
                renderTimer?.Stop();
                renderTimer?.Dispose();
                capture.StopRecording(); 
                capture.Dispose(); 
            };

            Array.Clear(smoothedSpectrum, 0, smoothedSpectrum.Length);
            Array.Clear(peakSpectrum, 0, peakSpectrum.Length);
            pipes.Clear();
            currentPosition = new Vector3(0, 0, 50); // start back
            currentDirection = new Vector3(0, 1, 0);

            pipeSystems.Clear();
            currentPositions.Clear();
            currentDirections.Clear();

            // Start with one pipe
            currentPositions.Add(new Vector3(0, 0, 50));
            currentDirections.Add(new Vector3(0, 1, 0));
            pipeSystems.Add(new List<PipeSegment>());
            
            // Set up render timer for smooth animation
            renderTimer = new System.Windows.Forms.Timer();
            renderTimer.Interval = 16; // ~60 FPS
            renderTimer.Tick += (s, e) => {
                if (isTransparent)
                {
                    UpdateLayeredWindowWithAlpha();
                }
                else
                {
                    Invalidate();
                }
            };
            
            // Apply layered window style for transparency BEFORE starting timer
            if (enableTransparency)
            {
                // Force handle creation and apply layered window style immediately
                this.CreateHandle();
                if (this.IsHandleCreated)
                {
                    int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
                    exStyle |= WS_EX_LAYERED;
                    SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle);
                }
            }
            
            renderTimer.Start();
            
            // Apply fullscreen default if enabled
            if (startFullScreen)
            {
                this.Load += (s, e) => {
                    this.WindowState = FormWindowState.Maximized;
                    // Only set border to None if not already set by transparency
                    // Transparency initialization already sets FormBorderStyle.None
                    if (!enableTransparency)
                    {
                        this.FormBorderStyle = FormBorderStyle.None;
                    }
                    isFullScreen = true;
                };
            }
        }

        private void OnResize(object? sender, EventArgs e)
        {
            Invalidate();
        }

        private void OnMove(object? sender, EventArgs e)
        {
            // Save window position when it moves (but only if not in fullscreen)
            if (!isFullScreen && this.WindowState == FormWindowState.Normal)
            {
                SaveWindowPosition();
            }
        }

        private void SaveWindowPosition()
        {
            // Only save position if the feature is enabled and window is in normal state
            if (SettingsManager.ToggleRememberPosition && this.WindowState == FormWindowState.Normal && !isFullScreen)
            {
                SettingsManager.WindowPositionX = this.Location.X;
                SettingsManager.WindowPositionY = this.Location.Y;
            }
        }

        private bool IsPositionOnScreen(int x, int y)
        {
            // Check if the position is within any of the available screens
            foreach (Screen screen in Screen.AllScreens)
            {
                // Allow some tolerance - the window just needs to be partially visible
                Rectangle bounds = screen.WorkingArea;
                if (x >= bounds.Left - 100 && x <= bounds.Right - 100 &&
                    y >= bounds.Top - 50 && y <= bounds.Bottom - 50)
                {
                    return true;
                }
            }
            return false;
        }

        private void OnPaint(object? sender, PaintEventArgs e)
        {
            if (!isTransparent)
            {
                // Only use normal bitmap rendering for non-transparent mode
                RenderToBitmap();
                if (bitmap != null)
                {
                    e.Graphics.DrawImage(bitmap, 0, 0);
                }
            }
            // For transparent mode, do nothing here - handled by timer
        }

        private void RenderToBitmap()
        {
            int width = ClientSize.Width;
            int height = ClientSize.Height;
            if (width <= 0 || height <= 0) return;

            if (bitmap == null || bitmap.Width != width || bitmap.Height != height)
            {
                bitmap?.Dispose();
                bitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            }

            var bmpData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

            using (var surface = SKSurface.Create(info, bmpData.Scan0, bmpData.Stride))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Black); // Always black for normal rendering

                switch (selectedVisual)
                {
                    case 0:
                        DrawSpectrumBars(canvas, width, height);
                        break;
                    case 1:
                        DrawWaveform(canvas, width, height);
                        break;
                    case 2:
                        DrawRadialSpectrum(canvas, width, height);
                        break;
                    case 3:
                        DrawFractalTree(canvas, width, height);
                        break;
                    case 4:
                        Draw3DPipes(canvas, width, height);
                        break;
                    case 5:
                        DrawKaleidoscope(canvas, width, height);
                        break;
                    case 6:
                        DrawAudioTerrain(canvas, width, height);
                        break;
                    case 7:
                        DrawNeonTunnel(canvas, width, height);
                        break;
                    case 8:
                        DrawMetaballs(canvas, width, height);
                        break;
                    case 9:
                        DrawBoids(canvas, width, height);
                        break;
                    case 10:
                        DrawCirclePacking(canvas, width, height);
                        break;
                    case 11:
                        DrawStarfield(canvas, width, height);
                        break;
                    case 12:
                        DrawCrtGlitch(canvas, width, height);
                        break;
                    case 13:
                        DrawFractalKaleidoscope(canvas, width, height);
                        break;
                    case 14:
                        DrawVoronoi(canvas, width, height);
                        break;
                    case 15:
                        DrawPlasma(canvas, width, height);
                        break;
                }
            }
            bitmap.UnlockBits(bmpData);
        }

        private void UpdateLayeredWindowWithAlpha()
        {
            int width = ClientSize.Width;
            int height = ClientSize.Height;
            if (width <= 0 || height <= 0) return;

            // Create a bitmap with proper alpha channel for per-pixel transparency
            using var transparentBitmap = new Bitmap(width, height, PixelFormat.Format32bppPArgb);
            
            var bmpData = transparentBitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, transparentBitmap.PixelFormat);
            var info = new SKImageInfo(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);

            using (var surface = SKSurface.Create(info, bmpData.Scan0, bmpData.Stride))
            {
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent); // Clear with full transparency

                switch (selectedVisual)
                {
                    case 0:
                        DrawSpectrumBars(canvas, width, height);
                        break;
                    case 1:
                        DrawWaveform(canvas, width, height);
                        break;
                    case 2:
                        DrawRadialSpectrum(canvas, width, height);
                        break;
                    case 3:
                        DrawFractalTree(canvas, width, height);
                        break;
                    case 4:
                        Draw3DPipes(canvas, width, height);
                        break;
                    case 5:
                        DrawKaleidoscope(canvas, width, height);
                        break;
                    case 6:
                        DrawAudioTerrain(canvas, width, height);
                        break;
                    case 7:
                        DrawNeonTunnel(canvas, width, height);
                        break;
                    case 8:
                        DrawMetaballs(canvas, width, height);
                        break;
                    case 9:
                        DrawBoids(canvas, width, height);
                        break;
                    case 10:
                        DrawCirclePacking(canvas, width, height);
                        break;
                    case 11:
                        DrawStarfield(canvas, width, height);
                        break;
                    case 12:
                        DrawCrtGlitch(canvas, width, height);
                        break;
                    case 13:
                        DrawFractalKaleidoscope(canvas, width, height);
                        break;
                    case 14:
                        DrawVoronoi(canvas, width, height);
                        break;
                    case 15:
                        DrawPlasma(canvas, width, height);
                        break;
                }
            }
            
            transparentBitmap.UnlockBits(bmpData);
            
            // Use UpdateLayeredWindow for proper per-pixel alpha transparency
            UpdateLayeredWindowFromBitmap(transparentBitmap);
        }

        private void UpdateLayeredWindowFromBitmap(Bitmap bitmap)
        {
            try
            {
                IntPtr screenDC = GetDC(IntPtr.Zero);
                IntPtr memDC = CreateCompatibleDC(screenDC);
                IntPtr hBitmap = bitmap.GetHbitmap(Color.FromArgb(0));
                IntPtr oldBitmap = SelectObject(memDC, hBitmap);

                var size = new SIZE { cx = bitmap.Width, cy = bitmap.Height };
                var pointSource = new POINT { x = 0, y = 0 };
                var pointLocation = new POINT { x = Left, y = Top };
                var blend = new BLENDFUNCTION
                {
                    BlendOp = 0, // AC_SRC_OVER
                    BlendFlags = 0,
                    SourceConstantAlpha = 255,
                    AlphaFormat = 1 // AC_SRC_ALPHA
                };

                UpdateLayeredWindow(this.Handle, screenDC, ref pointLocation, ref size, memDC, ref pointSource, 0, ref blend, ULW_ALPHA);

                SelectObject(memDC, oldBitmap);
                DeleteObject(hBitmap);
                DeleteDC(memDC);
                ReleaseDC(IntPtr.Zero, screenDC);
            }
            catch
            {
                // Ignore errors during shutdown or handle recreation
            }
        }

        private void OnDataAvailable(object? sender, WaveInEventArgs e)
        {
            if (e.BytesRecorded == 0) return;

            var buffer = new WaveBuffer(e.Buffer);
            int numSamples = e.BytesRecorded / 4; // 32-bit float per sample

            for (int i = 0; i < numSamples; i += capture.WaveFormat.Channels)
            {
                float sample = buffer.FloatBuffer[i]; // left channel
                waveformQueue.Enqueue(sample);
                while (waveformQueue.Count > FftLength * 4) waveformQueue.TryDequeue(out _);
            }

            // fft on the most recent window
            var waveformArray = waveformQueue.ToArray();
            int start = Math.Max(0, waveformArray.Length - FftLength);
            for (int i = 0; i < FftLength; i++)
            {
                fftBuffer[i].X = (i + start < waveformArray.Length) ? waveformArray[i + start] * (float)FastFourierTransform.HammingWindow(i, FftLength) : 0;
                fftBuffer[i].Y = 0;
            }

            int logN = (int)Math.Log(FftLength, 2);
            FastFourierTransform.FFT(true, logN, fftBuffer);

            float[] tempSpectrum = new float[spectrum.Length];
            for (int i = 0; i < spectrum.Length; i++)
            {
                tempSpectrum[i] = (float)(Math.Sqrt(fftBuffer[i].X * fftBuffer[i].X + fftBuffer[i].Y * fftBuffer[i].Y) * 2);
            }

            for (int i = 0; i < tempSpectrum.Length; i++)
            {
                smoothedSpectrum[i] = smoothedSpectrum[i] * 0.8f + tempSpectrum[i] * 0.2f;
                if (smoothedSpectrum[i] > peakSpectrum[i])
                {
                    peakSpectrum[i] = smoothedSpectrum[i];
                }
                else
                {
                    peakSpectrum[i] *= 0.99f;
                }
            }

            // update adaptive spectrum gain
            float currentPeak = smoothedSpectrum.Max();
            if (currentPeak > Epsilon)
            {
                float desiredGain = SpectrumTargetLevel / currentPeak;
                // smooth and clamp gain
                spectrumGain = spectrumGain * 0.9f + desiredGain * 0.1f;
                if (spectrumGain < 0.5f) spectrumGain = 0.5f;
                if (spectrumGain > 500f) spectrumGain = 500f;
            }

            // compute reactive metrics from smoothed spectrum
            {
                float sum = 0f;
                float weighted = 0f;
                float total = 0f;
                int n = smoothedSpectrum.Length;
                for (int i = 0; i < n; i++)
                {
                    float v = smoothedSpectrum[i];
                    sum += v;
                    total += v;
                    weighted += v * i;
                }
                float avgMag = (n > 0) ? sum / n : 0f;
                float level = MathF.Min(1f, avgMag * 50f); // gain for responsiveness
                reactiveLevel = reactiveLevel * 0.7f + level * 0.3f;

                // low-band average (bass) for beat detection
                int lowCount = Math.Max(1, n / 16);
                float lowSum = 0f;
                for (int i = 0; i < lowCount; i++) lowSum += smoothedSpectrum[i];
                float lowAvg = lowSum / lowCount;
                float lowLevel = MathF.Min(1f, lowAvg * 80f);
                lowBandLevel = lowBandLevel * 0.7f + lowLevel * 0.3f;

                // high-band average (treble) for jitter
                int highStart = (int)(n * 0.5f);
                float highSum = 0f; int highCount = Math.Max(1, n - highStart);
                for (int i = highStart; i < n; i++) highSum += smoothedSpectrum[i];
                float highAvg = highSum / highCount;
                float highLevel = MathF.Min(1f, highAvg * 60f);
                highBandLevel = highBandLevel * 0.7f + highLevel * 0.3f;

                // silence detection and gating
                float silenceThreshold = 0.02f;
                isSilent = reactiveLevel < silenceThreshold && lowLevel < silenceThreshold;

                // beat detection (rising low band)
                float rise = lowLevel - previousLowLevel;
                if (!isSilent && rise > 0.08f)
                {
                    beatPulse = 1f;
                }
                else
                {
                    beatPulse *= 0.86f;
                }
                previousLowLevel = previousLowLevel * 0.8f + lowLevel * 0.2f;

                // speed reacts to level and beat; no movement when silent
                float targetSpeed = isSilent ? 0f : (0.4f + reactiveLevel * 2.0f + beatPulse * 4.0f);
                treeSpeed = treeSpeed * 0.7f + targetSpeed * 0.3f;
                if (treeSpeed < 0.001f) treeSpeed = 0f;
                if (treeSpeed > 0f) treePhase += treeSpeed;

                // hue progression only when moving
                float centroidNorm = (total > 0f) ? (weighted / total) / Math.Max(1, n) : 0.5f; // 0..1
                if (treeSpeed > 0f)
                {
                    colorHueBase = (colorHueBase + centroidNorm * 5f + reactiveLevel * 2f + treeSpeed * 0.8f + beatPulse * 1.0f) % 360f;
                }
            }

            lock (dataLock)
            {
                Array.Copy(smoothedSpectrum, spectrum, smoothedSpectrum.Length);
            }

            BeginInvoke(new Action(() => Invalidate()));

            frameCount++;
            {
                // audio-reactive parameters
                float level = reactiveLevel; // 0..1
                float speed = 6f + level * 20f + beatPulse * 30f; // segment length per step
                float turn = 0.15f + level * 0.5f + beatPulse * 1.2f; // randomness strength
                int iterations = 1 + (int)(level * 2f) + (beatPulse > 0.6f ? 1 : 0); // more steps on strong beats

                for (int step = 0; step < iterations; step++)
                {
                    // iterate each active pipe
                    for (int i = 0; i < currentPositions.Count; i++)
                    {
                        var pos = currentPositions[i];
                        var dir = currentDirections[i];
                        var system = pipeSystems[i];

                        // random turn scaled by audio
                        dir = Vector3.Normalize(dir + new Vector3(
                            (float)(random.NextDouble() * 2 - 1) * turn,
                            (float)(random.NextDouble() * 2 - 1) * turn,
                            (float)(random.NextDouble() * 2 - 1) * turn
                        ));

                        Vector3 newPos = pos + dir * speed;

                        // keep within bounds and reflect direction
                        float boundXY = 250f;
                        float minZ = -80f, maxZ = 260f;
                        if (newPos.X < -boundXY || newPos.X > boundXY) { dir.X *= -1; newPos = pos + dir * speed; }
                        if (newPos.Y < -boundXY || newPos.Y > boundXY) { dir.Y *= -1; newPos = pos + dir * speed; }
                        if (newPos.Z < minZ || newPos.Z > maxZ) { dir.Z *= -1; newPos = pos + dir * speed; }

                        float hue = (colorHueBase + level * 120f + beatPulse * 180f + frameCount * 0.2f) % 360f;
                        byte sat = (byte)Math.Clamp(70 + (int)(level * 30) + (int)(beatPulse * 30), 0, 100);
                        byte val = (byte)Math.Clamp(80 + (int)(level * 20) + (int)(beatPulse * 30), 0, 100);
                        SKColor color = SKColor.FromHsv(hue, sat, val);

                        system.Add(new PipeSegment { Start = pos, End = newPos, Color = color });

                        currentPositions[i] = newPos;
                        currentDirections[i] = dir;

                        // branch with audio influence
                        double splitChance = 0.01 + 0.05 * level + 0.12 * Math.Min(1.0, beatPulse);
                        if (pipeSystems.Count < 8 && random.NextDouble() < splitChance)
                        {
                            Vector3 newDir = Vector3.Normalize(dir + new Vector3(
                                (float)(random.NextDouble() * 2 - 1),
                                (float)(random.NextDouble() * 2 - 1),
                                (float)(random.NextDouble() * 2 - 1)) * 0.6f);
                            currentPositions.Add(newPos);
                            currentDirections.Add(newDir);
                            var newSystem = new List<PipeSegment>();
                            newSystem.Add(new PipeSegment { Start = newPos, End = newPos + newDir * (speed * 0.5f), Color = color });
                            pipeSystems.Add(newSystem);
                        }

                        // limit per system
                        if (system.Count > 400) system.RemoveRange(0, system.Count - 400);
                    }

                    // if no pipes (edge), seed one
                    if (currentPositions.Count == 0)
                    {
                        currentPositions.Add(new Vector3(0, 0, 50));
                        currentDirections.Add(new Vector3(0, 1, 0));
                        pipeSystems.Add(new List<PipeSegment>());
                    }
                }
            }
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F11)
            {
                ToggleFullScreen();
            }
            else if (e.KeyCode == Keys.T && e.Control)
            {
                ToggleTransparency();
            }
            else if (e.KeyCode == Keys.A && e.Control)
            {
                ToggleAlwaysOnTop();
            }
            else if (e.KeyCode == Keys.Escape)
            {
                this.Close();
            }
        }

        private void ToggleFullScreen()
        {
            isFullScreen = !isFullScreen;
            if (isFullScreen)
            {
                // Always set border to None for fullscreen
                FormBorderStyle = FormBorderStyle.None;
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                // Only set border to Sizable if not transparent
                // Transparent windows must maintain FormBorderStyle.None
                if (!isTransparent)
                {
                    FormBorderStyle = FormBorderStyle.Sizable;
                }
                WindowState = FormWindowState.Normal;
            }
            Invalidate();
        }

        private void ToggleTransparency()
        {
            isTransparent = !isTransparent;
            
            if (isTransparent)
            {
                // Enable per-pixel alpha transparency
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.BackColor = Color.Transparent;
                this.TransparencyKey = Color.Empty;
                this.FormBorderStyle = FormBorderStyle.None;
                this.TopMost = true; // Force on top when transparent for OBS compatibility
                this.ShowInTaskbar = true; // Keep in taskbar for easy access
                
                // Update title to show current state
                string title = "SkiaLizer Visualizer (Transparent)";
                if (isAlwaysOnTop) title += " (Always on Top)";
                this.Text = title;
                
                // Apply layered window style
                if (this.IsHandleCreated)
                {
                    int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
                    exStyle |= WS_EX_LAYERED;
                    SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle);
                }
            }
            else
            {
                // Remove layered window style first
                if (this.IsHandleCreated)
                {
                    int exStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
                    exStyle &= ~WS_EX_LAYERED;
                    SetWindowLong(this.Handle, GWL_EXSTYLE, exStyle);
                }
                
                // Reset to normal window
                this.BackColor = SystemColors.Control;
                this.TransparencyKey = Color.Empty;
                this.FormBorderStyle = FormBorderStyle.Sizable;
                this.TopMost = isAlwaysOnTop; // Respect always on top setting when not transparent
                this.ShowInTaskbar = true;
                
                // Update title to show current state
                string title = "SkiaLizer Visualizer";
                if (isAlwaysOnTop) title += " (Always on Top)";
                this.Text = title;
            }
            Invalidate();
        }

        private void ToggleAlwaysOnTop()
        {
            isAlwaysOnTop = !isAlwaysOnTop;
            
            // Apply the setting immediately
            // If transparent, it stays on top regardless, but update for when transparency is turned off
            if (!isTransparent)
            {
                this.TopMost = isAlwaysOnTop;
            }
            
            // Update title to show current state
            string title = "SkiaLizer Visualizer";
            if (isTransparent) title += " (Transparent)";
            if (isAlwaysOnTop) title += " (Always on Top)";
            this.Text = title;
        }

        private SKColor GetPaletteColor(float t)
        {
            if (colorPalette.Length == 0) return SKColors.White;
            if (colorPalette.Length == 1) return colorPalette[0];
            
            // Normalize t to 0-1 range
            t = Math.Max(0, Math.Min(1, t));
            
            // Map t to palette index range
            float exactIndex = t * (colorPalette.Length - 1);
            int index1 = (int)Math.Floor(exactIndex);
            int index2 = Math.Min(index1 + 1, colorPalette.Length - 1);
            
            // If exact index, return that color
            if (index1 == index2) return colorPalette[index1];
            
            // Interpolate between two colors
            float blend = exactIndex - index1;
            var color1 = colorPalette[index1];
            var color2 = colorPalette[index2];
            
            byte r = (byte)(color1.Red * (1 - blend) + color2.Red * blend);
            byte g = (byte)(color1.Green * (1 - blend) + color2.Green * blend);
            byte b = (byte)(color1.Blue * (1 - blend) + color2.Blue * blend);
            
            return new SKColor(r, g, b);
        }
        
        private SKColor GetPaletteColorCyclic(float hue)
        {
            // Map hue (0-360) to palette cyclically
            float normalizedHue = (hue % 360f) / 360f;
            return GetPaletteColor(normalizedHue);
        }

        private Vector2 ProjectSafe(Vector3 point, float scale, int width, int height)
        {
            float z = point.Z;
            if (z < -90f) z = -90f; // avoid divide by near zero
            float factor = scale / (z + 100f);
            float x = point.X * factor + width / 2f;
            float y = -point.Y * factor + height / 2f;
            return new Vector2(x, y);
        }

        // The drawing methods are implemented in the partial files (vf.*.cs)
    }
}
