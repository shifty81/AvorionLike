using AvorionLike.Core.ECS;

namespace AvorionLike.Core.Resources;

/// <summary>
/// Component for inventory management
/// </summary>
public class InventoryComponent : IComponent
{
    public Guid EntityId { get; set; }
    public Inventory Inventory { get; set; }

    public InventoryComponent()
    {
        Inventory = new Inventory();
    }

    public InventoryComponent(int maxCapacity)
    {
        Inventory = new Inventory { MaxCapacity = maxCapacity };
    }
}

/// <summary>
/// Represents a ship subsystem upgrade
/// </summary>
public class SubsystemUpgrade
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = ""; // Shield, Weapon, Cargo, etc.
    public float EffectValue { get; set; }
    public int Level { get; set; }
    public Dictionary<ResourceType, int> CraftingCost { get; set; } = new();
}

/// <summary>
/// Manages crafting and upgrades
/// </summary>
public class CraftingSystem
{
    private readonly Dictionary<string, SubsystemUpgrade> _upgradeTemplates = new();

    public CraftingSystem()
    {
        InitializeUpgradeTemplates();
    }

    private void InitializeUpgradeTemplates()
    {
        // Shield Booster
        _upgradeTemplates["ShieldBooster1"] = new SubsystemUpgrade
        {
            Name = "Shield Booster Mk.I",
            Type = "Shield",
            EffectValue = 1.2f,
            Level = 1,
            CraftingCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Iron, 100 },
                { ResourceType.Credits, 1000 }
            }
        };

        // Cargo Extension
        _upgradeTemplates["CargoExtension1"] = new SubsystemUpgrade
        {
            Name = "Cargo Extension Mk.I",
            Type = "Cargo",
            EffectValue = 500f,
            Level = 1,
            CraftingCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Iron, 50 },
                { ResourceType.Titanium, 25 },
                { ResourceType.Credits, 500 }
            }
        };

        // Turret Control
        _upgradeTemplates["TurretControl1"] = new SubsystemUpgrade
        {
            Name = "Turret Control System Mk.I",
            Type = "Weapon",
            EffectValue = 2f,
            Level = 1,
            CraftingCost = new Dictionary<ResourceType, int>
            {
                { ResourceType.Titanium, 75 },
                { ResourceType.Credits, 1500 }
            }
        };
    }

    /// <summary>
    /// Attempt to craft an upgrade
    /// </summary>
    public SubsystemUpgrade? CraftUpgrade(string templateId, Inventory inventory)
    {
        if (!_upgradeTemplates.TryGetValue(templateId, out var template))
        {
            return null;
        }

        // Check if inventory has required resources
        foreach (var cost in template.CraftingCost)
        {
            if (!inventory.HasResource(cost.Key, cost.Value))
            {
                return null;
            }
        }

        // Deduct resources
        foreach (var cost in template.CraftingCost)
        {
            inventory.RemoveResource(cost.Key, cost.Value);
        }

        // Return crafted upgrade
        return new SubsystemUpgrade
        {
            Name = template.Name,
            Type = template.Type,
            EffectValue = template.EffectValue,
            Level = template.Level,
            CraftingCost = new Dictionary<ResourceType, int>(template.CraftingCost)
        };
    }

    /// <summary>
    /// Get all available upgrade templates
    /// </summary>
    public IEnumerable<SubsystemUpgrade> GetAvailableUpgrades()
    {
        return _upgradeTemplates.Values;
    }
}
