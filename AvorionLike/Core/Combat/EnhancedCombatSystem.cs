using System.Numerics;
using AvorionLike.Core.ECS;

namespace AvorionLike.Core.Combat;

/// <summary>
/// Extended weapon types matching Avorion-style combat
/// </summary>
public enum WeaponCategory
{
    Kinetic,     // Chainguns, Railguns - good against hull
    Energy,      // Lasers, Plasma - good against shields
    Explosive,   // Rockets, Torpedoes - area damage
    Pulse,       // Pulse cannons - bypass shields
    Lightning,   // Lightning guns - chain damage
    Mining       // Mining lasers
}

/// <summary>
/// Weapon firing modes
/// </summary>
public enum FiringMode
{
    Automatic,   // AI-controlled, fires at any valid target
    Manual,      // Player-controlled, fires when commanded
    PointDefense // Auto-fires at missiles/fighters only
}

/// <summary>
/// Enhanced turret with full Avorion-style capabilities
/// </summary>
public class EnhancedTurret
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Basic Turret";
    public WeaponType Type { get; set; } = WeaponType.Chaingun;
    public WeaponCategory Category { get; set; } = WeaponCategory.Kinetic;
    public FiringMode Mode { get; set; } = FiringMode.Automatic;
    
    // Damage stats
    public float BaseDamage { get; set; } = 10f;
    public float ShieldPenetration { get; set; } = 0f; // 0-1, how much bypasses shields
    public float HullDamageMultiplier { get; set; } = 1f;
    public float ShieldDamageMultiplier { get; set; } = 1f;
    
    // Firing stats
    public float FireRate { get; set; } = 1f; // Shots per second
    public float Range { get; set; } = 1000f;
    public float Accuracy { get; set; } = 0.95f; // 0-1
    public float ProjectileSpeed { get; set; } = 500f;
    
    // Energy and heat
    public float EnergyCostPerShot { get; set; } = 5f;
    public float HeatGeneration { get; set; } = 1f;
    public float CurrentHeat { get; set; } = 0f;
    public float MaxHeat { get; set; } = 100f;
    public float CooldownRate { get; set; } = 10f; // Per second
    
    // Targeting
    public bool IsAutoTargeting { get; set; } = false;
    public Vector3 MountPosition { get; set; } = Vector3.Zero;
    public Vector3 CurrentAimDirection { get; set; } = Vector3.UnitX;
    public float TrackingSpeed { get; set; } = 2f; // Radians per second
    
    // State
    public float TimeSinceLastShot { get; set; } = 0f;
    public Guid? CurrentTarget { get; set; } = null;
    public bool IsOverheated => CurrentHeat >= MaxHeat;
    public int WeaponGroupId { get; set; } = 0; // For manual firing groups
    
    /// <summary>
    /// Calculate actual damage against shields
    /// </summary>
    public float CalculateShieldDamage()
    {
        return BaseDamage * ShieldDamageMultiplier * (1f - ShieldPenetration);
    }
    
    /// <summary>
    /// Calculate actual damage against hull
    /// </summary>
    public float CalculateHullDamage()
    {
        return BaseDamage * HullDamageMultiplier;
    }
    
    /// <summary>
    /// Check if turret can fire
    /// </summary>
    public bool CanFire(float availableEnergy)
    {
        return !IsOverheated &&
               availableEnergy >= EnergyCostPerShot &&
               TimeSinceLastShot >= 1f / FireRate;
    }
}

/// <summary>
/// Turret Control Subsystem - increases turret slots
/// </summary>
public class TurretControlSubsystem : IComponent
{
    public Guid EntityId { get; set; }
    public string Name { get; set; } = "TCS Module";
    public int ArmedTurretSlots { get; set; } = 2;
    public int DefensiveTurretSlots { get; set; } = 1;
    public int AutoTurretSlots { get; set; } = 1;
    public float PowerRequirement { get; set; } = 20f;
}

/// <summary>
/// Fighter squadron component
/// </summary>
public class FighterSquadron
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Fighter Squadron";
    public FighterType Type { get; set; } = FighterType.Fighter;
    public int Count { get; set; } = 6; // Number of fighters in squadron
    public int MaxCount { get; set; } = 6;
    public float HullPerFighter { get; set; } = 50f;
    public float ShieldPerFighter { get; set; } = 25f;
    public float DamagePerFighter { get; set; } = 5f;
    public float Speed { get; set; } = 100f;
    public Vector3 CurrentPosition { get; set; }
    public Guid? CurrentTarget { get; set; }
    public SquadronState State { get; set; } = SquadronState.Docked;
}

public enum FighterType
{
    Fighter,      // Fast attack
    Bomber,       // Heavy damage
    Interceptor,  // Anti-fighter
    RepairDrone,  // Repairs ships
    CrewShuttle,  // For boarding
    Salvager      // Collects resources
}

public enum SquadronState
{
    Docked,       // In hangar
    Launching,    // Being launched
    Attacking,    // Engaging target
    Defending,    // Defending carrier
    Returning,    // Returning to carrier
    Salvaging     // Collecting resources
}

/// <summary>
/// Carrier component for deploying fighters
/// </summary>
public class CarrierComponent : IComponent
{
    public Guid EntityId { get; set; }
    public List<FighterSquadron> Squadrons { get; set; } = new();
    public int MaxSquadrons { get; set; } = 4;
    public float LaunchRate { get; set; } = 2f; // Squadrons per minute
    public float HangarCapacity { get; set; } = 100f; // Total fighter storage
    
    public void AddSquadron(FighterSquadron squadron)
    {
        if (Squadrons.Count < MaxSquadrons)
        {
            Squadrons.Add(squadron);
        }
    }
    
    public int GetDockedFighters()
    {
        return Squadrons.Where(s => s.State == SquadronState.Docked).Sum(s => s.Count);
    }
}

/// <summary>
/// Combat subsystem for special abilities and enhancements
/// </summary>
public class CombatSubsystem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; }
    public SubsystemType Type { get; set; }
    public float BonusValue { get; set; }
    public float PowerRequirement { get; set; }
    public bool IsActive { get; set; } = true;
    
    public CombatSubsystem(string name, SubsystemType type, float bonusValue, float power)
    {
        Name = name;
        Type = type;
        BonusValue = bonusValue;
        PowerRequirement = power;
    }
}

public enum SubsystemType
{
    ShieldBooster,           // Increases max shields
    ShieldRegenBooster,      // Increases shield regen
    EnergyToShieldConverter, // Converts energy to shields
    WeaponDamageBooster,     // Increases weapon damage
    WeaponRangeBooster,      // Increases weapon range
    TargetingComputer,       // Increases accuracy
    CoolantSystem,           // Reduces weapon heat
    HullReinforcement,       // Reduces hull damage taken
    PointDefenseArray,       // Auto-engages missiles/fighters
    HyperspaceBooster        // Increases jump range
}

/// <summary>
/// Enhanced combat component with full capabilities
/// </summary>
public class EnhancedCombatComponent : IComponent
{
    public Guid EntityId { get; set; }
    
    // Turrets and weapons
    public List<EnhancedTurret> Turrets { get; set; } = new();
    public List<TurretControlSubsystem> TCSModules { get; set; } = new();
    public List<int> WeaponGroups { get; set; } = new(); // For manual fire control
    
    // Defense
    public float CurrentShields { get; set; } = 0f;
    public float MaxShields { get; set; } = 0f;
    public float BaseShieldRegenRate { get; set; } = 10f;
    public float ShieldRegenDelay { get; set; } = 3f; // Seconds before regen starts
    public float TimeSinceLastHit { get; set; } = 0f;
    public float CurrentHull { get; set; } = 100f;
    public float MaxHull { get; set; } = 100f;
    public float ArmorRating { get; set; } = 0f; // Reduces incoming damage
    
    // Energy
    public float CurrentEnergy { get; set; } = 100f;
    public float MaxEnergy { get; set; } = 100f;
    public float EnergyRegenRate { get; set; } = 50f; // Per second
    
    // Targeting
    public Guid? CurrentTarget { get; set; } = null;
    public List<Guid> DetectedEnemies { get; set; } = new();
    public float SensorRange { get; set; } = 2000f;
    
    // Subsystems
    public List<CombatSubsystem> Subsystems { get; set; } = new();
    
    // Calculated stats (from subsystems and TCS)
    public int GetTotalArmedSlots() => 8 + TCSModules.Sum(t => t.ArmedTurretSlots);
    public int GetTotalDefensiveSlots() => 4 + TCSModules.Sum(t => t.DefensiveTurretSlots);
    public int GetTotalAutoSlots() => 4 + TCSModules.Sum(t => t.AutoTurretSlots);
    
    public float GetEffectiveShieldRegen()
    {
        float bonus = Subsystems
            .Where(s => s.Type == SubsystemType.ShieldRegenBooster && s.IsActive)
            .Sum(s => s.BonusValue);
        return BaseShieldRegenRate * (1f + bonus);
    }
    
    public float GetEffectiveMaxShields()
    {
        float bonus = Subsystems
            .Where(s => s.Type == SubsystemType.ShieldBooster && s.IsActive)
            .Sum(s => s.BonusValue);
        return MaxShields * (1f + bonus);
    }
    
    /// <summary>
    /// Get effective HP (shields + hull)
    /// </summary>
    public float GetEffectiveHP()
    {
        return CurrentShields + CurrentHull;
    }
    
    /// <summary>
    /// Check if ship is destroyed
    /// </summary>
    public bool IsDestroyed => CurrentHull <= 0f;
    
    /// <summary>
    /// Check if shields are down
    /// </summary>
    public bool ShieldsDown => CurrentShields <= 0f;
}

/// <summary>
/// Enhanced projectile with more properties
/// </summary>
public class EnhancedProjectile
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SourceEntityId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public float BaseDamage { get; set; }
    public float ShieldPenetration { get; set; }
    public float HullMultiplier { get; set; }
    public float ShieldMultiplier { get; set; }
    public WeaponType Type { get; set; }
    public WeaponCategory Category { get; set; }
    public float TimeToLive { get; set; }
    public float MaxRange { get; set; }
    public bool IsHoming { get; set; } = false;
    public Guid? HomingTarget { get; set; }
}
