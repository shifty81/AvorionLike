namespace AvorionLike.Core.Resources;

/// <summary>
/// Types of resources in the game
/// </summary>
public enum ResourceType
{
    Iron,
    Titanium,
    Naonite,
    Trinium,
    Xanion,
    Ogonite,
    Avorion,
    Credits
}

/// <summary>
/// Represents a resource item
/// </summary>
public class ResourceItem
{
    public ResourceType Type { get; set; }
    public int Amount { get; set; }
    public float Value { get; set; }

    public ResourceItem(ResourceType type, int amount, float value = 1.0f)
    {
        Type = type;
        Amount = amount;
        Value = value;
    }
}

/// <summary>
/// Manages resource inventory
/// </summary>
public class Inventory
{
    private readonly Dictionary<ResourceType, int> _resources = new();
    public int MaxCapacity { get; set; } = 1000;
    public int CurrentCapacity { get; private set; }

    public Inventory()
    {
        // Initialize all resource types
        foreach (ResourceType type in Enum.GetValues(typeof(ResourceType)))
        {
            _resources[type] = 0;
        }
    }

    /// <summary>
    /// Add resources to inventory
    /// </summary>
    public bool AddResource(ResourceType type, int amount)
    {
        if (CurrentCapacity + amount > MaxCapacity)
        {
            return false;
        }

        _resources[type] += amount;
        CurrentCapacity += amount;
        return true;
    }

    /// <summary>
    /// Remove resources from inventory
    /// </summary>
    public bool RemoveResource(ResourceType type, int amount)
    {
        if (_resources[type] < amount)
        {
            return false;
        }

        _resources[type] -= amount;
        CurrentCapacity -= amount;
        return true;
    }

    /// <summary>
    /// Get amount of a specific resource
    /// </summary>
    public int GetResourceAmount(ResourceType type)
    {
        return _resources[type];
    }

    /// <summary>
    /// Check if inventory has enough of a resource
    /// </summary>
    public bool HasResource(ResourceType type, int amount)
    {
        return _resources[type] >= amount;
    }

    /// <summary>
    /// Get all resources
    /// </summary>
    public Dictionary<ResourceType, int> GetAllResources()
    {
        return new Dictionary<ResourceType, int>(_resources);
    }

    /// <summary>
    /// Clear all resources
    /// </summary>
    public void Clear()
    {
        foreach (var key in _resources.Keys.ToList())
        {
            _resources[key] = 0;
        }
        CurrentCapacity = 0;
    }
}
