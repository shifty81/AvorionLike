using System.Numerics;
using AvorionLike.Core.ECS;

namespace AvorionLike.Core.Spatial;

/// <summary>
/// Represents a bounding box in 3D space
/// </summary>
public struct Bounds
{
    public Vector3 Center;
    public Vector3 Size;
    
    public Bounds(Vector3 center, Vector3 size)
    {
        Center = center;
        Size = size;
    }
    
    public Vector3 Min => Center - Size * 0.5f;
    public Vector3 Max => Center + Size * 0.5f;
    
    /// <summary>
    /// Check if this bounds contains a point
    /// </summary>
    public bool Contains(Vector3 point)
    {
        var min = Min;
        var max = Max;
        return point.X >= min.X && point.X <= max.X &&
               point.Y >= min.Y && point.Y <= max.Y &&
               point.Z >= min.Z && point.Z <= max.Z;
    }
    
    /// <summary>
    /// Check if this bounds intersects another bounds
    /// </summary>
    public bool Intersects(Bounds other)
    {
        var min1 = Min;
        var max1 = Max;
        var min2 = other.Min;
        var max2 = other.Max;
        
        return min1.X <= max2.X && max1.X >= min2.X &&
               min1.Y <= max2.Y && max1.Y >= min2.Y &&
               min1.Z <= max2.Z && max1.Z >= min2.Z;
    }
}

/// <summary>
/// Octree node for spatial partitioning in 3D space
/// Used for efficient collision detection, rendering, and pathfinding
/// </summary>
public class OctreeNode<T> where T : class
{
    public Bounds Bounds { get; private set; }
    public int Level { get; private set; }
    public List<(Vector3 position, T data)> Objects { get; private set; }
    public OctreeNode<T>[]? Children { get; private set; }
    
    private readonly int _maxObjectsPerNode;
    private readonly int _maxLevels;
    private bool _divided = false;
    
    public OctreeNode(Bounds bounds, int level, int maxObjectsPerNode = 8, int maxLevels = 6)
    {
        Bounds = bounds;
        Level = level;
        Objects = new List<(Vector3, T)>();
        _maxObjectsPerNode = maxObjectsPerNode;
        _maxLevels = maxLevels;
    }
    
    /// <summary>
    /// Subdivide this node into 8 child octants
    /// </summary>
    private void Subdivide()
    {
        if (_divided) return;
        
        Vector3 size = Bounds.Size * 0.5f;
        Vector3 quarter = size * 0.5f;
        
        Children = new OctreeNode<T>[8];
        
        // Create 8 octants
        // Bottom layer (Y-)
        Children[0] = new OctreeNode<T>(new Bounds(Bounds.Center + new Vector3(-quarter.X, -quarter.Y, -quarter.Z), size), Level + 1, _maxObjectsPerNode, _maxLevels);
        Children[1] = new OctreeNode<T>(new Bounds(Bounds.Center + new Vector3(quarter.X, -quarter.Y, -quarter.Z), size), Level + 1, _maxObjectsPerNode, _maxLevels);
        Children[2] = new OctreeNode<T>(new Bounds(Bounds.Center + new Vector3(-quarter.X, -quarter.Y, quarter.Z), size), Level + 1, _maxObjectsPerNode, _maxLevels);
        Children[3] = new OctreeNode<T>(new Bounds(Bounds.Center + new Vector3(quarter.X, -quarter.Y, quarter.Z), size), Level + 1, _maxObjectsPerNode, _maxLevels);
        
        // Top layer (Y+)
        Children[4] = new OctreeNode<T>(new Bounds(Bounds.Center + new Vector3(-quarter.X, quarter.Y, -quarter.Z), size), Level + 1, _maxObjectsPerNode, _maxLevels);
        Children[5] = new OctreeNode<T>(new Bounds(Bounds.Center + new Vector3(quarter.X, quarter.Y, -quarter.Z), size), Level + 1, _maxObjectsPerNode, _maxLevels);
        Children[6] = new OctreeNode<T>(new Bounds(Bounds.Center + new Vector3(-quarter.X, quarter.Y, quarter.Z), size), Level + 1, _maxObjectsPerNode, _maxLevels);
        Children[7] = new OctreeNode<T>(new Bounds(Bounds.Center + new Vector3(quarter.X, quarter.Y, quarter.Z), size), Level + 1, _maxObjectsPerNode, _maxLevels);
        
        _divided = true;
        
        // Redistribute existing objects to children
        var objectsToRedistribute = new List<(Vector3, T)>(Objects);
        Objects.Clear();
        
        foreach (var obj in objectsToRedistribute)
        {
            Insert(obj.Item1, obj.Item2);
        }
    }
    
    /// <summary>
    /// Insert an object at a position
    /// </summary>
    public bool Insert(Vector3 position, T data)
    {
        if (!Bounds.Contains(position))
            return false;
        
        // If we have children, try to insert into appropriate child
        if (_divided)
        {
            foreach (var child in Children!)
            {
                if (child.Insert(position, data))
                    return true;
            }
            // If no child accepted it, store at this level
            Objects.Add((position, data));
            return true;
        }
        
        // Add to this node
        Objects.Add((position, data));
        
        // Subdivide if we exceeded capacity and haven't reached max depth
        if (Objects.Count > _maxObjectsPerNode && Level < _maxLevels)
        {
            Subdivide();
        }
        
        return true;
    }
    
    /// <summary>
    /// Query objects within a bounds
    /// </summary>
    public List<(Vector3 position, T data)> Query(Bounds range)
    {
        var found = new List<(Vector3, T)>();
        
        if (!Bounds.Intersects(range))
            return found;
        
        // Check objects at this level
        foreach (var obj in Objects)
        {
            if (range.Contains(obj.position))
            {
                found.Add(obj);
            }
        }
        
        // Check children
        if (_divided)
        {
            foreach (var child in Children!)
            {
                found.AddRange(child.Query(range));
            }
        }
        
        return found;
    }
    
    /// <summary>
    /// Find nearest object to a point within a search radius
    /// </summary>
    public (Vector3 position, T? data, float distance) FindNearest(Vector3 point, float maxDistance)
    {
        float bestDistance = maxDistance;
        T? bestData = null;
        Vector3 bestPosition = point;
        
        // Create search bounds
        var searchBounds = new Bounds(point, new Vector3(maxDistance * 2, maxDistance * 2, maxDistance * 2));
        var candidates = Query(searchBounds);
        
        foreach (var candidate in candidates)
        {
            float distance = Vector3.Distance(point, candidate.position);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestData = candidate.data;
                bestPosition = candidate.position;
            }
        }
        
        return (bestPosition, bestData, bestDistance);
    }
    
    /// <summary>
    /// Clear all objects from this node and children
    /// </summary>
    public void Clear()
    {
        Objects.Clear();
        if (_divided)
        {
            foreach (var child in Children!)
            {
                child.Clear();
            }
            Children = null;
            _divided = false;
        }
    }
    
    /// <summary>
    /// Get total count of objects in this node and all children
    /// </summary>
    public int GetTotalCount()
    {
        int count = Objects.Count;
        if (_divided)
        {
            foreach (var child in Children!)
            {
                count += child.GetTotalCount();
            }
        }
        return count;
    }
}

/// <summary>
/// Octree for spatial partitioning in 3D space
/// Manages dynamic objects and provides efficient spatial queries
/// </summary>
public class Octree<T> where T : class
{
    private OctreeNode<T> _root;
    private readonly Bounds _worldBounds;
    
    public Octree(Vector3 center, Vector3 size)
    {
        _worldBounds = new Bounds(center, size);
        _root = new OctreeNode<T>(_worldBounds, 0);
    }
    
    /// <summary>
    /// Insert an object at a position
    /// </summary>
    public bool Insert(Vector3 position, T data)
    {
        return _root.Insert(position, data);
    }
    
    /// <summary>
    /// Query objects within a bounds
    /// </summary>
    public List<(Vector3 position, T data)> Query(Bounds range)
    {
        return _root.Query(range);
    }
    
    /// <summary>
    /// Query objects within a radius of a point
    /// </summary>
    public List<(Vector3 position, T data)> QueryRadius(Vector3 center, float radius)
    {
        var bounds = new Bounds(center, new Vector3(radius * 2, radius * 2, radius * 2));
        var candidates = Query(bounds);
        
        // Filter to actual sphere
        return candidates.Where(c => Vector3.Distance(center, c.position) <= radius).ToList();
    }
    
    /// <summary>
    /// Find nearest object to a point
    /// </summary>
    public (Vector3 position, T? data, float distance) FindNearest(Vector3 point, float maxDistance)
    {
        return _root.FindNearest(point, maxDistance);
    }
    
    /// <summary>
    /// Clear and rebuild the octree
    /// </summary>
    public void Rebuild()
    {
        _root = new OctreeNode<T>(_worldBounds, 0);
    }
    
    /// <summary>
    /// Get total count of objects in octree
    /// </summary>
    public int Count => _root.GetTotalCount();
}
