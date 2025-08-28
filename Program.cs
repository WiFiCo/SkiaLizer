using System;
using System.Windows.Forms;

namespace SkiaLizer
{
    class Program
    {
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

            bool running = true;
            while (running)
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
                    running = false;
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

            var form = new VisualizerForm(
                MenuSystem.SelectedVisual, 
                device, 
                SettingsManager.TransparencyMode, 
                SettingsManager.AlwaysOnTopMode, 
                SettingsManager.SelectedWindowWidth, 
                SettingsManager.SelectedWindowHeight, 
                SettingsManager.FullScreenDefault, 
                PaletteManager.GetCurrentPalette());
            Application.Run(form);
        }
    }
}
