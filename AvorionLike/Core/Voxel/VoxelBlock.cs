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
    public float Durability { get; set; }
    public float Mass { get; set; }
    public uint ColorRGB { get; set; } // RGB color as uint (e.g., 0xRRGGBB)

    public VoxelBlock(Vector3 position, Vector3 size, string materialType = "Iron")
    {
        Position = position;
        Size = size;
        MaterialType = materialType;
        Durability = 100f;
        Mass = size.X * size.Y * size.Z;
        ColorRGB = 0x808080; // Gray
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
