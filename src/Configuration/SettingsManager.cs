using System;

namespace SkiaLizer
{
    public static class SettingsManager
    {
        public static bool TransparencyMode
        {
            get => ConfigManager.TransparencyMode;
            set => ConfigManager.TransparencyMode = value;
        }

        public static bool AlwaysOnTopMode
        {
            get => ConfigManager.AlwaysOnTopMode;
            set => ConfigManager.AlwaysOnTopMode = value;
        }

        public static bool FullScreenDefault
        {
            get => ConfigManager.FullScreenDefault;
            set => ConfigManager.FullScreenDefault = value;
        }

        public static int SelectedWindowWidth
        {
            get => ConfigManager.SelectedWindowWidth;
            set => ConfigManager.SelectedWindowWidth = value;
        }

        public static int SelectedWindowHeight
        {
            get => ConfigManager.SelectedWindowHeight;
            set => ConfigManager.SelectedWindowHeight = value;
        }

        public static bool AutoStartVisualizer
        {
            get => ConfigManager.AutoStartVisualizer;
            set => ConfigManager.AutoStartVisualizer = value;
        }

        public static void ShowSettings()
        {
            bool stayInSettings = true;
            
            while (stayInSettings)
            {
                string[] settingsOptions = new string[]
                {
                    $"Toggle Transparency {(TransparencyMode ? "[ON]" : "[OFF]")}",
                    $"Toggle Always on Top {(AlwaysOnTopMode ? "[ON]" : "[OFF]")}",
                    $"Toggle FullScreen {(FullScreenDefault ? "[ON]" : "[OFF]")}",
                    $"Auto-Start Visualizer {(AutoStartVisualizer ? "[ON]" : "[OFF]")}",
                    "Set Window Size",
                    "Reset Settings",
                    "Back"
                };
                
                int sel = ConsoleMenu.ShowMenu("Settings:", settingsOptions);
                if (sel == -1) return; // User pressed Escape
                
                switch (sel)
                {
                    case 0:
                        TransparencyMode = !TransparencyMode;
                        Console.WriteLine($"Transparency is now {(TransparencyMode ? "ON" : "OFF")}");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 1:
                        AlwaysOnTopMode = !AlwaysOnTopMode;
                        Console.WriteLine($"Always on Top is now {(AlwaysOnTopMode ? "ON" : "OFF")}");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 2:
                        FullScreenDefault = !FullScreenDefault;
                        Console.WriteLine($"FullScreen Default is now {(FullScreenDefault ? "ON" : "OFF")}");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 3:
                        AutoStartVisualizer = !AutoStartVisualizer;
                        Console.WriteLine($"Auto-Start Visualizer is now {(AutoStartVisualizer ? "ON" : "OFF")}");
                        Console.WriteLine("When enabled, the app will start directly to the system tray with the visualizer running.");
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        break;
                    case 4:
                        SelectWindowSize();
                        break;
                    case 5:
                        ResetSettings();
                        break;
                    case 6:
                        stayInSettings = false; // Back to main menu
                        break;
                }
            }
        }

        public static void ResetSettings()
        {
            Console.WriteLine("Are you sure you want to reset all settings to defaults? (y/N)");
            string input = Console.ReadLine()?.ToLower().Trim() ?? "";
            
            if (input == "y" || input == "yes")
            {
                ConfigManager.ResetToDefaults();
                Console.WriteLine("All settings have been reset to defaults.");
            }
            else
            {
                Console.WriteLine("Reset cancelled.");
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        public static void SelectWindowSize()
        {
            string[] sizeOptions = new string[]
            {
                // Low Resolution
                "426x240 (240p)",
                "640x360 (360p)", 
                "854x480 (480p)",
                "1024x576 (576p)",
                
                // Standard Resolutions
                "800x600 (SVGA - Default)",
                "1024x768 (XGA)",
                "1280x720 (HD 720p)",
                "1280x800 (WXGA)",
                "1366x768 (WXGA)",
                "1440x900 (WXGA+)",
                "1600x900 (HD+)",
                "1680x1050 (WSXGA+)",
                
                // Full HD and Above
                "1920x1080 (Full HD 1080p)",
                "1920x1200 (WUXGA)",
                "2048x1152 (2K)",
                "2560x1440 (1440p QHD)",
                "2560x1600 (WQXGA)",
                
                // 4K Variants
                "3840x2160 (4K UHD)",
                "4096x2160 (4K DCI)",
                
                // Ultra-wide and Multi-Monitor
                "2560x1080 (21:9 Ultrawide)",
                "3440x1440 (21:9 Ultrawide QHD)",
                "3840x1080 (Dual 1080p)",
                "3840x1200 (Dual 1920x1200)",
                "5120x1440 (Dual 1440p)",
                "5760x1080 (Triple 1080p)",
                "7680x2160 (Dual 4K)",
                
                // 8K and Extreme
                "7680x4320 (8K UHD)",
                "11520x2160 (Triple 4K Ultrawide)",
                
                "Custom Size..."
            };
            
            // Find current selection index
            int currentIndex = -1;
            for (int i = 0; i < sizeOptions.Length - 1; i++) // -1 to exclude "Custom Size..."
            {
                var resolution = GetResolutionFromOption(sizeOptions[i]);
                if (resolution.Width == SelectedWindowWidth && resolution.Height == SelectedWindowHeight)
                {
                    currentIndex = i;
                    break;
                }
            }
            
            int sel = ConsoleMenu.ShowMenu("Select Window Size:", sizeOptions, currentIndex);
            if (sel == -1) return;
            
            if (sel == sizeOptions.Length - 1) // Custom Size
            {
                Console.Write("Enter width: ");
                if (int.TryParse(Console.ReadLine(), out int width) && width > 0)
                {
                    Console.Write("Enter height: ");
                    if (int.TryParse(Console.ReadLine(), out int height) && height > 0)
                    {
                        SelectedWindowWidth = width;
                        SelectedWindowHeight = height;
                        Console.WriteLine($"Window size set to {width}x{height}");
                    }
                    else
                    {
                        Console.WriteLine("Invalid height. Size unchanged.");
                    }
                }
                else
                {
                    Console.WriteLine("Invalid width. Size unchanged.");
                }
            }
            else
            {
                var resolution = GetResolutionFromOption(sizeOptions[sel]);
                SelectedWindowWidth = resolution.Width;
                SelectedWindowHeight = resolution.Height;
                Console.WriteLine($"Window size set to {SelectedWindowWidth}x{SelectedWindowHeight}");
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }

        private static (int Width, int Height) GetResolutionFromOption(string option)
        {
            // Extract resolution from option string like "1920x1080 (Full HD)"
            var parts = option.Split(' ')[0].Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
            {
                return (width, height);
            }
            return (800, 600); // fallback
        }
    }
}
