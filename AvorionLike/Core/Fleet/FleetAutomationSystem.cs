using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Resources;
using AvorionLike.Core.Mining;
using AvorionLike.Core.RPG;

namespace AvorionLike.Core.Fleet;

/// <summary>
/// Captain automation orders
/// </summary>
public enum CaptainOrder
{
    None,
    Mine,           // Mine resources automatically
    Salvage,        // Salvage wreckage
    Trade,          // Trade between stations
    Patrol,         // Patrol and defend area
    Attack,         // Attack enemies
    Escort,         // Escort another ship
    Scout,          // Explore sectors
    Refine,         // Deliver resources to refineries
    Transport       // Transport goods
}

/// <summary>
/// Captain skill specializations
/// </summary>
public enum CaptainSpecialization
{
    None,
    Miner,          // +50% mining efficiency
    Salvager,       // +50% salvaging efficiency
    Trader,         // +30% trading profits
    Commander,      // +25% combat effectiveness
    Explorer,       // +40% exploration rewards
    Engineer        // +20% production speed
}

/// <summary>
/// Captain component - Enables ship automation
/// </summary>
public class CaptainComponent : IComponent
{
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Captain's name
    /// </summary>
    public string Name { get; set; } = "Captain";
    
    /// <summary>
    /// Captain level (1-10)
    /// </summary>
    public int Level { get; set; } = 1;
    
    /// <summary>
    /// Current order being executed
    /// </summary>
    public CaptainOrder CurrentOrder { get; set; } = CaptainOrder.None;
    
    /// <summary>
    /// Captain's specialization
    /// </summary>
    public CaptainSpecialization Specialization { get; set; } = CaptainSpecialization.None;
    
    /// <summary>
    /// Is captain currently executing orders
    /// </summary>
    public bool IsActive { get; set; } = false;
    
    /// <summary>
    /// Target sector for current order (if applicable)
    /// </summary>
    public Vector3? TargetLocation { get; set; }
    
    /// <summary>
    /// Target entity for escort/attack orders
    /// </summary>
    public Guid? TargetEntityId { get; set; }
    
    /// <summary>
    /// Resources to trade (for trade orders)
    /// </summary>
    public ResourceType? TradeResource { get; set; }
    
    /// <summary>
    /// Experience gained (for leveling)
    /// </summary>
    public int Experience { get; set; } = 0;
    
    /// <summary>
    /// Monthly salary cost
    /// </summary>
    public int MonthlySalary { get; set; } = 1000;
    
    /// <summary>
    /// Get efficiency multiplier based on specialization and current order
    /// </summary>
    public float GetEfficiencyMultiplier()
    {
        return (Specialization, CurrentOrder) switch
        {
            (CaptainSpecialization.Miner, CaptainOrder.Mine) => 1.5f,
            (CaptainSpecialization.Salvager, CaptainOrder.Salvage) => 1.5f,
            (CaptainSpecialization.Trader, CaptainOrder.Trade) => 1.3f,
            (CaptainSpecialization.Commander, CaptainOrder.Attack) => 1.25f,
            (CaptainSpecialization.Commander, CaptainOrder.Patrol) => 1.25f,
            (CaptainSpecialization.Explorer, CaptainOrder.Scout) => 1.4f,
            (CaptainSpecialization.Engineer, CaptainOrder.Refine) => 1.2f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Get level multiplier
    /// </summary>
    public float GetLevelMultiplier()
    {
        return 1.0f + (Level - 1) * 0.1f; // +10% per level
    }
    
    /// <summary>
    /// Get total efficiency (base * specialization * level)
    /// </summary>
    public float GetTotalEfficiency()
    {
        return GetEfficiencyMultiplier() * GetLevelMultiplier();
    }
}

/// <summary>
/// Fleet automation system - Manages captains and automated ships
/// </summary>
public class FleetAutomationSystem : SystemBase
{
    private readonly EntityManager _entityManager;
    private readonly Random _random = new();
    
    public FleetAutomationSystem(EntityManager entityManager) : base("FleetAutomationSystem")
    {
        _entityManager = entityManager;
    }
    
    public override void Update(float deltaTime)
    {
        var captains = _entityManager.GetAllComponents<CaptainComponent>();
        
        foreach (var captain in captains)
        {
            if (!captain.IsActive) continue;
            
            UpdateCaptainOrder(captain, deltaTime);
        }
    }
    
    /// <summary>
    /// Hire a captain for a ship
    /// </summary>
    public CaptainComponent HireCaptain(Guid shipId, string name, CaptainSpecialization specialization)
    {
        var captain = new CaptainComponent
        {
            EntityId = shipId,
            Name = name,
            Specialization = specialization,
            Level = 1,
            MonthlySalary = CalculateSalary(1, specialization)
        };
        
        _entityManager.AddComponent(shipId, captain);
        
        Console.WriteLine($"✓ Hired Captain {name} (Lv.{captain.Level} {specialization})");
        Console.WriteLine($"  Monthly Salary: {captain.MonthlySalary:N0} credits");
        
        return captain;
    }
    
    /// <summary>
    /// Give an order to a captain
    /// </summary>
    public void GiveOrder(CaptainComponent captain, CaptainOrder order, 
        Vector3? targetLocation = null, Guid? targetEntityId = null, ResourceType? tradeResource = null)
    {
        captain.CurrentOrder = order;
        captain.TargetLocation = targetLocation;
        captain.TargetEntityId = targetEntityId;
        captain.TradeResource = tradeResource;
        captain.IsActive = true;
        
        Console.WriteLine($"Captain {captain.Name}: Executing order '{order}'");
    }
    
    /// <summary>
    /// Stop captain's current order
    /// </summary>
    public void StopOrder(CaptainComponent captain)
    {
        captain.CurrentOrder = CaptainOrder.None;
        captain.IsActive = false;
        captain.TargetLocation = null;
        captain.TargetEntityId = null;
        captain.TradeResource = null;
        
        Console.WriteLine($"Captain {captain.Name}: Order stopped");
    }
    
    /// <summary>
    /// Update captain order execution
    /// </summary>
    private void UpdateCaptainOrder(CaptainComponent captain, float deltaTime)
    {
        switch (captain.CurrentOrder)
        {
            case CaptainOrder.Mine:
                ExecuteMiningOrder(captain, deltaTime);
                break;
            case CaptainOrder.Salvage:
                ExecuteSalvageOrder(captain, deltaTime);
                break;
            case CaptainOrder.Trade:
                ExecuteTradeOrder(captain, deltaTime);
                break;
            case CaptainOrder.Patrol:
                ExecutePatrolOrder(captain, deltaTime);
                break;
            case CaptainOrder.Attack:
                ExecuteAttackOrder(captain, deltaTime);
                break;
            case CaptainOrder.Scout:
                ExecuteScoutOrder(captain, deltaTime);
                break;
        }
    }
    
    private void ExecuteMiningOrder(CaptainComponent captain, float deltaTime)
    {
        // Automated mining logic
        var efficiency = captain.GetTotalEfficiency();
        
        // Add mining component if not present
        var miningComp = _entityManager.GetComponent<MiningComponent>(captain.EntityId);
        if (miningComp == null) return;
        
        // Captain automatically finds and mines asteroids
        // This is simplified - full implementation would use actual mining system
        var inventory = _entityManager.GetComponent<InventoryComponent>(captain.EntityId);
        if (inventory != null && inventory.Inventory.CurrentCapacity < inventory.Inventory.MaxCapacity)
        {
            // Award experience for successful mining
            captain.Experience += (int)(10 * efficiency * deltaTime);
            CheckLevelUp(captain);
        }
    }
    
    private void ExecuteSalvageOrder(CaptainComponent captain, float deltaTime)
    {
        // Automated salvaging logic
        var efficiency = captain.GetTotalEfficiency();
        
        // Award experience
        captain.Experience += (int)(8 * efficiency * deltaTime);
        CheckLevelUp(captain);
    }
    
    private void ExecuteTradeOrder(CaptainComponent captain, float deltaTime)
    {
        // Automated trading logic
        var efficiency = captain.GetTotalEfficiency();
        
        // Award experience
        captain.Experience += (int)(15 * efficiency * deltaTime);
        CheckLevelUp(captain);
    }
    
    private void ExecutePatrolOrder(CaptainComponent captain, float deltaTime)
    {
        // Automated patrol logic
        var efficiency = captain.GetTotalEfficiency();
        
        // Award experience
        captain.Experience += (int)(5 * efficiency * deltaTime);
        CheckLevelUp(captain);
    }
    
    private void ExecuteAttackOrder(CaptainComponent captain, float deltaTime)
    {
        // Automated combat logic
        var efficiency = captain.GetTotalEfficiency();
        
        // Award experience
        captain.Experience += (int)(20 * efficiency * deltaTime);
        CheckLevelUp(captain);
    }
    
    private void ExecuteScoutOrder(CaptainComponent captain, float deltaTime)
    {
        // Automated exploration logic
        var efficiency = captain.GetTotalEfficiency();
        
        // Award experience
        captain.Experience += (int)(12 * efficiency * deltaTime);
        CheckLevelUp(captain);
    }
    
    /// <summary>
    /// Check if captain should level up
    /// </summary>
    private void CheckLevelUp(CaptainComponent captain)
    {
        int expNeeded = captain.Level * 1000; // 1000 exp per level
        
        if (captain.Experience >= expNeeded && captain.Level < 10)
        {
            captain.Level++;
            captain.Experience -= expNeeded;
            captain.MonthlySalary = CalculateSalary(captain.Level, captain.Specialization);
            
            Console.WriteLine($"🎉 Captain {captain.Name} reached Level {captain.Level}!");
            Console.WriteLine($"   New efficiency: {captain.GetTotalEfficiency():P0}");
        }
    }
    
    /// <summary>
    /// Calculate captain salary based on level and specialization
    /// </summary>
    private int CalculateSalary(int level, CaptainSpecialization specialization)
    {
        int baseSalary = 1000;
        float specMultiplier = specialization == CaptainSpecialization.None ? 1.0f : 1.5f;
        return (int)(baseSalary * level * specMultiplier);
    }
    
    /// <summary>
    /// Generate random captain name
    /// </summary>
    public string GenerateRandomCaptainName()
    {
        var firstNames = new[] { "James", "Sarah", "Marcus", "Elena", "Viktor", "Aria", "Chen", "Yuki", "Dmitri", "Zara" };
        var lastNames = new[] { "Steel", "Nova", "Orion", "Vega", "Atlas", "Phoenix", "Drake", "Frost", "Storm", "Blaze" };
        
        return $"{firstNames[_random.Next(firstNames.Length)]} {lastNames[_random.Next(lastNames.Length)]}";
    }
    
    /// <summary>
    /// Get recommended specialization for ship type
    /// </summary>
    public CaptainSpecialization GetRecommendedSpecialization(Guid shipId)
    {
        var miningComp = _entityManager.GetComponent<MiningComponent>(shipId);
        if (miningComp != null) return CaptainSpecialization.Miner;
        
        var salvageComp = _entityManager.GetComponent<SalvagingComponent>(shipId);
        if (salvageComp != null) return CaptainSpecialization.Salvager;
        
        return CaptainSpecialization.Commander; // Default to combat
    }
}
