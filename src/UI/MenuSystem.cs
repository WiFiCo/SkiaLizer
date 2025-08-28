using System;
using System.Collections.Generic;
using NAudio.CoreAudioApi;

namespace SkiaLizer
{
    public static class MenuSystem
    {
        private static MMDevice? selectedDevice = null;
        private static List<MMDevice> devices = new List<MMDevice>();

        public static int SelectedVisual 
        { 
            get => ConfigManager.SelectedVisual;
            set => ConfigManager.SelectedVisual = value;
        }
        
        public static MMDevice? SelectedDevice => selectedDevice;
        public static List<MMDevice> Devices => devices;

        public static void InitializeAudioDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            foreach (var dev in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                devices.Add(dev);
            }
            selectedDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
        }

        public static int ShowMainMenu()
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
            int choice = ConsoleMenu.ShowMenu("=== SkiaLizer - Audio Visualizer by WiFiPunk ===", menuOptions);

            if (choice == -1) return 5; // Exit

            switch (choice)
            {
                case 0:
                    return 0; // Start visualizer
                case 1:
                    SelectVisual();
                    break;
                case 2:
                    SelectSource();
                    break;
                case 3:
                    PaletteManager.SelectPalette();
                    break;
                case 4:
                    SettingsManager.ShowSettings();
                    break;
                case 5:
                    return 5; // Exit
            }
            
            return -1; // Continue menu loop
        }

        public static void SelectVisual()
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
            int sel = ConsoleMenu.ShowMenu("Select Visual:", visuals, SelectedVisual);
            if (sel == -1) return;
            SelectedVisual = sel;
        }

        public static void SelectSource()
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

        public static void EnsureDeviceSelected()
        {
            if (selectedDevice == null)
            {
                var enumerator = new MMDeviceEnumerator();
                selectedDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            }
        }


    }
}
