using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Resources;

namespace AvorionLike.Core.Navigation;

/// <summary>
/// Component for hyperdrive capabilities
/// </summary>
public class HyperdriveComponent : IComponent
{
    public Guid EntityId { get; set; }
    public float JumpRange { get; set; } = 5f; // Sectors
    public float JumpCooldown { get; set; } = 10f; // Seconds
    public float TimeSinceLastJump { get; set; } = 0f;
    public bool IsCharging { get; set; } = false;
    public float ChargeTime { get; set; } = 5f; // Seconds to charge
    public float CurrentCharge { get; set; } = 0f;
    public Vector3? TargetSector { get; set; } = null;
    
    public bool CanJump => TimeSinceLastJump >= JumpCooldown && !IsCharging;
    public bool IsFullyCharged => CurrentCharge >= ChargeTime;
}

/// <summary>
/// Represents a sector coordinate in the galaxy
/// </summary>
public class SectorCoordinate
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    
    public SectorCoordinate(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    
    /// <summary>
    /// Calculate distance to another sector
    /// </summary>
    public float DistanceTo(SectorCoordinate other)
    {
        int dx = X - other.X;
        int dy = Y - other.Y;
        int dz = Z - other.Z;
        return MathF.Sqrt(dx * dx + dy * dy + dz * dz);
    }
    
    /// <summary>
    /// Check if this sector is within jump range of another
    /// </summary>
    public bool IsInRangeOf(SectorCoordinate other, float range)
    {
        return DistanceTo(other) <= range;
    }
    
    /// <summary>
    /// Get distance from galaxy center (0,0,0)
    /// </summary>
    public float DistanceFromCenter()
    {
        return MathF.Sqrt(X * X + Y * Y + Z * Z);
    }
    
    /// <summary>
    /// Get tech level based on distance from center
    /// Outer regions (500+) = 1, Core (0) = 7
    /// </summary>
    public int GetTechLevel()
    {
        float distance = DistanceFromCenter();
        
        if (distance >= 500) return 1; // Iron
        if (distance >= 400) return 2; // Titanium
        if (distance >= 300) return 3; // Naonite
        if (distance >= 200) return 4; // Trinium
        if (distance >= 100) return 5; // Xanion
        if (distance >= 50) return 6;  // Ogonite
        return 7; // Avorion (core)
    }
}

/// <summary>
/// Component for current sector location
/// </summary>
public class SectorLocationComponent : IComponent
{
    public Guid EntityId { get; set; }
    public SectorCoordinate CurrentSector { get; set; } = new(0, 0, 0);
}

/// <summary>
/// System for hyperdrive jumps and navigation
/// </summary>
public class NavigationSystem : SystemBase
{
    private readonly EntityManager _entityManager;

    public NavigationSystem(EntityManager entityManager) : base("NavigationSystem")
    {
        _entityManager = entityManager;
    }

    public override void Update(float deltaTime)
    {
        var hyperdrives = _entityManager.GetAllComponents<HyperdriveComponent>();
        
        foreach (var hyperdrive in hyperdrives)
        {
            // Update cooldown
            if (hyperdrive.TimeSinceLastJump < hyperdrive.JumpCooldown)
            {
                hyperdrive.TimeSinceLastJump += deltaTime;
            }
            
            // Update charging
            if (hyperdrive.IsCharging && hyperdrive.TargetSector.HasValue)
            {
                hyperdrive.CurrentCharge += deltaTime;
                
                // Complete jump when fully charged
                if (hyperdrive.IsFullyCharged)
                {
                    ExecuteJump(hyperdrive);
                }
            }
        }
    }
    
    /// <summary>
    /// Start charging hyperdrive for a jump
    /// </summary>
    public bool StartJumpCharge(Guid entityId, SectorCoordinate targetSector)
    {
        var hyperdrive = _entityManager.GetComponent<HyperdriveComponent>(entityId);
        var location = _entityManager.GetComponent<SectorLocationComponent>(entityId);
        
        if (hyperdrive == null || location == null)
        {
            return false;
        }
        
        // Check if can jump
        if (!hyperdrive.CanJump)
        {
            return false;
        }
        
        // Check range
        float distance = location.CurrentSector.DistanceTo(targetSector);
        if (distance > hyperdrive.JumpRange)
        {
            return false;
        }
        
        // Start charging
        hyperdrive.IsCharging = true;
        hyperdrive.CurrentCharge = 0f;
        hyperdrive.TargetSector = new Vector3(targetSector.X, targetSector.Y, targetSector.Z);
        
        return true;
    }
    
    /// <summary>
    /// Cancel jump charge
    /// </summary>
    public void CancelJump(Guid entityId)
    {
        var hyperdrive = _entityManager.GetComponent<HyperdriveComponent>(entityId);
        if (hyperdrive != null)
        {
            hyperdrive.IsCharging = false;
            hyperdrive.CurrentCharge = 0f;
            hyperdrive.TargetSector = null;
        }
    }
    
    /// <summary>
    /// Execute the hyperdrive jump
    /// </summary>
    private void ExecuteJump(HyperdriveComponent hyperdrive)
    {
        if (!hyperdrive.TargetSector.HasValue)
        {
            return;
        }
        
        var location = _entityManager.GetComponent<SectorLocationComponent>(hyperdrive.EntityId);
        if (location == null)
        {
            return;
        }
        
        // Move to target sector
        var target = hyperdrive.TargetSector.Value;
        location.CurrentSector = new SectorCoordinate((int)target.X, (int)target.Y, (int)target.Z);
        
        // Reset hyperdrive state
        hyperdrive.IsCharging = false;
        hyperdrive.CurrentCharge = 0f;
        hyperdrive.TargetSector = null;
        hyperdrive.TimeSinceLastJump = 0f;
    }
    
    /// <summary>
    /// Upgrade hyperdrive range
    /// </summary>
    public bool UpgradeJumpRange(Guid entityId, float rangeIncrease, int creditCost, Inventory inventory)
    {
        var hyperdrive = _entityManager.GetComponent<HyperdriveComponent>(entityId);
        if (hyperdrive == null)
        {
            return false;
        }
        
        // Check if player has enough credits
        if (!inventory.HasResource(ResourceType.Credits, creditCost))
        {
            return false;
        }
        
        // Upgrade and deduct cost
        hyperdrive.JumpRange += rangeIncrease;
        inventory.RemoveResource(ResourceType.Credits, creditCost);
        
        return true;
    }
    
    /// <summary>
    /// Get all sectors within jump range
    /// </summary>
    public List<SectorCoordinate> GetReachableSectors(SectorCoordinate from, float jumpRange)
    {
        var reachable = new List<SectorCoordinate>();
        
        // Simple approach: check sectors in a cube around current position
        int range = (int)Math.Ceiling(jumpRange);
        
        for (int x = from.X - range; x <= from.X + range; x++)
        {
            for (int y = from.Y - range; y <= from.Y + range; y++)
            {
                for (int z = from.Z - range; z <= from.Z + range; z++)
                {
                    var sector = new SectorCoordinate(x, y, z);
                    if (sector.IsInRangeOf(from, jumpRange))
                    {
                        reachable.Add(sector);
                    }
                }
            }
        }
        
        return reachable;
    }
}
