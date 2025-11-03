using System.Collections.Concurrent;

namespace AvorionLike.Core.ECS;

/// <summary>
/// Manages entities and their components in the ECS architecture
/// </summary>
public class EntityManager
{
    private readonly ConcurrentDictionary<Guid, Entity> _entities = new();
    private readonly ConcurrentDictionary<Type, ConcurrentDictionary<Guid, IComponent>> _components = new();
    private readonly List<SystemBase> _systems = new();

    /// <summary>
    /// Create a new entity
    /// </summary>
    public Entity CreateEntity(string name = "Entity")
    {
        var entity = new Entity(name);
        _entities[entity.Id] = entity;
        return entity;
    }

    /// <summary>
    /// Destroy an entity and remove all its components
    /// </summary>
    public void DestroyEntity(Guid entityId)
    {
        if (_entities.TryRemove(entityId, out _))
        {
            foreach (var componentDict in _components.Values)
            {
                componentDict.TryRemove(entityId, out _);
            }
        }
    }

    /// <summary>
    /// Add a component to an entity
    /// </summary>
    public T AddComponent<T>(Guid entityId, T component) where T : IComponent
    {
        component.EntityId = entityId;
        var componentType = typeof(T);
        
        if (!_components.ContainsKey(componentType))
        {
            _components[componentType] = new ConcurrentDictionary<Guid, IComponent>();
        }
        
        _components[componentType][entityId] = component;
        return component;
    }

    /// <summary>
    /// Get a component from an entity
    /// </summary>
    public T? GetComponent<T>(Guid entityId) where T : class, IComponent
    {
        var componentType = typeof(T);
        if (_components.TryGetValue(componentType, out var componentDict))
        {
            if (componentDict.TryGetValue(entityId, out var component))
            {
                return component as T;
            }
        }
        return null;
    }

    /// <summary>
    /// Check if an entity has a specific component
    /// </summary>
    public bool HasComponent<T>(Guid entityId) where T : IComponent
    {
        var componentType = typeof(T);
        return _components.ContainsKey(componentType) && 
               _components[componentType].ContainsKey(entityId);
    }

    /// <summary>
    /// Remove a component from an entity
    /// </summary>
    public void RemoveComponent<T>(Guid entityId) where T : IComponent
    {
        var componentType = typeof(T);
        if (_components.TryGetValue(componentType, out var componentDict))
        {
            componentDict.TryRemove(entityId, out _);
        }
    }

    /// <summary>
    /// Get all components of a specific type
    /// </summary>
    public IEnumerable<T> GetAllComponents<T>() where T : class, IComponent
    {
        var componentType = typeof(T);
        if (_components.TryGetValue(componentType, out var componentDict))
        {
            return componentDict.Values.Cast<T>();
        }
        return Enumerable.Empty<T>();
    }

    /// <summary>
    /// Get all entities
    /// </summary>
    public IEnumerable<Entity> GetAllEntities()
    {
        return _entities.Values;
    }

    /// <summary>
    /// Get entity by ID
    /// </summary>
    public Entity? GetEntity(Guid entityId)
    {
        _entities.TryGetValue(entityId, out var entity);
        return entity;
    }

    /// <summary>
    /// Register a system
    /// </summary>
    public void RegisterSystem(SystemBase system)
    {
        _systems.Add(system);
        system.Initialize();
    }

    /// <summary>
    /// Update all systems
    /// </summary>
    public void UpdateSystems(float deltaTime)
    {
        foreach (var system in _systems.Where(s => s.IsEnabled))
        {
            system.Update(deltaTime);
        }
    }

    /// <summary>
    /// Shutdown all systems
    /// </summary>
    public void Shutdown()
    {
        foreach (var system in _systems)
        {
            system.Shutdown();
        }
    }
}
