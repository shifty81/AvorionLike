using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Physics;
using AvorionLike.Core.Combat;
using AvorionLike.Core.Mining;
using AvorionLike.Core.Resources;
using AvorionLike.Core.Voxel;
using AvorionLike.Core.Events;
using AvorionLike.Core.Logging;

namespace AvorionLike.Core.AI;

/// <summary>
/// Main AI system that manages all AI entities and orchestrates their behaviors
/// </summary>
public class AISystem : SystemBase
{
    private readonly EntityManager _entityManager;
    private readonly AIPerceptionSystem _perceptionSystem;
    private readonly AIDecisionSystem _decisionSystem;
    private readonly AIMovementSystem _movementSystem;
    private readonly MiningSystem _miningSystem;
    private readonly CombatSystem _combatSystem;
    
    // Cache for perception data
    private readonly Dictionary<Guid, AIPerception> _perceptionCache = new();
    
    public AISystem(
        EntityManager entityManager,
        MiningSystem miningSystem,
        CombatSystem combatSystem) : base("AISystem")
    {
        _entityManager = entityManager;
        _miningSystem = miningSystem;
        _combatSystem = combatSystem;
        _perceptionSystem = new AIPerceptionSystem(entityManager);
        _decisionSystem = new AIDecisionSystem(entityManager);
        _movementSystem = new AIMovementSystem(entityManager);
    }
    
    public override void Initialize()
    {
        Logger.Instance.Info("AISystem", "AI System initialized");
    }
    
    public override void Update(float deltaTime)
    {
        var aiComponents = _entityManager.GetAllComponents<AIComponent>();
        
        foreach (var ai in aiComponents)
        {
            if (!ai.IsEnabled)
                continue;
            
            try
            {
                UpdateAI(ai, deltaTime);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error("AISystem", $"Error updating AI {ai.EntityId}: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Update a single AI entity
    /// </summary>
    private void UpdateAI(AIComponent ai, float deltaTime)
    {
        ai.StateTimer += deltaTime;
        ai.TimeSinceLastEvaluation += deltaTime;
        
        // Update perception
        var perception = UpdatePerception(ai, deltaTime);
        
        // Evaluate state periodically
        if (ai.TimeSinceLastEvaluation >= ai.EvaluationInterval)
        {
            EvaluateAndTransitionState(ai, perception);
            ai.TimeSinceLastEvaluation = 0f;
        }
        
        // Execute current state behavior
        ExecuteStateBehavior(ai, perception, deltaTime);
        
        // Execute movement
        _movementSystem.ExecuteMovement(ai, deltaTime);
    }
    
    /// <summary>
    /// Update perception for an AI
    /// </summary>
    private AIPerception UpdatePerception(AIComponent ai, float deltaTime)
    {
        // Get or create perception cache
        if (!_perceptionCache.TryGetValue(ai.EntityId, out var perception))
        {
            perception = new AIPerception();
            _perceptionCache[ai.EntityId] = perception;
        }
        
        // Update perception data
        perception = _perceptionSystem.UpdatePerception(ai.EntityId, _miningSystem);
        _perceptionCache[ai.EntityId] = perception;
        
        return perception;
    }
    
    /// <summary>
    /// Evaluate state and transition if needed
    /// </summary>
    private void EvaluateAndTransitionState(AIComponent ai, AIPerception perception)
    {
        var newState = _decisionSystem.EvaluateState(ai, perception);
        
        if (newState != ai.CurrentState)
        {
            TransitionState(ai, newState, perception);
        }
    }
    
    /// <summary>
    /// Transition to a new state
    /// </summary>
    private void TransitionState(AIComponent ai, AIState newState, AIPerception perception)
    {
        Logger.Instance.Debug("AISystem", $"AI {ai.EntityId} transitioning from {ai.CurrentState} to {newState}");
        
        // Exit old state
        OnStateExit(ai, ai.CurrentState);
        
        // Update state
        ai.PreviousState = ai.CurrentState;
        ai.CurrentState = newState;
        ai.StateTimer = 0f;
        
        // Enter new state
        OnStateEnter(ai, newState, perception);
        
        // Publish state change event
        EventSystem.Instance.Publish("AI.StateChanged", new EntityEvent
        {
            EntityId = ai.EntityId,
            EntityName = $"AI_{ai.EntityId}"
        });
    }
    
    /// <summary>
    /// Called when entering a state
    /// </summary>
    private void OnStateEnter(AIComponent ai, AIState state, AIPerception perception)
    {
        switch (state)
        {
            case AIState.Combat:
                // Select combat target
                var target = _perceptionSystem.FindBestTarget(perception, ai.Personality);
                ai.CurrentTarget = target?.EntityId;
                break;
            
            case AIState.Mining:
                // Select asteroid to mine
                var asteroid = _perceptionSystem.FindBestAsteroid(perception, Vector3.Zero);
                if (asteroid != null)
                {
                    ai.CurrentTarget = asteroid.AsteroidId;
                    ai.CurrentWaypoint = asteroid.Position;
                }
                break;
            
            case AIState.Patrol:
                // Reset patrol index if starting fresh
                if (ai.PreviousState == AIState.Idle)
                {
                    ai.CurrentPatrolIndex = 0;
                }
                break;
            
            case AIState.Fleeing:
                // Clear current target when fleeing
                ai.CurrentTarget = null;
                break;
            
            case AIState.ReturningToBase:
                // Set waypoint to home base
                if (ai.HomeBase.HasValue)
                {
                    ai.CurrentWaypoint = ai.HomeBase.Value;
                }
                break;
        }
    }
    
    /// <summary>
    /// Called when exiting a state
    /// </summary>
    private void OnStateExit(AIComponent ai, AIState state)
    {
        switch (state)
        {
            case AIState.Mining:
                // Stop mining
                var miningComp = _entityManager.GetComponent<MiningComponent>(ai.EntityId);
                if (miningComp != null)
                {
                    miningComp.IsMining = false;
                }
                break;
            
            case AIState.Combat:
                // Clear combat target from combat component
                var combatComp = _entityManager.GetComponent<CombatComponent>(ai.EntityId);
                if (combatComp != null)
                {
                    combatComp.CurrentTarget = null;
                }
                break;
        }
    }
    
    /// <summary>
    /// Execute behavior for current state
    /// </summary>
    private void ExecuteStateBehavior(AIComponent ai, AIPerception perception, float deltaTime)
    {
        switch (ai.CurrentState)
        {
            case AIState.Idle:
                ExecuteIdleBehavior(ai, deltaTime);
                break;
            
            case AIState.Patrol:
                ExecutePatrolBehavior(ai, deltaTime);
                break;
            
            case AIState.Mining:
                ExecuteMiningBehavior(ai, deltaTime);
                break;
            
            case AIState.Salvaging:
                ExecuteSalvagingBehavior(ai, deltaTime);
                break;
            
            case AIState.Trading:
                ExecuteTradingBehavior(ai, deltaTime);
                break;
            
            case AIState.Combat:
                ExecuteCombatBehavior(ai, perception, deltaTime);
                break;
            
            case AIState.Fleeing:
                ExecuteFleeingBehavior(ai, perception, deltaTime);
                break;
            
            case AIState.Evasion:
                ExecuteEvasionBehavior(ai, deltaTime);
                break;
            
            case AIState.ReturningToBase:
                ExecuteReturnToBaseBehavior(ai, deltaTime);
                break;
        }
    }
    
    /// <summary>
    /// Idle behavior - do nothing or transition to patrol
    /// </summary>
    private void ExecuteIdleBehavior(AIComponent ai, float deltaTime)
    {
        // Transition to patrol after idle timeout
        if (ai.StateTimer >= ai.IdleTimeout && ai.PatrolWaypoints.Count > 0)
        {
            ai.CurrentState = AIState.Patrol;
        }
    }
    
    /// <summary>
    /// Patrol behavior - handled by movement system
    /// </summary>
    private void ExecutePatrolBehavior(AIComponent ai, float deltaTime)
    {
        // Movement is handled by AIMovementSystem
    }
    
    /// <summary>
    /// Mining behavior
    /// </summary>
    private void ExecuteMiningBehavior(AIComponent ai, float deltaTime)
    {
        if (!ai.CurrentTarget.HasValue)
            return;
        
        var physics = _entityManager.GetComponent<PhysicsComponent>(ai.EntityId);
        var miningComp = _entityManager.GetComponent<MiningComponent>(ai.EntityId);
        
        if (physics == null || miningComp == null)
            return;
        
        // Check if close enough to mine
        if (ai.CurrentWaypoint.HasValue)
        {
            float distance = Vector3.Distance(physics.Position, ai.CurrentWaypoint.Value);
            
            if (distance < miningComp.MiningRange)
            {
                // Start mining if not already
                if (!miningComp.IsMining)
                {
                    _miningSystem.StartMining(miningComp, ai.CurrentTarget.Value, physics.Position);
                }
            }
        }
    }
    
    /// <summary>
    /// Salvaging behavior
    /// </summary>
    private void ExecuteSalvagingBehavior(AIComponent ai, float deltaTime)
    {
        // Similar to mining but for wreckage
        // Implementation would be similar to mining
    }
    
    /// <summary>
    /// Trading behavior
    /// </summary>
    private void ExecuteTradingBehavior(AIComponent ai, float deltaTime)
    {
        // Implementation would interact with trading system
        // Check if at station, sell resources, buy goods, etc.
    }
    
    /// <summary>
    /// Combat behavior
    /// </summary>
    private void ExecuteCombatBehavior(AIComponent ai, AIPerception perception, float deltaTime)
    {
        if (!ai.CurrentTarget.HasValue)
        {
            // Try to find a new target
            var threat = _perceptionSystem.FindBestTarget(perception, ai.Personality);
            if (threat != null)
            {
                ai.CurrentTarget = threat.EntityId;
            }
            return;
        }
        
        var physics = _entityManager.GetComponent<PhysicsComponent>(ai.EntityId);
        var combat = _entityManager.GetComponent<CombatComponent>(ai.EntityId);
        var targetPhysics = _entityManager.GetComponent<PhysicsComponent>(ai.CurrentTarget.Value);
        
        if (physics == null || combat == null || targetPhysics == null)
            return;
        
        // Update combat component target
        combat.CurrentTarget = ai.CurrentTarget;
        
        // Fire weapons at target
        float distance = Vector3.Distance(physics.Position, targetPhysics.Position);
        
        foreach (var turret in combat.Turrets)
        {
            if (distance <= turret.Range && combat.CanFire(turret))
            {
                _combatSystem.FireTurret(combat, turret, targetPhysics.Position, physics.Position);
            }
        }
    }
    
    /// <summary>
    /// Fleeing behavior
    /// </summary>
    private void ExecuteFleeingBehavior(AIComponent ai, AIPerception perception, float deltaTime)
    {
        // Update flee target if being chased
        if (perception.Threats.Count > 0)
        {
            var closestThreat = perception.Threats
                .OrderBy(t => t.Distance)
                .FirstOrDefault();
            
            if (closestThreat != null)
            {
                ai.CurrentTarget = closestThreat.EntityId;
            }
        }
    }
    
    /// <summary>
    /// Evasion behavior
    /// </summary>
    private void ExecuteEvasionBehavior(AIComponent ai, float deltaTime)
    {
        // Similar to fleeing but more tactical
    }
    
    /// <summary>
    /// Return to base behavior
    /// </summary>
    private void ExecuteReturnToBaseBehavior(AIComponent ai, float deltaTime)
    {
        if (!ai.HomeBase.HasValue)
            return;
        
        var physics = _entityManager.GetComponent<PhysicsComponent>(ai.EntityId);
        if (physics == null)
            return;
        
        float distance = Vector3.Distance(physics.Position, ai.HomeBase.Value);
        
        // If reached base, switch to repairing/trading
        if (distance < 100f)
        {
            // Would interact with station here
            Logger.Instance.Info("AISystem", $"AI {ai.EntityId} reached home base");
        }
    }
    
    /// <summary>
    /// Add an AI component to an entity
    /// </summary>
    public void AddAI(Guid entityId, AIPersonality personality = AIPersonality.Balanced)
    {
        var ai = new AIComponent
        {
            EntityId = entityId,
            Personality = personality,
            IsEnabled = true
        };
        
        _entityManager.AddComponent(entityId, ai);
        
        Logger.Instance.Info("AISystem", $"Added AI component to entity {entityId} with personality {personality}");
    }
    
    /// <summary>
    /// Set patrol waypoints for an AI
    /// </summary>
    public void SetPatrolWaypoints(Guid entityId, List<Vector3> waypoints)
    {
        var ai = _entityManager.GetComponent<AIComponent>(entityId);
        if (ai != null)
        {
            ai.PatrolWaypoints = waypoints;
            ai.CurrentPatrolIndex = 0;
        }
    }
    
    /// <summary>
    /// Set home base for an AI
    /// </summary>
    public void SetHomeBase(Guid entityId, Vector3 homePosition)
    {
        var ai = _entityManager.GetComponent<AIComponent>(entityId);
        if (ai != null)
        {
            ai.HomeBase = homePosition;
        }
    }
    
    public override void Shutdown()
    {
        _perceptionCache.Clear();
        Logger.Instance.Info("AISystem", "AI System shut down");
    }
}
