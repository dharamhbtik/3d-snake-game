# Snake 3D (Uno Platform)

A production-quality 3D desktop version of the classic Snake game built with **Uno Platform** and **SkiaSharp** targeting **macOS**, **Windows**, and **Linux** desktop environments.

---

## Features

- **Built on .NET 10**: Modern C# 13 / .NET 10.0 runtime targeting macOS, Windows, and Linux.
- **Custom High-Fidelity 3D Rendering Pipeline**:
  - Elevated perspective camera with dynamic tracking and menu orbit showcase.
  - Multi-faceted 3D geometry for grass lawn tiles, dense swaying 3D grass tufts, blooming meadow flowers, and stone perimeter walls.
  - Continuous 10-sided tubular snake mesh with natural lateral serpentine slither waves and progressive anatomical tapering.
  - Food digestion lump animation that travels down the spine as food is consumed.
  - Sculpted viper head with amber glass eyes, slit pupils, nostrils, and 2-phase flicking fork tongue.
  - Blinn-Phong specular lighting and dynamic Blinn-Phong highlights.
  - High-poly sculpted 3D Apple with wooden stem and green leaf (+ glowing gilded Golden Apple).
  - Floating 3D ambient fireflies and celebration particle bursts with floor bounce physics.
- **Speed & Difficulty Control**:
  - 3 Selectable speed modes: Relaxed (🐢), Normal (🎯), and Fast (⚡).
  - Persistent High Score and speed preferences.
- **Cross-Platform Persistence**:
  - Local JSON storage saving High Score and preferences across macOS, Linux, and Windows.
- **Audio Feedback**:
  - Modular audio feedback with mute toggle.
- **Automated Test Suite**:
  - Comprehensive unit tests covering snake initialization, direction changes, reversal prevention, key buffering, food spawning, growth, collisions, speed progression, and state transitions.

---

## Architecture

```
Snake3D.sln
├── Snake3D.Core/           # Portable .NET 9 domain model (zero UI dependencies)
│   ├── Direction.cs        # Cardinal directions & 180° reversal prevention
│   ├── GridPoint.cs        # 2D board coordinates & bounds checks
│   ├── GameBoard.cs        # Grid dimensions & collision-free food spawner
│   ├── Snake.cs            # Body segments, movement step & input buffer
│   ├── Food.cs             # Normal and Golden food models
│   ├── GameState.cs        # State machine enum (Menu, Playing, Paused, GameOver)
│   └── GameEngine.cs       # Tick loop, accumulator, score & speed curve
│
├── Snake3D/                # Uno Platform Desktop application
│   ├── Rendering/          # 3D Graphics Engine
│   │   ├── Camera3D.cs         # 3D View/Projection matrices & screen mapping
│   │   ├── Lighting3D.cs       # Diffuse, ambient & Blinn-Phong specular shading
│   │   ├── Polygon3D.cs        # 3D faces, backface culling & depth sorting
│   │   ├── MeshBuilder.cs      # 3D geometry generator (snake, board, food, head)
│   │   ├── ParticleSystem3D.cs # 3D particle physics & bursts
│   │   └── GameRenderer3D.cs   # Main 3D rendering pipeline
│   ├── ViewModels/         # MVVM Presentation (MainViewModel.cs)
│   ├── Services/           # HighScoreService.cs & GameAudioService.cs
│   ├── Converters/         # BoolToVisibilityConverter.cs
│   ├── MainPage.xaml       # Skia XAML canvas host & modern Fluent dark HUD
│   └── Platforms/Desktop/  # Multi-platform desktop host (macOS, Linux, Windows)
│
└── Snake3D.Tests/          # xUnit automated test suite
    ├── SnakeTests.cs       # Movement, length, reversal prevention & buffering
    ├── GameBoardTests.cs   # Bounds checks & free cell allocation
    └── GameEngineTests.cs  # Scoring, growth, collisions, speed & state transitions
```

---

## Controls

| Key | Action |
|---|---|
| **W / Up Arrow** | Move Up / North |
| **S / Down Arrow** | Move Down / South |
| **A / Left Arrow** | Move Left / West |
| **D / Right Arrow** | Move Right / East |
| **Space** | Pause / Resume / Start |
| **Enter** | Start / Restart |
| **Escape** | Pause / Return to Menu |

---

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

---

## How to Build & Run

### macOS Desktop (Skia Desktop)

```bash
# Build the project
dotnet build Snake3D.sln

# Run the game
dotnet run --project Snake3D/Snake3D.csproj -f net9.0-desktop
```

### Linux Desktop (X11 / Wayland / FrameBuffer)

```bash
dotnet run --project Snake3D/Snake3D.csproj -f net9.0-desktop
```

### Windows Desktop (WinUI / Windows App SDK or Skia)

```bash
# Run via Skia Desktop
dotnet run --project Snake3D/Snake3D.csproj -f net9.0-desktop

# Or run via native WinUI 3 (on Windows with Windows SDK installed)
dotnet run --project Snake3D/Snake3D.csproj -f net9.0-windows10.0.26100
```

---

## Running Automated Tests

```bash
dotnet test
```
