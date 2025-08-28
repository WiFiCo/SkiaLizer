# SkiaLizer - Audio Visualizer

<div align="center">

![SkiaLizer Logo](https://img.shields.io/badge/SkiaLizer-Audio%20Visualizer-brightgreen?style=for-the-badge)
[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-blue?style=for-the-badge)](LICENSE)
[![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey?style=for-the-badge)](https://www.microsoft.com/windows)

**A lightweight audio visualizer for DJs, streamers, and music lovers**

[Features](#features) • [Installation](#installation) • [Usage](#usage) • [Visualizations](#visualizations) • [Contributing](#contributing)

</div>

---

SkiaLizer is built with a clear mission: **provide quality audio visualization with minimal system impact**. Whether you're a DJ performing live, a streamer creating content, or a hobbyist enjoying music at home, SkiaLizer adapts to your needs without consuming excessive system resources.

## Why SkiaLizer?

- **Lightweight** - Optimized for performance
- **Versatile** - 16+ different visualization modes with customizable palettes
- **Stream-ready** - Transparency support

---

## Features

### Audio Processing
- Real-time FFT analysis with beat detection
- Works with any system audio device
- Low-latency response
- Pauses when audio stops

### Visuals
- 16 built-in visualization modes
- 8 color palettes plus custom palette creator
- 60 FPS smooth animation
- Audio-reactive timing

### Display Options
- Transparency mode for streaming overlays
- Fullscreen support
- Always on top option
- Custom window sizes from 240p to 8K

---

## Installation

**Requirements:** Windows 10/11 and .NET 8.0 Runtime

1. Download the latest release
2. Extract and run `SkiaLizer.exe`
3. Pick your audio source and go

**Build from source:**
```bash
git clone https://github.com/yourusername/SkiaLizer.git
cd SkiaLizer
dotnet restore && dotnet build && dotnet run
```

---

## Usage

The menu is pretty straightforward:
- **Start Visualizer**
- **Select Visual**
- **Select Audio Source**
- **Select Palette**
- **Settings**

### Hotkeys During Visualization

> **Key Controls:**
> - **F11** - Toggle Fullscreen
> - **Ctrl + T** - Toggle Transparency (great for OBS)
> - **Ctrl + A** - Toggle Always On Top
> - **Escape** - Back to Menu

---

## Visualizations

| Mode | What It Does | Good For |
|------|-------------|----------|
| **Spectrum Bars** | Classic frequency bars | General music, live sets |
| **Waveform** | Audio waveform display | Vocals, podcasts |
| **Radial Spectrum** | Circular frequency display | Electronic music |
| **Fractal Tree** | Branching patterns | Organic music |
| **3D Pipes** | Flowing tube structures | Trance, progressive |
| **Kaleidoscope** | Symmetric patterns | Psychedelic stuff |
| **Audio Terrain** | 3D landscape | Cinematic music |
| **Neon Tunnel** | Synthwave tunnel effect | Retro, synthwave |
| **Liquid Metaballs** | Flowing blobs | Ambient, chill |
| **Particle Boids** | Flocking particles | Complex rhythms |
| **Circle Packing** | Bubble patterns | Acoustic, indie |
| **Hyperwarp** | Starfield effect | High-energy EDM |
| **CRT/Glitch** | Retro TV effects | Lo-fi, experimental |
| **Fractal Kaleidoscope** | Math patterns | Minimal, ambient |
| **Voronoi Cells** | Cellular patterns | Progressive, techno |
| **Plasma Swirls** | Energy flow | Trance, ethereal |

---

## Color Palettes

**Built-in options:**
- **Rainbow** - Classic spectrum
- **Neon** - Bright electric colors
- **Ocean** - Blues and teals
- **Fire** - Reds and oranges
- **Sunset** - Purple to yellow
- **Synthwave** - 80s retro
- **Monochrome** - Grayscale
- **Forest** - Natural greens

**Custom palettes:** Interactive color picker with RGB sliders and hex input.



---

## Technical Details

**Built with:**
- C# .NET 8.0
- SkiaSharp for graphics
- NAudio for audio processing
- Windows Forms

**Performance:**
- CPU usage under 5%
- Memory under 100MB
- Sub-20ms audio latency


**Requirements:**
- Windows 10/11 (x64)
- 4GB RAM (8GB recommended)
- Any modern CPU
- DirectX 11 GPU (integrated is fine)

---




---

## License

MIT License - do what you want with it.

---

## Thanks

- SkiaSharp team for the graphics framework
- NAudio project for audio processing
- Everyone who's tested and given feedback

---

<div align="center">

**Made for music lovers**

[Star this repo](../../stargazers) • [Report Bug](../../issues) • [Request Feature](../../issues)

</div>