using System;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace SkiaLizer
{
    public static class TrayManager
    {
        private static NotifyIcon? trayIcon;
        private static ContextMenuStrip? contextMenu;
        private static bool isConsoleHidden = false;
        private static IntPtr consoleHandle = IntPtr.Zero;

        // Windows API for console manipulation
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private const int SW_RESTORE = 9;

        public static event Action? OnSettingsRequested;
        public static event Action? OnExitRequested;

        public static void Initialize()
        {
            if (trayIcon != null) return; // Already initialized

            consoleHandle = GetConsoleWindow();

            // Create context menu
            contextMenu = new ContextMenuStrip();
            
            var settingsItem = new ToolStripMenuItem("Settings");
            settingsItem.Click += (s, e) => OnSettingsRequested?.Invoke();
            
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => OnExitRequested?.Invoke();

            contextMenu.Items.Add(settingsItem);
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(exitItem);

            // Create tray icon
            trayIcon = new NotifyIcon()
            {
                Icon = CreateDefaultIcon(),
                ContextMenuStrip = contextMenu,
                Text = "SkiaLizer - Audio Visualizer",
                Visible = false
            };

            // Double-click to restore console
            trayIcon.DoubleClick += (s, e) => ShowConsole();
        }

        public static void ShowInTray()
        {
            if (trayIcon == null) Initialize();
            
            trayIcon!.Visible = true;
            HideConsole();
        }

        public static void HideFromTray()
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
            }
            ShowConsole();
        }

        public static void ShowConsole()
        {
            if (consoleHandle != IntPtr.Zero && isConsoleHidden)
            {
                ShowWindow(consoleHandle, SW_RESTORE);
                isConsoleHidden = false;
            }
        }

        public static void HideConsole()
        {
            if (consoleHandle != IntPtr.Zero && !isConsoleHidden)
            {
                ShowWindow(consoleHandle, SW_HIDE);
                isConsoleHidden = true;
            }
        }

        public static void UpdateTrayText(string text)
        {
            if (trayIcon != null)
            {
                trayIcon.Text = text;
            }
        }

        public static void ShowBalloonTip(string title, string text, ToolTipIcon icon = ToolTipIcon.Info)
        {
            if (trayIcon != null && trayIcon.Visible)
            {
                trayIcon.ShowBalloonTip(3000, title, text, icon);
            }
        }

        public static void Dispose()
        {
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
                trayIcon = null;
            }
            
            contextMenu?.Dispose();
            contextMenu = null;
        }

        private static Icon CreateDefaultIcon()
        {
            // Create a simple default icon
            using (var bitmap = new Bitmap(16, 16))
            using (var graphics = Graphics.FromImage(bitmap))
            {
                // Draw a simple audio waveform icon
                graphics.FillRectangle(Brushes.Black, 0, 0, 16, 16);
                
                // Draw some bars to represent audio spectrum
                using (var brush = new SolidBrush(Color.LimeGreen))
                {
                    graphics.FillRectangle(brush, 1, 12, 2, 3);   // Short bar
                    graphics.FillRectangle(brush, 4, 8, 2, 7);    // Medium bar
                    graphics.FillRectangle(brush, 7, 4, 2, 11);   // Tall bar
                    graphics.FillRectangle(brush, 10, 6, 2, 9);   // Medium bar
                    graphics.FillRectangle(brush, 13, 10, 2, 5);  // Short bar
                }
                
                return Icon.FromHandle(bitmap.GetHicon());
            }
        }
    }
}
