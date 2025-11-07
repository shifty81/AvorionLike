using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Physics;
using AvorionLike.Core.Combat;
using AvorionLike.Core.Mining;
using AvorionLike.Core.Voxel;
using AvorionLike.Core.RPG;

namespace AvorionLike.Core.AI;

/// <summary>
/// System that handles AI perception of the environment
/// </summary>
public class AIPerceptionSystem
{
    private readonly EntityManager _entityManager;
    private readonly float _perceptionRange;
    
    public AIPerceptionSystem(EntityManager entityManager, float perceptionRange = 2000f)
    {
        _entityManager = entityManager;
        _perceptionRange = perceptionRange;
    }
    
    /// <summary>
    /// Update perception data for an AI entity
    /// </summary>
    public AIPerception UpdatePerception(Guid entityId, MiningSystem miningSystem)
    {
        var perception = new AIPerception();
        
        var physics = _entityManager.GetComponent<PhysicsComponent>(entityId);
        if (physics == null)
        {
            return perception;
        }
        
        var myPosition = physics.Position;
        
        // Perceive nearby entities
        perception.NearbyEntities = PerceiveEntities(entityId, myPosition);
        
        // Perceive asteroids
        perception.NearbyAsteroids = PerceiveAsteroids(myPosition, miningSystem);
        
        // Detect threats
        perception.Threats = DetectThreats(entityId, perception.NearbyEntities);
        
        return perception;
    }
    
    /// <summary>
    /// Perceive nearby entities
    /// </summary>
    private List<PerceivedEntity> PerceiveEntities(Guid selfId, Vector3 position)
    {
        var perceived = new List<PerceivedEntity>();
        
        var allPhysics = _entityManager.GetAllComponents<PhysicsComponent>();
        
        foreach (var physics in allPhysics)
        {
            // Skip self
            if (physics.EntityId == selfId)
                continue;
            
            float distance = Vector3.Distance(position, physics.Position);
            
            // Check if in perception range
            if (distance > _perceptionRange)
                continue;
            
            // Get combat component if exists
            var combat = _entityManager.GetComponent<CombatComponent>(physics.EntityId);
            var structure = _entityManager.GetComponent<VoxelStructureComponent>(physics.EntityId);
            
            // Check faction relations for hostility
            bool isHostile = false;
            bool isFriendly = false;
            
            var selfFaction = _entityManager.GetComponent<FactionComponent>(selfId);
            var targetFaction = _entityManager.GetComponent<FactionComponent>(physics.EntityId);
            
            if (selfFaction != null && targetFaction != null)
            {
                // Check faction standing based on reputation
                if (selfFaction.Reputation.TryGetValue(targetFaction.FactionName, out int standing))
                {
                    isHostile = standing < -30; // Hostile if reputation below -30
                    isFriendly = standing > 30; // Friendly if reputation above 30
                }
                else
                {
                    // No previous interaction - treat as neutral unless they're different factions
                    isHostile = selfFaction.FactionName != targetFaction.FactionName && 
                               selfFaction.FactionName != "Neutral" && 
                               targetFaction.FactionName != "Neutral";
                }
            }
            
            float shieldPercentage = 0f;
            float hullPercentage = 1f;
            
            if (combat != null)
            {
                shieldPercentage = combat.MaxShields > 0 
                    ? combat.CurrentShields / combat.MaxShields 
                    : 0f;
            }
            
            if (structure != null && structure.Blocks.Count > 0)
            {
                // Calculate hull percentage based on block durability
                float totalDurability = 0f;
                float maxDurability = 0f;
                
                foreach (var block in structure.Blocks)
                {
                    totalDurability += block.Durability;
                    maxDurability += block.MaxDurability;
                }
                
                hullPercentage = maxDurability > 0 ? totalDurability / maxDurability : 1f;
            }
            
            perceived.Add(new PerceivedEntity
            {
                EntityId = physics.EntityId,
                Position = physics.Position,
                Velocity = physics.Velocity,
                Distance = distance,
                IsHostile = isHostile,
                IsFriendly = isFriendly,
                ShieldPercentage = shieldPercentage,
                HullPercentage = hullPercentage
            });
        }
        
        return perceived;
    }
    
    /// <summary>
    /// Perceive nearby asteroids
    /// </summary>
    private List<PerceivedAsteroid> PerceiveAsteroids(Vector3 position, MiningSystem miningSystem)
    {
        var perceived = new List<PerceivedAsteroid>();
        
        // Get asteroids from mining system
        var asteroids = miningSystem.GetAllAsteroids();
        
        foreach (var asteroid in asteroids)
        {
            float distance = Vector3.Distance(position, asteroid.Position);
            
            if (distance > _perceptionRange)
                continue;
            
            perceived.Add(new PerceivedAsteroid
            {
                AsteroidId = asteroid.Id,
                Position = asteroid.Position,
                Distance = distance,
                ResourceType = asteroid.ResourceType.ToString(),
                RemainingResources = asteroid.RemainingResources
            });
        }
        
        return perceived;
    }
    
    /// <summary>
    /// Detect threats from perceived entities
    /// </summary>
    private List<ThreatInfo> DetectThreats(Guid selfId, List<PerceivedEntity> entities)
    {
        var threats = new List<ThreatInfo>();
        
        foreach (var entity in entities)
        {
            // Check if entity is hostile
            if (!entity.IsHostile)
                continue;
            
            // Calculate threat level based on distance and entity stats
            float threatLevel = CalculateThreatLevel(entity);
            
            // Check if entity is attacking us
            var combat = _entityManager.GetComponent<CombatComponent>(entity.EntityId);
            bool isAttacking = combat?.CurrentTarget == selfId;
            
            // Determine priority
            TargetPriority priority = DetermineThreatPriority(entity, threatLevel, isAttacking);
            
            threats.Add(new ThreatInfo
            {
                EntityId = entity.EntityId,
                Position = entity.Position,
                Distance = entity.Distance,
                Priority = priority,
                ThreatLevel = threatLevel,
                IsAttacking = isAttacking
            });
        }
        
        // Sort threats by priority and distance
        threats.Sort((a, b) =>
        {
            int priorityCompare = b.Priority.CompareTo(a.Priority);
            if (priorityCompare != 0)
                return priorityCompare;
            return a.Distance.CompareTo(b.Distance);
        });
        
        return threats;
    }
    
    /// <summary>
    /// Calculate threat level of an entity
    /// </summary>
    private float CalculateThreatLevel(PerceivedEntity entity)
    {
        float threatLevel = 0f;
        
        // Base threat on distance (closer = higher threat)
        float distanceFactor = 1f - Math.Min(entity.Distance / _perceptionRange, 1f);
        threatLevel += distanceFactor * 0.3f;
        
        // Higher threat if shields are strong
        threatLevel += entity.ShieldPercentage * 0.2f;
        
        // Higher threat if hull is strong
        threatLevel += entity.HullPercentage * 0.3f;
        
        // Higher threat if entity is approaching
        // (Would need to calculate velocity towards us)
        threatLevel += 0.2f; // Placeholder
        
        return Math.Min(threatLevel, 1f);
    }
    
    /// <summary>
    /// Determine priority of a threat
    /// </summary>
    private TargetPriority DetermineThreatPriority(PerceivedEntity entity, float threatLevel, bool isAttacking)
    {
        // Attacking entities are critical priority
        if (isAttacking)
            return TargetPriority.Critical;
        
        // Very close and hostile
        if (entity.Distance < 500f && entity.IsHostile)
            return TargetPriority.High;
        
        // Based on threat level
        if (threatLevel > 0.7f)
            return TargetPriority.High;
        if (threatLevel > 0.4f)
            return TargetPriority.Medium;
        if (threatLevel > 0.2f)
            return TargetPriority.Low;
        
        return TargetPriority.None;
    }
    
    /// <summary>
    /// Find best asteroid to mine
    /// </summary>
    public PerceivedAsteroid? FindBestAsteroid(AIPerception perception, Vector3 currentPosition)
    {
        if (perception.NearbyAsteroids.Count == 0)
            return null;
        
        // Filter asteroids with resources
        var validAsteroids = perception.NearbyAsteroids
            .Where(a => a.RemainingResources > 0)
            .ToList();
        
        if (validAsteroids.Count == 0)
            return null;
        
        // Sort by distance (closest first)
        validAsteroids.Sort((a, b) => a.Distance.CompareTo(b.Distance));
        
        return validAsteroids.First();
    }
    
    /// <summary>
    /// Find best target to attack
    /// </summary>
    public ThreatInfo? FindBestTarget(AIPerception perception, AIPersonality personality)
    {
        if (perception.Threats.Count == 0)
            return null;
        
        // Aggressive personalities prefer strongest targets
        if (personality == AIPersonality.Aggressive)
        {
            return perception.Threats
                .OrderByDescending(t => t.ThreatLevel)
                .FirstOrDefault();
        }
        
        // Defensive personalities prefer closest threats
        if (personality == AIPersonality.Defensive)
        {
            return perception.Threats
                .OrderBy(t => t.Distance)
                .FirstOrDefault();
        }
        
        // Default: highest priority, then closest
        return perception.Threats.FirstOrDefault();
    }
}
