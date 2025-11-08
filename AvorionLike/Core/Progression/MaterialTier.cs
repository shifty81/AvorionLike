using AvorionLike.Core.Resources;

namespace AvorionLike.Core.Progression;

/// <summary>
/// Material tier system representing progression from galaxy rim to center
/// Each tier unlocks better stats and new capabilities
/// </summary>
public enum MaterialTier
{
    /// <summary>
    /// Starting material - Available everywhere
    /// </summary>
    Iron = 0,
    
    /// <summary>
    /// Tier 1 - Available from sector distance ~350
    /// Unlocks: Better hull strength
    /// </summary>
    Titanium = 1,
    
    /// <summary>
    /// Tier 2 - Available from sector distance ~250
    /// Unlocks: Shield generators, better weapons
    /// </summary>
    Naonite = 2,
    
    /// <summary>
    /// Tier 3 - Available from sector distance ~150
    /// Unlocks: Energy systems, advanced thrusters
    /// </summary>
    Trinium = 3,
    
    /// <summary>
    /// Tier 4 - Available from sector distance ~75
    /// Unlocks: Improved generators, better mining
    /// </summary>
    Xanion = 4,
    
    /// <summary>
    /// Tier 5 - Available from sector distance ~50
    /// Unlocks: Advanced systems, better trading
    /// </summary>
    Ogonite = 5,
    
    /// <summary>
    /// Tier 6 - Only at galactic core (distance < 25)
    /// Unlocks: All systems, best stats, boss fights
    /// </summary>
    Avorion = 6
}

/// <summary>
/// Material tier properties and gameplay effects
/// </summary>
public static class MaterialTierInfo
{
    /// <summary>
    /// Get the distance from galactic center where this material becomes available
    /// </summary>
    public static int GetUnlockDistance(MaterialTier tier)
    {
        return tier switch
        {
            MaterialTier.Iron => int.MaxValue,
            MaterialTier.Titanium => 350,
            MaterialTier.Naonite => 250,
            MaterialTier.Trinium => 150,
            MaterialTier.Xanion => 75,
            MaterialTier.Ogonite => 50,
            MaterialTier.Avorion => 25,
            _ => int.MaxValue
        };
    }
    
    /// <summary>
    /// Get stat multiplier for this material (compared to Iron)
    /// </summary>
    public static float GetStatMultiplier(MaterialTier tier)
    {
        return tier switch
        {
            MaterialTier.Iron => 1.0f,
            MaterialTier.Titanium => 1.3f,
            MaterialTier.Naonite => 1.6f,
            MaterialTier.Trinium => 2.0f,
            MaterialTier.Xanion => 2.5f,
            MaterialTier.Ogonite => 3.0f,
            MaterialTier.Avorion => 4.0f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Get tech points multiplier (for research/upgrades)
    /// </summary>
    public static float GetTechPointsMultiplier(MaterialTier tier)
    {
        return tier switch
        {
            MaterialTier.Iron => 1.0f,
            MaterialTier.Titanium => 1.5f,
            MaterialTier.Naonite => 2.5f,
            MaterialTier.Trinium => 4.0f,
            MaterialTier.Xanion => 6.0f,
            MaterialTier.Ogonite => 9.0f,
            MaterialTier.Avorion => 15.0f,
            _ => 1.0f
        };
    }
    
    /// <summary>
    /// Check what features are unlocked at this tier
    /// </summary>
    public static HashSet<string> GetUnlockedFeatures(MaterialTier tier)
    {
        var features = new HashSet<string>();
        
        // Base features (always available)
        features.Add("Basic Mining");
        features.Add("Basic Hull");
        features.Add("Basic Engines");
        
        if (tier >= MaterialTier.Titanium)
        {
            features.Add("Improved Hull Strength");
            features.Add("Better Weapons");
        }
        
        if (tier >= MaterialTier.Naonite)
        {
            features.Add("Shield Generators");
            features.Add("Advanced Weapons");
            features.Add("Salvaging");
        }
        
        if (tier >= MaterialTier.Trinium)
        {
            features.Add("Energy Management");
            features.Add("Advanced Thrusters");
            features.Add("Jump Drives");
        }
        
        if (tier >= MaterialTier.Xanion)
        {
            features.Add("Improved Power Generation");
            features.Add("Advanced Mining");
            features.Add("Refining Stations");
        }
        
        if (tier >= MaterialTier.Ogonite)
        {
            features.Add("Advanced Trading");
            features.Add("Fleet Management");
            features.Add("Captain Automation");
        }
        
        if (tier >= MaterialTier.Avorion)
        {
            features.Add("Barrier Access");
            features.Add("Boss Encounters");
            features.Add("Endgame Content");
            features.Add("Ultimate Upgrades");
        }
        
        return features;
    }
    
    /// <summary>
    /// Get the color associated with this material (for UI display)
    /// </summary>
    public static (float r, float g, float b) GetMaterialColor(MaterialTier tier)
    {
        return tier switch
        {
            MaterialTier.Iron => (0.5f, 0.5f, 0.5f),         // Gray
            MaterialTier.Titanium => (0.7f, 0.8f, 0.9f),     // Silver-Blue
            MaterialTier.Naonite => (0.2f, 0.9f, 0.3f),      // Bright Green
            MaterialTier.Trinium => (0.3f, 0.6f, 1.0f),      // Blue
            MaterialTier.Xanion => (1.0f, 0.9f, 0.2f),       // Gold
            MaterialTier.Ogonite => (1.0f, 0.4f, 0.1f),      // Orange-Red
            MaterialTier.Avorion => (0.8f, 0.2f, 1.0f),      // Purple
            _ => (1.0f, 1.0f, 1.0f)
        };
    }
    
    /// <summary>
    /// Convert resource type to material tier
    /// </summary>
    public static MaterialTier ResourceToTier(ResourceType resource)
    {
        return resource switch
        {
            ResourceType.Iron => MaterialTier.Iron,
            ResourceType.Titanium => MaterialTier.Titanium,
            ResourceType.Naonite => MaterialTier.Naonite,
            ResourceType.Trinium => MaterialTier.Trinium,
            ResourceType.Xanion => MaterialTier.Xanion,
            ResourceType.Ogonite => MaterialTier.Ogonite,
            ResourceType.Avorion => MaterialTier.Avorion,
            _ => MaterialTier.Iron
        };
    }
    
    /// <summary>
    /// Get display name for material tier
    /// </summary>
    public static string GetDisplayName(MaterialTier tier)
    {
        return tier.ToString();
    }
    
    /// <summary>
    /// Get description of material tier benefits
    /// </summary>
    public static string GetDescription(MaterialTier tier)
    {
        return tier switch
        {
            MaterialTier.Iron => "Basic starting material. Available everywhere in the galaxy.",
            MaterialTier.Titanium => "Stronger than iron. Provides improved hull strength and weapon damage.",
            MaterialTier.Naonite => "Unlocks shields! Essential for survival in dangerous sectors.",
            MaterialTier.Trinium => "Lightweight and energy-efficient. Great for thrusters and power systems.",
            MaterialTier.Xanion => "Advanced material with excellent power generation capabilities.",
            MaterialTier.Ogonite => "Heavy and durable. Perfect for large capital ships and defensive structures.",
            MaterialTier.Avorion => "The ultimate material. Only found at the galactic core. Unlocks all capabilities.",
            _ => "Unknown material"
        };
    }
}
