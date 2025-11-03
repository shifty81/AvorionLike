using AvorionLike.Core.ECS;
using AvorionLike.Core.Physics;
using AvorionLike.Core.Scripting;
using AvorionLike.Core.Networking;
using AvorionLike.Core.Procedural;
using AvorionLike.Core.Resources;
using AvorionLike.Core.RPG;

namespace AvorionLike.Core;

/// <summary>
/// Main game engine that manages all core systems
/// </summary>
public class GameEngine
{
    // Core ECS
    public EntityManager EntityManager { get; private set; } = null!;
    
    // Systems
    public PhysicsSystem PhysicsSystem { get; private set; } = null!;
    public ScriptingEngine ScriptingEngine { get; private set; } = null!;
    public GalaxyGenerator GalaxyGenerator { get; private set; } = null!;
    public CraftingSystem CraftingSystem { get; private set; } = null!;
    public LootSystem LootSystem { get; private set; } = null!;
    public TradingSystem TradingSystem { get; private set; } = null!;
    
    // Networking
    public GameServer? GameServer { get; private set; }
    
    // State
    public bool IsRunning { get; private set; }
    private DateTime _lastUpdateTime;
    private readonly int _galaxySeed;

    public GameEngine(int galaxySeed = 0)
    {
        _galaxySeed = galaxySeed;
        Initialize();
    }

    /// <summary>
    /// Initialize all engine systems
    /// </summary>
    private void Initialize()
    {
        Console.WriteLine("Initializing AvorionLike Game Engine...");

        // Initialize ECS
        EntityManager = new EntityManager();

        // Initialize systems
        PhysicsSystem = new PhysicsSystem(EntityManager);
        ScriptingEngine = new ScriptingEngine();
        GalaxyGenerator = new GalaxyGenerator(_galaxySeed);
        CraftingSystem = new CraftingSystem();
        LootSystem = new LootSystem();
        TradingSystem = new TradingSystem();

        // Register systems with entity manager
        EntityManager.RegisterSystem(PhysicsSystem);

        // Register engine API for scripting
        ScriptingEngine.RegisterObject("Engine", this);
        ScriptingEngine.RegisterObject("EntityManager", EntityManager);

        _lastUpdateTime = DateTime.UtcNow;

        Console.WriteLine("Game Engine initialized successfully");
    }

    /// <summary>
    /// Start the game engine
    /// </summary>
    public void Start()
    {
        if (IsRunning) return;

        IsRunning = true;
        _lastUpdateTime = DateTime.UtcNow;
        
        Console.WriteLine("Game Engine started");
    }

    /// <summary>
    /// Stop the game engine
    /// </summary>
    public void Stop()
    {
        if (!IsRunning) return;

        IsRunning = false;
        EntityManager.Shutdown();
        GameServer?.Stop();
        ScriptingEngine.Dispose();
        
        Console.WriteLine("Game Engine stopped");
    }

    /// <summary>
    /// Update the game engine (call this each frame)
    /// </summary>
    public void Update()
    {
        if (!IsRunning) return;

        // Calculate delta time
        var currentTime = DateTime.UtcNow;
        float deltaTime = (float)(currentTime - _lastUpdateTime).TotalSeconds;
        _lastUpdateTime = currentTime;

        // Clamp delta time to prevent huge jumps
        deltaTime = Math.Min(deltaTime, 0.1f);

        // Update all systems
        EntityManager.UpdateSystems(deltaTime);
    }

    /// <summary>
    /// Start multiplayer server
    /// </summary>
    public void StartServer(int port = 27015)
    {
        if (GameServer != null && GameServer.IsRunning)
        {
            Console.WriteLine("Server is already running");
            return;
        }

        GameServer = new GameServer(port);
        GameServer.Start();
    }

    /// <summary>
    /// Stop multiplayer server
    /// </summary>
    public void StopServer()
    {
        GameServer?.Stop();
    }

    /// <summary>
    /// Load a mod script
    /// </summary>
    public bool LoadMod(string modPath)
    {
        return ScriptingEngine.LoadMod(modPath);
    }

    /// <summary>
    /// Execute a script command
    /// </summary>
    public object[]? ExecuteScript(string script)
    {
        return ScriptingEngine.ExecuteScript(script);
    }

    /// <summary>
    /// Generate a galaxy sector
    /// </summary>
    public Procedural.GalaxySector GenerateSector(int x, int y, int z)
    {
        return GalaxyGenerator.GenerateSector(x, y, z);
    }

    /// <summary>
    /// Get engine statistics
    /// </summary>
    public EngineStatistics GetStatistics()
    {
        return new EngineStatistics
        {
            TotalEntities = EntityManager.GetAllEntities().Count(),
            TotalComponents = EntityManager.GetAllComponents<IComponent>().Count(),
            IsServerRunning = GameServer?.IsRunning ?? false,
            PhysicsEnabled = PhysicsSystem.IsEnabled
        };
    }
}

/// <summary>
/// Engine statistics data
/// </summary>
public class EngineStatistics
{
    public int TotalEntities { get; set; }
    public int TotalComponents { get; set; }
    public bool IsServerRunning { get; set; }
    public bool PhysicsEnabled { get; set; }
}
