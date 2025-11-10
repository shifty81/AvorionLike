using System.Numerics;
using AvorionLike.Core.Voxel;
using AvorionLike.Core.Logging;
using AvorionLike.Core.ECS;

namespace AvorionLike.Core.Procedural;

/// <summary>
/// Types of massive claimable asteroids
/// </summary>
public enum MassiveAsteroidType
{
    IronGiant,      // Large metallic asteroid
    StoneMonolith,  // Rocky formation
    IceColossus,    // Frozen mass
    CrystalSpire,   // Crystalline structure
    Composite       // Mixed materials
}

/// <summary>
/// Configuration for massive asteroid generation
/// </summary>
public class MassiveAsteroidConfig
{
    public MassiveAsteroidType Type { get; set; } = MassiveAsteroidType.Composite;
    public int Seed { get; set; } = 0;
    public float MinSize { get; set; } = 2000f;  // Similar to small stations
    public float MaxSize { get; set; } = 5000f;  // Can be as large as medium stations
}

/// <summary>
/// Result of massive asteroid generation
/// </summary>
public class GeneratedMassiveAsteroid
{
    public VoxelStructureComponent Structure { get; set; } = new();
    public MassiveAsteroidConfig Config { get; set; } = new();
    public Vector3 LandingZone { get; set; }  // Safe landing area inside/on asteroid
    public List<Vector3> DockingPoints { get; set; } = new();
    public bool IsClaimed { get; set; } = false;
    public string OwnerName { get; set; } = "";
    public string HubName { get; set; } = "Unclaimed Asteroid";
    public int BlockCount => Structure.Blocks.Count;
    public float TotalMass => Structure.TotalMass;
    public bool HasHyperdriveCore { get; set; } = false;  // Upgraded for hyperspace network
}

/// <summary>
/// Procedurally generates massive claimable asteroids
/// These are rare (5% spawn chance) and have no resources but can be claimed as player hubs
/// </summary>
public class MassiveAsteroidGenerator
{
    private Random _random;
    private readonly Logger _logger = Logger.Instance;
    
    public MassiveAsteroidGenerator(int seed = 0)
    {
        _random = seed == 0 ? new Random() : new Random(seed);
    }
    
    /// <summary>
    /// Check if a massive asteroid should spawn (5% chance)
    /// </summary>
    public static bool ShouldSpawnMassiveAsteroid(Random random)
    {
        return random.Next(100) < 5;  // 5% chance
    }
    
    /// <summary>
    /// Generate a massive claimable asteroid
    /// </summary>
    public GeneratedMassiveAsteroid GenerateAsteroid(MassiveAsteroidConfig config)
    {
        _random = new Random(config.Seed == 0 ? Environment.TickCount : config.Seed);
        
        var result = new GeneratedMassiveAsteroid { Config = config };
        
        _logger.Info("MassiveAsteroidGenerator", $"Generating massive {config.Type} asteroid");
        
        // Determine size
        float size = config.MinSize + (float)_random.NextDouble() * (config.MaxSize - config.MinSize);
        
        // Generate core asteroid structure based on type
        GenerateAsteroidCore(result, size, config);
        
        // Create natural landing zone (hollow area or flat surface)
        CreateLandingZone(result, size);
        
        // Add potential docking points for future player construction
        AddNaturalDockingPoints(result, size);
        
        // Add surface details and features
        AddSurfaceFeatures(result, config);
        
        _logger.Info("MassiveAsteroidGenerator", $"Generated massive asteroid with {result.BlockCount} blocks");
        
        return result;
    }
    
    /// <summary>
    /// Generate the core asteroid structure
    /// </summary>
    private void GenerateAsteroidCore(GeneratedMassiveAsteroid asteroid, float size, MassiveAsteroidConfig config)
    {
        string material = GetAsteroidMaterial(config.Type);
        float radius = size / 2f;
        
        // Generate irregular asteroid shape using noise-based deformation
        for (float x = -radius; x <= radius; x += 3)
        {
            for (float y = -radius; y <= radius; y += 3)
            {
                for (float z = -radius; z <= radius; z += 3)
                {
                    float distance = MathF.Sqrt(x * x + y * y + z * z);
                    
                    // Add noise for irregular shape
                    float noise = GetNoise3D(x * 0.05f, y * 0.05f, z * 0.05f);
                    float deformedRadius = radius * (0.7f + noise * 0.3f);
                    
                    // Check if this point is within the deformed radius
                    if (distance <= deformedRadius)
                    {
                        // Add some interior hollow spaces (caves)
                        float interiorNoise = GetNoise3D(x * 0.1f, y * 0.1f, z * 0.1f);
                        bool isHollow = distance < radius * 0.8f && interiorNoise > 0.6f;
                        
                        if (!isHollow)
                        {
                            // Variable block sizes for natural appearance
                            float blockSize = 2f + (float)_random.NextDouble() * 2f;
                            Vector3 blockSizeVec = new Vector3(blockSize, blockSize, blockSize);
                            
                            // Randomly vary block dimensions for more organic look
                            if (_random.Next(100) < 30)
                            {
                                blockSizeVec = new Vector3(
                                    blockSize * (0.7f + (float)_random.NextDouble() * 0.6f),
                                    blockSize * (0.7f + (float)_random.NextDouble() * 0.6f),
                                    blockSize * (0.7f + (float)_random.NextDouble() * 0.6f)
                                );
                            }
                            
                            asteroid.Structure.AddBlock(new VoxelBlock(
                                new Vector3(x, y, z),
                                blockSizeVec,
                                material,
                                BlockType.Hull
                            ));
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Create a landing zone inside or on the asteroid
    /// </summary>
    private void CreateLandingZone(GeneratedMassiveAsteroid asteroid, float size)
    {
        // Create a large hollow chamber in the asteroid for landing
        Vector3 landingCenter = new Vector3(
            (float)(_random.NextDouble() - 0.5) * size * 0.3f,
            (float)(_random.NextDouble() - 0.5) * size * 0.3f,
            (float)(_random.NextDouble() - 0.5) * size * 0.3f
        );
        
        float landingRadius = 30f + (float)_random.NextDouble() * 20f;  // Large enough for ships
        
        // Remove blocks in landing zone area
        var blocksToRemove = asteroid.Structure.Blocks
            .Where(b => Vector3.Distance(b.Position, landingCenter) < landingRadius)
            .ToList();
        
        foreach (var block in blocksToRemove)
        {
            asteroid.Structure.RemoveBlock(block);
        }
        
        asteroid.LandingZone = landingCenter;
        
        _logger.Info("MassiveAsteroidGenerator", $"Created landing zone at {landingCenter} with radius {landingRadius}");
    }
    
    /// <summary>
    /// Add natural points where players can attach docking structures
    /// </summary>
    private void AddNaturalDockingPoints(GeneratedMassiveAsteroid asteroid, float size)
    {
        int dockingPointCount = 6 + _random.Next(6);
        float dockingDistance = size * 0.45f;
        
        for (int i = 0; i < dockingPointCount; i++)
        {
            float angle = (float)(i * 2 * Math.PI / dockingPointCount);
            Vector3 dockingPoint = new Vector3(
                MathF.Cos(angle) * dockingDistance,
                MathF.Sin(angle) * dockingDistance * 0.5f,
                (i % 2 == 0 ? 1 : -1) * dockingDistance * 0.7f
            );
            
            asteroid.DockingPoints.Add(dockingPoint);
        }
    }
    
    /// <summary>
    /// Add surface features like craters, spires, and formations
    /// </summary>
    private void AddSurfaceFeatures(GeneratedMassiveAsteroid asteroid, MassiveAsteroidConfig config)
    {
        string material = GetAsteroidMaterial(config.Type);
        
        // Add some surface protrusions (spires, ridges)
        int featureCount = 10 + _random.Next(15);
        for (int i = 0; i < featureCount; i++)
        {
            Vector3 featurePos = new Vector3(
                (float)(_random.NextDouble() - 0.5) * config.MaxSize * 0.8f,
                (float)(_random.NextDouble() - 0.5) * config.MaxSize * 0.8f,
                (float)(_random.NextDouble() - 0.5) * config.MaxSize * 0.8f
            );
            
            // Create small spire or outcropping
            int spireHeight = 5 + _random.Next(10);
            for (int h = 0; h < spireHeight; h++)
            {
                Vector3 spireBlockPos = featurePos + new Vector3(0, h * 2, 0);
                float blockSize = 3f - (h * 0.2f);  // Taper
                
                if (blockSize > 1f)
                {
                    asteroid.Structure.AddBlock(new VoxelBlock(
                        spireBlockPos,
                        new Vector3(blockSize, blockSize, blockSize),
                        material,
                        BlockType.Hull
                    ));
                }
            }
        }
    }
    
    /// <summary>
    /// Get material type based on asteroid type
    /// NOTE: These asteroids have no extractable resources - material is just for appearance
    /// </summary>
    private string GetAsteroidMaterial(MassiveAsteroidType type)
    {
        return type switch
        {
            MassiveAsteroidType.IronGiant => "Iron",
            MassiveAsteroidType.StoneMonolith => "Iron",  // Gray/rocky appearance
            MassiveAsteroidType.IceColossus => "Titanium",  // Bluish appearance
            MassiveAsteroidType.CrystalSpire => "Xanion",  // Crystalline/golden appearance
            MassiveAsteroidType.Composite => "Naonite",  // Mixed greenish appearance
            _ => "Iron"
        };
    }
    
    /// <summary>
    /// Simple 3D noise function for organic shapes
    /// </summary>
    private float GetNoise3D(float x, float y, float z)
    {
        // Simple pseudo-random noise based on position
        float n = MathF.Sin(x * 12.9898f + y * 78.233f + z * 45.164f) * 43758.5453f;
        return (n - MathF.Floor(n));
    }
}

/// <summary>
/// Component for tracking asteroid hub ownership and upgrades
/// </summary>
public class AsteroidHubComponent : IComponent
{
    public Guid EntityId { get; set; }
    public bool IsClaimed { get; set; } = false;
    public string OwnerPlayerId { get; set; } = "";
    public string HubName { get; set; } = "Unclaimed Asteroid";
    public DateTime ClaimDate { get; set; }
    
    // Upgrade levels
    public int DockingBayLevel { get; set; } = 0;
    public int ShieldLevel { get; set; } = 0;
    public int PowerLevel { get; set; } = 0;
    public bool HasHyperdriveCore { get; set; } = false;
    
    // Hyperspace network
    public List<Guid> ConnectedHubs { get; set; } = new();  // Other player hubs connected via hyperspace
    
    // Storage and facilities
    public Dictionary<string, int> StoredResources { get; set; } = new();
    public List<string> Facilities { get; set; } = new();
    
    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["EntityId"] = EntityId.ToString(),
            ["IsClaimed"] = IsClaimed,
            ["OwnerPlayerId"] = OwnerPlayerId,
            ["HubName"] = HubName,
            ["ClaimDate"] = ClaimDate.ToString("o"),
            ["DockingBayLevel"] = DockingBayLevel,
            ["ShieldLevel"] = ShieldLevel,
            ["PowerLevel"] = PowerLevel,
            ["HasHyperdriveCore"] = HasHyperdriveCore,
            ["ConnectedHubs"] = ConnectedHubs.Select(g => g.ToString()).ToList(),
            ["StoredResources"] = StoredResources,
            ["Facilities"] = Facilities
        };
    }
}
