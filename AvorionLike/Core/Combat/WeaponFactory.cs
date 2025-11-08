using System.Numerics;

namespace AvorionLike.Core.Combat;

/// <summary>
/// Factory for creating various weapon types with Avorion-style stats
/// </summary>
public static class WeaponFactory
{
    /// <summary>
    /// Create a weapon based on type and tech level
    /// </summary>
    public static EnhancedTurret CreateWeapon(WeaponType type, int techLevel = 1)
    {
        float techMultiplier = 1f + (techLevel - 1) * 0.3f; // 30% increase per tech level
        
        return type switch
        {
            WeaponType.Chaingun => CreateChaingun(techMultiplier),
            WeaponType.Laser => CreateLaser(techMultiplier),
            WeaponType.Cannon => CreateCannon(techMultiplier),
            WeaponType.RocketLauncher => CreateRocketLauncher(techMultiplier),
            WeaponType.Railgun => CreateRailgun(techMultiplier),
            WeaponType.PlasmaGun => CreatePlasmaGun(techMultiplier),
            _ => CreateChaingun(techMultiplier)
        };
    }
    
    private static EnhancedTurret CreateChaingun(float multiplier)
    {
        return new EnhancedTurret
        {
            Name = "Chaingun Turret",
            Type = WeaponType.Chaingun,
            Category = WeaponCategory.Kinetic,
            Mode = FiringMode.Automatic,
            BaseDamage = 8f * multiplier,
            ShieldPenetration = 0.1f,
            HullDamageMultiplier = 1.5f, // Good against hull
            ShieldDamageMultiplier = 0.7f, // Weak against shields
            FireRate = 10f, // Fast firing
            Range = 800f,
            Accuracy = 0.85f,
            ProjectileSpeed = 800f,
            EnergyCostPerShot = 2f,
            HeatGeneration = 0.5f,
            MaxHeat = 100f,
            CooldownRate = 15f,
            TrackingSpeed = 3f
        };
    }
    
    private static EnhancedTurret CreateLaser(float multiplier)
    {
        return new EnhancedTurret
        {
            Name = "Laser Turret",
            Type = WeaponType.Laser,
            Category = WeaponCategory.Energy,
            Mode = FiringMode.Automatic,
            BaseDamage = 15f * multiplier,
            ShieldPenetration = 0f,
            HullDamageMultiplier = 0.8f,
            ShieldDamageMultiplier = 1.5f, // Good against shields
            FireRate = 3f,
            Range = 1200f,
            Accuracy = 0.95f, // Very accurate
            ProjectileSpeed = 3000f, // Nearly instant
            EnergyCostPerShot = 10f,
            HeatGeneration = 2f,
            MaxHeat = 100f,
            CooldownRate = 10f,
            TrackingSpeed = 4f
        };
    }
    
    private static EnhancedTurret CreateCannon(float multiplier)
    {
        return new EnhancedTurret
        {
            Name = "Pulse Cannon",
            Type = WeaponType.Cannon,
            Category = WeaponCategory.Pulse,
            Mode = FiringMode.Automatic,
            BaseDamage = 40f * multiplier,
            ShieldPenetration = 0.5f, // Bypasses shields!
            HullDamageMultiplier = 1.8f, // Excellent against hull
            ShieldDamageMultiplier = 0.6f,
            FireRate = 0.5f, // Slow firing
            Range = 1000f,
            Accuracy = 0.9f,
            ProjectileSpeed = 600f,
            EnergyCostPerShot = 25f,
            HeatGeneration = 5f,
            MaxHeat = 100f,
            CooldownRate = 8f,
            TrackingSpeed = 1.5f
        };
    }
    
    private static EnhancedTurret CreateRocketLauncher(float multiplier)
    {
        return new EnhancedTurret
        {
            Name = "Rocket Launcher",
            Type = WeaponType.RocketLauncher,
            Category = WeaponCategory.Explosive,
            Mode = FiringMode.Manual, // Usually manual fire
            BaseDamage = 100f * multiplier,
            ShieldPenetration = 0.2f,
            HullDamageMultiplier = 2f, // Massive hull damage
            ShieldDamageMultiplier = 1f,
            FireRate = 0.2f, // Very slow
            Range = 2000f,
            Accuracy = 0.7f, // Can be dodged
            ProjectileSpeed = 300f,
            EnergyCostPerShot = 50f,
            HeatGeneration = 10f,
            MaxHeat = 100f,
            CooldownRate = 5f,
            TrackingSpeed = 0.5f
        };
    }
    
    private static EnhancedTurret CreateRailgun(float multiplier)
    {
        return new EnhancedTurret
        {
            Name = "Railgun",
            Type = WeaponType.Railgun,
            Category = WeaponCategory.Kinetic,
            Mode = FiringMode.Manual,
            BaseDamage = 80f * multiplier,
            ShieldPenetration = 0.3f,
            HullDamageMultiplier = 2.5f, // Devastating against hull
            ShieldDamageMultiplier = 0.5f,
            FireRate = 0.3f,
            Range = 2500f, // Long range
            Accuracy = 0.98f, // Very precise
            ProjectileSpeed = 5000f, // Near instant
            EnergyCostPerShot = 60f,
            HeatGeneration = 15f,
            MaxHeat = 100f,
            CooldownRate = 6f,
            TrackingSpeed = 1f
        };
    }
    
    private static EnhancedTurret CreatePlasmaGun(float multiplier)
    {
        return new EnhancedTurret
        {
            Name = "Plasma Gun",
            Type = WeaponType.PlasmaGun,
            Category = WeaponCategory.Energy,
            Mode = FiringMode.Automatic,
            BaseDamage = 30f * multiplier,
            ShieldPenetration = 0.15f,
            HullDamageMultiplier = 1.2f,
            ShieldDamageMultiplier = 1.8f, // Excellent against shields
            FireRate = 1.5f,
            Range = 900f,
            Accuracy = 0.88f,
            ProjectileSpeed = 700f,
            EnergyCostPerShot = 20f,
            HeatGeneration = 4f,
            MaxHeat = 100f,
            CooldownRate = 12f,
            TrackingSpeed = 2.5f
        };
    }
    
    /// <summary>
    /// Create a point defense turret
    /// </summary>
    public static EnhancedTurret CreatePointDefense(int techLevel = 1)
    {
        float multiplier = 1f + (techLevel - 1) * 0.3f;
        
        return new EnhancedTurret
        {
            Name = "Point Defense Turret",
            Type = WeaponType.Chaingun,
            Category = WeaponCategory.Kinetic,
            Mode = FiringMode.PointDefense,
            BaseDamage = 5f * multiplier,
            ShieldPenetration = 0f,
            HullDamageMultiplier = 1f,
            ShieldDamageMultiplier = 1f,
            FireRate = 20f, // Very fast
            Range = 600f,
            Accuracy = 0.95f,
            ProjectileSpeed = 1200f,
            EnergyCostPerShot = 1f,
            HeatGeneration = 0.2f,
            MaxHeat = 100f,
            CooldownRate = 20f,
            TrackingSpeed = 10f, // Tracks fast targets
            IsAutoTargeting = true
        };
    }
    
    /// <summary>
    /// Create a mining laser
    /// </summary>
    public static EnhancedTurret CreateMiningLaser(int techLevel = 1)
    {
        float multiplier = 1f + (techLevel - 1) * 0.4f;
        
        return new EnhancedTurret
        {
            Name = "Mining Laser",
            Type = WeaponType.Laser,
            Category = WeaponCategory.Mining,
            Mode = FiringMode.Manual,
            BaseDamage = 0f, // Doesn't damage ships
            ShieldPenetration = 0f,
            HullDamageMultiplier = 0f,
            ShieldDamageMultiplier = 0f,
            FireRate = 1f,
            Range = 500f,
            Accuracy = 1f,
            ProjectileSpeed = 3000f,
            EnergyCostPerShot = 5f * multiplier,
            HeatGeneration = 1f,
            MaxHeat = 100f,
            CooldownRate = 15f,
            TrackingSpeed = 2f
        };
    }
    
    /// <summary>
    /// Create a random weapon for loot/drops
    /// </summary>
    public static EnhancedTurret CreateRandomWeapon(Random random, int minTechLevel = 1, int maxTechLevel = 5)
    {
        var weaponTypes = Enum.GetValues<WeaponType>();
        var randomType = weaponTypes[random.Next(weaponTypes.Length)];
        int techLevel = random.Next(minTechLevel, maxTechLevel + 1);
        
        return CreateWeapon(randomType, techLevel);
    }
}
