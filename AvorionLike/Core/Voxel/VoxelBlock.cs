using System.Numerics;

namespace AvorionLike.Core.Voxel;

/// <summary>
/// Represents a voxel block with position, size, and material properties
/// </summary>
public class VoxelBlock
{
    public Vector3 Position { get; set; }
    public Vector3 Size { get; set; }
    public string MaterialType { get; set; }
    public BlockType BlockType { get; set; } = BlockType.Hull;
    public float Durability { get; set; }
    public float MaxDurability { get; set; }
    public float Mass { get; set; }
    public uint ColorRGB { get; set; } // RGB color as uint (e.g., 0xRRGGBB)
    public bool IsDestroyed { get; set; } = false;
    
    // Functional properties for engines, thrusters, etc.
    public float ThrustPower { get; set; } = 0f; // For engines/thrusters
    public float PowerGeneration { get; set; } = 0f; // For generators
    public float ShieldCapacity { get; set; } = 0f; // For shield generators

    public VoxelBlock(Vector3 position, Vector3 size, string materialType = "Iron", BlockType blockType = BlockType.Hull)
    {
        Position = position;
        Size = size;
        MaterialType = materialType;
        BlockType = blockType;
        
        var material = MaterialProperties.GetMaterial(materialType);
        float volume = size.X * size.Y * size.Z;
        
        // Calculate properties based on material and block type
        Mass = volume * material.MassMultiplier;
        MaxDurability = 100f * material.DurabilityMultiplier * volume;
        Durability = MaxDurability;
        ColorRGB = material.Color;
        
        // Set functional properties based on block type
        switch (blockType)
        {
            case BlockType.Engine:
                ThrustPower = 50f * volume * material.EnergyEfficiency;
                break;
            case BlockType.Thruster:
                ThrustPower = 30f * volume * material.EnergyEfficiency;
                break;
            case BlockType.GyroArray:
                ThrustPower = 20f * volume * material.EnergyEfficiency; // Torque
                break;
            case BlockType.Generator:
                PowerGeneration = 100f * volume * material.EnergyEfficiency;
                break;
            case BlockType.ShieldGenerator:
                ShieldCapacity = 200f * volume * material.ShieldMultiplier;
                break;
        }
    }

    /// <summary>
    /// Take damage to this block
    /// </summary>
    public void TakeDamage(float damage)
    {
        Durability -= damage;
        if (Durability <= 0)
        {
            Durability = 0;
            IsDestroyed = true;
        }
    }

    /// <summary>
    /// Check if this voxel intersects with another
    /// </summary>
    public bool Intersects(VoxelBlock other)
    {
        return Position.X < other.Position.X + other.Size.X &&
               Position.X + Size.X > other.Position.X &&
               Position.Y < other.Position.Y + other.Size.Y &&
               Position.Y + Size.Y > other.Position.Y &&
               Position.Z < other.Position.Z + other.Size.Z &&
               Position.Z + Size.Z > other.Position.Z;
    }
}
