using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Physics;
using AvorionLike.Core.Combat;

namespace AvorionLike.Core.AI;

/// <summary>
/// System that handles AI movement and navigation behaviors
/// </summary>
public class AIMovementSystem
{
    private readonly EntityManager _entityManager;
    
    public AIMovementSystem(EntityManager entityManager)
    {
        _entityManager = entityManager;
    }
    
    /// <summary>
    /// Execute movement behavior based on AI state
    /// </summary>
    public void ExecuteMovement(AIComponent ai, float deltaTime)
    {
        var physics = _entityManager.GetComponent<PhysicsComponent>(ai.EntityId);
        if (physics == null)
            return;
        
        switch (ai.CurrentState)
        {
            case AIState.Patrol:
                ExecutePatrolMovement(ai, physics, deltaTime);
                break;
            
            case AIState.Combat:
                ExecuteCombatMovement(ai, physics, deltaTime);
                break;
            
            case AIState.Fleeing:
                ExecuteFleeingMovement(ai, physics, deltaTime);
                break;
            
            case AIState.Mining:
            case AIState.Salvaging:
            case AIState.Trading:
                ExecuteApproachTarget(ai, physics, deltaTime);
                break;
            
            case AIState.ReturningToBase:
                ExecuteReturnToBase(ai, physics, deltaTime);
                break;
            
            case AIState.Idle:
                ExecuteIdleMovement(ai, physics, deltaTime);
                break;
        }
    }
    
    /// <summary>
    /// Patrol movement - move between waypoints
    /// </summary>
    private void ExecutePatrolMovement(AIComponent ai, PhysicsComponent physics, float deltaTime)
    {
        if (ai.PatrolWaypoints.Count == 0)
            return;
        
        // Get current waypoint
        var targetWaypoint = ai.PatrolWaypoints[ai.CurrentPatrolIndex];
        
        // Move towards waypoint
        MoveTowardsPosition(physics, targetWaypoint, 300f, deltaTime);
        
        // Check if reached waypoint
        float distance = Vector3.Distance(physics.Position, targetWaypoint);
        if (distance < 100f)
        {
            // Move to next waypoint
            ai.CurrentPatrolIndex = (ai.CurrentPatrolIndex + 1) % ai.PatrolWaypoints.Count;
        }
    }
    
    /// <summary>
    /// Combat movement - tactical maneuvering
    /// </summary>
    private void ExecuteCombatMovement(AIComponent ai, PhysicsComponent physics, float deltaTime)
    {
        if (!ai.CurrentTarget.HasValue)
            return;
        
        var targetPhysics = _entityManager.GetComponent<PhysicsComponent>(ai.CurrentTarget.Value);
        if (targetPhysics == null)
            return;
        
        switch (ai.CombatTactic)
        {
            case CombatTactic.Aggressive:
                ExecuteAggressiveManeuver(ai, physics, targetPhysics, deltaTime);
                break;
            
            case CombatTactic.Kiting:
                ExecuteKitingManeuver(ai, physics, targetPhysics, deltaTime);
                break;
            
            case CombatTactic.Strafing:
                ExecuteStrafingManeuver(ai, physics, targetPhysics, deltaTime);
                break;
            
            case CombatTactic.Broadsiding:
                ExecuteBroadsideManeuver(ai, physics, targetPhysics, deltaTime);
                break;
            
            case CombatTactic.Defensive:
                ExecuteDefensiveManeuver(ai, physics, targetPhysics, deltaTime);
                break;
        }
    }
    
    /// <summary>
    /// Aggressive maneuver - close distance and attack
    /// </summary>
    private void ExecuteAggressiveManeuver(AIComponent ai, PhysicsComponent physics, PhysicsComponent target, float deltaTime)
    {
        float distance = Vector3.Distance(physics.Position, target.Position);
        
        if (distance > ai.MinCombatDistance)
        {
            // Close in on target
            MoveTowardsPosition(physics, target.Position, 400f, deltaTime);
        }
        else
        {
            // Maintain minimum distance
            Vector3 awayDirection = Vector3.Normalize(physics.Position - target.Position);
            ApplyThrust(physics, awayDirection * 200f, deltaTime);
        }
        
        // Face the target
        FaceTarget(physics, target.Position, deltaTime);
    }
    
    /// <summary>
    /// Kiting maneuver - keep distance and attack
    /// </summary>
    private void ExecuteKitingManeuver(AIComponent ai, PhysicsComponent physics, PhysicsComponent target, float deltaTime)
    {
        float distance = Vector3.Distance(physics.Position, target.Position);
        float idealDistance = (ai.MinCombatDistance + ai.MaxCombatDistance) / 2f;
        
        if (distance < idealDistance)
        {
            // Move away to maintain distance
            Vector3 awayDirection = Vector3.Normalize(physics.Position - target.Position);
            MoveTowardsPosition(physics, physics.Position + awayDirection * 500f, 300f, deltaTime);
        }
        else if (distance > ai.MaxCombatDistance)
        {
            // Move closer if too far
            MoveTowardsPosition(physics, target.Position, 300f, deltaTime);
        }
        
        // Face the target while moving
        FaceTarget(physics, target.Position, deltaTime);
    }
    
    /// <summary>
    /// Strafing maneuver - circle around target
    /// </summary>
    private void ExecuteStrafingManeuver(AIComponent ai, PhysicsComponent physics, PhysicsComponent target, float deltaTime)
    {
        float distance = Vector3.Distance(physics.Position, target.Position);
        float idealDistance = (ai.MinCombatDistance + ai.MaxCombatDistance) / 2f;
        
        // Calculate perpendicular vector for circling
        Vector3 toTarget = Vector3.Normalize(target.Position - physics.Position);
        Vector3 perpendicular = new Vector3(-toTarget.Z, toTarget.Y, toTarget.X);
        
        // Adjust distance
        if (distance < idealDistance)
        {
            Vector3 awayDirection = Vector3.Normalize(physics.Position - target.Position);
            ApplyThrust(physics, awayDirection * 150f, deltaTime);
        }
        else if (distance > idealDistance)
        {
            ApplyThrust(physics, toTarget * 150f, deltaTime);
        }
        
        // Strafe perpendicular
        ApplyThrust(physics, perpendicular * 250f, deltaTime);
        
        // Face the target
        FaceTarget(physics, target.Position, deltaTime);
    }
    
    /// <summary>
    /// Broadside maneuver - position for maximum turret coverage
    /// </summary>
    private void ExecuteBroadsideManeuver(AIComponent ai, PhysicsComponent physics, PhysicsComponent target, float deltaTime)
    {
        float distance = Vector3.Distance(physics.Position, target.Position);
        float idealDistance = ai.MinCombatDistance + 200f;
        
        // Adjust distance
        if (Math.Abs(distance - idealDistance) > 50f)
        {
            if (distance < idealDistance)
            {
                Vector3 awayDirection = Vector3.Normalize(physics.Position - target.Position);
                ApplyThrust(physics, awayDirection * 200f, deltaTime);
            }
            else
            {
                MoveTowardsPosition(physics, target.Position, 200f, deltaTime);
            }
        }
        
        // Orient perpendicular to target for broadside
        Vector3 toTarget = Vector3.Normalize(target.Position - physics.Position);
        Vector3 perpendicular = new Vector3(-toTarget.Z, toTarget.Y, toTarget.X);
        FaceDirection(physics, perpendicular, deltaTime);
    }
    
    /// <summary>
    /// Defensive maneuver - maintain distance and evade
    /// </summary>
    private void ExecuteDefensiveManeuver(AIComponent ai, PhysicsComponent physics, PhysicsComponent target, float deltaTime)
    {
        float distance = Vector3.Distance(physics.Position, target.Position);
        
        // Try to maintain maximum combat distance
        if (distance < ai.MaxCombatDistance)
        {
            Vector3 awayDirection = Vector3.Normalize(physics.Position - target.Position);
            MoveTowardsPosition(physics, physics.Position + awayDirection * 500f, 250f, deltaTime);
        }
        
        // Face the target but be ready to evade
        FaceTarget(physics, target.Position, deltaTime);
        
        // Add some random evasive movement
        if (ai.StateTimer % 3f < 1f)
        {
            Vector3 evasionVector = new Vector3(
                (float)(Random.Shared.NextDouble() - 0.5),
                (float)(Random.Shared.NextDouble() - 0.5),
                (float)(Random.Shared.NextDouble() - 0.5)
            );
            ApplyThrust(physics, evasionVector * 150f, deltaTime);
        }
    }
    
    /// <summary>
    /// Fleeing movement - run away from threats
    /// </summary>
    private void ExecuteFleeingMovement(AIComponent ai, PhysicsComponent physics, float deltaTime)
    {
        // If has target, flee away from it
        if (ai.CurrentTarget.HasValue)
        {
            var targetPhysics = _entityManager.GetComponent<PhysicsComponent>(ai.CurrentTarget.Value);
            if (targetPhysics != null)
            {
                Vector3 fleeDirection = Vector3.Normalize(physics.Position - targetPhysics.Position);
                MoveTowardsPosition(physics, physics.Position + fleeDirection * 2000f, 500f, deltaTime);
                return;
            }
        }
        
        // Otherwise head to home base if available
        if (ai.HomeBase.HasValue)
        {
            MoveTowardsPosition(physics, ai.HomeBase.Value, 500f, deltaTime);
        }
        else
        {
            // Just move away in current direction
            if (physics.Velocity.Length() > 0.1f)
            {
                Vector3 moveDirection = Vector3.Normalize(physics.Velocity);
                ApplyThrust(physics, moveDirection * 400f, deltaTime);
            }
        }
    }
    
    /// <summary>
    /// Approach target for resource gathering or trading
    /// </summary>
    private void ExecuteApproachTarget(AIComponent ai, PhysicsComponent physics, float deltaTime)
    {
        if (!ai.CurrentWaypoint.HasValue)
            return;
        
        MoveTowardsPosition(physics, ai.CurrentWaypoint.Value, 250f, deltaTime);
        
        // Slow down when close
        float distance = Vector3.Distance(physics.Position, ai.CurrentWaypoint.Value);
        if (distance < 200f)
        {
            // Apply braking
            ApplyThrust(physics, -physics.Velocity * 0.5f, deltaTime);
        }
    }
    
    /// <summary>
    /// Return to base movement
    /// </summary>
    private void ExecuteReturnToBase(AIComponent ai, PhysicsComponent physics, float deltaTime)
    {
        if (!ai.HomeBase.HasValue)
            return;
        
        MoveTowardsPosition(physics, ai.HomeBase.Value, 350f, deltaTime);
    }
    
    /// <summary>
    /// Idle movement - stay relatively still
    /// </summary>
    private void ExecuteIdleMovement(AIComponent ai, PhysicsComponent physics, float deltaTime)
    {
        // Apply light braking to slow down
        if (physics.Velocity.Length() > 10f)
        {
            ApplyThrust(physics, -physics.Velocity * 0.3f, deltaTime);
        }
    }
    
    /// <summary>
    /// Move towards a position
    /// </summary>
    private void MoveTowardsPosition(PhysicsComponent physics, Vector3 targetPosition, float maxSpeed, float deltaTime)
    {
        Vector3 direction = targetPosition - physics.Position;
        float distance = direction.Length();
        
        if (distance < 0.1f)
            return;
        
        direction = Vector3.Normalize(direction);
        
        // Calculate desired velocity
        float speed = Math.Min(maxSpeed, distance);
        Vector3 desiredVelocity = direction * speed;
        
        // Apply force to reach desired velocity
        Vector3 force = (desiredVelocity - physics.Velocity) * physics.Mass * 2f;
        ApplyThrust(physics, force, deltaTime);
    }
    
    /// <summary>
    /// Apply thrust force
    /// </summary>
    private void ApplyThrust(PhysicsComponent physics, Vector3 force, float deltaTime)
    {
        physics.AddForce(force);
    }
    
    /// <summary>
    /// Face towards a target position
    /// </summary>
    private void FaceTarget(PhysicsComponent physics, Vector3 targetPosition, float deltaTime)
    {
        Vector3 direction = Vector3.Normalize(targetPosition - physics.Position);
        FaceDirection(physics, direction, deltaTime);
    }
    
    /// <summary>
    /// Face towards a direction
    /// </summary>
    private void FaceDirection(PhysicsComponent physics, Vector3 direction, float deltaTime)
    {
        // Calculate rotation needed
        // Simplified - in a full implementation would use quaternions
        Vector3 forward = Vector3.Normalize(physics.Velocity.Length() > 0.1f ? physics.Velocity : new Vector3(0, 0, 1));
        
        // Apply angular velocity to turn towards target
        float turnRate = 2f; // Radians per second
        Vector3 angularForce = direction * turnRate * physics.Mass;
        
        // Apply rotational force (simplified)
        physics.AngularVelocity += angularForce * deltaTime;
        
        // Limit angular velocity
        float maxAngularVelocity = 5f;
        if (physics.AngularVelocity.Length() > maxAngularVelocity)
        {
            physics.AngularVelocity = Vector3.Normalize(physics.AngularVelocity) * maxAngularVelocity;
        }
    }
}
