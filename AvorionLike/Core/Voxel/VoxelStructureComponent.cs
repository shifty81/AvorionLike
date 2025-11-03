using System.Numerics;
using AvorionLike.Core.ECS;

namespace AvorionLike.Core.Voxel;

/// <summary>
/// Component that contains voxel-based structure data for ships and stations
/// </summary>
public class VoxelStructureComponent : IComponent
{
    public Guid EntityId { get; set; }
    public List<VoxelBlock> Blocks { get; set; } = new();
    public Vector3 CenterOfMass { get; private set; }
    public float TotalMass { get; private set; }

    /// <summary>
    /// Add a voxel block to the structure
    /// </summary>
    public void AddBlock(VoxelBlock block)
    {
        Blocks.Add(block);
        RecalculateProperties();
    }

    /// <summary>
    /// Remove a voxel block from the structure
    /// </summary>
    public bool RemoveBlock(VoxelBlock block)
    {
        bool removed = Blocks.Remove(block);
        if (removed)
        {
            RecalculateProperties();
        }
        return removed;
    }

    /// <summary>
    /// Recalculate center of mass and total mass
    /// </summary>
    private void RecalculateProperties()
    {
        if (Blocks.Count == 0)
        {
            CenterOfMass = Vector3.Zero;
            TotalMass = 0f;
            return;
        }

        float totalMass = 0f;
        Vector3 weightedPosition = Vector3.Zero;

        foreach (var block in Blocks)
        {
            totalMass += block.Mass;
            weightedPosition += block.Position * block.Mass;
        }

        TotalMass = totalMass;
        CenterOfMass = weightedPosition / totalMass;
    }

    /// <summary>
    /// Get blocks at a specific position
    /// </summary>
    public IEnumerable<VoxelBlock> GetBlocksAt(Vector3 position, float tolerance = 0.1f)
    {
        return Blocks.Where(b => Vector3.Distance(b.Position, position) < tolerance);
    }
}
