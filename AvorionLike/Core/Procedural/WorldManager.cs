using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Voxel;
using AvorionLike.Core.Physics;
using AvorionLike.Core.Mining;

namespace AvorionLike.Core.Procedural;

/// <summary>
/// Integrates world generation, chunk management, and multithreading
/// </summary>
public class WorldManager
{
    private readonly ChunkManager _chunkManager;
    private readonly ThreadedWorldGenerator _worldGenerator;
    private readonly EntityManager _entityManager;
    private readonly MiningSystem _miningSystem;
    private readonly int _seed;
    
    private Vector3 _lastPlayerPosition = Vector3.Zero;
    private float _updateInterval = 1.0f; // Update chunks every 1 second
    private float _timeSinceLastUpdate = 0f;
    
    public WorldManager(
        EntityManager entityManager,
        MiningSystem miningSystem,
        int seed = 0,
        int chunkSize = 100)
    {
        _entityManager = entityManager;
        _miningSystem = miningSystem;
        _seed = seed == 0 ? Environment.TickCount : seed;
        
        // Initialize chunk manager
        _chunkManager = new ChunkManager(
            chunkSize: chunkSize,
            loadRadius: 500f,
            unloadRadius: 750f,
            maxLoadedChunks: 100
        );
        
        // Initialize threaded world generator
        _worldGenerator = new ThreadedWorldGenerator(
            seed: _seed,
            chunkManager: _chunkManager,
            threadCount: 0 // Auto-detect
        );
        
        // Start generation threads
        _worldGenerator.Start();
    }
    
    /// <summary>
    /// Update world generation and chunk management
    /// </summary>
    public void Update(float deltaTime, Vector3 playerPosition)
    {
        _timeSinceLastUpdate += deltaTime;
        
        // Throttle chunk updates to avoid hitching
        if (_timeSinceLastUpdate >= _updateInterval || 
            Vector3.Distance(playerPosition, _lastPlayerPosition) > 100f)
        {
            // Update chunk loading/unloading
            _chunkManager.Update(playerPosition);
            
            _lastPlayerPosition = playerPosition;
            _timeSinceLastUpdate = 0f;
        }
        
        // Process completed generation results
        _worldGenerator.ProcessResults();
    }
    
    /// <summary>
    /// Generate a sector at specified coordinates
    /// </summary>
    public void GenerateSector(int x, int y, int z)
    {
        _worldGenerator.RequestSectorGeneration(x, y, z);
    }
    
    /// <summary>
    /// Generate asteroids in the current sector
    /// </summary>
    public void GenerateAsteroidsInRadius(Vector3 position, float radius)
    {
        var generator = new GalaxyGenerator(_seed);
        
        // Determine which sector(s) we're in
        int sectorX = (int)Math.Floor(position.X / 10000f);
        int sectorY = (int)Math.Floor(position.Y / 10000f);
        int sectorZ = (int)Math.Floor(position.Z / 10000f);
        
        // Generate sectors in a small radius
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    var sector = generator.GenerateSector(
                        sectorX + x,
                        sectorY + y,
                        sectorZ + z
                    );
                    
                    // Add asteroids from this sector
                    foreach (var asteroidData in sector.Asteroids)
                    {
                        if (Vector3.Distance(asteroidData.Position, position) <= radius)
                        {
                            CreateAsteroidEntity(asteroidData);
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Create an asteroid entity with voxel structure
    /// </summary>
    private void CreateAsteroidEntity(AsteroidData asteroidData)
    {
        // Create entity
        var asteroid = _entityManager.CreateEntity($"Asteroid-{Guid.NewGuid()}");
        
        // Generate voxel blocks for asteroid
        var asteroidGenerator = new AsteroidVoxelGenerator(_seed);
        var blocks = asteroidGenerator.GenerateAsteroid(asteroidData, voxelResolution: 6);
        
        // Add voxel structure component
        var voxelComponent = new VoxelStructureComponent();
        foreach (var block in blocks)
        {
            voxelComponent.AddBlock(block);
            _chunkManager.AddBlock(block); // Add to chunk manager
        }
        _entityManager.AddComponent(asteroid.Id, voxelComponent);
        
        // Add physics component
        var physicsComponent = new PhysicsComponent
        {
            Position = asteroidData.Position,
            Mass = voxelComponent.TotalMass,
            IsStatic = true, // Asteroids don't move
            CollisionRadius = asteroidData.Size
        };
        _entityManager.AddComponent(asteroid.Id, physicsComponent);
        
        // Add to mining system
        var miningAsteroid = new Asteroid(asteroidData);
        _miningSystem.AddAsteroid(miningAsteroid);
    }
    
    /// <summary>
    /// Get chunk manager for external access
    /// </summary>
    public ChunkManager GetChunkManager()
    {
        return _chunkManager;
    }
    
    /// <summary>
    /// Get world generator statistics
    /// </summary>
    public WorldStats GetStats()
    {
        var chunkStats = _chunkManager.GetStats();
        
        return new WorldStats
        {
            Seed = _seed,
            TotalChunks = chunkStats.TotalChunks,
            LoadedChunks = chunkStats.LoadedChunks,
            DirtyChunks = chunkStats.DirtyChunks,
            TotalBlocks = chunkStats.TotalBlocks,
            PendingGenerationTasks = _worldGenerator.GetPendingTaskCount(),
            PendingGenerationResults = _worldGenerator.GetPendingResultCount()
        };
    }
    
    /// <summary>
    /// Shutdown the world manager
    /// </summary>
    public void Shutdown()
    {
        _worldGenerator.Stop();
    }
}

/// <summary>
/// World manager statistics
/// </summary>
public class WorldStats
{
    public int Seed { get; set; }
    public int TotalChunks { get; set; }
    public int LoadedChunks { get; set; }
    public int DirtyChunks { get; set; }
    public int TotalBlocks { get; set; }
    public int PendingGenerationTasks { get; set; }
    public int PendingGenerationResults { get; set; }
}
