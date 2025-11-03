namespace AvorionLike.Core.ECS;

/// <summary>
/// Base class for systems that process entities with specific components
/// </summary>
public abstract class SystemBase
{
    public string Name { get; protected set; }
    public bool IsEnabled { get; set; }

    protected SystemBase(string name)
    {
        Name = name;
        IsEnabled = true;
    }

    /// <summary>
    /// Update the system
    /// </summary>
    /// <param name="deltaTime">Time elapsed since last update in seconds</param>
    public abstract void Update(float deltaTime);

    /// <summary>
    /// Initialize the system
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Clean up system resources
    /// </summary>
    public virtual void Shutdown() { }
}
