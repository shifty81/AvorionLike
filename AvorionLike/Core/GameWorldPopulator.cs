using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Voxel;
using AvorionLike.Core.Physics;
using AvorionLike.Core.Resources;
using AvorionLike.Core.Combat;
using AvorionLike.Core.Navigation;
using AvorionLike.Core.AI;
using AvorionLike.Core.RPG;

namespace AvorionLike.Core;

/// <summary>
/// Populates the game world with AI ships, asteroids, stations, and other entities
/// Creates a living, breathing universe for the player to interact with
/// </summary>
public class GameWorldPopulator
{
    private readonly GameEngine _gameEngine;
    private readonly Random _random;
    
    public GameWorldPopulator(GameEngine gameEngine, int? seed = null)
    {
        _gameEngine = gameEngine;
        _random = seed.HasValue ? new Random(seed.Value) : new Random();
    }
    
    /// <summary>
    /// Creates a populated starter area with various entities for the player
    /// </summary>
    public void PopulateStarterArea(Vector3 playerPosition, float radius = 1000f)
    {
        Console.WriteLine($"Populating starter area around {playerPosition} (radius: {radius}m)...");
        
        // Create asteroid field for mining
        CreateAsteroidField(playerPosition, radius, 15);
        
        // Create friendly traders
        CreateTraderShips(playerPosition, radius, 3);
        
        // Create neutral miners
        CreateMinerShips(playerPosition, radius, 4);
        
        // Create some pirates (but not too close to player)
        CreatePirateShips(playerPosition, radius * 0.8f, 2);
        
        // Create a friendly station
        CreateStation(playerPosition + new Vector3(radius * 0.5f, 0, 0), StationType.TradingPost, "Haven Station");
        
        Console.WriteLine("Starter area population complete!");
    }
    
    /// <summary>
    /// Creates an asteroid field for mining
    /// </summary>
    private void CreateAsteroidField(Vector3 center, float radius, int count)
    {
        Console.WriteLine($"  Creating {count} asteroids...");
        
        for (int i = 0; i < count; i++)
        {
            // Random position within radius
            var offset = new Vector3(
                (float)(_random.NextDouble() * 2 - 1) * radius,
                (float)(_random.NextDouble() * 2 - 1) * radius * 0.3f, // Less vertical spread
                (float)(_random.NextDouble() * 2 - 1) * radius
            );
            
            var position = center + offset;
            
            // Random asteroid properties
            var size = (float)(_random.NextDouble() * 10 + 5); // 5-15 meters
            var resourceTypes = Enum.GetValues<ResourceType>().Where(r => r != ResourceType.Credits).ToArray();
            var resourceType = resourceTypes[_random.Next(resourceTypes.Length)];
            
            CreateAsteroid(position, size, resourceType, $"Asteroid-{i + 1}");
        }
    }
    
    /// <summary>
    /// Creates a single minable asteroid
    /// </summary>
    private Guid CreateAsteroid(Vector3 position, float size, ResourceType resourceType, string name)
    {
        var asteroid = _gameEngine.EntityManager.CreateEntity(name);
        
        // Create voxel structure
        var voxelComponent = new VoxelStructureComponent();
        
        // Create multiple blocks for interesting asteroid shape
        int blockCount = Math.Max(2, (int)(size / 3));
        for (int i = 0; i < blockCount; i++)
        {
            var blockOffset = new Vector3(
                (float)(_random.NextDouble() * 2 - 1) * size * 0.3f,
                (float)(_random.NextDouble() * 2 - 1) * size * 0.3f,
                (float)(_random.NextDouble() * 2 - 1) * size * 0.3f
            );
            
            var blockSize = size * (0.6f + (float)_random.NextDouble() * 0.4f);
            
            voxelComponent.AddBlock(new VoxelBlock(
                blockOffset,
                new Vector3(blockSize, blockSize, blockSize),
                "Iron", // Asteroids are made of iron/stone
                BlockType.Armor
            ));
        }
        
        _gameEngine.EntityManager.AddComponent(asteroid.Id, voxelComponent);
        
        // Add physics
        var physicsComponent = new PhysicsComponent
        {
            Position = position,
            Mass = voxelComponent.TotalMass,
            Velocity = new Vector3(
                (float)(_random.NextDouble() * 2 - 1) * 2f,
                (float)(_random.NextDouble() * 2 - 1) * 2f,
                (float)(_random.NextDouble() * 2 - 1) * 2f
            ) // Slow drift
        };
        _gameEngine.EntityManager.AddComponent(asteroid.Id, physicsComponent);
        
        // Add inventory with resources to mine
        var inventoryComponent = new InventoryComponent(1000);
        int resourceAmount = (int)(size * size * 10); // More resources in larger asteroids
        inventoryComponent.Inventory.AddResource(resourceType, resourceAmount);
        _gameEngine.EntityManager.AddComponent(asteroid.Id, inventoryComponent);
        
        return asteroid.Id;
    }
    
    /// <summary>
    /// Creates AI trader ships
    /// </summary>
    private void CreateTraderShips(Vector3 center, float radius, int count)
    {
        Console.WriteLine($"  Creating {count} trader ships...");
        
        for (int i = 0; i < count; i++)
        {
            var position = center + new Vector3(
                (float)(_random.NextDouble() * 2 - 1) * radius * 0.6f,
                (float)(_random.NextDouble() * 2 - 1) * radius * 0.2f,
                (float)(_random.NextDouble() * 2 - 1) * radius * 0.6f
            );
            
            CreateAIShip(position, AIPersonality.Trader, "Trader", i + 1);
        }
    }
    
    /// <summary>
    /// Creates AI miner ships
    /// </summary>
    private void CreateMinerShips(Vector3 center, float radius, int count)
    {
        Console.WriteLine($"  Creating {count} miner ships...");
        
        for (int i = 0; i < count; i++)
        {
            var position = center + new Vector3(
                (float)(_random.NextDouble() * 2 - 1) * radius,
                (float)(_random.NextDouble() * 2 - 1) * radius * 0.2f,
                (float)(_random.NextDouble() * 2 - 1) * radius
            );
            
            CreateAIShip(position, AIPersonality.Miner, "Miner", i + 1);
        }
    }
    
    /// <summary>
    /// Creates AI pirate ships
    /// </summary>
    private void CreatePirateShips(Vector3 center, float radius, int count)
    {
        Console.WriteLine($"  Creating {count} pirate ships...");
        
        for (int i = 0; i < count; i++)
        {
            var position = center + new Vector3(
                (float)(_random.NextDouble() * 2 - 1) * radius,
                (float)(_random.NextDouble() * 2 - 1) * radius * 0.3f,
                (float)(_random.NextDouble() * 2 - 1) * radius
            );
            
            CreateAIShip(position, AIPersonality.Aggressive, "Pirate", i + 1);
        }
    }
    
    /// <summary>
    /// Creates an AI-controlled ship
    /// </summary>
    private Guid CreateAIShip(Vector3 position, AIPersonality personality, string typePrefix, int number)
    {
        var ship = _gameEngine.EntityManager.CreateEntity($"{typePrefix} Ship {number}");
        
        // Create ship structure based on personality
        var voxelComponent = new VoxelStructureComponent();
        
        switch (personality)
        {
            case AIPersonality.Trader:
                // Cargo-heavy ship
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 0, 0), new Vector3(4, 3, 3), "Titanium", BlockType.Hull));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(-4, 0, 0), new Vector3(2, 2, 2), "Iron", BlockType.Engine));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(4, 0, 0), new Vector3(3, 3, 3), "Iron", BlockType.Cargo));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 3, 0), new Vector3(2, 2, 2), "Titanium", BlockType.ShieldGenerator));
                break;
                
            case AIPersonality.Miner:
                // Functional mining ship
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 0, 0), new Vector3(3, 3, 3), "Titanium", BlockType.Hull));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(-3, 0, 0), new Vector3(2, 2, 2), "Naonite", BlockType.Engine));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(3, 0, 0), new Vector3(2, 2, 2), "Naonite", BlockType.Generator));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 3, 0), new Vector3(2, 2, 2), "Iron", BlockType.Cargo));
                break;
                
            case AIPersonality.Aggressive:
                // Combat-focused pirate ship
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 0, 0), new Vector3(3, 2, 2), "Ogonite", BlockType.Hull));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(-3, 0, 0), new Vector3(2, 2, 2), "Ogonite", BlockType.Engine));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(3, 0, 0), new Vector3(2, 2, 2), "Ogonite", BlockType.Engine));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 2, 0), new Vector3(2, 1.5f, 1.5f), "Trinium", BlockType.Thruster));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, -2, 0), new Vector3(2, 1.5f, 1.5f), "Trinium", BlockType.Thruster));
                break;
                
            default:
                // Generic ship
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 0, 0), new Vector3(3, 3, 3), "Titanium", BlockType.Hull));
                voxelComponent.AddBlock(new VoxelBlock(new Vector3(-3, 0, 0), new Vector3(2, 2, 2), "Iron", BlockType.Engine));
                break;
        }
        
        _gameEngine.EntityManager.AddComponent(ship.Id, voxelComponent);
        
        // Add physics
        var physicsComponent = new PhysicsComponent
        {
            Position = position,
            Mass = voxelComponent.TotalMass,
            MaxThrust = voxelComponent.TotalThrust,
            MaxTorque = voxelComponent.TotalTorque,
            Velocity = new Vector3(
                (float)(_random.NextDouble() * 2 - 1) * 10f,
                (float)(_random.NextDouble() * 2 - 1) * 5f,
                (float)(_random.NextDouble() * 2 - 1) * 10f
            )
        };
        _gameEngine.EntityManager.AddComponent(ship.Id, physicsComponent);
        
        // Add inventory
        var inventoryComponent = new InventoryComponent(500);
        inventoryComponent.Inventory.AddResource(ResourceType.Credits, _random.Next(1000, 5000));
        if (personality == AIPersonality.Trader)
        {
            // Traders carry goods
            inventoryComponent.Inventory.AddResource(ResourceType.Iron, _random.Next(50, 200));
            inventoryComponent.Inventory.AddResource(ResourceType.Titanium, _random.Next(20, 100));
        }
        _gameEngine.EntityManager.AddComponent(ship.Id, inventoryComponent);
        
        // Add combat component
        var combatComponent = new CombatComponent
        {
            EntityId = ship.Id,
            MaxShields = voxelComponent.ShieldCapacity,
            CurrentShields = voxelComponent.ShieldCapacity,
            MaxEnergy = voxelComponent.PowerGeneration,
            CurrentEnergy = voxelComponent.PowerGeneration
        };
        _gameEngine.EntityManager.AddComponent(ship.Id, combatComponent);
        
        // Add AI component
        var aiComponent = new AIComponent
        {
            EntityId = ship.Id,
            Personality = personality,
            CurrentState = AIState.Patrol
        };
        _gameEngine.EntityManager.AddComponent(ship.Id, aiComponent);
        
        // Add faction component
        var factionComponent = new FactionComponent
        {
            EntityId = ship.Id,
            FactionName = personality == AIPersonality.Aggressive ? "Pirates" : "Neutral"
        };
        _gameEngine.EntityManager.AddComponent(ship.Id, factionComponent);
        
        // Add hyperdrive
        var hyperdriveComponent = new HyperdriveComponent
        {
            EntityId = ship.Id,
            JumpRange = 5f
        };
        _gameEngine.EntityManager.AddComponent(ship.Id, hyperdriveComponent);
        
        // Add sector location
        var locationComponent = new SectorLocationComponent
        {
            EntityId = ship.Id,
            CurrentSector = new SectorCoordinate(0, 0, 0)
        };
        _gameEngine.EntityManager.AddComponent(ship.Id, locationComponent);
        
        return ship.Id;
    }
    
    /// <summary>
    /// Creates a space station
    /// </summary>
    private Guid CreateStation(Vector3 position, StationType stationType, string name)
    {
        Console.WriteLine($"  Creating {stationType} station: {name}...");
        
        var station = _gameEngine.EntityManager.CreateEntity(name);
        
        // Create larger, more impressive voxel structure for station
        var voxelComponent = new VoxelStructureComponent();
        
        // Core modules
        voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 0, 0), new Vector3(10, 10, 10), "Titanium", BlockType.Hull));
        
        // Docking bays
        voxelComponent.AddBlock(new VoxelBlock(new Vector3(12, 0, 0), new Vector3(5, 8, 8), "Titanium", BlockType.PodDocking));
        voxelComponent.AddBlock(new VoxelBlock(new Vector3(-12, 0, 0), new Vector3(5, 8, 8), "Titanium", BlockType.PodDocking));
        
        // Trading modules
        if (stationType == StationType.TradingPost)
        {
            voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 12, 0), new Vector3(8, 6, 8), "Naonite", BlockType.Cargo));
            voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, -12, 0), new Vector3(8, 6, 8), "Naonite", BlockType.Cargo));
        }
        
        // Shields
        voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 0, 12), new Vector3(6, 6, 6), "Xanion", BlockType.ShieldGenerator));
        voxelComponent.AddBlock(new VoxelBlock(new Vector3(0, 0, -12), new Vector3(6, 6, 6), "Xanion", BlockType.ShieldGenerator));
        
        _gameEngine.EntityManager.AddComponent(station.Id, voxelComponent);
        
        // Add physics (stationary)
        var physicsComponent = new PhysicsComponent
        {
            Position = position,
            Mass = voxelComponent.TotalMass * 10, // Stations are HEAVY
            Velocity = Vector3.Zero // Stationary
        };
        _gameEngine.EntityManager.AddComponent(station.Id, physicsComponent);
        
        // Add large inventory for trading
        var inventoryComponent = new InventoryComponent(10000);
        inventoryComponent.Inventory.AddResource(ResourceType.Credits, 1000000);
        inventoryComponent.Inventory.AddResource(ResourceType.Iron, 5000);
        inventoryComponent.Inventory.AddResource(ResourceType.Titanium, 2000);
        inventoryComponent.Inventory.AddResource(ResourceType.Naonite, 1000);
        _gameEngine.EntityManager.AddComponent(station.Id, inventoryComponent);
        
        // Add combat component (stations can defend themselves)
        var combatComponent = new CombatComponent
        {
            EntityId = station.Id,
            MaxShields = voxelComponent.ShieldCapacity * 5, // Strong shields
            CurrentShields = voxelComponent.ShieldCapacity * 5,
            MaxEnergy = voxelComponent.PowerGeneration * 3,
            CurrentEnergy = voxelComponent.PowerGeneration * 3
        };
        _gameEngine.EntityManager.AddComponent(station.Id, combatComponent);
        
        // Add docking component
        var dockingComponent = new DockingComponent
        {
            EntityId = station.Id,
            HasDockingPort = true
        };
        _gameEngine.EntityManager.AddComponent(station.Id, dockingComponent);
        
        return station.Id;
    }
}

/// <summary>
/// Station types
/// </summary>
public enum StationType
{
    TradingPost,
    Military,
    Research,
    Mining,
    Shipyard
}
