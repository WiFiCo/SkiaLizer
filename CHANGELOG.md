# Changelog

All notable changes to SkiaLizer will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.1.0] - 2024-12-19

### Added
- **Configuration System**: Persistent settings stored in `SkiaLizer.cfg`
- **Automated Releases**: GitHub Actions workflows for building and releasing
- **Enhanced Settings Menu**: Back navigation and toggle state display
- **Reset Settings**: Option to restore default configuration
- **Color Preview**: ANSI color blocks in palette preview
- **Project Organization**: Proper folder structure following .NET best practices

### Fixed
- **Transparency Bug**: Window no longer forces always-on-top when transparency is enabled
- **Waveform Colors**: Now properly uses selected color palette instead of hardcoded colors
- **Palette Alignment**: Consistent formatting in color previews
- **Settings Persistence**: All settings now save automatically when changed

### Changed
- **File Organization**: Moved source files to appropriate subdirectories
  - `src/Core/` - Main application components
  - `src/Visualizers/` - All visualization implementations
  - `src/UI/` - Menu and interface components
  - `src/Configuration/` - Settings and config management
- **Documentation**: Added comprehensive project structure documentation

## [1.0.0] - Initial Release

### Added
- **Audio Visualization**: Multiple visualization modes
  - Spectrum Bars
  - Waveform
  - Radial Spectrum
  - 3D Audio Terrain
  - Fractal Visualizations
  - Particle Systems
  - And many more
- **Color Palettes**: Predefined and custom color schemes
- **Transparency Mode**: Desktop overlay support
- **Always-on-Top**: Window positioning options
- **Fullscreen Support**: Immersive visualization mode
- **Hotkey Controls**: Keyboard shortcuts for quick adjustments
- **Audio Device Selection**: Choose input source
- **Window Size Options**: Multiple resolution presets
