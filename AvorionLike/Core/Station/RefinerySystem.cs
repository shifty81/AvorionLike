using AvorionLike.Core.ECS;

namespace AvorionLike.Core.Station;

/// <summary>
/// Status of a refinery order
/// </summary>
public enum RefineryOrderStatus
{
    Pending,        // Order placed, not started
    Processing,     // Currently being processed
    Complete,       // Ready for pickup
    PickedUp        // Player has collected the ingots
}

/// <summary>
/// Represents an ore processing order at a refinery
/// </summary>
public class RefineryOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string PlayerId { get; set; } = "";
    public string OreType { get; set; } = "Iron";
    public int OreAmount { get; set; }
    public int IngotAmount { get; set; }  // Processed output
    public RefineryOrderStatus Status { get; set; } = RefineryOrderStatus.Pending;
    public DateTime OrderTime { get; set; } = DateTime.UtcNow;
    public DateTime CompletionTime { get; set; }
    public float ProcessingTimeMinutes { get; set; }
    public float ProcessingCost { get; set; }
    
    /// <summary>
    /// Calculate processing time based on ore amount
    /// More ore = more time to process
    /// </summary>
    public static float CalculateProcessingTime(int oreAmount)
    {
        // Base: 1 minute per 10 ore, minimum 5 minutes
        float minutes = Math.Max(5, oreAmount / 10f);
        return minutes;
    }
    
    /// <summary>
    /// Calculate ingot output (some loss during refining)
    /// </summary>
    public static int CalculateIngotOutput(int oreAmount, Random random)
    {
        // 70-85% efficiency in refining
        float efficiency = 0.70f + (float)random.NextDouble() * 0.15f;
        return (int)(oreAmount * efficiency);
    }
    
    /// <summary>
    /// Calculate processing cost
    /// </summary>
    public static float CalculateProcessingCost(int oreAmount)
    {
        // Cost scales with amount
        return oreAmount * 0.5f;  // 0.5 credits per ore
    }
}

/// <summary>
/// Component for refinery stations that process ore into ingots
/// </summary>
public class RefineryComponent : IComponent
{
    public Guid EntityId { get; set; }
    public List<RefineryOrder> ActiveOrders { get; set; } = new();
    public List<RefineryOrder> CompletedOrders { get; set; } = new();
    public int MaxConcurrentOrders { get; set; } = 10;
    public float ProcessingSpeedMultiplier { get; set; } = 1.0f;  // Can be upgraded
    public float EfficiencyBonus { get; set; } = 0f;  // Bonus to ingot output (0-0.15)
    
    // Storage for ores and ingots
    public Dictionary<string, int> OreStorage { get; set; } = new();
    public Dictionary<string, int> IngotStorage { get; set; } = new();
    public int MaxStoragePerType { get; set; } = 10000;
    
    /// <summary>
    /// Place a new refinery order
    /// </summary>
    public bool PlaceOrder(string playerId, string oreType, int oreAmount, Random random, out string errorMessage)
    {
        errorMessage = "";
        
        // Check if refinery can accept more orders
        if (ActiveOrders.Count >= MaxConcurrentOrders)
        {
            errorMessage = "Refinery is at maximum capacity. Please wait for other orders to complete.";
            return false;
        }
        
        // Check if we have storage space
        if (!OreStorage.ContainsKey(oreType))
        {
            OreStorage[oreType] = 0;
        }
        
        if (OreStorage[oreType] + oreAmount > MaxStoragePerType)
        {
            errorMessage = $"Not enough storage space for {oreType} ore.";
            return false;
        }
        
        // Create order
        var order = new RefineryOrder
        {
            PlayerId = playerId,
            OreType = oreType,
            OreAmount = oreAmount,
            IngotAmount = RefineryOrder.CalculateIngotOutput(oreAmount, random),
            ProcessingTimeMinutes = RefineryOrder.CalculateProcessingTime(oreAmount) / ProcessingSpeedMultiplier,
            ProcessingCost = RefineryOrder.CalculateProcessingCost(oreAmount),
            Status = RefineryOrderStatus.Pending,
            OrderTime = DateTime.UtcNow
        };
        
        // Apply efficiency bonus
        order.IngotAmount = (int)(order.IngotAmount * (1.0f + EfficiencyBonus));
        
        // Set completion time
        order.CompletionTime = DateTime.UtcNow.AddMinutes(order.ProcessingTimeMinutes);
        
        // Add ore to storage
        OreStorage[oreType] += oreAmount;
        
        // Add order to queue
        ActiveOrders.Add(order);
        
        return true;
    }
    
    /// <summary>
    /// Update processing orders (call this periodically)
    /// </summary>
    public void UpdateOrders()
    {
        var now = DateTime.UtcNow;
        
        foreach (var order in ActiveOrders.ToList())
        {
            if (order.Status == RefineryOrderStatus.Pending)
            {
                order.Status = RefineryOrderStatus.Processing;
            }
            
            if (order.Status == RefineryOrderStatus.Processing)
            {
                // Check if processing is complete
                if (now >= order.CompletionTime)
                {
                    CompleteOrder(order);
                }
            }
        }
        
        // Clean up old completed orders (older than 7 days)
        CompletedOrders.RemoveAll(o => 
            o.Status == RefineryOrderStatus.PickedUp && 
            (now - o.CompletionTime).TotalDays > 7);
    }
    
    /// <summary>
    /// Complete an order and produce ingots
    /// </summary>
    private void CompleteOrder(RefineryOrder order)
    {
        // Remove ore from storage
        if (OreStorage.ContainsKey(order.OreType))
        {
            OreStorage[order.OreType] -= order.OreAmount;
            if (OreStorage[order.OreType] <= 0)
            {
                OreStorage.Remove(order.OreType);
            }
        }
        
        // Add ingots to storage
        string ingotType = $"{order.OreType}Ingot";
        if (!IngotStorage.ContainsKey(ingotType))
        {
            IngotStorage[ingotType] = 0;
        }
        IngotStorage[ingotType] += order.IngotAmount;
        
        // Update order status
        order.Status = RefineryOrderStatus.Complete;
        order.CompletionTime = DateTime.UtcNow;
        
        // Move to completed orders
        ActiveOrders.Remove(order);
        CompletedOrders.Add(order);
    }
    
    /// <summary>
    /// Pickup completed ingots
    /// </summary>
    public bool PickupOrder(Guid orderId, out RefineryOrder? order, out string errorMessage)
    {
        errorMessage = "";
        order = null;
        
        // Find order
        order = CompletedOrders.FirstOrDefault(o => o.Id == orderId);
        
        if (order == null)
        {
            errorMessage = "Order not found.";
            return false;
        }
        
        if (order.Status != RefineryOrderStatus.Complete)
        {
            errorMessage = "Order is not ready for pickup yet.";
            return false;
        }
        
        // Remove ingots from storage
        string ingotType = $"{order.OreType}Ingot";
        if (IngotStorage.ContainsKey(ingotType))
        {
            IngotStorage[ingotType] -= order.IngotAmount;
            if (IngotStorage[ingotType] <= 0)
            {
                IngotStorage.Remove(ingotType);
            }
        }
        
        // Mark as picked up
        order.Status = RefineryOrderStatus.PickedUp;
        
        return true;
    }
    
    /// <summary>
    /// Get all orders for a specific player
    /// </summary>
    public List<RefineryOrder> GetPlayerOrders(string playerId)
    {
        var orders = new List<RefineryOrder>();
        orders.AddRange(ActiveOrders.Where(o => o.PlayerId == playerId));
        orders.AddRange(CompletedOrders.Where(o => o.PlayerId == playerId && o.Status != RefineryOrderStatus.PickedUp));
        return orders;
    }
    
    /// <summary>
    /// Get estimated time remaining for an order
    /// </summary>
    public TimeSpan GetTimeRemaining(Guid orderId)
    {
        var order = ActiveOrders.FirstOrDefault(o => o.Id == orderId);
        if (order == null || order.Status == RefineryOrderStatus.Complete)
        {
            return TimeSpan.Zero;
        }
        
        var remaining = order.CompletionTime - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }
    
    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["EntityId"] = EntityId.ToString(),
            ["MaxConcurrentOrders"] = MaxConcurrentOrders,
            ["ProcessingSpeedMultiplier"] = ProcessingSpeedMultiplier,
            ["EfficiencyBonus"] = EfficiencyBonus,
            ["ActiveOrdersCount"] = ActiveOrders.Count,
            ["CompletedOrdersCount"] = CompletedOrders.Count,
            ["OreStorage"] = OreStorage,
            ["IngotStorage"] = IngotStorage
        };
    }
}

/// <summary>
/// System for managing refinery operations
/// </summary>
public class RefinerySystem
{
    private readonly Random _random = new();
    
    /// <summary>
    /// Update all refinery stations
    /// </summary>
    public void Update(List<RefineryComponent> refineries)
    {
        foreach (var refinery in refineries)
        {
            refinery.UpdateOrders();
        }
    }
    
    /// <summary>
    /// Drop off ore at a refinery
    /// </summary>
    public bool DropOffOre(RefineryComponent refinery, string playerId, string oreType, int amount, out string message)
    {
        return refinery.PlaceOrder(playerId, oreType, amount, _random, out message);
    }
    
    /// <summary>
    /// Pick up processed ingots
    /// </summary>
    public bool PickUpIngots(RefineryComponent refinery, Guid orderId, out Dictionary<string, int> ingots, out string message)
    {
        ingots = new Dictionary<string, int>();
        
        if (refinery.PickupOrder(orderId, out var order, out message))
        {
            if (order != null)
            {
                string ingotType = $"{order.OreType}Ingot";
                ingots[ingotType] = order.IngotAmount;
                return true;
            }
        }
        
        return false;
    }
}
