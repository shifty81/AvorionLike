using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Events;
using AvorionLike.Core.Mining;
using AvorionLike.Core.Procedural;
using AvorionLike.Core.Graphics;
using AvorionLike.Core.Combat;
using AvorionLike.Core.Physics;

namespace AvorionLike.Examples;

/// <summary>
/// Comprehensive example demonstrating the space exploration and mining system
/// </summary>
public class SpaceExplorationExample
{
    private EntityManager _entityManager = null!;
    private EventSystem _eventSystem = null!;
    private MiningSystem _miningSystem = null!;
    private PhysicsSystem _physicsSystem = null!;
    private DestructionSystem _destructionSystem = null!;
    private WorldManager _worldManager = null!;
    private RenderingManager _renderingManager = null!;
    private LightingSystem _lightingSystem = null!;
    private VisualEffectsSystem _effectsSystem = null!;
    
    public void RunExample()
    {
        Console.WriteLine("=== Space Exploration & Mining System Demo ===\n");
        
        // Initialize all systems
        InitializeSystems();
        
        // Create a player ship
        var playerShip = CreatePlayerShip(new Vector3(0, 0, 0));
        Console.WriteLine($"✓ Created player ship at origin");
        
        // Generate the game world
        GenerateWorld(playerShip);
        
        // Run simulation
        RunSimulation(playerShip);
        
        // Display statistics
        DisplayStatistics();
        
        // Cleanup
        Shutdown();
        
        Console.WriteLine("\n=== Demo Complete ===");
    }
    
    private void InitializeSystems()
    {
        Console.WriteLine("Initializing systems...");
        
        // Core systems
        _entityManager = new EntityManager();
        _eventSystem = new EventSystem();
        _miningSystem = new MiningSystem(_entityManager);
        _physicsSystem = new PhysicsSystem(_entityManager);
        _destructionSystem = new DestructionSystem(_entityManager, _eventSystem);
        
        // World generation and rendering
        _worldManager = new WorldManager(
            _entityManager,
            _miningSystem,
            seed: 12345,
            chunkSize: 100
        );
        
        _renderingManager = new RenderingManager(
            _worldManager.GetChunkManager(),
            meshThreadCount: 2
        );
        
        // Lighting and effects
        _lightingSystem = new LightingSystem();
        _effectsSystem = new VisualEffectsSystem();
        
        // Add a star light source
        var starLight = LightingSystem.CreateStarLight(
            new Vector3(5000, 5000, 5000),
            new Vector3(1.0f, 0.95f, 0.8f), // Warm white
            intensity: 2.0f
        );
        _lightingSystem.AddLight(starLight);
        
        Console.WriteLine("✓ All systems initialized\n");
    }
    
    private Guid CreatePlayerShip(Vector3 position)
    {
        var ship = _entityManager.CreateEntity("Player Ship");
        
        // Add voxel structure (simple ship design)
        var voxelComponent = new Core.Voxel.VoxelStructureComponent();
        
        // Create a basic ship shape
        for (int x = -2; x <= 2; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -3; z <= 3; z++)
                {
                    var block = new Core.Voxel.VoxelBlock(
                        position + new Vector3(x * 2, y * 2, z * 2),
                        new Vector3(2, 2, 2),
                        "Titanium",
                        Core.Voxel.BlockType.Hull
                    );
                    voxelComponent.AddBlock(block);
                }
            }
        }
        
        // Add engine blocks
        for (int i = -1; i <= 1; i++)
        {
            var engine = new Core.Voxel.VoxelBlock(
                position + new Vector3(i * 2, 0, -8),
                new Vector3(2, 2, 2),
                "Titanium",
                Core.Voxel.BlockType.Engine
            );
            voxelComponent.AddBlock(engine);
        }
        
        _entityManager.AddComponent(ship.Id, voxelComponent);
        
        // Add physics
        var physicsComponent = new Core.Physics.PhysicsComponent
        {
            Position = position,
            Mass = voxelComponent.TotalMass,
            MaxThrust = voxelComponent.TotalThrust,
            MaxTorque = voxelComponent.TotalTorque,
            CollisionRadius = 10f
        };
        _entityManager.AddComponent(ship.Id, physicsComponent);
        
        // Add mining capability
        var miningComponent = new Core.Mining.MiningComponent
        {
            MiningPower = 20f,
            MiningRange = 100f
        };
        _entityManager.AddComponent(ship.Id, miningComponent);
        
        // Add inventory
        var inventoryComponent = new Core.Resources.InventoryComponent
        {
            Inventory = new Core.Resources.Inventory { MaxCapacity = 10000 }
        };
        _entityManager.AddComponent(ship.Id, inventoryComponent);
        
        return ship.Id;
    }
    
    private void GenerateWorld(Guid playerShipId)
    {
        Console.WriteLine("Generating world around player...");
        
        var physics = _entityManager.GetComponent<Core.Physics.PhysicsComponent>(playerShipId);
        if (physics == null) return;
        
        // Generate asteroids in a radius around the player
        _worldManager.GenerateAsteroidsInRadius(physics.Position, 500f);
        
        Console.WriteLine($"✓ World generation complete");
        Console.WriteLine($"  - Asteroids: {_miningSystem.GetAsteroids().Count()}");
        Console.WriteLine();
    }
    
    private void RunSimulation(Guid playerShipId)
    {
        Console.WriteLine("Running simulation (10 seconds)...\n");
        
        float deltaTime = 0.1f; // 100ms per tick
        int ticks = 100; // 10 seconds
        
        var physics = _entityManager.GetComponent<Core.Physics.PhysicsComponent>(playerShipId);
        if (physics == null) return;
        
        for (int i = 0; i < ticks; i++)
        {
            // Move player forward
            physics.ApplyThrust(Vector3.UnitZ, 50f);
            
            // Update world (chunk loading, generation)
            _worldManager.Update(deltaTime, physics.Position);
            
            // Update physics
            _physicsSystem.Update(deltaTime);
            
            // Update mining (if in range of asteroid)
            _miningSystem.Update(deltaTime);
            
            // Update destruction
            _destructionSystem.Update(deltaTime);
            
            // Update rendering (mesh building)
            _renderingManager.Update();
            
            // Update effects
            _effectsSystem.Update(deltaTime);
            
            // Progress indicator
            if (i % 10 == 0)
            {
                Console.Write(".");
            }
        }
        
        Console.WriteLine("\n\n✓ Simulation complete");
        Console.WriteLine($"  - Final position: {physics.Position}");
        Console.WriteLine($"  - Final velocity: {physics.Velocity}\n");
    }
    
    private void DisplayStatistics()
    {
        Console.WriteLine("=== System Statistics ===\n");
        
        // World stats
        var worldStats = _worldManager.GetStats();
        Console.WriteLine("World Generation:");
        Console.WriteLine($"  - Seed: {worldStats.Seed}");
        Console.WriteLine($"  - Total Chunks: {worldStats.TotalChunks}");
        Console.WriteLine($"  - Loaded Chunks: {worldStats.LoadedChunks}");
        Console.WriteLine($"  - Dirty Chunks: {worldStats.DirtyChunks}");
        Console.WriteLine($"  - Total Blocks: {worldStats.TotalBlocks}");
        Console.WriteLine($"  - Pending Generation: {worldStats.PendingGenerationTasks}");
        Console.WriteLine();
        
        // Rendering stats
        var renderStats = _renderingManager.GetStats();
        Console.WriteLine("Rendering:");
        Console.WriteLine($"  - Chunks with Meshes: {renderStats.ChunksWithMeshes}");
        Console.WriteLine($"  - Total Vertices: {renderStats.TotalVertices:N0}");
        Console.WriteLine($"  - Total Faces: {renderStats.TotalFaces:N0}");
        Console.WriteLine($"  - Greedy Meshing: {(renderStats.GreedyMeshingEnabled ? "Enabled" : "Disabled")}");
        Console.WriteLine($"  - Pending Mesh Builds: {renderStats.PendingMeshBuilds}");
        Console.WriteLine();
        
        // Entity stats
        Console.WriteLine("Entities:");
        Console.WriteLine($"  - Total Entities: {_entityManager.GetAllEntities().Count()}");
        Console.WriteLine($"  - Asteroids: {_miningSystem.GetAsteroids().Count()}");
        Console.WriteLine();
        
        // Lighting stats
        Console.WriteLine("Lighting & Effects:");
        Console.WriteLine($"  - Active Lights: {_lightingSystem.GetLights().Count()}");
        Console.WriteLine($"  - Active Effects: {_effectsSystem.GetActiveEffects().Count()}");
        Console.WriteLine();
    }
    
    private void Shutdown()
    {
        Console.WriteLine("Shutting down systems...");
        _worldManager.Shutdown();
        _renderingManager.Shutdown();
        Console.WriteLine("✓ Shutdown complete");
    }
}
