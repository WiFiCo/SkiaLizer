using System;
using System.Collections.Generic;
using System.Linq;
using SkiaSharp;

namespace SkiaLizer
{
    public static class PaletteManager
    {
        public static int SelectedPalette
        {
            get => ConfigManager.SelectedPalette;
            set => ConfigManager.SelectedPalette = value;
        }

        public static List<SKColor> CustomPalette
        {
            get
            {
                var customPalette = ConfigManager.LoadCustomPalette();
                return customPalette.Count > 0 ? customPalette : new List<SKColor>();
            }
            set
            {
                ConfigManager.SaveCustomPalette(value);
            }
        }

        // Predefined color palettes
        public static readonly Dictionary<string, SKColor[]> PredefinedPalettes = new Dictionary<string, SKColor[]>
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

        public static void SelectPalette()
        {
            var paletteNames = PredefinedPalettes.Keys.ToArray();
            var menuOptions = new List<string>();
            
            // Add predefined palettes
            foreach (var name in paletteNames)
            {
                menuOptions.Add(name + (SelectedPalette < paletteNames.Length && paletteNames[SelectedPalette] == name ? " (Current)" : ""));
            }
            
            // Add custom palette options
            var customPaletteColors = CustomPalette;
            menuOptions.Add($"Custom Palette {(SelectedPalette == paletteNames.Length ? "(Current) " : "")}({customPaletteColors.Count} colors)");
            menuOptions.Add("Create Custom Palette");
            menuOptions.Add("Edit Custom Palette");
            
            int sel = ConsoleMenu.ShowMenu("Select Color Palette:", menuOptions.ToArray(), SelectedPalette);
            if (sel == -1) return;
            
            if (sel < paletteNames.Length)
            {
                // Selected a predefined palette
                SelectedPalette = sel;
                Console.WriteLine($"Selected palette: {paletteNames[sel]}");
                ShowPalettePreview(PredefinedPalettes[paletteNames[sel]]);
            }
            else if (sel == paletteNames.Length)
            {
                // Selected existing custom palette
                if (customPaletteColors.Count > 0)
                {
                    SelectedPalette = paletteNames.Length;
                    Console.WriteLine($"Selected custom palette with {customPaletteColors.Count} colors");
                    ShowPalettePreview(customPaletteColors.ToArray());
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

        public static SKColor[] GetCurrentPalette()
        {
            var paletteNames = PredefinedPalettes.Keys.ToArray();
            
            if (SelectedPalette < paletteNames.Length)
            {
                // Return predefined palette
                return PredefinedPalettes[paletteNames[SelectedPalette]];
            }
            else
            {
                var customPaletteColors = CustomPalette;
                if (customPaletteColors.Count > 0)
                {
                    // Return custom palette
                    return customPaletteColors.ToArray();
                }
                else
                {
                    // Fallback to rainbow (default)
                    return PredefinedPalettes["Rainbow"];
                }
            }
        }

        private static void ShowPalettePreview(SKColor[] colors)
        {
            Console.WriteLine("\nPalette Preview:");
            for (int i = 0; i < colors.Length; i++)
            {
                var color = colors[i];
                
                // Create ANSI color block
                string colorBlock = CreateColorBlock(color);
                
                // Right-align the number for consistent spacing
                Console.WriteLine($"  {i + 1,2}: {colorBlock} #{color.Red:X2}{color.Green:X2}{color.Blue:X2} (R:{color.Red,3} G:{color.Green,3} B:{color.Blue,3})");
            }
        }

        private static string CreateColorBlock(SKColor color)
        {
            // Create a visual color block using ANSI escape codes
            try
            {
                return $"\u001b[48;2;{color.Red};{color.Green};{color.Blue}m    \u001b[0m";
            }
            catch
            {
                // Fallback if ANSI colors aren't supported
                return "████";
            }
        }

        private static void CreateCustomPalette()
        {
            Console.WriteLine("\n=== Create Custom Palette ===");
            Console.WriteLine("Enter hex colors (e.g., FF0000, #FF0000, or ff0000)");
            Console.WriteLine("Press Enter with empty input to finish");
            
            var newPalette = new List<SKColor>();
            
            while (true)
            {
                Console.Write($"Color {newPalette.Count + 1} (hex): ");
                string input = Console.ReadLine()?.Trim() ?? "";
                
                if (string.IsNullOrEmpty(input)) break;
                
                // Clean up input
                if (!input.StartsWith("#")) input = "#" + input;
                
                try
                {
                    var color = SKColor.Parse(input);
                    newPalette.Add(color);
                    Console.WriteLine($"Added: {input} -> R:{color.Red,3} G:{color.Green,3} B:{color.Blue,3}");
                }
                catch
                {
                    Console.WriteLine("Invalid hex color. Try again (e.g., FF0000 or #FF0000)");
                }
            }
            
            if (newPalette.Count > 0)
            {
                CustomPalette = newPalette;
                SelectedPalette = PredefinedPalettes.Keys.Count; // Set to custom palette
                Console.WriteLine($"\nCustom palette created with {newPalette.Count} colors!");
                ShowPalettePreview(newPalette.ToArray());
            }
        }

        private static void EditCustomPalette()
        {
            var currentPalette = CustomPalette;
            if (currentPalette.Count == 0)
            {
                Console.WriteLine("No custom palette exists. Create one first.");
                return;
            }
            
            Console.WriteLine("\n=== Edit Custom Palette ===");
            Console.WriteLine("Current palette:");
            ShowPalettePreview(currentPalette.ToArray());
            
            Console.WriteLine("\nOptions:");
            Console.WriteLine("[1] Add more colors");
            Console.WriteLine("[2] Remove a color");
            Console.WriteLine("[3] Clear all colors");
            Console.WriteLine("[4] Replace a color");
            Console.Write("Choice (1-4): ");
            
            string input = Console.ReadLine() ?? "";
            
            switch (input)
            {
                case "1":
                    AddMoreColors(currentPalette);
                    break;
                case "2":
                    RemoveColor(currentPalette);
                    break;
                case "3":
                    currentPalette.Clear();
                    CustomPalette = currentPalette;
                    Console.WriteLine("Custom palette cleared.");
                    break;
                case "4":
                    ReplaceColor(currentPalette);
                    break;
                default:
                    Console.WriteLine("Invalid choice.");
                    break;
            }
            
            if (currentPalette.Count > 0)
            {
                Console.WriteLine($"\nUpdated custom palette:");
                ShowPalettePreview(currentPalette.ToArray());
            }
        }

        private static void AddMoreColors(List<SKColor> palette)
        {
            Console.WriteLine("Add more colors (enter hex values, empty to finish):");
            
            while (true)
            {
                Console.Write($"Color {palette.Count + 1} (hex): ");
                string input = Console.ReadLine()?.Trim() ?? "";
                
                if (string.IsNullOrEmpty(input)) break;
                
                if (!input.StartsWith("#")) input = "#" + input;
                
                try
                {
                    var color = SKColor.Parse(input);
                    palette.Add(color);
                    Console.WriteLine($"Added: {input} -> R:{color.Red,3} G:{color.Green,3} B:{color.Blue,3}");
                }
                catch
                {
                    Console.WriteLine("Invalid hex color. Try again.");
                }
            }
            
            CustomPalette = palette;
        }

        private static void RemoveColor(List<SKColor> palette)
        {
            if (palette.Count == 0) return;
            
            Console.WriteLine("Select color to remove:");
            for (int i = 0; i < palette.Count; i++)
            {
                var color = palette[i];
                Console.WriteLine($"[{i + 1,2}] #{color.Red:X2}{color.Green:X2}{color.Blue:X2}");
            }
            Console.Write($"Enter number (1-{palette.Count}): ");
            
            if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= palette.Count)
            {
                palette.RemoveAt(index - 1);
                CustomPalette = palette;
                Console.WriteLine("Color removed.");
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }

        private static void ReplaceColor(List<SKColor> palette)
        {
            if (palette.Count == 0) return;
            
            Console.WriteLine("Select color to replace:");
            for (int i = 0; i < palette.Count; i++)
            {
                var color = palette[i];
                Console.WriteLine($"[{i + 1,2}] #{color.Red:X2}{color.Green:X2}{color.Blue:X2}");
            }
            Console.Write($"Enter number (1-{palette.Count}): ");
            
            if (int.TryParse(Console.ReadLine(), out int index) && index >= 1 && index <= palette.Count)
            {
                Console.Write("New hex color: ");
                string hex = Console.ReadLine()?.Trim() ?? "";
                if (!hex.StartsWith("#")) hex = "#" + hex;
                
                try
                {
                    var newColor = SKColor.Parse(hex);
                    palette[index - 1] = newColor;
                    CustomPalette = palette;
                    Console.WriteLine("Color replaced.");
                }
                catch
                {
                    Console.WriteLine("Invalid hex color.");
                }
            }
            else
            {
                Console.WriteLine("Invalid selection.");
            }
        }
    }
}