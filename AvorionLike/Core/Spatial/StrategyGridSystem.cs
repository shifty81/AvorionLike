using System.Numerics;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Logging;

namespace AvorionLike.Core.Spatial;

/// <summary>
/// Grid cell data for strategy planning
/// </summary>
public class GridCellData
{
    public List<Guid> Units { get; set; } = new();
    public List<Guid> Obstacles { get; set; } = new();
    public bool IsPassable { get; set; } = true;
    public float ThreatLevel { get; set; } = 0f;
    public Vector3? GravitySource { get; set; }
    public float GravityStrength { get; set; } = 0f;
}

/// <summary>
/// Strategy grid system for RTS-style fleet management
/// Uses octree for spatial partitioning and efficient queries
/// </summary>
public class StrategyGridSystem : SystemBase
{
    private readonly EntityManager _entityManager;
    private readonly Logger _logger;
    private Octree<GridCellData> _octree;
    private readonly Vector3 _sectorSize;
    private readonly float _cellSize;
    
    // Cache for quick lookups
    private readonly Dictionary<Guid, Vector3> _entityPositions = new();
    
    public bool IsStrategyViewActive { get; set; } = false;
    
    public StrategyGridSystem(EntityManager entityManager, Logger logger, Vector3 sectorSize, float cellSize = 100f) 
        : base("StrategyGridSystem")
    {
        _entityManager = entityManager;
        _logger = logger;
        _sectorSize = sectorSize;
        _cellSize = cellSize;
        
        // Create octree for the entire sector
        _octree = new Octree<GridCellData>(Vector3.Zero, sectorSize);
        
        _logger.Log(LogLevel.Info, "StrategyGridSystem", 
            $"Initialized strategy grid: Sector={sectorSize}, CellSize={cellSize}");
    }
    
    public override void Update(float deltaTime)
    {
        if (!IsStrategyViewActive) return;
        
        // Rebuild octree each frame with current entity positions
        // For performance, this could be optimized to only update moved entities
        RebuildGrid();
    }
    
    /// <summary>
    /// Rebuild the strategy grid with current entity positions
    /// </summary>
    public void RebuildGrid()
    {
        _octree.Rebuild();
        _entityPositions.Clear();
        
        var entities = _entityManager.GetAllEntities();
        
        foreach (var entity in entities)
        {
            var physics = _entityManager.GetComponent<Physics.PhysicsComponent>(entity.Id);
            if (physics != null)
            {
                var position = physics.Position;
                _entityPositions[entity.Id] = position;
                
                // Create or get cell data
                var cellData = new GridCellData();
                cellData.Units.Add(entity.Id);
                
                // Check if entity is an obstacle or passable
                var voxels = _entityManager.GetComponent<Voxel.VoxelStructureComponent>(entity.Id);
                if (voxels != null && voxels.TotalMass > 1000f)
                {
                    // Large objects are obstacles
                    cellData.Obstacles.Add(entity.Id);
                    cellData.IsPassable = false;
                }
                
                _octree.Insert(position, cellData);
            }
        }
    }
    
    /// <summary>
    /// Get entities within a radius
    /// </summary>
    public List<Guid> GetEntitiesInRadius(Vector3 center, float radius)
    {
        var results = _octree.QueryRadius(center, radius);
        var entities = new HashSet<Guid>();
        
        foreach (var result in results)
        {
            foreach (var entityId in result.data.Units)
            {
                entities.Add(entityId);
            }
        }
        
        return entities.ToList();
    }
    
    /// <summary>
    /// Get entities within a bounding box
    /// </summary>
    public List<Guid> GetEntitiesInBounds(Bounds bounds)
    {
        var results = _octree.Query(bounds);
        var entities = new HashSet<Guid>();
        
        foreach (var result in results)
        {
            foreach (var entityId in result.data.Units)
            {
                entities.Add(entityId);
            }
        }
        
        return entities.ToList();
    }
    
    /// <summary>
    /// Find nearest entity to a point
    /// </summary>
    public Guid? FindNearestEntity(Vector3 point, float maxDistance)
    {
        var result = _octree.FindNearest(point, maxDistance);
        
        if (result.data != null && result.data.Units.Count > 0)
        {
            return result.data.Units[0];
        }
        
        return null;
    }
    
    /// <summary>
    /// Check if a position is passable for pathfinding
    /// </summary>
    public bool IsPositionPassable(Vector3 position)
    {
        var cellSize = new Vector3(_cellSize, _cellSize, _cellSize);
        var bounds = new Bounds(position, cellSize);
        var results = _octree.Query(bounds);
        
        foreach (var result in results)
        {
            if (!result.data.IsPassable)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get threat level at a position (useful for tactical decisions)
    /// </summary>
    public float GetThreatLevel(Vector3 position, float radius)
    {
        var entities = GetEntitiesInRadius(position, radius);
        float threat = 0f;
        
        foreach (var entityId in entities)
        {
            var combat = _entityManager.GetComponent<Combat.CombatComponent>(entityId);
            if (combat != null)
            {
                // Calculate threat based on firepower
                threat += combat.Turrets.Count * 10f;
            }
        }
        
        return threat;
    }
    
    /// <summary>
    /// Get obstacles in a region
    /// </summary>
    public List<Guid> GetObstacles(Bounds bounds)
    {
        var results = _octree.Query(bounds);
        var obstacles = new HashSet<Guid>();
        
        foreach (var result in results)
        {
            foreach (var obstacleId in result.data.Obstacles)
            {
                obstacles.Add(obstacleId);
            }
        }
        
        return obstacles.ToList();
    }
    
    /// <summary>
    /// Toggle strategy view on/off
    /// </summary>
    public void ToggleStrategyView()
    {
        IsStrategyViewActive = !IsStrategyViewActive;
        
        if (IsStrategyViewActive)
        {
            _logger.Log(LogLevel.Info, "StrategyGridSystem", "Strategy view activated");
            RebuildGrid();
        }
        else
        {
            _logger.Log(LogLevel.Info, "StrategyGridSystem", "Strategy view deactivated");
        }
    }
    
    /// <summary>
    /// Raycast from screen position to world position
    /// Used for mouse picking in 3D space
    /// </summary>
    public Vector3? RaycastToWorld(Vector2 screenPos, Graphics.Camera camera, Vector2 screenSize)
    {
        // Convert screen space to normalized device coordinates
        float x = (2.0f * screenPos.X) / screenSize.X - 1.0f;
        float y = 1.0f - (2.0f * screenPos.Y) / screenSize.Y;
        
        // Create ray in camera space
        Vector3 rayNds = new Vector3(x, y, 1.0f);
        
        // Get camera matrices
        var view = camera.GetViewMatrix();
        var projection = camera.GetProjectionMatrix(screenSize.X / screenSize.Y);
        
        // Inverse transform to world space
        Matrix4x4.Invert(projection, out var invProjection);
        Matrix4x4.Invert(view, out var invView);
        
        Vector4 rayClip = new Vector4(rayNds.X, rayNds.Y, -1.0f, 1.0f);
        Vector4 rayEye = Vector4.Transform(rayClip, invProjection);
        rayEye = new Vector4(rayEye.X, rayEye.Y, -1.0f, 0.0f);
        
        Vector3 rayWorld = Vector3.Normalize(new Vector3(
            Vector4.Transform(rayEye, invView).X,
            Vector4.Transform(rayEye, invView).Y,
            Vector4.Transform(rayEye, invView).Z
        ));
        
        // Raycast to find intersection (simplified - intersect with Y=0 plane)
        Vector3 rayOrigin = camera.Position;
        
        // Find t where ray hits Y=0 plane
        if (Math.Abs(rayWorld.Y) > 0.001f)
        {
            float t = -rayOrigin.Y / rayWorld.Y;
            if (t > 0)
            {
                return rayOrigin + rayWorld * t;
            }
        }
        
        return null;
    }
}
