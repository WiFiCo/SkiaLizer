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
    class Program
    {
        static int selectedVisual = 0;
        static MMDevice? selectedDevice = null;
        static List<MMDevice> devices = new List<MMDevice>();
        static bool transparencyMode = false;
        static bool alwaysOnTopMode = false;
        static bool fullScreenDefault = false;
        static int selectedWindowWidth = 800;
        static int selectedWindowHeight = 600;
        static int selectedPalette = 0; // 0 = Rainbow (default)
        static List<SKColor> customPalette = new List<SKColor>();

        // Predefined color palettes
        static readonly Dictionary<string, SKColor[]> PredefinedPalettes = new Dictionary<string, SKColor[]>
        {
            ["Rainbow"] = new SKColor[]
            {
                SKColor.Parse("#FF0000"), // Red
                SKColor.Parse("#FF8000"), // Orange
                SKColor.Parse("#FFFF00"), // Yellow
                SKColor.Parse("#80FF00"), // Yellow-Green
                SKColor.Parse("#00FF00"), // Green
                SKColor.Parse("#00FF80"), // Green-Cyan
                SKColor.Parse("#00FFFF"), // Cyan
                SKColor.Parse("#0080FF"), // Cyan-Blue
                SKColor.Parse("#0000FF"), // Blue
                SKColor.Parse("#8000FF"), // Blue-Purple
                SKColor.Parse("#FF00FF"), // Purple
                SKColor.Parse("#FF0080")  // Purple-Red
            },
            ["Neon"] = new SKColor[]
            {
                SKColor.Parse("#FF0080"), // Hot Pink
                SKColor.Parse("#FF00FF"), // Magenta
                SKColor.Parse("#8000FF"), // Electric Purple
                SKColor.Parse("#0080FF"), // Electric Blue
                SKColor.Parse("#00FFFF"), // Cyan
                SKColor.Parse("#00FF80"), // Spring Green
                SKColor.Parse("#80FF00"), // Electric Lime
                SKColor.Parse("#FFFF00")  // Electric Yellow
            },
            ["Ocean"] = new SKColor[]
            {
                SKColor.Parse("#001122"), // Deep Ocean
                SKColor.Parse("#003366"), // Dark Blue
                SKColor.Parse("#0066AA"), // Ocean Blue
                SKColor.Parse("#0099CC"), // Sky Blue
                SKColor.Parse("#33AADD"), // Light Blue
                SKColor.Parse("#66CCEE"), // Powder Blue
                SKColor.Parse("#99DDFF"), // Pale Blue
                SKColor.Parse("#CCEEEE")  // Foam
            },
            ["Fire"] = new SKColor[]
            {
                SKColor.Parse("#660000"), // Dark Red
                SKColor.Parse("#CC0000"), // Red
                SKColor.Parse("#FF3300"), // Red-Orange
                SKColor.Parse("#FF6600"), // Orange
                SKColor.Parse("#FF9900"), // Yellow-Orange
                SKColor.Parse("#FFCC00"), // Gold
                SKColor.Parse("#FFFF00"), // Yellow
                SKColor.Parse("#FFFF99")  // Light Yellow
            },
            ["Sunset"] = new SKColor[]
            {
                SKColor.Parse("#2E1065"), // Deep Purple
                SKColor.Parse("#6A1B9A"), // Purple
                SKColor.Parse("#AD1457"), // Pink
                SKColor.Parse("#D32F2F"), // Red
                SKColor.Parse("#F57C00"), // Orange
                SKColor.Parse("#FBC02D"), // Yellow
                SKColor.Parse("#FFE082"), // Light Yellow
                SKColor.Parse("#FFF3E0")  // Cream
            },
            ["Synthwave"] = new SKColor[]
            {
                SKColor.Parse("#FF00FF"), // Magenta
                SKColor.Parse("#FF0080"), // Hot Pink
                SKColor.Parse("#8000FF"), // Purple
                SKColor.Parse("#0080FF"), // Blue
                SKColor.Parse("#00FFFF"), // Cyan
                SKColor.Parse("#FF8000"), // Orange
                SKColor.Parse("#FFFF00"), // Yellow
                SKColor.Parse("#FF0040")  // Rose
            },
            ["Monochrome"] = new SKColor[]
            {
                SKColor.Parse("#000000"), // Black
                SKColor.Parse("#333333"), // Dark Gray
                SKColor.Parse("#666666"), // Medium Gray
                SKColor.Parse("#999999"), // Light Gray
                SKColor.Parse("#CCCCCC"), // Very Light Gray
                SKColor.Parse("#FFFFFF")  // White
            },
            ["Forest"] = new SKColor[]
            {
                SKColor.Parse("#0D2818"), // Dark Forest
                SKColor.Parse("#1B5E20"), // Dark Green
                SKColor.Parse("#2E7D32"), // Green
                SKColor.Parse("#43A047"), // Medium Green
                SKColor.Parse("#66BB6A"), // Light Green
                SKColor.Parse("#81C784"), // Pale Green
                SKColor.Parse("#A5D6A7"), // Very Light Green
                SKColor.Parse("#C8E6C9")  // Mint
            }
        };

        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Green;

            var enumerator = new MMDeviceEnumerator();
            foreach (var dev in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                devices.Add(dev);
            }
            selectedDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            bool running = true;
            while (running)
            {
                string[] menuOptions = new string[]
                {
                    "Start Visualizer",
                    "Select Visual",
                    "Select Audio Source",
                    "Select Palette",
                    "Settings",
                    "Exit"
                };
                int choice = ConsoleMenu.ShowMenu("=== SkiaLizer - Audio Visualizer Terminal ===", menuOptions);

                if (choice == -1) { running = false; break; }

                switch (choice)
                {
                    case 0:
                        StartVisualizer();
                        break;
                    case 1:
                        SelectVisual();
                        break;
                    case 2:
                        SelectSource();
                        break;
                    case 3:
                        SelectPalette();
                        break;
                    case 4:
                        ShowSettings();
                        break;
                    case 5:
                        running = false;
                        break;
                }
            }
        }

        static void SelectVisual()
        {
            string[] visuals = new string[]
            {
                "Spectrum Bars",
                "Waveform",
                "Radial Spectrum",
                "Fractal Tree",
                "3D Pipes",
                "Kaleidoscope",
                "Audio Terrain",
                "Neon Tunnel",
                "Liquid Metaballs",
                "Particle Boids/Confetti",
                "Circle Packing Pulses",
                "Hyperwarp",
                "CRT/Glitch Post",
                "Fractal Kaleidoscope",
                "Voronoi Cells",
                "Plasma Swirls"
            };
            int sel = ConsoleMenu.ShowMenu("Select Visual:", visuals, selectedVisual);
            if (sel == -1) return;
            selectedVisual = sel;
        }

        static void SelectSource()
        {
            string[] sourceOptions = new string[devices.Count];
            for (int i = 0; i < devices.Count; i++)
            {
                sourceOptions[i] = devices[i].FriendlyName;
            }
            int currentIndex = devices.FindIndex(d => d.ID == selectedDevice?.ID);
            int sel = ConsoleMenu.ShowMenu("Select Audio Source:", sourceOptions, currentIndex);
            if (sel == -1) return;
            selectedDevice = devices[sel];
        }

        static void ShowSettings()
        {
            string[] settingsOptions = new string[]
            {
                $"Toggle Transparency {(transparencyMode ? "[ON]" : "[OFF]")}",
                $"Toggle Always on Top {(alwaysOnTopMode ? "[ON]" : "[OFF]")}",
                $"Toggle FullScreen {(fullScreenDefault ? "[ON]" : "[OFF]")}",
                "Set Window Size"
            };
            int sel = ConsoleMenu.ShowMenu("Settings:", settingsOptions);
            if (sel == -1) return;
            
            switch (sel)
            {
                case 0:
                    transparencyMode = !transparencyMode;
                    break;
                case 1:
                    alwaysOnTopMode = !alwaysOnTopMode;
                    break;
                case 2:
                    fullScreenDefault = !fullScreenDefault;
                    break;
                case 3:
                    SelectWindowSize();
                    break;
            }
        }

        static void SelectWindowSize()
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
                if (resolution.Width == selectedWindowWidth && resolution.Height == selectedWindowHeight)
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
                        selectedWindowWidth = width;
                        selectedWindowHeight = height;
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
                selectedWindowWidth = resolution.Width;
                selectedWindowHeight = resolution.Height;
                Console.WriteLine($"Window size set to {selectedWindowWidth}x{selectedWindowHeight}");
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        
        static (int Width, int Height) GetResolutionFromOption(string option)
        {
            // Extract resolution from option string like "1920x1080 (Full HD)"
            var parts = option.Split(' ')[0].Split('x');
            if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
            {
                return (width, height);
            }
            return (800, 600); // fallback
        }

        static void SelectPalette()
        {
            var paletteNames = PredefinedPalettes.Keys.ToArray();
            var menuOptions = new List<string>();
            
            // Add predefined palettes
            foreach (var name in paletteNames)
            {
                menuOptions.Add(name + (selectedPalette < paletteNames.Length && paletteNames[selectedPalette] == name ? " (Current)" : ""));
            }
            
            // Add custom palette options
            menuOptions.Add($"Custom Palette {(selectedPalette == paletteNames.Length ? "(Current) " : "")}({customPalette.Count} colors)");
            menuOptions.Add("Create Custom Palette");
            menuOptions.Add("Edit Custom Palette");
            
            int sel = ConsoleMenu.ShowMenu("Select Color Palette:", menuOptions.ToArray(), selectedPalette);
            if (sel == -1) return;
            
            if (sel < paletteNames.Length)
            {
                // Selected a predefined palette
                selectedPalette = sel;
                Console.WriteLine($"Selected palette: {paletteNames[sel]}");
                ShowPalettePreview(PredefinedPalettes[paletteNames[sel]]);
            }
            else if (sel == paletteNames.Length)
            {
                // Selected existing custom palette
                if (customPalette.Count > 0)
                {
                    selectedPalette = paletteNames.Length;
                    Console.WriteLine($"Selected custom palette with {customPalette.Count} colors");
                    ShowPalettePreview(customPalette.ToArray());
                }
                else
                {
                    Console.WriteLine("No custom palette created yet. Use 'Create Custom Palette' to make one.");
                }
            }
            else if (sel == paletteNames.Length + 1)
            {
                // Create custom palette
                CreateCustomPalette();
            }
            else if (sel == paletteNames.Length + 2)
            {
                // Edit custom palette
                EditCustomPalette();
            }
            
            Console.WriteLine("Press any key to continue...");
            Console.ReadKey();
        }
        
        static void ShowPalettePreview(SKColor[] colors)
        {
            Console.WriteLine("\nPalette Preview:");
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                var ansiColor = GetClosestAnsiColor(color.Red, color.Green, color.Blue);
                
                // Use ANSI color codes for true color display if supported
                string colorBlock = "";
                if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                {
                    // Use ANSI 256-color or true color escape sequences
                    colorBlock = $"\x1b[48;2;{color.Red};{color.Green};{color.Blue}m    \x1b[0m";
                }
                else
                {
                    // Fallback to console colors
                    Console.BackgroundColor = ansiColor;
                    colorBlock = "    ";
                    Console.ResetColor();
                }
                
                Console.WriteLine($"  {i + 1}: {colorBlock} #{color.Red:X2}{color.Green:X2}{color.Blue:X2} (R:{color.Red} G:{color.Green} B:{color.Blue})");
            }
        }
        
        static void CreateCustomPalette()
        {
            customPalette.Clear();
            Console.WriteLine("\n=== Create Custom Palette ===");
            Console.WriteLine("Choose how to add colors:");
            Console.WriteLine("[1] Enter hex colors manually");
            Console.WriteLine("[2] Use interactive color picker");
            Console.WriteLine("[3] Mix both methods");
            Console.Write("Choice (1-3): ");
            
            string input = Console.ReadLine() ?? "";
            
            switch (input)
            {
                case "1":
                    AddColorsManually();
                    break;
                case "2":
                    AddColorsWithPicker();
                    break;
                case "3":
                    Console.WriteLine("You can mix both methods. Start with one and use 'Edit Custom Palette' to add more.");
                    AddColorsManually();
                    break;
                default:
                    Console.WriteLine("Invalid choice. Using manual entry...");
                    AddColorsManually();
                    break;
            }
            
            if (customPalette.Count > 0)
            {
                selectedPalette = PredefinedPalettes.Keys.Count; // Set to custom palette
                Console.WriteLine($"\nCustom palette created with {customPalette.Count} colors!");
                ShowPalettePreview(customPalette.ToArray());
            }
        }
        
        static void EditCustomPalette()
        {
            if (customPalette.Count == 0)
            {
                Console.WriteLine("No custom palette exists. Create one first.");
                return;
            }
            
            Console.WriteLine("\n=== Edit Custom Palette ===");
            Console.WriteLine("Current palette:");
            ShowPalettePreview(customPalette.ToArray());
            
            Console.WriteLine("\nOptions:");
            Console.WriteLine("[1] Add more colors (hex entry)");
            Console.WriteLine("[2] Add colors with picker");
            Console.WriteLine("[3] Remove a color");
            Console.WriteLine("[4] Clear all colors");
            Console.WriteLine("[5] Replace a color");
            Console.Write("Choice (1-5): ");
            
            string input = Console.ReadLine() ?? "";
            
            switch (input)
            {
                case "1":
                    AddColorsManually();
                    break;
                case "2":
                    AddColorsWithPicker();
                    break;
                case "3":
                    RemoveColor();
                    break;
                case "4":
                    customPalette.Clear();
                    Console.WriteLine("Custom palette cleared.");
                    break;
                case "5":
                    ReplaceColor();
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
            
            if (customPalette.Count > 0)
            {
                Console.WriteLine($"\nUpdated custom palette:");
                ShowPalettePreview(customPalette.ToArray());
            }
        }
        
        static void AddColorsManually()
        {
            Console.WriteLine("\n=== Manual Hex Color Entry ===");
            Console.WriteLine("Enter hex colors (e.g., FF0000, #FF0000, or ff0000)");
            Console.WriteLine("Press Enter with empty input to finish");
            
            while (true)
            {
                Console.Write($"Color {customPalette.Count + 1} (hex): ");
                string input = Console.ReadLine()?.Trim() ?? "";
                
                if (string.IsNullOrEmpty(input)) break;
                
                // Clean up input
                if (!input.StartsWith("#")) input = "#" + input;
                
                try
                {
                    var color = SKColor.Parse(input);
                    customPalette.Add(color);
                    Console.WriteLine($"Added: {input} -> R:{color.Red} G:{color.Green} B:{color.Blue}");
                }
                catch
                {
                    Console.WriteLine("Invalid hex color. Try again (e.g., FF0000 or #FF0000)");
                }
            }
        }
        
        static void AddColorsWithPicker()
        {
            Console.WriteLine("\n=== Interactive Color Picker ===");
            Console.WriteLine("Use arrow keys to adjust RGB values, Enter to add color, Esc when done");
            
            while (true)
            {
                var color = InteractiveColorPicker();
                if (color.HasValue)
                {
                    customPalette.Add(color.Value);
                    Console.WriteLine($"Added color: #{color.Value.Red:X2}{color.Value.Green:X2}{color.Value.Blue:X2}");
                    Console.WriteLine($"Total colors in palette: {customPalette.Count}");
                    Console.WriteLine("Press any key to add another color, or Esc to finish...");
                    
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Escape) break;
                }
                else
                {
                    break; // User escaped
                }
            }
        }
        
        static SKColor? InteractiveColorPicker()
        {
            int red = 255, green = 0, blue = 0;
            int selectedChannel = 0; // 0=R, 1=G, 2=B
            
            while (true)
            {
                Console.Clear();
                Console.WriteLine("=== Interactive Color Picker ===");
                Console.WriteLine("Use Up/Down arrows to change values, Left/Right to switch channels");
                Console.WriteLine("Press Enter to add this color, Esc to cancel");
                Console.WriteLine();
                
                // Show current color
                Console.WriteLine($"Current Color: #{red:X2}{green:X2}{blue:X2}");
                Console.WriteLine($"RGB Values: R:{red} G:{green} B:{blue}");
                Console.WriteLine();
                
                // Show sliders
                Console.WriteLine($"{(selectedChannel == 0 ? "► " : "  ")}Red   (R): {red:D3} [{new string('█', red / 8)}{new string('░', 32 - red / 8)}]");
                Console.WriteLine($"{(selectedChannel == 1 ? "► " : "  ")}Green (G): {green:D3} [{new string('█', green / 8)}{new string('░', 32 - green / 8)}]");
                Console.WriteLine($"{(selectedChannel == 2 ? "► " : "  ")}Blue  (B): {blue:D3} [{new string('█', blue / 8)}{new string('░', 32 - blue / 8)}]");
                Console.WriteLine();
                
                // Show color preview with ANSI colors (approximate)
                var ansiColor = GetClosestAnsiColor(red, green, blue);
                Console.Write("Preview: ");
                Console.BackgroundColor = ansiColor;
                Console.Write("    ");
                Console.ResetColor();
                Console.WriteLine($" (Approximate - actual color is #{red:X2}{green:X2}{blue:X2})");
                
                var key = Console.ReadKey(true);
                
                switch (key.Key)
                {
                    case ConsoleKey.UpArrow:
                        if (selectedChannel == 0) red = Math.Min(255, red + 5);
                        else if (selectedChannel == 1) green = Math.Min(255, green + 5);
                        else blue = Math.Min(255, blue + 5);
                        break;
                        
                    case ConsoleKey.DownArrow:
                        if (selectedChannel == 0) red = Math.Max(0, red - 5);
                        else if (selectedChannel == 1) green = Math.Max(0, green - 5);
                        else blue = Math.Max(0, blue - 5);
                        break;
                        
                    case ConsoleKey.RightArrow:
                        selectedChannel = (selectedChannel + 1) % 3;
                        break;
                        
                    case ConsoleKey.LeftArrow:
                        selectedChannel = (selectedChannel - 1 + 3) % 3;
                        break;
                        
                    case ConsoleKey.Enter:
                        return new SKColor((byte)red, (byte)green, (byte)blue);
                        
                    case ConsoleKey.Escape:
                        return null;
                }
            }
        }
        
        static ConsoleColor GetClosestAnsiColor(int r, int g, int b)
        {
            // Simple mapping to closest console colors
            if (r > 200 && g > 200 && b > 200) return ConsoleColor.White;
            if (r < 50 && g < 50 && b < 50) return ConsoleColor.Black;
            if (r > 150 && g < 100 && b < 100) return ConsoleColor.Red;
            if (r < 100 && g > 150 && b < 100) return ConsoleColor.Green;
            if (r < 100 && g < 100 && b > 150) return ConsoleColor.Blue;
            if (r > 150 && g > 150 && b < 100) return ConsoleColor.Yellow;
            if (r > 150 && g < 100 && b > 150) return ConsoleColor.Magenta;
            if (r < 100 && g > 150 && b > 150) return ConsoleColor.Cyan;
            if (r > 100 && g > 100 && b > 100) return ConsoleColor.Gray;
            return ConsoleColor.DarkGray;
        }
        
        static void RemoveColor()
        {
            if (customPalette.Count == 0) return;
            
            Console.WriteLine("Select color to remove:");
            for (int i = 0; i < customPalette.Count; i++)
            {
                var color = customPalette[i];
                Console.WriteLine($"[{i + 1}] #{color.Red:X2}{color.Green:X2}{color.Blue:X2}");
            }
            Console.Write("Enter number (1-{0}): ", customPalette.Count);
            
            if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= customPalette.Count)
            {
                customPalette.RemoveAt(index - 1);
                Console.WriteLine("Color removed.");
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }
        
        static void ReplaceColor()
        {
            if (customPalette.Count == 0) return;
            
            Console.WriteLine("Select color to replace:");
            for (int i = 0; i < customPalette.Count; i++)
            {
                var color = customPalette[i];
                Console.WriteLine($"[{i + 1}] #{color.Red:X2}{color.Green:X2}{color.Blue:X2}");
            }
            Console.Write($"Enter number (1-{customPalette.Count}): ");
            
            if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= customPalette.Count)
            {
                Console.WriteLine("Choose replacement method:");
                Console.WriteLine("[1] Enter hex color");
                Console.WriteLine("[2] Use color picker");
                Console.Write("Choice: ");
                
                string method = Console.ReadLine() ?? "";
                SKColor? newColor = null;
                
                if (method == "1")
                {
                    Console.Write("New hex color: ");
                    string hex = Console.ReadLine()?.Trim() ?? "";
                    if (!hex.StartsWith("#")) hex = "#" + hex;
                    try
                    {
                        newColor = SKColor.Parse(hex);
                    }
                    catch
                    {
                        Console.WriteLine("Invalid hex color.");
                        return;
                    }
                }
                else if (method == "2")
                {
                    newColor = InteractiveColorPicker();
                }
                
                if (newColor.HasValue)
                {
                    customPalette[index - 1] = newColor.Value;
                    Console.WriteLine("Color replaced.");
                }
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }

        static void StartVisualizer()
        {
            if (selectedDevice == null)
            {
                var enumerator = new MMDeviceEnumerator();
                selectedDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }
            var form = new VisualizerForm(selectedVisual, selectedDevice, transparencyMode, alwaysOnTopMode, selectedWindowWidth, selectedWindowHeight, fullScreenDefault, GetCurrentPalette());
            Application.Run(form);
        }

        static SKColor[] GetCurrentPalette()
        {
            var paletteNames = PredefinedPalettes.Keys.ToArray();
            
            if (selectedPalette < paletteNames.Length)
            {
                // Return predefined palette
                return PredefinedPalettes[paletteNames[selectedPalette]];
            }
            else if (customPalette.Count > 0)
            {
                // Return custom palette
                return customPalette.ToArray();
            }
            else
            {
                // Fallback to rainbow (default)
                return PredefinedPalettes["Rainbow"];
            }
        }

        // removed local ShowMenu; using ConsoleMenu.ShowMenu
    }

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

        private float reactiveLevel = 0f; // 0..1 smoothed audio level
        private float beatPulse = 0f;     // transient pulse on peaks
        private float colorHueBase = 0f;  // rolling hue offset
        private int currentTreeDepth = 10;
        private float treePhase = 0f;     // drives animation
        private float treeSpeed = 1f;     // audio-reactive speed
        private float previousLowLevel = 0f; // for beat detection
        private bool isSilent = true;
        private float lowBandLevel = 0f;  // 0..1 bass
        private float highBandLevel = 0f; // 0..1 treble
        private float spectrumGain = 10f; // adaptive gain for spectrum visuals
        private const float SpectrumTargetLevel = 0.7f;
        private const float Epsilon = 1e-6f;

        // smoothing/adaptive state
        private float starSpeedSmooth = 1f;

        private class PipeSegment
        {
            public Vector3 Start { get; set; }
            public Vector3 End { get; set; }
            public SKColor Color { get; set; }
        }

        // Metaballs
        private class Ball
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Radius;
            public SKColor Color;
        }
        private readonly List<Ball> metaballs = new List<Ball>();

        // Boids/Confetti particles
        private class Particle
        {
            public Vector2 Position;
            public Vector2 Velocity;
            public float Size;
            public float Hue;
            public float Life;     // frames remaining
            public float MaxLife;  // initial lifetime
        }
        private readonly List<Particle> boids = new List<Particle>();

        private readonly List<PipeSegment> pipes = new List<PipeSegment>();
        private Vector3 currentPosition = new Vector3(0, 0, 0);
        private Vector3 currentDirection = new Vector3(0, 1, 0);
        private int frameCount = 0;

        private List<List<PipeSegment>> pipeSystems = new List<List<PipeSegment>>(); // multiple pipes
        private List<Vector3> currentPositions = new List<Vector3>();
        private List<Vector3> currentDirections = new List<Vector3>();

        private const int TerrainCols = 64;
        private const int TerrainRows = 64;
        private readonly float[,] terrainHeights = new float[TerrainRows, TerrainCols];

        private float tunnelPhase = 0f;

        // Circle Packing Pulses
        private class Circle
        {
            public Vector2 Center;
            public float Radius;
            public float Target;
            public SKColor Color;
            public Vector2 Velocity;
            public float Alpha;   // 0..255
            public float Life;    // frames remaining
            public float MaxLife; // initial life
            public Vector2 Destination; // target position
            public bool Arrived;        // reached destination at least once
            public int AliveFrames;     // frames since spawn/respawn
            public int MinAliveFrames;  // must exceed before pop allowed
            public float PopBias;       // 0..1 personal sensitivity to pop
        }
        private readonly List<Circle> circles = new List<Circle>();

        // Starfield
        private class Star
        {
            public Vector3 Pos;
            public float Speed;
            public float Hue;
            public float Phase; // twinkle phase
        }
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
            colorPalette = palette ?? new SKColor[]
            {
                SKColor.Parse("#FF0000"), SKColor.Parse("#FF8000"), SKColor.Parse("#FFFF00"), SKColor.Parse("#80FF00"),
                SKColor.Parse("#00FF00"), SKColor.Parse("#00FF80"), SKColor.Parse("#00FFFF"), SKColor.Parse("#0080FF"),
                SKColor.Parse("#0000FF"), SKColor.Parse("#8000FF"), SKColor.Parse("#FF00FF"), SKColor.Parse("#FF0080")
            };
            
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

            // Enable transparency for desktop and OBS
            if (enableTransparency)
            {
                // Enable per-pixel alpha transparency
                this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
                this.FormBorderStyle = FormBorderStyle.None;
                this.TopMost = true; // Force on top when transparent for OBS compatibility
                this.ShowInTaskbar = true; // Keep in taskbar for easy access
                this.BackColor = Color.Transparent;
                this.TransparencyKey = Color.Empty; // No color key - use per-pixel alpha
            }

            this.Paint += OnPaint;
            this.Resize += OnResize;

            capture = new WasapiLoopbackCapture(device);
            capture.DataAvailable += OnDataAvailable;
            capture.StartRecording();

            this.KeyDown += OnKeyDown;
            this.FormClosing += (s, e) => { 
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

        // moved to vf.SpectrumBars.cs

        // moved to vf.Waveform.cs

        // moved to vf.RadialSpectrum.cs

        // moved to vf.FractalTree.cs

        // moved to vf.Pipes3D.cs

        #if false // moved to partial files
        private void DrawKaleidoscope(SKCanvas canvas, int width, int height)
        {
            int w = width, h = height;
            using var offBmp = new SKBitmap(w, h, true);
            using var offSurface = new SKCanvas(offBmp);

            // Draw a simple seed scene using spectrum: radial lines
            float[] localSpectrum;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); }
            float centroid = 0f, total = 0f; for (int i = 0; i < localSpectrum.Length; i++) { centroid += localSpectrum[i] * i; total += localSpectrum[i]; }
            float centroidNorm = (total > 0) ? centroid / total / Math.Max(1, localSpectrum.Length) : 0.5f;
            float hueBase = (colorHueBase + centroidNorm * 180f) % 360f;

            offSurface.Clear(SKColors.Black);

            using (var paint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2 })
            {
                int rays = 128;
                float cx = w / 2f, cy = h / 2f;
                float maxR = Math.Min(cx, cy) * 0.9f;
                for (int i = 0; i < rays; i++)
                {
                    float t = i / (float)rays;
                    int idx = (int)(t * (localSpectrum.Length - 1));
                    float mag = localSpectrum[idx];
                    float r = Math.Min(maxR, maxR * (0.2f + mag * 2.5f));
                    float angle = t * MathF.PI * 2f;
                    float x2 = cx + MathF.Cos(angle) * r;
                    float y2 = cy + MathF.Sin(angle) * r;
                    paint.Color = SKColor.FromHsv((hueBase + t * 360f) % 360f, 90, (byte)Math.Clamp(60 + (int)(mag * 200), 0, 100));
                    offSurface.DrawLine(cx, cy, x2, y2, paint);
                }
            }

            // Kaleidoscope: mirror the offBmp into wedges
            int wedges = 8; // 8-fold symmetry
            float level = reactiveLevel;
            float zoom = 1f + level * 0.3f + beatPulse * 0.5f;
            float angleStep = 360f / wedges;
            float rotate = (treePhase * 0.2f + beatPulse * 10f) % 360f;

            canvas.Save();
            canvas.Translate(w / 2f, h / 2f);
            canvas.Scale(zoom, zoom);
            for (int i = 0; i < wedges; i++)
            {
                canvas.Save();
                canvas.RotateDegrees(rotate + i * angleStep);
                var src = new SKRect(0, 0, w, h);
                var dst = new SKRect(-w / 2f, -h / 2f, w / 2f, h / 2f);
                canvas.DrawBitmap(offBmp, src, dst);
                canvas.Scale(-1, 1); // mirror
                canvas.DrawBitmap(offBmp, src, dst);
                canvas.Restore();
            }
            canvas.Restore();
        }

        private void DrawAudioTerrain(SKCanvas canvas, int width, int height)
        {
            float[] localSpectrum;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); }

            // Update terrain heights: push a new row at the front based on spectrum bands
            // using TerrainCols directly
            for (int c = 0; c < TerrainCols; c++)
            {
                int idx = (int)(c / (float)TerrainCols * (localSpectrum.Length - 1));
                float v = localSpectrum[idx];
                float hgt = MathF.Min(1.5f, v * 8f); // scale
                terrainHeights[0, c] = hgt;
            }
            // Scroll rows back
            for (int r = TerrainRows - 1; r > 0; r--)
                for (int c = 0; c < TerrainCols; c++)
                    terrainHeights[r, c] = terrainHeights[r - 1, c] * 0.98f;

            // Camera orbit and zoom driven by audio
            float level = reactiveLevel;
            float camDist = 220f - level * 80f - beatPulse * 60f;
            Vector3 cam = new Vector3(0f, 60f + level * 60f, 120f + camDist);

            // Grid spacing
            float sx = 10f, sz = 10f, sy = 40f;

            // Draw as wireframe mesh with color gradient
            float hue = (colorHueBase + level * 60f + beatPulse * 90f) % 360f;
            using SKPaint paint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColor.FromHsv(hue, 80, 100) };

            // Simple projection relative to camera (translate world so cam moves)
            float fov = 60f;
            float scale = (float)(height / (2 * Math.Tan(fov / 2 * Math.PI / 180)));

            // Draw rows
            for (int r = 0; r < TerrainRows - 1; r++)
            {
                SKPath path = new SKPath();
                for (int c = 0; c < TerrainCols; c++)
                {
                    Vector3 world = new Vector3((c - TerrainCols / 2) * sx - cam.X, (-terrainHeights[r, c] * sy) - cam.Y, (r * sz) - cam.Z);
                    Vector2 p = ProjectSafe(world, scale, width, height);
                    if (c == 0) path.MoveTo(p.X, p.Y); else path.LineTo(p.X, p.Y);
                }
                canvas.DrawPath(path, paint);
            }
            // Draw columns
            for (int c = 0; c < TerrainCols; c += 2)
            {
                SKPath path = new SKPath();
                for (int r = 0; r < TerrainRows; r++)
                {
                    Vector3 world = new Vector3((c - TerrainCols / 2) * sx - cam.X, (-terrainHeights[r, c] * sy) - cam.Y, (r * sz) - cam.Z);
                    Vector2 p = ProjectSafe(world, scale, width, height);
                    if (r == 0) path.MoveTo(p.X, p.Y); else path.LineTo(p.X, p.Y);
                }
                canvas.DrawPath(path, paint);
            }
        }

        private void DrawNeonTunnel(SKCanvas canvas, int width, int height)
        {
            float[] localSpectrum;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); }
            // Compute centroid for color
            float wsum = 0f, tsum = 0f; for (int i = 0; i < localSpectrum.Length; i++) { wsum += localSpectrum[i] * i; tsum += localSpectrum[i]; }
            float centroid = (tsum > 0) ? wsum / tsum / Math.Max(1, localSpectrum.Length) : 0.5f;

            float level = reactiveLevel;
            float speed = 0.2f + level * 1.2f + beatPulse * 2.5f;
            float fov = 50f + level * 20f + beatPulse * 30f;
            tunnelPhase += speed * 3f;

            float scale = (float)(height / (2 * Math.Tan(fov / 2 * Math.PI / 180)));
            float hue = (colorHueBase + centroid * 360f + tunnelPhase * 2f) % 360f;

            using SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 2,
                Color = SKColor.FromHsv(hue, 90, 100)
            };

            // Draw receding squares forming a tunnel
            int rings = 24;
            for (int i = 0; i < rings; i++)
            {
                float z = i * 30f - (tunnelPhase % 30f) * 30f; // move forward
                float size = 120f + i * 25f;
                SKPath p = new SKPath();
                Vector2 p1 = ProjectSafe(new Vector3(-size, -size, z), scale, width, height);
                Vector2 p2 = ProjectSafe(new Vector3(size, -size, z), scale, width, height);
                Vector2 p3 = ProjectSafe(new Vector3(size, size, z), scale, width, height);
                Vector2 p4 = ProjectSafe(new Vector3(-size, size, z), scale, width, height);
                p.MoveTo(p1.X, p1.Y);
                p.LineTo(p2.X, p2.Y);
                p.LineTo(p3.X, p3.Y);
                p.LineTo(p4.X, p4.Y);
                p.Close();
                canvas.DrawPath(p, paint);
            }

            // Draw cross grid lines for neon effect
            using SKPaint gridPaint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColor.FromHsv((hue + 120f) % 360f, 80, 100) };
            for (int i = -4; i <= 4; i++)
            {
                SKPath line = new SKPath();
                Vector2 a = ProjectSafe(new Vector3(i * 50f, -200f, 0f), scale, width, height);
                Vector2 b = ProjectSafe(new Vector3(i * 50f, 200f, 300f), scale, width, height);
                line.MoveTo(a.X, a.Y);
                line.LineTo(b.X, b.Y);
                canvas.DrawPath(line, gridPaint);
            }
        }

        private void DrawMetaballs(SKCanvas canvas, int width, int height)
        {
            // Adjust count by audio level
            int target = Math.Clamp(20 + (int)(reactiveLevel * 60f) + (int)(beatPulse * 40f), 20, 120);
            while (metaballs.Count < target)
            {
                metaballs.Add(new Ball
                {
                    Position = new Vector2(random.Next(0, width), random.Next(0, height)),
                    Velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * (2f + reactiveLevel * 6f),
                    Radius = 10f + (float)random.NextDouble() * 20f,
                    Color = SKColor.FromHsv((colorHueBase + random.Next(0, 60)) % 360f, 80, 100)
                });
            }
            if (metaballs.Count > target) metaballs.RemoveRange(0, metaballs.Count - target);

            // Update
            float speedBoost = 1f + reactiveLevel * 2f + beatPulse * 3f;
            for (int i = 0; i < metaballs.Count; i++)
            {
                var b = metaballs[i];
                b.Position += b.Velocity * speedBoost;
                // wrap
                if (b.Position.X < -50) b.Position.X = width + 50;
                if (b.Position.X > width + 50) b.Position.X = -50;
                if (b.Position.Y < -50) b.Position.Y = height + 50;
                if (b.Position.Y > height + 50) b.Position.Y = -50;
                // pulsate on beat
                float pulse = 1f + beatPulse * 0.8f;
                b.Radius = MathF.Max(6f, b.Radius * (0.98f + reactiveLevel * 0.01f) * pulse);
                metaballs[i] = b;
            }

            // Render additive blurred circles to simulate metaballs
            using SKPaint paint = new SKPaint
            {
                Style = SKPaintStyle.Fill,
                BlendMode = SKBlendMode.Plus,
                MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 16f)
            };

            for (int i = 0; i < metaballs.Count; i++)
            {
                var b = metaballs[i];
                paint.Color = b.Color.WithAlpha(120);
                canvas.DrawCircle(b.Position.X, b.Position.Y, b.Radius, paint);
            }

            // Outline for definition
            using SKPaint outline = new SKPaint { Style = SKPaintStyle.Stroke, Color = SKColors.White.WithAlpha(30), StrokeWidth = 1 };
            for (int i = 0; i < metaballs.Count; i++)
            {
                var b = metaballs[i];
                canvas.DrawCircle(b.Position.X, b.Position.Y, b.Radius, outline);
            }
        }

        private void DrawBoids(SKCanvas canvas, int width, int height)
        {
            int target = Math.Clamp(150 + (int)(reactiveLevel * 300f), 150, 600);
            while (boids.Count < target)
            {
                boids.Add(new Particle
                {
                    Position = new Vector2(random.Next(0, width), random.Next(0, height)),
                    Velocity = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * 2f,
                    Size = 1.5f + (float)random.NextDouble() * 2f,
                    Hue = (colorHueBase + random.Next(0, 360)) % 360f,
                    Life = 0, // Initialize life to 0
                    MaxLife = 100 + (int)(reactiveLevel * 100f) + (int)(beatPulse * 50f) // Initial lifetime
                });
            }
            if (boids.Count > target) boids.RemoveRange(0, boids.Count - target);

            // Audio-driven attractor (bass point moves in a circle)
            float t = treePhase * 0.01f;
            Vector2 bassPoint = new Vector2(width / 2f + MathF.Cos(t) * 200f, height / 2f + MathF.Sin(t * 1.3f) * 120f);

            // Update particles
            for (int i = 0; i < boids.Count; i++)
            {
                var p = boids[i];
                p.Life++; // Increment life
                if (p.Life > p.MaxLife) // Remove if life is over
                {
                    boids.RemoveAt(i);
                    i--; // Adjust index after removal
                    continue;
                }

                Vector2 toBass = bassPoint - p.Position;
                float dist = MathF.Max(20f, toBass.Length());
                Vector2 dir = toBass / dist;

                // Acceleration terms
                Vector2 accel = dir * (0.5f + lowBandLevel * 6f + beatPulse * 10f);
                // cohesion to center
                accel += (new Vector2(width / 2f, height / 2f) - p.Position) * 0.0008f;
                // jitter from highs
                accel += new Vector2((float)(random.NextDouble() - 0.5), (float)(random.NextDouble() - 0.5)) * (0.5f + highBandLevel * 4f);

                p.Velocity += accel * 0.1f;
                // damping
                p.Velocity *= 0.96f;
                float maxSpeed = 2f + reactiveLevel * 6f + beatPulse * 8f;
                float spd = p.Velocity.Length();
                if (spd > maxSpeed) p.Velocity = p.Velocity * (maxSpeed / spd);

                p.Position += p.Velocity;
                // wrap
                if (p.Position.X < -10) p.Position.X = width + 10;
                if (p.Position.X > width + 10) p.Position.X = -10;
                if (p.Position.Y < -10) p.Position.Y = height + 10;
                if (p.Position.Y > height + 10) p.Position.Y = -10;

                // Color shift
                p.Hue = (p.Hue + highBandLevel * 6f + beatPulse * 12f) % 360f;
                boids[i] = p;
            }

            // Render as additive streaks
            using SKPaint paint = new SKPaint { Style = SKPaintStyle.Stroke, BlendMode = SKBlendMode.Screen };
            int stride = Math.Max(1, boids.Count / 800);
            for (int i = 0; i < boids.Count; i += stride)
            {
                var p = boids[i];
                paint.Color = SKColor.FromHsv(p.Hue, 90, 100).WithAlpha(140);
                paint.StrokeWidth = 1.2f + p.Size * (0.5f + reactiveLevel * 0.8f);
                Vector2 tail = p.Position - Vector2.Normalize(p.Velocity == Vector2.Zero ? new Vector2(1, 0) : p.Velocity) * (6f + reactiveLevel * 18f + beatPulse * 24f);
                canvas.DrawLine(p.Position.X, p.Position.Y, tail.X, tail.Y, paint);
            }
        }

        private void DrawCirclePacking(SKCanvas canvas, int width, int height)
        {
            // Initialize circles mapped to bands
            int bands = Math.Min(64, spectrum.Length);
            while (circles.Count < bands)
            {
                float t = circles.Count / (float)bands;
                Vector2 c = new Vector2(width * (0.1f + 0.8f * (float)random.NextDouble()), height * (0.1f + 0.8f * (float)random.NextDouble()));
                circles.Add(new Circle { Center = c, Radius = 5f, Target = 10f, Color = SKColor.FromHsv((colorHueBase + t * 360f) % 360f, 80, 100) });
            }

            // Update radii from spectrum and peaks
            float[] localSpectrum; float[] localPeaks;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); localPeaks = (float[])peakSpectrum.Clone(); }
            for (int i = 0; i < circles.Count; i++)
            {
                int idx = (int)(i / (float)circles.Count * (localSpectrum.Length - 1));
                float energy = localSpectrum[idx];
                float peak = localPeaks[idx];
                float bump = 1f + (peak > energy * 1.2f ? 1.0f : 0f) + beatPulse * 0.5f;
                float target = 8f + energy * 200f * bump;
                circles[i].Target = target;
                circles[i].Radius = circles[i].Radius * 0.85f + target * 0.15f;
                circles[i].Color = SKColor.FromHsv((colorHueBase + i * 6f) % 360f, 80, 100);
            }

            // Simple collision separation for packing
            for (int iter = 0; iter < 2; iter++)
            {
                for (int i = 0; i < circles.Count; i++)
                {
                    for (int j = i + 1; j < circles.Count; j++)
                    {
                        var a = circles[i]; var b = circles[j];
                        Vector2 d = b.Center - a.Center;
                        float dist = d.Length();
                        float minDist = a.Radius + b.Radius + 4f;
                        if (dist > 0 && dist < minDist)
                        {
                            Vector2 dir = d / dist;
                            float push = (minDist - dist) * 0.5f;
                            a.Center -= dir * push;
                            b.Center += dir * push;
                            circles[i] = a; circles[j] = b;
                        }
                    }
                }
            }

            // Draw with glow
            using SKPaint fill = new SKPaint { Style = SKPaintStyle.Fill, BlendMode = SKBlendMode.Plus, MaskFilter = SKMaskFilter.CreateBlur(SKBlurStyle.Normal, 10f) };
            using SKPaint stroke = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 2, Color = SKColors.White.WithAlpha(80) };
            foreach (var c in circles)
            {
                fill.Color = c.Color.WithAlpha(120);
                canvas.DrawCircle(c.Center.X, c.Center.Y, c.Radius, fill);
                canvas.DrawCircle(c.Center.X, c.Center.Y, c.Radius, stroke);
            }
        }

        private void DrawStarfield(SKCanvas canvas, int width, int height)
        {
            int target = 800;
            while (stars.Count < target)
            {
                stars.Add(new Star
                {
                    Pos = new Vector3((float)(random.NextDouble() * 2 - 1) * 200f, (float)(random.NextDouble() * 2 - 1) * 200f, (float)(random.NextDouble()) * 400f),
                    Speed = 0.5f + (float)random.NextDouble() * 1.5f,
                    Hue = (float)random.NextDouble() * 360f
                });
            }

            float loud = reactiveLevel;
            float speedScale = 1f + loud * 6f + beatPulse * 8f;

            // Approx stereo drift: use left vs right by sampling waveform even/odd
            float lr = 0f; int count = 0;
            var wave = waveformQueue.ToArray();
            for (int i = 0; i + 1 < wave.Length; i += 2) { lr += wave[i] - wave[i + 1]; count++; }
            float stereo = count > 0 ? ClampF(lr / count, -1f, 1f) : 0f;

            float fov = 60f; float scale = (float)(height / (2 * Math.Tan(fov / 2 * Math.PI / 180)));

            using SKPaint paint = new SKPaint { Style = SKPaintStyle.Stroke, StrokeCap = SKStrokeCap.Round };

            for (int i = 0; i < stars.Count; i++)
            {
                var s = stars[i];
                s.Pos.Z -= s.Speed * speedScale;
                s.Pos.X += stereo * 0.8f * s.Speed; // drift horizontally with stereo balance
                if (s.Pos.Z < 10f)
                {
                    s.Pos.Z = 400f;
                    s.Pos.X = (float)(random.NextDouble() * 2 - 1) * 200f;
                    s.Pos.Y = (float)(random.NextDouble() * 2 - 1) * 200f;
                }
                stars[i] = s;

                Vector2 p = ProjectSafe(s.Pos, scale, width, height);
                float depth = Math.Max(10f, s.Pos.Z);
                paint.Color = SKColor.FromHsv((s.Hue + colorHueBase) % 360f, 80, 100).WithAlpha(180);
                paint.StrokeWidth = Math.Max(1f, 6f / depth);
                Vector2 tail = ProjectSafe(new Vector3(s.Pos.X, s.Pos.Y, s.Pos.Z + 18f * speedScale), scale, width, height);
                canvas.DrawLine(p.X, p.Y, tail.X, tail.Y, paint);
            }
        }

        private void DrawCrtGlitch(SKCanvas canvas, int width, int height)
        {
            // Base background shimmer
            using SKPaint bg = new SKPaint { Color = SKColors.Black.WithAlpha(220) };
            canvas.DrawRect(0, 0, width, height, bg);

            // Scanlines
            using SKPaint scan = new SKPaint { Color = SKColors.White.WithAlpha(10) };
            for (int y = 0; y < height; y += 3)
            {
                canvas.DrawLine(0, y, width, y, scan);
            }

            // Vignette
            using SKPaint vignette = new SKPaint { Shader = SKShader.CreateRadialGradient(new SKPoint(width / 2f, height / 2f), Math.Max(width, height) / 1.2f, new[] { SKColors.Transparent, SKColors.Black.WithAlpha(180) }, new float[] { 0.7f, 1f }, SKShaderTileMode.Clamp) };
            canvas.DrawRect(0, 0, width, height, vignette);

            // Chromatic aberration by offsetting RGB channels on beat
            float shift = 2f + beatPulse * 12f;
            using SKPaint red = new SKPaint { Color = SKColors.Red.WithAlpha((byte)(40 + beatPulse * 80)) };
            using SKPaint green = new SKPaint { Color = SKColors.Green.WithAlpha((byte)(40 + beatPulse * 80)) };
            using SKPaint blue = new SKPaint { Color = SKColors.Blue.WithAlpha((byte)(40 + beatPulse * 80)) };

            // Edges
            canvas.DrawRect(-shift, 0, width, height, red);
            canvas.DrawRect(shift, 0, width, height, blue);
            canvas.DrawRect(0, -shift, width, height, green);

            // Random jitter rectangles on strong transients
            if (beatPulse > 0.6f)
            {
                int glitches = 3 + (int)(beatPulse * 6f);
                using SKPaint glitchPaint = new SKPaint { BlendMode = SKBlendMode.Difference, Color = SKColor.FromHsv((colorHueBase + beatPulse * 180f) % 360f, 80, 100).WithAlpha(120) };
                for (int i = 0; i < glitches; i++)
                {
                    int gw = random.Next(40, 160);
                    int gh = random.Next(8, 24);
                    int gx = random.Next(0, Math.Max(1, width - gw));
                    int gy = random.Next(0, Math.Max(1, height - gh));
                    canvas.DrawRect(gx, gy, gw, gh, glitchPaint);
                }
            }
        }

        private void DrawFractalKaleidoscope(SKCanvas canvas, int width, int height)
        {
            int texW = 256, texH = 256;
            using var bmp = new SKBitmap(texW, texH, true);
            float level = reactiveLevel;
            float zoom = 1f + level * 1.5f + beatPulse * 2.5f;
            float cx = (float)(Math.Sin(treePhase * 0.01f) * 0.3f);
            float cy = (float)(Math.Cos(treePhase * 0.013f) * 0.3f);

            // Spectral centroid for palette shift
            float wsum = 0f, tsum = 0f; for (int i = 0; i < spectrum.Length; i++) { wsum += spectrum[i] * i; tsum += spectrum[i]; }
            float centroid = (tsum > 0) ? wsum / tsum / Math.Max(1, spectrum.Length) : 0.5f;
            float hueBase = (colorHueBase + centroid * 360f) % 360f;

            for (int y = 0; y < texH; y++)
            {
                for (int x = 0; x < texW; x++)
                {
                    // Map to complex plane
                    float nx = (x - texW / 2f) / texW;
                    float ny = (y - texH / 2f) / texH;
                    nx /= zoom; ny /= zoom;
                    float zx = nx + cx, zy = ny + cy;
                    float cxx = -0.7f + level * 0.4f, cyy = 0.27015f + beatPulse * 0.2f;

                    int iter = 0; int maxIter = 40 + (int)(level * 60f);
                    while (zx * zx + zy * zy < 4f && iter < maxIter)
                    {
                        float xt = zx * zx - zy * zy + cxx;
                        zy = 2f * zx * zy + cyy;
                        zx = xt;
                        iter++;
                    }
                    float t = iter / (float)maxIter;
                    byte val = (byte)Math.Clamp(20 + (int)(t * 200), 0, 255);
                    var col = SKColor.FromHsv((hueBase + t * 180f) % 360f, 90, Math.Max((byte)30, val));
                    bmp.SetPixel(x, y, col);
                }
            }

            // Kaleidoscope mirror wedges
            canvas.Save();
            canvas.Translate(width / 2f, height / 2f);
            int wedges = 10;
            float angleStep = 360f / wedges;
            float rot = (treePhase * 0.15f + beatPulse * 8f) % 360f;
            var src = new SKRect(0, 0, texW, texH);
            var dst = new SKRect(-width / 2f, -height / 2f, width / 2f, height / 2f);

            for (int i = 0; i < wedges; i++)
            {
                canvas.Save();
                canvas.RotateDegrees(rot + i * angleStep);
                canvas.DrawBitmap(bmp, src, dst);
                canvas.Scale(-1, 1);
                canvas.DrawBitmap(bmp, src, dst);
                canvas.Restore();
            }
            canvas.Restore();
        }

        private void DrawVoronoi(SKCanvas canvas, int width, int height)
        {
            // Initialize sites mapped to bands
            int sites = 64;
            while (voronoiSites.Count < sites)
            {
                voronoiSites.Add(new Vector2((float)random.NextDouble() * width, (float)random.NextDouble() * height));
            }

            float[] localSpectrum;
            lock (dataLock) { localSpectrum = (float[])spectrum.Clone(); }

            // Jitter sites by band energy
            for (int i = 0; i < voronoiSites.Count; i++)
            {
                int idx = (int)(i / (float)voronoiSites.Count * (localSpectrum.Length - 1));
                float e = MathF.Min(1.5f, localSpectrum[idx] * 8f);
                Vector2 jitter = new Vector2((float)(random.NextDouble() * 2 - 1), (float)(random.NextDouble() * 2 - 1)) * (2f + e * 8f + beatPulse * 6f);
                var p = voronoiSites[i] + jitter;
                if (p.X < 0) p.X = width + p.X; if (p.X > width) p.X -= width;
                if (p.Y < 0) p.Y = height + p.Y; if (p.Y > height) p.Y -= height;
                voronoiSites[i] = p;
            }

            // Approximate edges by connecting each site to its nearest neighbors
            int k = 3;
            using SKPaint edge = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1.5f, Color = SKColors.White.WithAlpha((byte)(60 + beatPulse * 120)), BlendMode = SKBlendMode.Screen };
            using SKPaint fill = new SKPaint { Style = SKPaintStyle.Fill, BlendMode = SKBlendMode.Plus };

            for (int i = 0; i < voronoiSites.Count; i++)
            {
                var a = voronoiSites[i];
                // find k nearest
                List<(float d, int j)> neigh = new List<(float, int)>();
                for (int j = 0; j < voronoiSites.Count; j++) if (j != i)
                {
                    float dx = voronoiSites[j].X - a.X; float dy = voronoiSites[j].Y - a.Y; float d = dx * dx + dy * dy;
                    neigh.Add((d, j));
                }
                neigh.Sort((x, y) => x.d.CompareTo(y.d));
                // cell color
                float hue = (colorHueBase + i * 5f) % 360f;
                fill.Color = SKColor.FromHsv(hue, 70, 40).WithAlpha(60);
                // draw faint cell center glow
                canvas.DrawCircle(a.X, a.Y, 6f + beatPulse * 8f, fill);
                // draw edges to k nearest
                for (int n = 0; n < Math.Min(k, neigh.Count); n++)
                {
                    var b = voronoiSites[neigh[n].j];
                    canvas.DrawLine(a.X, a.Y, b.X, b.Y, edge);
                }
            }
        }

        #endif
        private Vector2 ProjectSafe(Vector3 point, float scale, int width, int height)
        {
            float z = point.Z;
            if (z < -90f) z = -90f; // avoid divide by near zero
            float factor = scale / (z + 100f);
            float x = point.X * factor + width / 2f;
            float y = -point.Y * factor + height / 2f;
            return new Vector2(x, y);
        }
    }
}