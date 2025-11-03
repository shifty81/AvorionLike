using System.Numerics;
using AvorionLike.Core.ECS;

namespace AvorionLike.Core.Physics;

/// <summary>
/// Component for Newtonian physics properties
/// </summary>
public class PhysicsComponent : IComponent
{
    public Guid EntityId { get; set; }
    
    // Linear motion
    public Vector3 Position { get; set; }
    public Vector3 Velocity { get; set; }
    public Vector3 Acceleration { get; set; }
    
    // Rotational motion
    public Vector3 Rotation { get; set; }
    public Vector3 AngularVelocity { get; set; }
    public Vector3 AngularAcceleration { get; set; }
    
    // Physical properties
    public float Mass { get; set; } = 1000f;
    public float Drag { get; set; } = 0.1f;
    public float AngularDrag { get; set; } = 0.1f;
    
    // Forces
    public Vector3 AppliedForce { get; set; }
    public Vector3 AppliedTorque { get; set; }
    
    // Collision
    public float CollisionRadius { get; set; } = 10f;
    public bool IsStatic { get; set; } = false;

    /// <summary>
    /// Apply a force to the object
    /// </summary>
    public void AddForce(Vector3 force)
    {
        AppliedForce += force;
    }

    /// <summary>
    /// Apply torque to the object
    /// </summary>
    public void AddTorque(Vector3 torque)
    {
        AppliedTorque += torque;
    }

    /// <summary>
    /// Clear all applied forces
    /// </summary>
    public void ClearForces()
    {
        AppliedForce = Vector3.Zero;
        AppliedTorque = Vector3.Zero;
    }
}
