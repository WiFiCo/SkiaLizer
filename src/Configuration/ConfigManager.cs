using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using SkiaSharp;

namespace SkiaLizer
{
    public static class ConfigManager
    {
        private const string ConfigFileName = "SkiaLizer.cfg";
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

        public class AppConfig
        {
            public bool TransparencyMode { get; set; } = false;
            public bool AlwaysOnTopMode { get; set; } = false;
            public bool FullScreenDefault { get; set; } = false;
            public int SelectedWindowWidth { get; set; } = 800;
            public int SelectedWindowHeight { get; set; } = 600;
            public int SelectedVisual { get; set; } = 0;
            public int SelectedPalette { get; set; } = 0;
            public bool AutoStartVisualizer { get; set; } = false;
            public bool ToggleRememberPosition { get; set; } = false;
            public int WindowPositionX { get; set; } = -1; // -1 indicates not set
            public int WindowPositionY { get; set; } = -1; // -1 indicates not set
            public List<string> CustomPaletteColors { get; set; } = new List<string>();
        }

        private static AppConfig _config = new AppConfig();

        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string jsonString = File.ReadAllText(ConfigFilePath);
                    var loadedConfig = JsonSerializer.Deserialize<AppConfig>(jsonString);
                    if (loadedConfig != null)
                    {
                        _config = loadedConfig;
                        Console.WriteLine($"Configuration loaded from {ConfigFileName}");
                    }
                }
                else
                {
                    // Create default config file
                    SaveConfig();
                    Console.WriteLine($"Created default configuration file: {ConfigFileName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading config: {ex.Message}");
                Console.WriteLine("Using default settings...");
                _config = new AppConfig();
            }
        }

        public static void SaveConfig()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string jsonString = JsonSerializer.Serialize(_config, options);
                File.WriteAllText(ConfigFilePath, jsonString);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving config: {ex.Message}");
            }
        }

        public static void ResetToDefaults()
        {
            try
            {
                _config = new AppConfig();
                SaveConfig();
                Console.WriteLine("Settings reset to defaults");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting config: {ex.Message}");
            }
        }

        // Settings properties with automatic saving
        public static bool TransparencyMode
        {
            get => _config.TransparencyMode;
            set
            {
                _config.TransparencyMode = value;
                SaveConfig();
            }
        }

        public static bool AlwaysOnTopMode
        {
            get => _config.AlwaysOnTopMode;
            set
            {
                _config.AlwaysOnTopMode = value;
                SaveConfig();
            }
        }

        public static bool FullScreenDefault
        {
            get => _config.FullScreenDefault;
            set
            {
                _config.FullScreenDefault = value;
                SaveConfig();
            }
        }

        public static int SelectedWindowWidth
        {
            get => _config.SelectedWindowWidth;
            set
            {
                _config.SelectedWindowWidth = value;
                SaveConfig();
            }
        }

        public static int SelectedWindowHeight
        {
            get => _config.SelectedWindowHeight;
            set
            {
                _config.SelectedWindowHeight = value;
                SaveConfig();
            }
        }

        public static int SelectedVisual
        {
            get => _config.SelectedVisual;
            set
            {
                _config.SelectedVisual = value;
                SaveConfig();
            }
        }

        public static int SelectedPalette
        {
            get => _config.SelectedPalette;
            set
            {
                _config.SelectedPalette = value;
                SaveConfig();
            }
        }

        public static bool AutoStartVisualizer
        {
            get => _config.AutoStartVisualizer;
            set
            {
                _config.AutoStartVisualizer = value;
                SaveConfig();
            }
        }

        public static List<string> CustomPaletteColors
        {
            get => _config.CustomPaletteColors;
            set
            {
                _config.CustomPaletteColors = value;
                SaveConfig();
            }
        }

        public static bool ToggleRememberPosition
        {
            get => _config.ToggleRememberPosition;
            set
            {
                _config.ToggleRememberPosition = value;
                SaveConfig();
            }
        }

        public static int WindowPositionX
        {
            get => _config.WindowPositionX;
            set
            {
                _config.WindowPositionX = value;
                SaveConfig();
            }
        }

        public static int WindowPositionY
        {
            get => _config.WindowPositionY;
            set
            {
                _config.WindowPositionY = value;
                SaveConfig();
            }
        }

        // Helper methods for palette management
        public static void SaveCustomPalette(List<SKColor> palette)
        {
            var colorStrings = new List<string>();
            foreach (var color in palette)
            {
                colorStrings.Add($"#{color.Red:X2}{color.Green:X2}{color.Blue:X2}");
            }
            CustomPaletteColors = colorStrings;
        }

        public static List<SKColor> LoadCustomPalette()
        {
            var palette = new List<SKColor>();
            foreach (var colorString in CustomPaletteColors)
            {
                try
                {
                    var color = SKColor.Parse(colorString);
                    palette.Add(color);
                }
                catch
                {
                    // Skip invalid colors
                }
            }
            return palette;
        }
    }
}
