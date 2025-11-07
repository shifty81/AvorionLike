using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Physics;
using AvorionLike.Core.Combat;
using AvorionLike.Core.Resources;
using AvorionLike.Core.Voxel;

namespace AvorionLike.Core.AI;

/// <summary>
/// System that handles AI decision making and prioritization
/// </summary>
public class AIDecisionSystem
{
    private readonly EntityManager _entityManager;
    
    public AIDecisionSystem(EntityManager entityManager)
    {
        _entityManager = entityManager;
    }
    
    /// <summary>
    /// Evaluate and decide which state the AI should be in
    /// </summary>
    public AIState EvaluateState(AIComponent ai, AIPerception perception)
    {
        var physics = _entityManager.GetComponent<PhysicsComponent>(ai.EntityId);
        var combat = _entityManager.GetComponent<CombatComponent>(ai.EntityId);
        var structure = _entityManager.GetComponent<VoxelStructureComponent>(ai.EntityId);
        var inventory = _entityManager.GetComponent<InventoryComponent>(ai.EntityId);
        
        if (physics == null)
            return AIState.Idle;
        
        // Calculate current status
        float hullPercentage = CalculateHullPercentage(structure);
        float shieldPercentage = combat != null 
            ? (combat.MaxShields > 0 ? combat.CurrentShields / combat.MaxShields : 0f)
            : 0f;
        float cargoPercentage = inventory != null
            ? (inventory.Inventory.MaxCapacity > 0 
                ? (float)inventory.Inventory.CurrentCapacity / inventory.Inventory.MaxCapacity 
                : 0f)
            : 0f;
        
        // Priority 1: Fleeing - if severely damaged
        if (ShouldFlee(ai, hullPercentage, shieldPercentage, perception))
        {
            return AIState.Fleeing;
        }
        
        // Priority 2: Combat - if under attack or hostile detected
        if (ShouldEnterCombat(ai, perception, hullPercentage))
        {
            return AIState.Combat;
        }
        
        // Priority 3: Return to base - if cargo full or need repairs
        if (ShouldReturnToBase(ai, cargoPercentage, hullPercentage))
        {
            return AIState.ReturningToBase;
        }
        
        // Priority 4: Resource gathering - based on personality and availability
        AIState? gatheringState = EvaluateGatheringState(ai, perception, cargoPercentage);
        if (gatheringState.HasValue)
        {
            return gatheringState.Value;
        }
        
        // Priority 5: Patrol - if has waypoints
        if (ai.PatrolWaypoints.Count > 0)
        {
            return AIState.Patrol;
        }
        
        // Default: Idle
        return AIState.Idle;
    }
    
    /// <summary>
    /// Check if AI should flee
    /// </summary>
    private bool ShouldFlee(AIComponent ai, float hullPercentage, float shieldPercentage, AIPerception perception)
    {
        // Already fleeing and not safe yet
        if (ai.CurrentState == AIState.Fleeing && hullPercentage < ai.ReturnToCombatThreshold)
        {
            return true;
        }
        
        // Cowardly personality flees more easily
        float fleeThreshold = ai.Personality == AIPersonality.Coward 
            ? ai.FleeThreshold * 2f 
            : ai.FleeThreshold;
        
        // Flee if hull is low
        if (hullPercentage < fleeThreshold)
        {
            return true;
        }
        
        // Flee if surrounded by many threats
        if (perception.Threats.Count >= 3 && hullPercentage < 0.5f)
        {
            return true;
        }
        
        // Flee if being attacked while shields are down and hull is damaged
        if (shieldPercentage <= 0 && hullPercentage < 0.5f && perception.Threats.Any(t => t.IsAttacking))
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Check if AI should enter combat
    /// </summary>
    private bool ShouldEnterCombat(AIComponent ai, AIPerception perception, float hullPercentage)
    {
        // Don't enter combat if fleeing or too damaged
        if (hullPercentage < ai.FleeThreshold * 1.5f)
        {
            return false;
        }
        
        // Trader/Miner personalities avoid combat unless attacked
        if (ai.Personality == AIPersonality.Trader || ai.Personality == AIPersonality.Miner)
        {
            return perception.Threats.Any(t => t.IsAttacking);
        }
        
        // Coward personality avoids combat
        if (ai.Personality == AIPersonality.Coward)
        {
            return false;
        }
        
        // Aggressive personality seeks combat
        if (ai.Personality == AIPersonality.Aggressive && perception.Threats.Count > 0)
        {
            return true;
        }
        
        // Defensive personality only fights when threatened
        if (ai.Personality == AIPersonality.Defensive)
        {
            return perception.Threats.Any(t => t.Priority >= TargetPriority.High || t.IsAttacking);
        }
        
        // Default: fight if being attacked or critical threat nearby
        return perception.Threats.Any(t => t.IsAttacking || t.Priority == TargetPriority.Critical);
    }
    
    /// <summary>
    /// Check if AI should return to base
    /// </summary>
    private bool ShouldReturnToBase(AIComponent ai, float cargoPercentage, float hullPercentage)
    {
        if (!ai.HomeBase.HasValue)
            return false;
        
        // Return if cargo is full
        if (cargoPercentage >= ai.CargoReturnThreshold)
        {
            return true;
        }
        
        // Return if damaged and not in combat
        if (hullPercentage < 0.5f && ai.CurrentState != AIState.Combat)
        {
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// Evaluate what gathering activity to do
    /// </summary>
    private AIState? EvaluateGatheringState(AIComponent ai, AIPerception perception, float cargoPercentage)
    {
        // Don't gather if cargo is full
        if (cargoPercentage >= ai.CargoReturnThreshold)
        {
            return null;
        }
        
        // Mining personality prefers mining
        if (ai.Personality == AIPersonality.Miner && ai.CanMine && perception.NearbyAsteroids.Count > 0)
        {
            return AIState.Mining;
        }
        
        // Salvager personality prefers salvaging
        if (ai.Personality == AIPersonality.Salvager && ai.CanSalvage)
        {
            // Would check for wreckage here
            return AIState.Salvaging;
        }
        
        // Trader personality prefers trading
        if (ai.Personality == AIPersonality.Trader && ai.CanTrade && perception.NearbyStations.Count > 0)
        {
            return AIState.Trading;
        }
        
        // Balanced personality tries all activities
        if (ai.Personality == AIPersonality.Balanced)
        {
            // Prefer mining if asteroids available
            if (ai.CanMine && perception.NearbyAsteroids.Count > 0)
            {
                return AIState.Mining;
            }
            
            // Trading if near station
            if (ai.CanTrade && perception.NearbyStations.Count > 0)
            {
                return AIState.Trading;
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Calculate hull percentage from structure
    /// </summary>
    private float CalculateHullPercentage(VoxelStructureComponent? structure)
    {
        if (structure == null)
            return 1f;
        
        // Calculate based on damaged blocks
        int totalBlocks = structure.Blocks.Count;
        if (totalBlocks == 0)
            return 0f;
        
        // Count undamaged blocks (blocks with durability >= 80% of max)
        int healthyBlocks = structure.Blocks.Count(b => b.Durability >= b.MaxDurability * 0.8f);
        
        return (float)healthyBlocks / totalBlocks;
    }
    
    /// <summary>
    /// Select target based on current state and perception
    /// </summary>
    public Guid? SelectTarget(AIComponent ai, AIPerception perception, AIPerceptionSystem perceptionSystem)
    {
        switch (ai.CurrentState)
        {
            case AIState.Combat:
                var threat = perceptionSystem.FindBestTarget(perception, ai.Personality);
                return threat?.EntityId;
            
            case AIState.Mining:
                var asteroid = perceptionSystem.FindBestAsteroid(perception, Vector3.Zero);
                return asteroid?.AsteroidId;
            
            case AIState.Salvaging:
                // Would find best wreckage
                return null;
            
            case AIState.Trading:
                // Would find best station
                var station = perception.NearbyStations.FirstOrDefault();
                return station?.StationId;
            
            default:
                return null;
        }
    }
    
    /// <summary>
    /// Calculate priority score for an action
    /// </summary>
    public float CalculateActionPriority(AIComponent ai, AIState state, AIPerception perception)
    {
        float priority = 0f;
        
        switch (state)
        {
            case AIState.Combat:
                priority = perception.Threats.Count > 0 ? 0.8f : 0f;
                if (perception.Threats.Any(t => t.IsAttacking))
                    priority = 1f;
                break;
            
            case AIState.Fleeing:
                // Calculated elsewhere
                priority = 1f;
                break;
            
            case AIState.Mining:
                priority = perception.NearbyAsteroids.Count > 0 ? 0.5f : 0f;
                if (ai.Personality == AIPersonality.Miner)
                    priority += 0.2f;
                break;
            
            case AIState.Trading:
                priority = perception.NearbyStations.Count > 0 ? 0.4f : 0f;
                if (ai.Personality == AIPersonality.Trader)
                    priority += 0.2f;
                break;
            
            case AIState.Patrol:
                priority = 0.3f;
                break;
            
            case AIState.Idle:
                priority = 0.1f;
                break;
        }
        
        return Math.Min(priority, 1f);
    }
}
