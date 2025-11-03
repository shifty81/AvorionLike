using System.Collections.Concurrent;
using AvorionLike.Core.Logging;
using AvorionLike.Core.Events;
using AvorionLike.Core.Common;

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
        ValidationHelper.ValidateNotNullOrEmpty(name, nameof(name));
        
        var entity = new Entity(name);
        _entities[entity.Id] = entity;
        
        // Publish entity created event
        EventSystem.Instance.QueueEvent(GameEvents.EntityCreated, new EntityEvent
        {
            EntityId = entity.Id,
            EntityName = entity.Name
        });
        
        Logger.Instance.Debug("EntityManager", $"Created entity: {entity.Name} ({entity.Id})");
        return entity;
    }

    /// <summary>
    /// Destroy an entity and remove all its components
    /// </summary>
    public void DestroyEntity(Guid entityId)
    {
        ValidationHelper.ValidateNotEmpty(entityId, nameof(entityId));
        
        if (_entities.TryRemove(entityId, out var entity))
        {
            foreach (var componentDict in _components.Values)
            {
                componentDict.TryRemove(entityId, out _);
            }
            
            // Publish entity destroyed event
            EventSystem.Instance.QueueEvent(GameEvents.EntityDestroyed, new EntityEvent
            {
                EntityId = entityId,
                EntityName = entity.Name
            });
            
            Logger.Instance.Debug("EntityManager", $"Destroyed entity: {entity.Name} ({entityId})");
        }
    }

    /// <summary>
    /// Add a component to an entity
    /// </summary>
    public T AddComponent<T>(Guid entityId, T component) where T : IComponent
    {
        ValidationHelper.ValidateNotEmpty(entityId, nameof(entityId));
        ValidationHelper.ValidateNotNull(component, nameof(component));
        
        if (!_entities.ContainsKey(entityId))
        {
            throw new InvalidOperationException($"Cannot add component to non-existent entity: {entityId}");
        }
        
        component.EntityId = entityId;
        var componentType = typeof(T);
        
        if (!_components.ContainsKey(componentType))
        {
            _components[componentType] = new ConcurrentDictionary<Guid, IComponent>();
        }
        
        _components[componentType][entityId] = component;
        
        // Publish component added event
        EventSystem.Instance.QueueEvent(GameEvents.ComponentAdded, new EntityEvent
        {
            EntityId = entityId,
            EntityName = _entities[entityId].Name
        });
        
        Logger.Instance.Debug("EntityManager", $"Added {componentType.Name} to entity {entityId}");
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
