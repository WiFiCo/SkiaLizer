# SkiaLizer Project Structure

This document describes the organization of the SkiaLizer codebase.

## Directory Structure

```
SkiaLizer/
├── .github/                    # GitHub Actions workflows
│   └── workflows/
│       ├── build.yml          # Build testing on pushes
│       └── release.yml        # Automated releases
├── docs/                      # Documentation
│   ├── PROJECT_STRUCTURE.md   # This file
│   └── RELEASE_GUIDE.md       # How to create releases
├── src/                       # Source code
│   ├── Configuration/         # Settings and config management
│   │   ├── ConfigManager.cs   # JSON config file handling
│   │   ├── PaletteManager.cs  # Color palette management
│   │   └── SettingsManager.cs # Application settings
│   ├── Core/                  # Core application components
│   │   └── VisualizerForm.cs  # Main visualization form
│   ├── UI/                    # User interface components
│   │   ├── ConsoleMenu.cs     # Console menu utilities
│   │   └── MenuSystem.cs      # Main menu system
│   └── Visualizers/           # Audio visualization implementations
│       ├── vf.AudioTerrain.cs # 3D terrain visualization
│       ├── vf.Boids.cs        # Flocking simulation
│       ├── vf.CirclePacking.cs# Circle packing algorithm
│       ├── vf.CrtGlitch.cs    # CRT/glitch effects
│       ├── vf.FractalKaleidoscope.cs # Kaleidoscopic fractals
│       ├── vf.FractalTree.cs  # Tree fractal visualization
│       ├── vf.Kaleidoscope.cs # Kaleidoscope effects
│       ├── vf.Metaballs.cs    # Metaball simulation
│       ├── vf.NeonTunnel.cs   # Neon tunnel effect
│       ├── vf.Pipes3D.cs      # 3D pipe visualization
│       ├── vf.Plasma.cs       # Plasma effect
│       ├── vf.RadialSpectrum.cs # Radial spectrum display
│       ├── vf.SpectrumBars.cs # Traditional spectrum bars
│       ├── vf.Starfield.cs    # Starfield simulation
│       ├── vf.Voronoi.cs      # Voronoi diagram
│       └── vf.Waveform.cs     # Waveform display
├── Program.cs                 # Application entry point
├── SkiaLizer.csproj          # Project configuration
├── README.md                 # Project overview
├── LICENSE                   # License information
└── .gitignore               # Git ignore rules
```

## Architecture Overview

### Core Components

- **Program.cs**: Entry point, initializes the application and menu system
- **VisualizerForm.cs**: Main Windows Forms application that handles audio capture and rendering

### Configuration System

- **ConfigManager.cs**: Handles persistent storage of settings in `SkiaLizer.cfg`
- **SettingsManager.cs**: Provides high-level settings management
- **PaletteManager.cs**: Manages color palettes (predefined and custom)

### User Interface

- **MenuSystem.cs**: Main menu navigation and options
- **ConsoleMenu.cs**: Console-based menu utilities

### Visualizers

All visualizer files follow the pattern `vf.{Name}.cs` and are partial classes of `VisualizerForm`:

- Each visualizer implements a specific `Draw{Name}` method
- Visualizers use SkiaSharp for 2D/3D rendering
- Audio data is provided through spectrum analysis and waveform data

## Design Patterns

- **Partial Classes**: `VisualizerForm` is split across multiple files for maintainability
- **Static Managers**: Configuration and settings use static classes for global access
- **Strategy Pattern**: Different visualizers implement the same interface pattern
- **Factory Pattern**: Visualizer selection through index-based switching

## Dependencies

- **.NET 8.0**: Target framework
- **SkiaSharp**: 2D/3D graphics rendering
- **NAudio**: Audio capture and processing
- **Windows Forms**: UI framework

## Build System

- **GitHub Actions**: Automated testing and releases
- **Self-contained**: Builds include .NET runtime for distribution
- **Multi-platform**: Supports both x86 and x64 Windows
