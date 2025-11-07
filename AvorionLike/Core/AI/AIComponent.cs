using System.Numerics;
using AvorionLike.Core.ECS;

namespace AvorionLike.Core.AI;

/// <summary>
/// Component that gives an entity AI capabilities
/// </summary>
public class AIComponent : IComponent
{
    public Guid EntityId { get; set; }
    
    /// <summary>
    /// Current state of the AI
    /// </summary>
    public AIState CurrentState { get; set; } = AIState.Idle;
    
    /// <summary>
    /// Previous state for state transitions
    /// </summary>
    public AIState PreviousState { get; set; } = AIState.Idle;
    
    /// <summary>
    /// AI personality affects decision making
    /// </summary>
    public AIPersonality Personality { get; set; } = AIPersonality.Balanced;
    
    /// <summary>
    /// Current target entity (for combat, mining, etc.)
    /// </summary>
    public Guid? CurrentTarget { get; set; }
    
    /// <summary>
    /// Current waypoint for patrol/movement
    /// </summary>
    public Vector3? CurrentWaypoint { get; set; }
    
    /// <summary>
    /// List of patrol waypoints
    /// </summary>
    public List<Vector3> PatrolWaypoints { get; set; } = new();
    
    /// <summary>
    /// Current patrol waypoint index
    /// </summary>
    public int CurrentPatrolIndex { get; set; } = 0;
    
    /// <summary>
    /// Time spent in current state
    /// </summary>
    public float StateTimer { get; set; } = 0f;
    
    /// <summary>
    /// Threshold for fleeing based on hull percentage
    /// </summary>
    public float FleeThreshold { get; set; } = 0.2f; // Flee at 20% health
    
    /// <summary>
    /// Threshold for returning to combat based on hull percentage
    /// </summary>
    public float ReturnToCombatThreshold { get; set; } = 0.6f; // Return at 60% health
    
    /// <summary>
    /// Minimum distance to maintain from target (combat)
    /// </summary>
    public float MinCombatDistance { get; set; } = 300f;
    
    /// <summary>
    /// Maximum distance to maintain from target (combat)
    /// </summary>
    public float MaxCombatDistance { get; set; } = 800f;
    
    /// <summary>
    /// Preferred combat tactic
    /// </summary>
    public CombatTactic CombatTactic { get; set; } = CombatTactic.Strafing;
    
    /// <summary>
    /// How long to idle before switching to patrol
    /// </summary>
    public float IdleTimeout { get; set; } = 10f;
    
    /// <summary>
    /// Whether AI can mine
    /// </summary>
    public bool CanMine { get; set; } = false;
    
    /// <summary>
    /// Whether AI can salvage
    /// </summary>
    public bool CanSalvage { get; set; } = false;
    
    /// <summary>
    /// Whether AI can trade
    /// </summary>
    public bool CanTrade { get; set; } = false;
    
    /// <summary>
    /// Home base location for returning
    /// </summary>
    public Vector3? HomeBase { get; set; }
    
    /// <summary>
    /// Cargo threshold percentage to return to base
    /// </summary>
    public float CargoReturnThreshold { get; set; } = 0.9f; // Return at 90% full
    
    /// <summary>
    /// Whether AI is enabled and active
    /// </summary>
    public bool IsEnabled { get; set; } = true;
    
    /// <summary>
    /// Time since last state evaluation
    /// </summary>
    public float TimeSinceLastEvaluation { get; set; } = 0f;
    
    /// <summary>
    /// How often to re-evaluate state (seconds)
    /// </summary>
    public float EvaluationInterval { get; set; } = 1f;
}

/// <summary>
/// Perception data about the AI's surroundings
/// </summary>
public class AIPerception
{
    /// <summary>
    /// Nearby entities that the AI is aware of
    /// </summary>
    public List<PerceivedEntity> NearbyEntities { get; set; } = new();
    
    /// <summary>
    /// Nearby asteroids
    /// </summary>
    public List<PerceivedAsteroid> NearbyAsteroids { get; set; } = new();
    
    /// <summary>
    /// Nearby stations
    /// </summary>
    public List<PerceivedStation> NearbyStations { get; set; } = new();
    
    /// <summary>
    /// Current threats detected
    /// </summary>
    public List<ThreatInfo> Threats { get; set; } = new();
    
    /// <summary>
    /// Last update time
    /// </summary>
    public float LastUpdateTime { get; set; } = 0f;
}

/// <summary>
/// Information about a perceived entity
/// </summary>
public class PerceivedEntity
{
    public Guid EntityId { get; set; }
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public float Distance { get; set; }
    public bool IsHostile { get; set; }
    public bool IsFriendly { get; set; }
    public float ShieldPercentage { get; set; }
    public float HullPercentage { get; set; }
}

/// <summary>
/// Information about a perceived asteroid
/// </summary>
public class PerceivedAsteroid
{
    public Guid AsteroidId { get; set; }
    public Vector3 Position { get; set; }
    public float Distance { get; set; }
    public string ResourceType { get; set; } = "Iron";
    public float RemainingResources { get; set; }
}

/// <summary>
/// Information about a perceived station
/// </summary>
public class PerceivedStation
{
    public Guid StationId { get; set; }
    public Vector3 Position { get; set; }
    public float Distance { get; set; }
    public string StationType { get; set; } = "Trading";
    public bool IsHostile { get; set; }
}

/// <summary>
/// Information about a threat
/// </summary>
public class ThreatInfo
{
    public Guid EntityId { get; set; }
    public Vector3 Position { get; set; }
    public float Distance { get; set; }
    public TargetPriority Priority { get; set; }
    public float ThreatLevel { get; set; } // 0-1 scale
    public bool IsAttacking { get; set; }
}
