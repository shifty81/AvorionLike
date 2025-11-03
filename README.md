# AvorionLike - Custom Game Engine

A custom game engine implementation inspired by Avorion, built as a standalone Windows application using C# and .NET 9.0.

## Overview

AvorionLike is a custom-built game engine that replicates the core systems found in Avorion. It features a modular architecture with support for voxel-based ship building, Newtonian physics, procedural generation, multiplayer networking, and extensive modding capabilities through Lua scripting.

## Core Systems

### 1. Entity-Component System (ECS)
- Flexible architecture for managing game objects and their properties
- Efficient component storage and retrieval
- System-based update loop for processing entities

**Key Classes:**
- `Entity` - Represents a game object with a unique identifier
- `IComponent` - Interface for all components
- `EntityManager` - Manages entities and their components
- `SystemBase` - Base class for game systems

### 2. Voxel-Based Architecture
- Arbitrary-sized blocks for flexible ship and station construction
- Automatic center of mass and total mass calculation
- Collision detection between voxel blocks

**Key Classes:**
- `VoxelBlock` - Represents a single voxel with position, size, and material properties
- `VoxelStructureComponent` - Component containing voxel structure data for entities

### 3. Newtonian Physics System
- Realistic physics simulation with forces, acceleration, velocity
- Linear and rotational motion support
- Drag and collision detection
- Elastic collision response

**Key Classes:**
- `PhysicsComponent` - Component for physics properties
- `PhysicsSystem` - System that handles physics simulation

### 4. Procedural Generation
- Deterministic galaxy sector generation using seed-based algorithms
- Procedural asteroid fields with resource types
- Random station generation with various types
- Consistent generation based on coordinates

**Key Classes:**
- `GalaxyGenerator` - Generates galaxy sectors with asteroids and stations
- `GalaxySector` - Represents a sector in the galaxy
- `AsteroidData`, `StationData`, `ShipData` - Data structures for sector objects

### 5. Scripting API (Lua Integration)
- NLua-based scripting engine for modding support
- Register C# objects for Lua access
- Execute scripts and call Lua functions from C#
- Mod loading system

**Key Classes:**
- `ScriptingEngine` - Manages Lua scripting and mod loading

### 6. Networking/Multiplayer
- TCP-based client-server architecture
- Sector-based multiplayer with server-side sector management
- Multi-threaded sector handling for scalability
- Message-based communication protocol

**Key Classes:**
- `GameServer` - Main server for handling multiplayer connections
- `ClientConnection` - Represents a connected client
- `SectorServer` - Manages a single sector on the server
- `NetworkMessage` - Message structure for network communication

### 7. Resource and Inventory Management
- Multiple resource types (Iron, Titanium, Naonite, etc.)
- Inventory system with capacity limits
- Crafting system for ship upgrades
- Subsystem upgrades (shields, weapons, cargo)

**Key Classes:**
- `Inventory` - Manages resource storage
- `InventoryComponent` - Component for entity inventory
- `CraftingSystem` - Handles crafting of upgrades
- `SubsystemUpgrade` - Represents a ship upgrade

### 8. RPG Elements
- Ship progression with experience and levels
- Faction relations and reputation system
- Loot drop system
- Trading system with buy/sell mechanics

**Key Classes:**
- `ProgressionComponent` - Manages entity progression
- `FactionComponent` - Handles faction relations
- `LootSystem` - Generates loot drops
- `TradingSystem` - Manages resource trading

### 9. Development Tools
- **Debug Renderer** - Debug visualization for game objects and physics
- **Performance Profiler** - FPS and frame timing tracking
- **Memory Tracker** - Memory usage monitoring (including GPU when available)
- **OpenGL Debugger** - Error detection and logging for graphics
- **Debug Console** - Runtime command console (press `` ` `` key)
- **Script Compiler** - Runtime script compilation and hot-reloading

**Key Classes:**
- `DevToolsManager` - Manages all development tools
- `DebugRenderer` - Visual debug rendering
- `PerformanceProfiler` - Performance metrics tracking
- `MemoryTracker` - Memory usage monitoring
- `OpenGLDebugger` - OpenGL error tracking
- `DebugConsole` - Interactive debug console
- `ScriptCompiler` - Runtime script execution

## Getting Started

### Prerequisites

#### For Visual Studio 2022 Users (Recommended)
- **Visual Studio 2022** (Community, Professional, or Enterprise)
  - Download: https://visualstudio.microsoft.com/vs/
  - Required Workloads: .NET desktop development
- **.NET 9.0 SDK or later** (included with Visual Studio 2022)
- **Windows** (for Visual Studio 2022)

#### For Command Line / Other IDEs
- **.NET 9.0 SDK or later**
  - Download: https://dotnet.microsoft.com/download
- **Windows, Linux, or macOS**
- Any text editor or IDE (VS Code, Rider, etc.)

**Note:** The current implementation uses a cross-platform console interface. For a Windows-specific GUI version using Windows Forms, modify the `.csproj` file to target `net9.0-windows` and enable Windows Forms by adding `<UseWindowsForms>true</UseWindowsForms>` to the PropertyGroup section. This requires building on a Windows machine.

### Building with Visual Studio 2022

1. **Clone the repository**
   ```bash
   git clone https://github.com/shifty81/AvorionLike.git
   cd AvorionLike
   ```

2. **Open the solution**
   - Open `AvorionLike.sln` in Visual Studio 2022
   - The solution will automatically restore NuGet packages

3. **Build and Run**
   - Press `F5` to build and run with debugging
   - Or press `Ctrl+F5` to run without debugging
   - Or use Build → Build Solution (Ctrl+Shift+B)

### Building with Command Line

```bash
# Clone the repository
git clone https://github.com/shifty81/AvorionLike.git
cd AvorionLike

# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run --project AvorionLike
```

### Running the Application

The application provides an interactive console menu with various demos:

1. **Engine Demo** - Create a test ship with voxel structure, physics, and inventory
2. **Voxel System Demo** - Build custom ship structures
3. **Physics Demo** - Simulate Newtonian physics
4. **Procedural Generation** - Generate galaxy sectors
5. **Resource Management** - Manage inventory and crafting
6. **RPG Systems** - Trading, progression, and loot
7. **Scripting** - Execute Lua scripts
8. **Multiplayer** - Start multiplayer server
9. **Statistics** - View engine statistics
10. **Development Tools** - Access debug console and profiling tools

#### Using Development Tools

Press the backtick key (`` ` ``) during runtime to open the debug console. Available console commands:

- `help` - Show all available commands
- `fps` - Display current FPS and frame timing
- `profile` - Generate performance profile report
- `memory` - Show memory usage statistics
- `glerrors` - Display OpenGL errors (when rendering is active)
- `scripts` - List loaded Lua scripts
- `debug` - Toggle debug rendering
- `devtools` - Show status of all development tools
- `compile <file>` - Compile and load a Lua script
- `reload <script>` - Reload a previously loaded script
- `lua <code>` - Execute Lua code directly
- `gc` - Force garbage collection
- `clear` - Clear console output
- `exit` - Close the debug console

## Architecture

```
AvorionLike/
├── Core/
│   ├── ECS/              # Entity-Component System
│   │   ├── Entity.cs
│   │   ├── IComponent.cs
│   │   ├── EntityManager.cs
│   │   └── SystemBase.cs
│   ├── Voxel/            # Voxel-based architecture
│   │   ├── VoxelBlock.cs
│   │   └── VoxelStructureComponent.cs
│   ├── Physics/          # Physics system
│   │   ├── PhysicsComponent.cs
│   │   └── PhysicsSystem.cs
│   ├── Procedural/       # Procedural generation
│   │   └── GalaxyGenerator.cs
│   ├── Scripting/        # Lua scripting API
│   │   └── ScriptingEngine.cs
│   ├── Networking/       # Multiplayer networking
│   │   └── GameServer.cs
│   ├── Resources/        # Resource and inventory management
│   │   ├── Inventory.cs
│   │   └── CraftingSystem.cs
│   ├── RPG/              # RPG elements
│   │   └── RPGSystems.cs
│   ├── DevTools/         # Development tools
│   │   ├── DevToolsManager.cs
│   │   ├── DebugRenderer.cs
│   │   ├── PerformanceProfiler.cs
│   │   ├── MemoryTracker.cs
│   │   ├── OpenGLDebugger.cs
│   │   ├── DebugConsole.cs
│   │   └── ScriptCompiler.cs
│   └── GameEngine.cs     # Main engine class
├── Program.cs            # Application entry point
└── AvorionLike.csproj    # Project configuration
```

## Example Usage

### Creating a Ship Entity

```csharp
var engine = new GameEngine(12345);
engine.Start();

// Create entity
var ship = engine.EntityManager.CreateEntity("Player Ship");

// Add voxel structure
var voxelComponent = new VoxelStructureComponent();
voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 0, 0), new Vector3(2, 2, 2), "Iron"));
engine.EntityManager.AddComponent(ship.Id, voxelComponent);

// Add physics
var physicsComponent = new PhysicsComponent
{
    Position = new Vector3(100, 100, 100),
    Mass = voxelComponent.TotalMass
};
engine.EntityManager.AddComponent(ship.Id, physicsComponent);

// Update engine (call in game loop)
engine.Update();
```

### Using the Scripting API

```csharp
var engine = new GameEngine();

// Execute Lua script
engine.ExecuteScript(@"
    function createShip(name)
        log('Creating ship: ' .. name)
        -- Access engine from Lua
        return name
    end
");

// Call Lua function
var result = engine.ScriptingEngine.CallFunction("createShip", "MyShip");
```

### Starting Multiplayer Server

```csharp
var engine = new GameEngine();
engine.StartServer(27015); // Start on port 27015
```

## Technologies Used

- **C# / .NET 9.0** - Core programming language and framework
- **NLua (v1.7.3)** - Lua scripting integration for modding
- **System.Numerics** - Vector math for physics and positions
- **System.Net.Sockets** - TCP networking for multiplayer
- **Visual Studio 2022** - Primary development environment

For detailed credits and acknowledgments, see [CREDITS.md](CREDITS.md).

## Features

✅ Entity-Component System (ECS) architecture  
✅ Voxel-based ship/station building  
✅ Newtonian physics simulation  
✅ Procedural galaxy generation  
✅ Lua scripting for modding  
✅ TCP multiplayer networking  
✅ Resource and inventory management  
✅ Crafting system  
✅ RPG progression and faction systems  
✅ Trading system  
✅ Loot generation  
✅ Development tools (Debug Console, Profiler, Memory Tracker)  
✅ Runtime script compilation and hot-reloading  
✅ Visual Studio 2022 solution support  

## Future Enhancements

- Graphics rendering (OpenGL/DirectX)
- Advanced collision detection with voxel geometry
- More complex procedural generation algorithms
- Save/load game state
- Advanced AI systems
- More RPG features (quests, dialog systems)
- Steam Workshop integration
- Performance optimizations for large-scale multiplayer

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Acknowledgments

Inspired by the game [Avorion](https://www.avorion.net/) developed by Boxelware.

For detailed credits and acknowledgments of all libraries and inspirations used in this project, please see [CREDITS.md](CREDITS.md).

**Note:** This project is not affiliated with, endorsed by, or connected to Boxelware or the official Avorion game. This is a fan-made educational implementation.

## Contributing

Contributions are welcome! Please see [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines on:
- Setting up your development environment with Visual Studio 2022
- Code style and conventions
- How to submit changes
- Development workflow

For questions or feedback, please open an issue on GitHub.
