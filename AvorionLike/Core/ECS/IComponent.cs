namespace AvorionLike.Core.ECS;

/// <summary>
/// Base interface for all components in the ECS system
/// </summary>
public interface IComponent
{
    /// <summary>
    /// Gets the unique identifier of the entity this component belongs to
    /// </summary>
    Guid EntityId { get; set; }
}
