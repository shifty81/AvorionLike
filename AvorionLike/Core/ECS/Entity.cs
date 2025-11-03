namespace AvorionLike.Core.ECS;

/// <summary>
/// Represents a game entity with a unique identifier
/// </summary>
public class Entity
{
    public Guid Id { get; }
    public string Name { get; set; }
    public bool IsActive { get; set; }

    public Entity(string name = "Entity")
    {
        Id = Guid.NewGuid();
        Name = name;
        IsActive = true;
    }
}
