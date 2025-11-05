using AvorionLike.Core.ECS;

namespace AvorionLike.Core.Fleet;

/// <summary>
/// Types of crew members
/// </summary>
public enum CrewType
{
    Engineer,
    Gunner,
    Pilot,
    Miner,
    Trader,
    Sergeant,
    Lieutenant,
    Captain
}

/// <summary>
/// Represents a crew member
/// </summary>
public class CrewMember
{
    public string Name { get; set; } = "Unknown";
    public CrewType Type { get; set; }
    public int Level { get; set; } = 1;
    public float Efficiency { get; set; } = 1.0f; // Skill multiplier
    public int HiringCost { get; set; }
    public int MonthlySalary { get; set; }
}

/// <summary>
/// Component for crew management
/// </summary>
public class CrewComponent : IComponent
{
    public Guid EntityId { get; set; }
    public List<CrewMember> Crew { get; set; } = new();
    public int MaxCrewCapacity { get; set; } = 10;
    public int CurrentCrewCount => Crew.Count;
    
    /// <summary>
    /// Hire a crew member
    /// </summary>
    public bool HireCrew(CrewMember member)
    {
        if (CurrentCrewCount >= MaxCrewCapacity)
        {
            return false;
        }
        
        Crew.Add(member);
        return true;
    }
    
    /// <summary>
    /// Fire a crew member
    /// </summary>
    public bool FireCrew(CrewMember member)
    {
        return Crew.Remove(member);
    }
    
    /// <summary>
    /// Get crew by type
    /// </summary>
    public IEnumerable<CrewMember> GetCrewByType(CrewType type)
    {
        return Crew.Where(c => c.Type == type);
    }
    
    /// <summary>
    /// Get total salary cost
    /// </summary>
    public int GetTotalMonthlySalary()
    {
        return Crew.Sum(c => c.MonthlySalary);
    }
}

/// <summary>
/// Captain that can automate ship operations
/// </summary>
public class Captain : CrewMember
{
    public CaptainCommand? CurrentCommand { get; set; } = null;
    public Guid? TargetSectorX { get; set; }
    public Guid? TargetSectorY { get; set; }
    public Guid? TargetSectorZ { get; set; }
    
    public Captain()
    {
        Type = CrewType.Captain;
        HiringCost = 50000;
        MonthlySalary = 5000;
    }
}

/// <summary>
/// Commands that a captain can execute
/// </summary>
public enum CaptainCommand
{
    Mine,
    Salvage,
    Trade,
    Patrol,
    Escort,
    Attack
}

/// <summary>
/// Component for ship automation via captain
/// </summary>
public class AutomationComponent : IComponent
{
    public Guid EntityId { get; set; }
    public Captain? Captain { get; set; }
    public bool IsAutomated => Captain != null;
    public CaptainCommand? CurrentCommand => Captain?.CurrentCommand;
}

/// <summary>
/// Represents a fleet of ships
/// </summary>
public class Fleet
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Fleet";
    public Guid OwnerId { get; set; }
    public List<Guid> ShipIds { get; set; } = new();
    public Guid? FlagshipId { get; set; }
    
    public void AddShip(Guid shipId)
    {
        if (!ShipIds.Contains(shipId))
        {
            ShipIds.Add(shipId);
        }
    }
    
    public void RemoveShip(Guid shipId)
    {
        ShipIds.Remove(shipId);
    }
}

/// <summary>
/// Component for fleet membership
/// </summary>
public class FleetMemberComponent : IComponent
{
    public Guid EntityId { get; set; }
    public Guid? FleetId { get; set; }
    public bool IsFleetLeader { get; set; } = false;
}

/// <summary>
/// System for managing fleets and crew
/// </summary>
public class FleetManagementSystem : SystemBase
{
    private readonly EntityManager _entityManager;
    private readonly Dictionary<Guid, Fleet> _fleets = new();

    public FleetManagementSystem(EntityManager entityManager) : base("FleetManagementSystem")
    {
        _entityManager = entityManager;
    }

    public override void Update(float deltaTime)
    {
        UpdateAutomatedShips(deltaTime);
    }
    
    /// <summary>
    /// Create a new fleet
    /// </summary>
    public Fleet CreateFleet(string name, Guid ownerId)
    {
        var fleet = new Fleet
        {
            Name = name,
            OwnerId = ownerId
        };
        
        _fleets[fleet.Id] = fleet;
        return fleet;
    }
    
    /// <summary>
    /// Add ship to fleet
    /// </summary>
    public bool AddShipToFleet(Guid shipId, Guid fleetId)
    {
        if (!_fleets.TryGetValue(fleetId, out var fleet))
        {
            return false;
        }
        
        fleet.AddShip(shipId);
        
        // Update ship's fleet component
        var fleetComponent = _entityManager.GetComponent<FleetMemberComponent>(shipId);
        if (fleetComponent != null)
        {
            fleetComponent.FleetId = fleetId;
        }
        
        return true;
    }
    
    /// <summary>
    /// Hire a crew member
    /// </summary>
    public bool HireCrewMember(Guid shipId, CrewMember member, int credits)
    {
        var crew = _entityManager.GetComponent<CrewComponent>(shipId);
        if (crew == null) return false;
        
        // Check credits
        if (credits < member.HiringCost)
        {
            return false;
        }
        
        return crew.HireCrew(member);
    }
    
    /// <summary>
    /// Hire a captain for automation
    /// </summary>
    public bool HireCaptain(Guid shipId, Captain captain, int credits)
    {
        if (credits < captain.HiringCost)
        {
            return false;
        }
        
        var automation = _entityManager.GetComponent<AutomationComponent>(shipId);
        if (automation == null)
        {
            automation = new AutomationComponent { EntityId = shipId };
            _entityManager.AddComponent(shipId, automation);
        }
        
        automation.Captain = captain;
        return true;
    }
    
    /// <summary>
    /// Give command to a captain
    /// </summary>
    public bool GiveCaptainCommand(Guid shipId, CaptainCommand command)
    {
        var automation = _entityManager.GetComponent<AutomationComponent>(shipId);
        if (automation?.Captain == null)
        {
            return false;
        }
        
        automation.Captain.CurrentCommand = command;
        return true;
    }
    
    /// <summary>
    /// Update automated ships
    /// </summary>
    private void UpdateAutomatedShips(float deltaTime)
    {
        var automatedShips = _entityManager.GetAllComponents<AutomationComponent>();
        
        foreach (var automation in automatedShips)
        {
            if (!automation.IsAutomated || automation.CurrentCommand == null)
            {
                continue;
            }
            
            // Execute captain's command
            switch (automation.CurrentCommand)
            {
                case CaptainCommand.Mine:
                    // Would integrate with mining system
                    break;
                case CaptainCommand.Salvage:
                    // Would integrate with salvage system
                    break;
                case CaptainCommand.Trade:
                    // Would integrate with trading system
                    break;
                case CaptainCommand.Patrol:
                    // Patrol around sector
                    break;
            }
        }
    }
    
    /// <summary>
    /// Get all fleets owned by a player
    /// </summary>
    public IEnumerable<Fleet> GetFleetsOwnedBy(Guid ownerId)
    {
        return _fleets.Values.Where(f => f.OwnerId == ownerId);
    }
}
