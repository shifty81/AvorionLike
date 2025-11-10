using System.Numerics;
using AvorionLike.Core.Voxel;
using AvorionLike.Core.Logging;

namespace AvorionLike.Core.Procedural;

/// <summary>
/// Station size categories - all stations are massive structures
/// </summary>
public enum StationSize
{
    Small,      // 2000-3000 blocks (minimum)
    Medium,     // 3000-5000 blocks
    Large,      // 5000-8000 blocks
    Massive     // 8000+ blocks
}

/// <summary>
/// Station architectural style
/// </summary>
public enum StationArchitecture
{
    Modular,        // Connected modules and sections
    Ring,           // Ring-shaped station (rotating habitat)
    Tower,          // Tall spire structure
    Industrial,     // Complex industrial framework
    Sprawling       // Spread-out complex structure
}

/// <summary>
/// Configuration for station generation
/// </summary>
public class StationGenerationConfig
{
    public StationSize Size { get; set; } = StationSize.Medium;
    public string StationType { get; set; } = "Trading";
    public string Material { get; set; } = "Titanium";
    public StationArchitecture Architecture { get; set; } = StationArchitecture.Modular;
    public int Seed { get; set; } = 0;
    public bool IncludeDockingBays { get; set; } = true;
    public int MinDockingBays { get; set; } = 4;
}

/// <summary>
/// Result of station generation
/// </summary>
public class GeneratedStation
{
    public VoxelStructureComponent Structure { get; set; } = new();
    public StationGenerationConfig Config { get; set; } = new();
    public List<Vector3> DockingPoints { get; set; } = new();
    public List<string> Facilities { get; set; } = new();
    public int BlockCount => Structure.Blocks.Count;
    public float TotalMass => Structure.TotalMass;
}

/// <summary>
/// Procedurally generates massive space stations (minimum 2000 blocks)
/// </summary>
public class ProceduralStationGenerator
{
    private Random _random;
    private readonly Logger _logger = Logger.Instance;
    
    public ProceduralStationGenerator(int seed = 0)
    {
        _random = seed == 0 ? new Random() : new Random(seed);
    }
    
    /// <summary>
    /// Generate a complete massive station
    /// </summary>
    public GeneratedStation GenerateStation(StationGenerationConfig config)
    {
        _random = new Random(config.Seed == 0 ? Environment.TickCount : config.Seed);
        
        var result = new GeneratedStation { Config = config };
        
        _logger.Info("StationGenerator", $"Generating {config.Size} {config.StationType} station with {config.Architecture} architecture");
        
        // Step 1: Determine station dimensions based on size (ensure 2000+ blocks)
        var coreDimensions = GetStationCoreDimensions(config.Size);
        
        // Step 2: Generate core structure based on architecture
        GenerateCoreStructure(result, coreDimensions, config);
        
        // Step 3: Add type-specific facilities
        AddStationFacilities(result, config);
        
        // Step 4: Add docking bays
        if (config.IncludeDockingBays)
        {
            AddDockingBays(result, config);
        }
        
        // Step 5: Add external details and armor
        AddExternalDetails(result, config);
        
        // Step 6: Add internal superstructure for realism
        AddInternalSuperstructure(result, config);
        
        _logger.Info("StationGenerator", $"Generated station with {result.BlockCount} blocks, {result.DockingPoints.Count} docking bays");
        
        return result;
    }
    
    /// <summary>
    /// Get core dimensions ensuring minimum 2000 block count
    /// </summary>
    private Vector3 GetStationCoreDimensions(StationSize size)
    {
        return size switch
        {
            StationSize.Small => new Vector3(40, 40, 40),      // ~2000-3000 blocks
            StationSize.Medium => new Vector3(60, 60, 60),     // ~3000-5000 blocks
            StationSize.Large => new Vector3(80, 80, 80),      // ~5000-8000 blocks
            StationSize.Massive => new Vector3(120, 120, 120), // ~8000+ blocks
            _ => new Vector3(60, 60, 60)
        };
    }
    
    /// <summary>
    /// Generate core station structure based on architecture
    /// </summary>
    private void GenerateCoreStructure(GeneratedStation station, Vector3 dimensions, StationGenerationConfig config)
    {
        switch (config.Architecture)
        {
            case StationArchitecture.Modular:
                GenerateModularStation(station, dimensions, config);
                break;
            case StationArchitecture.Ring:
                GenerateRingStation(station, dimensions, config);
                break;
            case StationArchitecture.Tower:
                GenerateTowerStation(station, dimensions, config);
                break;
            case StationArchitecture.Industrial:
                GenerateIndustrialStation(station, dimensions, config);
                break;
            case StationArchitecture.Sprawling:
                GenerateSprawlingStation(station, dimensions, config);
                break;
        }
    }
    
    /// <summary>
    /// Generate modular station with connected sections
    /// </summary>
    private void GenerateModularStation(GeneratedStation station, Vector3 dimensions, StationGenerationConfig config)
    {
        // Central hub - large sphere-like core
        float coreRadius = Math.Min(dimensions.X, Math.Min(dimensions.Y, dimensions.Z)) / 3;
        GenerateSphereSection(station, Vector3.Zero, coreRadius, config.Material);
        
        // Add 6-8 large modules connected to the core
        int moduleCount = 6 + _random.Next(3);
        float moduleDistance = coreRadius * 2.5f;
        
        for (int i = 0; i < moduleCount; i++)
        {
            float angle = (float)(i * 2 * Math.PI / moduleCount);
            Vector3 modulePos = new Vector3(
                MathF.Cos(angle) * moduleDistance,
                MathF.Sin(angle) * moduleDistance,
                (i % 2 == 0 ? 1 : -1) * moduleDistance * 0.3f
            );
            
            // Generate module
            Vector3 moduleSize = new Vector3(
                15 + _random.Next(10),
                15 + _random.Next(10),
                20 + _random.Next(15)
            );
            GenerateBox(station, modulePos, moduleSize, config.Material);
            
            // Connect module to core with corridor
            GenerateCorridor(station, Vector3.Zero, modulePos, 3f, config.Material);
        }
    }
    
    /// <summary>
    /// Generate ring-shaped rotating habitat station
    /// </summary>
    private void GenerateRingStation(GeneratedStation station, Vector3 dimensions, StationGenerationConfig config)
    {
        float ringRadius = dimensions.X / 2;
        float ringThickness = 8f;
        float ringWidth = 12f;
        
        // Main ring
        int segments = 64;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (float)(i * 2 * Math.PI / segments);
            float angle2 = (float)((i + 1) * 2 * Math.PI / segments);
            
            Vector3 pos1 = new Vector3(
                MathF.Cos(angle1) * ringRadius,
                MathF.Sin(angle1) * ringRadius,
                0
            );
            
            // Ring cross-section blocks
            for (float y = -ringWidth / 2; y <= ringWidth / 2; y += 3)
            {
                for (float z = -ringThickness / 2; z <= ringThickness / 2; z += 3)
                {
                    Vector3 blockPos = pos1 + new Vector3(0, y, z);
                    float blockSize = 2.5f + (float)_random.NextDouble() * 0.5f;
                    station.Structure.AddBlock(new VoxelBlock(
                        blockPos,
                        new Vector3(blockSize, blockSize, blockSize),
                        config.Material,
                        BlockType.Hull
                    ));
                }
            }
        }
        
        // Central hub with spokes connecting to ring
        GenerateSphereSection(station, Vector3.Zero, 15f, config.Material);
        
        // Add 8 spokes from center to ring
        for (int i = 0; i < 8; i++)
        {
            float angle = (float)(i * 2 * Math.PI / 8);
            Vector3 ringPoint = new Vector3(
                MathF.Cos(angle) * ringRadius,
                MathF.Sin(angle) * ringRadius,
                0
            );
            GenerateCorridor(station, Vector3.Zero, ringPoint, 4f, config.Material);
        }
    }
    
    /// <summary>
    /// Generate tall tower/spire station
    /// </summary>
    private void GenerateTowerStation(GeneratedStation station, Vector3 dimensions, StationGenerationConfig config)
    {
        float height = dimensions.Y;
        float baseRadius = dimensions.X / 4;
        
        // Build tower from bottom to top with tapering
        for (float y = -height / 2; y < height / 2; y += 3)
        {
            float progress = (y + height / 2) / height;
            float currentRadius = baseRadius * (1.2f - progress * 0.4f);  // Slight taper
            
            // Create cross-section at this height
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(i * 2 * Math.PI / segments);
                Vector3 pos = new Vector3(
                    MathF.Cos(angle) * currentRadius,
                    y,
                    MathF.Sin(angle) * currentRadius
                );
                
                float blockSize = 2f + (float)_random.NextDouble();
                station.Structure.AddBlock(new VoxelBlock(
                    pos,
                    new Vector3(blockSize, blockSize, blockSize),
                    config.Material,
                    BlockType.Hull
                ));
            }
            
            // Add cross-bracing every 15 units
            if ((int)y % 15 == 0)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = (float)(i * Math.PI / 2);
                    Vector3 strutEnd = new Vector3(
                        MathF.Cos(angle) * currentRadius,
                        y,
                        MathF.Sin(angle) * currentRadius
                    );
                    GenerateCorridor(station, new Vector3(0, y, 0), strutEnd, 2f, config.Material);
                }
            }
        }
        
        // Add platforms at intervals
        for (float y = -height / 2 + 20; y < height / 2; y += 30)
        {
            float platformRadius = baseRadius * 1.5f;
            GeneratePlatform(station, new Vector3(0, y, 0), platformRadius, config.Material);
        }
    }
    
    /// <summary>
    /// Generate complex industrial framework station
    /// </summary>
    private void GenerateIndustrialStation(GeneratedStation station, Vector3 dimensions, StationGenerationConfig config)
    {
        // Create large framework structure
        GenerateFramework(station, Vector3.Zero, dimensions, config.Material);
        
        // Add industrial modules randomly throughout
        int moduleCount = 20 + _random.Next(10);
        for (int i = 0; i < moduleCount; i++)
        {
            Vector3 modulePos = new Vector3(
                (_random.NextSingle() - 0.5f) * dimensions.X * 0.8f,
                (_random.NextSingle() - 0.5f) * dimensions.Y * 0.8f,
                (_random.NextSingle() - 0.5f) * dimensions.Z * 0.8f
            );
            
            Vector3 moduleSize = new Vector3(
                8 + _random.Next(8),
                8 + _random.Next(8),
                12 + _random.Next(12)
            );
            
            GenerateBox(station, modulePos, moduleSize, config.Material);
        }
        
        // Add large cylindrical storage tanks
        int tankCount = 8 + _random.Next(4);
        for (int i = 0; i < tankCount; i++)
        {
            Vector3 tankPos = new Vector3(
                (_random.NextSingle() - 0.5f) * dimensions.X * 0.9f,
                (_random.NextSingle() - 0.5f) * dimensions.Y * 0.9f,
                (_random.NextSingle() - 0.5f) * dimensions.Z * 0.9f
            );
            
            float tankRadius = 6 + _random.Next(6);
            float tankHeight = 20 + _random.Next(20);
            GenerateCylinder(station, tankPos, tankRadius, tankHeight, config.Material);
        }
    }
    
    /// <summary>
    /// Generate sprawling complex with many interconnected sections
    /// </summary>
    private void GenerateSprawlingStation(GeneratedStation station, Vector3 dimensions, StationGenerationConfig config)
    {
        // Start with central section
        GenerateBox(station, Vector3.Zero, new Vector3(25, 25, 25), config.Material);
        
        // Grow outward with branching sections
        List<Vector3> growthPoints = new List<Vector3> { Vector3.Zero };
        int sectionCount = 30 + _random.Next(20);
        
        for (int i = 0; i < sectionCount; i++)
        {
            // Pick a random existing point to grow from
            Vector3 fromPoint = growthPoints[_random.Next(growthPoints.Count)];
            
            // Generate random direction
            Vector3 direction = new Vector3(
                (_random.NextSingle() - 0.5f) * 2f,
                (_random.NextSingle() - 0.5f) * 2f,
                (_random.NextSingle() - 0.5f) * 2f
            );
            direction = Vector3.Normalize(direction);
            
            float distance = 20 + _random.Next(20);
            Vector3 newPoint = fromPoint + direction * distance;
            
            // Add new section
            Vector3 sectionSize = new Vector3(
                10 + _random.Next(10),
                10 + _random.Next(10),
                15 + _random.Next(10)
            );
            GenerateBox(station, newPoint, sectionSize, config.Material);
            
            // Connect with corridor
            GenerateCorridor(station, fromPoint, newPoint, 3f, config.Material);
            
            growthPoints.Add(newPoint);
        }
    }
    
    /// <summary>
    /// Add station-type specific facilities
    /// </summary>
    private void AddStationFacilities(GeneratedStation station, StationGenerationConfig config)
    {
        switch (config.StationType.ToLower())
        {
            case "refinery":
                station.Facilities.Add("Ore Processing Plant");
                station.Facilities.Add("Ingot Storage");
                station.Facilities.Add("Quality Control Lab");
                break;
            case "trading":
                station.Facilities.Add("Market Hall");
                station.Facilities.Add("Cargo Storage");
                station.Facilities.Add("Trading Floor");
                break;
            case "military":
                station.Facilities.Add("Barracks");
                station.Facilities.Add("Armory");
                station.Facilities.Add("Command Center");
                break;
            case "shipyard":
                station.Facilities.Add("Construction Bay");
                station.Facilities.Add("Parts Warehouse");
                station.Facilities.Add("Engineering Lab");
                break;
            default:
                station.Facilities.Add("General Purpose Bay");
                break;
        }
    }
    
    /// <summary>
    /// Add docking bays for ships
    /// </summary>
    private void AddDockingBays(GeneratedStation station, StationGenerationConfig config)
    {
        int bayCount = Math.Max(config.MinDockingBays, 4 + _random.Next(8));
        float bayDistance = 40f;
        
        for (int i = 0; i < bayCount; i++)
        {
            float angle = (float)(i * 2 * Math.PI / bayCount);
            Vector3 bayPos = new Vector3(
                MathF.Cos(angle) * bayDistance,
                0,
                MathF.Sin(angle) * bayDistance
            );
            
            station.DockingPoints.Add(bayPos);
            
            // Create docking structure
            Vector3 baySize = new Vector3(12, 8, 15);
            GenerateBox(station, bayPos, baySize, config.Material);
            
            // Add docking arm
            Vector3 armDirection = Vector3.Normalize(bayPos);
            for (float d = 5; d < 15; d += 3)
            {
                Vector3 armPos = bayPos + armDirection * d;
                station.Structure.AddBlock(new VoxelBlock(
                    armPos,
                    new Vector3(2, 2, 6),
                    config.Material,
                    BlockType.Hull
                ));
            }
        }
    }
    
    /// <summary>
    /// Add external details and armor plating
    /// </summary>
    private void AddExternalDetails(GeneratedStation station, StationGenerationConfig config)
    {
        // Add communication arrays
        int arrayCount = 4 + _random.Next(4);
        for (int i = 0; i < arrayCount; i++)
        {
            Vector3 arrayPos = new Vector3(
                (_random.NextSingle() - 0.5f) * 100,
                (_random.NextSingle() - 0.5f) * 100,
                (_random.NextSingle() - 0.5f) * 100
            );
            
            // Dish array
            for (int j = 0; j < 5; j++)
            {
                station.Structure.AddBlock(new VoxelBlock(
                    arrayPos + new Vector3(j * 2, j, 0),
                    new Vector3(3, 3, 1),
                    config.Material,
                    BlockType.Hull
                ));
            }
        }
    }
    
    /// <summary>
    /// Add internal superstructure for structural realism
    /// </summary>
    private void AddInternalSuperstructure(GeneratedStation station, StationGenerationConfig config)
    {
        // This ensures we meet the minimum block count
        // Add internal framework beams throughout the station
        var existingBlocks = station.Structure.Blocks.ToList();
        int targetBlocks = config.Size switch
        {
            StationSize.Small => 2000,
            StationSize.Medium => 3000,
            StationSize.Large => 5000,
            StationSize.Massive => 8000,
            _ => 3000
        };
        
        while (station.BlockCount < targetBlocks && existingBlocks.Count > 0)
        {
            // Pick random existing block
            var refBlock = existingBlocks[_random.Next(existingBlocks.Count)];
            
            // Add nearby structural elements
            for (int i = 0; i < 3; i++)
            {
                Vector3 offset = new Vector3(
                    (_random.NextSingle() - 0.5f) * 10,
                    (_random.NextSingle() - 0.5f) * 10,
                    (_random.NextSingle() - 0.5f) * 10
                );
                
                Vector3 newPos = refBlock.Position + offset;
                float blockSize = 1.5f + (float)_random.NextDouble();
                
                station.Structure.AddBlock(new VoxelBlock(
                    newPos,
                    new Vector3(blockSize, blockSize, blockSize),
                    config.Material,
                    BlockType.Hull
                ));
                
                if (station.BlockCount >= targetBlocks) break;
            }
        }
    }
    
    // Helper geometry generation methods
    
    private void GenerateSphereSection(GeneratedStation station, Vector3 center, float radius, string material)
    {
        for (float x = -radius; x <= radius; x += 3)
        {
            for (float y = -radius; y <= radius; y += 3)
            {
                for (float z = -radius; z <= radius; z += 3)
                {
                    float distance = MathF.Sqrt(x * x + y * y + z * z);
                    if (distance <= radius && distance >= radius - 6)
                    {
                        float blockSize = 2f + (float)_random.NextDouble();
                        station.Structure.AddBlock(new VoxelBlock(
                            center + new Vector3(x, y, z),
                            new Vector3(blockSize, blockSize, blockSize),
                            material,
                            BlockType.Hull
                        ));
                    }
                }
            }
        }
    }
    
    private void GenerateBox(GeneratedStation station, Vector3 center, Vector3 size, string material)
    {
        // Generate hollow box
        for (float x = -size.X / 2; x <= size.X / 2; x += 2.5f)
        {
            for (float y = -size.Y / 2; y <= size.Y / 2; y += 2.5f)
            {
                for (float z = -size.Z / 2; z <= size.Z / 2; z += 2.5f)
                {
                    // Only create shell
                    bool isEdge = x <= -size.X / 2 + 3 || x >= size.X / 2 - 3 ||
                                  y <= -size.Y / 2 + 3 || y >= size.Y / 2 - 3 ||
                                  z <= -size.Z / 2 + 3 || z >= size.Z / 2 - 3;
                    
                    if (isEdge)
                    {
                        float blockSize = 2f + (float)_random.NextDouble();
                        station.Structure.AddBlock(new VoxelBlock(
                            center + new Vector3(x, y, z),
                            new Vector3(blockSize, blockSize, blockSize),
                            material,
                            BlockType.Hull
                        ));
                    }
                }
            }
        }
    }
    
    private void GenerateCorridor(GeneratedStation station, Vector3 start, Vector3 end, float thickness, string material)
    {
        Vector3 direction = end - start;
        float length = direction.Length();
        direction = Vector3.Normalize(direction);
        
        for (float d = 0; d < length; d += 2.5f)
        {
            Vector3 pos = start + direction * d;
            float blockSize = thickness;
            station.Structure.AddBlock(new VoxelBlock(
                pos,
                new Vector3(blockSize, blockSize, blockSize),
                material,
                BlockType.Hull
            ));
        }
    }
    
    private void GenerateCylinder(GeneratedStation station, Vector3 center, float radius, float height, string material)
    {
        for (float y = -height / 2; y <= height / 2; y += 3)
        {
            int segments = 16;
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)(i * 2 * Math.PI / segments);
                Vector3 pos = center + new Vector3(
                    MathF.Cos(angle) * radius,
                    y,
                    MathF.Sin(angle) * radius
                );
                
                float blockSize = 2f + (float)_random.NextDouble();
                station.Structure.AddBlock(new VoxelBlock(
                    pos,
                    new Vector3(blockSize, blockSize, blockSize),
                    material,
                    BlockType.Hull
                ));
            }
        }
    }
    
    private void GeneratePlatform(GeneratedStation station, Vector3 center, float radius, string material)
    {
        for (float x = -radius; x <= radius; x += 3)
        {
            for (float z = -radius; z <= radius; z += 3)
            {
                float distance = MathF.Sqrt(x * x + z * z);
                if (distance <= radius)
                {
                    float blockSize = 2f + (float)_random.NextDouble();
                    station.Structure.AddBlock(new VoxelBlock(
                        center + new Vector3(x, 0, z),
                        new Vector3(blockSize, 1.5f, blockSize),
                        material,
                        BlockType.Hull
                    ));
                }
            }
        }
    }
    
    private void GenerateFramework(GeneratedStation station, Vector3 center, Vector3 dimensions, string material)
    {
        // Create large framework beams along all axes
        float spacing = 10f;
        
        // X-axis beams
        for (float y = -dimensions.Y / 2; y <= dimensions.Y / 2; y += spacing)
        {
            for (float z = -dimensions.Z / 2; z <= dimensions.Z / 2; z += spacing)
            {
                for (float x = -dimensions.X / 2; x <= dimensions.X / 2; x += 3)
                {
                    station.Structure.AddBlock(new VoxelBlock(
                        center + new Vector3(x, y, z),
                        new Vector3(2, 2, 2),
                        material,
                        BlockType.Hull
                    ));
                }
            }
        }
        
        // Y-axis beams
        for (float x = -dimensions.X / 2; x <= dimensions.X / 2; x += spacing)
        {
            for (float z = -dimensions.Z / 2; z <= dimensions.Z / 2; z += spacing)
            {
                for (float y = -dimensions.Y / 2; y <= dimensions.Y / 2; y += 3)
                {
                    station.Structure.AddBlock(new VoxelBlock(
                        center + new Vector3(x, y, z),
                        new Vector3(2, 2, 2),
                        material,
                        BlockType.Hull
                    ));
                }
            }
        }
        
        // Z-axis beams
        for (float x = -dimensions.X / 2; x <= dimensions.X / 2; x += spacing)
        {
            for (float y = -dimensions.Y / 2; y <= dimensions.Y / 2; y += spacing)
            {
                for (float z = -dimensions.Z / 2; z <= dimensions.Z / 2; z += 3)
                {
                    station.Structure.AddBlock(new VoxelBlock(
                        center + new Vector3(x, y, z),
                        new Vector3(2, 2, 2),
                        material,
                        BlockType.Hull
                    ));
                }
            }
        }
    }
}
