using System.Numerics;

namespace AvorionLike.Core.Procedural;

/// <summary>
/// Procedural galaxy generator
/// </summary>
public class GalaxyGenerator
{
    private readonly Random _random;
    private readonly int _seed;

    public GalaxyGenerator(int seed = 0)
    {
        _seed = seed == 0 ? Environment.TickCount : seed;
        _random = new Random(_seed);
    }

    /// <summary>
    /// Generate a galaxy sector
    /// </summary>
    public GalaxySector GenerateSector(int x, int y, int z)
    {
        // Use sector coordinates as seed for consistent generation
        var sectorSeed = HashCoordinates(x, y, z, _seed);
        var sectorRandom = new Random(sectorSeed);

        var sector = new GalaxySector(x, y, z);
        
        // Generate asteroids
        int asteroidCount = sectorRandom.Next(5, 20);
        for (int i = 0; i < asteroidCount; i++)
        {
            var position = new Vector3(
                (float)sectorRandom.NextDouble() * 10000 - 5000,
                (float)sectorRandom.NextDouble() * 10000 - 5000,
                (float)sectorRandom.NextDouble() * 10000 - 5000
            );
            
            sector.Asteroids.Add(new AsteroidData
            {
                Position = position,
                Size = (float)sectorRandom.NextDouble() * 50 + 10,
                ResourceType = GetRandomResourceType(sectorRandom)
            });
        }

        // Potentially generate a station
        if (sectorRandom.NextDouble() < 0.2) // 20% chance
        {
            sector.Station = new StationData
            {
                Position = new Vector3(0, 0, 0),
                StationType = GetRandomStationType(sectorRandom),
                Name = GenerateStationName(sectorRandom)
            };
        }

        return sector;
    }

    private int HashCoordinates(int x, int y, int z, int seed)
    {
        unchecked
        {
            int hash = seed;
            hash = hash * 397 ^ x;
            hash = hash * 397 ^ y;
            hash = hash * 397 ^ z;
            return hash;
        }
    }

    private string GetRandomResourceType(Random random)
    {
        string[] types = { "Iron", "Titanium", "Naonite", "Trinium", "Xanion", "Ogonite", "Avorion" };
        return types[random.Next(types.Length)];
    }

    private string GetRandomStationType(Random random)
    {
        string[] types = { "Trading", "Military", "Mining", "Shipyard", "Research" };
        return types[random.Next(types.Length)];
    }

    private string GenerateStationName(Random random)
    {
        string[] prefixes = { "Alpha", "Beta", "Gamma", "Delta", "Epsilon", "Zeta" };
        string[] suffixes = { "Outpost", "Station", "Base", "Hub", "Terminal" };
        
        return $"{prefixes[random.Next(prefixes.Length)]} {suffixes[random.Next(suffixes.Length)]}";
    }
}

/// <summary>
/// Represents a sector in the galaxy
/// </summary>
public class GalaxySector
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public List<AsteroidData> Asteroids { get; set; } = new();
    public StationData? Station { get; set; }
    public List<ShipData> Ships { get; set; } = new();

    public GalaxySector(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
}

public class AsteroidData
{
    public Vector3 Position { get; set; }
    public float Size { get; set; }
    public string ResourceType { get; set; } = "Iron";
}

public class StationData
{
    public Vector3 Position { get; set; }
    public string StationType { get; set; } = "Trading";
    public string Name { get; set; } = "Unknown Station";
}

public class ShipData
{
    public Vector3 Position { get; set; }
    public string ShipType { get; set; } = "Fighter";
    public string Faction { get; set; } = "Neutral";
}
