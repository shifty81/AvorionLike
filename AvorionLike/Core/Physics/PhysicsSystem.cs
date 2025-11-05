using System.Numerics;
using AvorionLike.Core.ECS;

namespace AvorionLike.Core.Physics;

/// <summary>
/// System that handles Newtonian physics simulation
/// </summary>
public class PhysicsSystem : SystemBase
{
    private readonly EntityManager _entityManager;
    private const float MaxVelocity = 1000f;

    public PhysicsSystem(EntityManager entityManager) : base("PhysicsSystem")
    {
        _entityManager = entityManager;
    }

    public override void Update(float deltaTime)
    {
        var physicsComponents = _entityManager.GetAllComponents<PhysicsComponent>();

        foreach (var physics in physicsComponents)
        {
            if (physics.IsStatic) continue;

            // Calculate acceleration from forces (F = ma, a = F/m)
            physics.Acceleration = physics.AppliedForce / physics.Mass;
            
            // Calculate angular acceleration from torque (τ = Iα, α = τ/I)
            physics.AngularAcceleration = physics.AppliedTorque / physics.MomentOfInertia;

            // Update velocities
            physics.Velocity += physics.Acceleration * deltaTime;
            physics.AngularVelocity += physics.AngularAcceleration * deltaTime;

            // Apply drag
            physics.Velocity *= (1f - physics.Drag * deltaTime);
            physics.AngularVelocity *= (1f - physics.AngularDrag * deltaTime);

            // Clamp velocities to prevent excessive speeds
            if (physics.Velocity.Length() > MaxVelocity)
            {
                physics.Velocity = Vector3.Normalize(physics.Velocity) * MaxVelocity;
            }

            // Update positions
            physics.Position += physics.Velocity * deltaTime;
            physics.Rotation += physics.AngularVelocity * deltaTime;

            // Clear forces for next frame
            physics.ClearForces();
        }

        // Simple collision detection
        DetectCollisions(physicsComponents.ToList());
    }

    private void DetectCollisions(List<PhysicsComponent> components)
    {
        for (int i = 0; i < components.Count; i++)
        {
            for (int j = i + 1; j < components.Count; j++)
            {
                var comp1 = components[i];
                var comp2 = components[j];

                float distance = Vector3.Distance(comp1.Position, comp2.Position);
                float minDistance = comp1.CollisionRadius + comp2.CollisionRadius;

                if (distance < minDistance)
                {
                    // Simple elastic collision response
                    HandleCollision(comp1, comp2);
                }
            }
        }
    }

    private void HandleCollision(PhysicsComponent obj1, PhysicsComponent obj2)
    {
        if (obj1.IsStatic && obj2.IsStatic) return;

        Vector3 normal = Vector3.Normalize(obj2.Position - obj1.Position);
        
        if (!obj1.IsStatic && !obj2.IsStatic)
        {
            // Both objects are dynamic - exchange velocities along collision normal
            float v1 = Vector3.Dot(obj1.Velocity, normal);
            float v2 = Vector3.Dot(obj2.Velocity, normal);

            float m1 = obj1.Mass;
            float m2 = obj2.Mass;

            float newV1 = (v1 * (m1 - m2) + 2 * m2 * v2) / (m1 + m2);
            float newV2 = (v2 * (m2 - m1) + 2 * m1 * v1) / (m1 + m2);

            obj1.Velocity += (newV1 - v1) * normal;
            obj2.Velocity += (newV2 - v2) * normal;
        }
        else if (obj1.IsStatic)
        {
            // obj1 is static, reflect obj2
            float v = Vector3.Dot(obj2.Velocity, normal);
            obj2.Velocity -= 2 * v * normal;
        }
        else
        {
            // obj2 is static, reflect obj1
            float v = Vector3.Dot(obj1.Velocity, normal);
            obj1.Velocity -= 2 * v * normal;
        }
    }
}
