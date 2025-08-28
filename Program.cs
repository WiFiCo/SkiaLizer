using System;
using System.Windows.Forms;

namespace SkiaLizer
{
    class Program
    {
        private static bool applicationRunning = true;

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;

            // Load configuration
            ConfigManager.LoadConfig();

            // Initialize audio devices
            MenuSystem.InitializeAudioDevices();

            // Initialize tray manager
            TrayManager.Initialize();
            TrayManager.OnSettingsRequested += HandleSettingsRequest;
            TrayManager.OnExitRequested += HandleExitRequest;

            // Check if auto-start is enabled
            if (SettingsManager.AutoStartVisualizer)
            {
                Console.WriteLine("Auto-start enabled. Starting visualizer and minimizing to tray...");
                StartVisualizerToTray();
            }

            // Main application loop
            RunMainLoop();

            // Cleanup
            TrayManager.Dispose();
        }

        static void RunMainLoop()
        {
            while (applicationRunning)
            {
                int choice = MenuSystem.ShowMainMenu();
                if (choice == 0)
                {
                    // Start visualizer
                    StartVisualizer();
                }
                else if (choice == 5)
                {
                    // Exit
                    applicationRunning = false;
                }
                // For other choices (-1), continue the menu loop
            }
        }

        static void StartVisualizer()
        {
            MenuSystem.EnsureDeviceSelected();
            var device = MenuSystem.SelectedDevice;
            if (device == null)
            {
                Console.WriteLine("Error: No audio device available!");
                Console.WriteLine("Press any key to continue...");
                Console.ReadKey();
                return;
            }

            // Hide console to tray when visualizer starts
            TrayManager.ShowInTray();
            TrayManager.ShowBalloonTip("SkiaLizer", "Visualizer started. Right-click tray icon for options.", ToolTipIcon.Info);

            var form = new VisualizerForm(
                MenuSystem.SelectedVisual, 
                device, 
                SettingsManager.TransparencyMode, 
                SettingsManager.AlwaysOnTopMode, 
                SettingsManager.SelectedWindowWidth, 
                SettingsManager.SelectedWindowHeight, 
                SettingsManager.FullScreenDefault, 
                PaletteManager.GetCurrentPalette());

            // When visualizer closes, restore console from tray
            form.FormClosed += (s, e) => {
                TrayManager.HideFromTray();
            };

            Application.Run(form);
        }

        static void StartVisualizerToTray()
        {
            MenuSystem.EnsureDeviceSelected();
            var device = MenuSystem.SelectedDevice;
            if (device == null)
            {
                Console.WriteLine("Error: No audio device available! Continuing to main menu...");
                System.Threading.Thread.Sleep(2000);
                return;
            }

            // Start in tray immediately
            TrayManager.ShowInTray();
            TrayManager.ShowBalloonTip("SkiaLizer", "Auto-started to tray. Right-click for options.", ToolTipIcon.Info);

            var form = new VisualizerForm(
                MenuSystem.SelectedVisual, 
                device, 
                SettingsManager.TransparencyMode, 
                SettingsManager.AlwaysOnTopMode, 
                SettingsManager.SelectedWindowWidth, 
                SettingsManager.SelectedWindowHeight, 
                SettingsManager.FullScreenDefault, 
                PaletteManager.GetCurrentPalette());

            // When visualizer closes, restore console from tray
            form.FormClosed += (s, e) => {
                TrayManager.HideFromTray();
            };

            // Start the form without blocking
            form.Show();
        }

        static void HandleSettingsRequest()
        {
            // Close any running visualizer and show settings
            TrayManager.HideFromTray();
            TrayManager.ShowBalloonTip("SkiaLizer", "Opening settings menu...", ToolTipIcon.Info);
            
            // Force the main loop to show settings
            MenuSystem.ForceShowSettings();
        }

        static void HandleExitRequest()
        {
            applicationRunning = false;
            TrayManager.Dispose();
            Application.Exit();
        }
    }
}
