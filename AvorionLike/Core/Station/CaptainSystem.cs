using AvorionLike.Core.ECS;
using System.Numerics;

namespace AvorionLike.Core.Station;

/// <summary>
/// Captain specialization types
/// </summary>
public enum CaptainSpecialization
{
    Combat,         // Good at fighting
    Trading,        // Good at trading runs
    Mining,         // Efficient mining
    Salvage,        // Good at salvaging
    Exploration,    // Long-range exploration
    Transport,      // Cargo hauling
    Defense         // Station/fleet defense
}

/// <summary>
/// Captain personality traits affecting behavior
/// </summary>
public enum CaptainPersonality
{
    Aggressive,
    Cautious,
    Efficient,
    Brave,
    Greedy,
    Loyal,
    Reckless
}

/// <summary>
/// Represents a hireable captain at a station
/// </summary>
public class Captain
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "Captain";
    public CaptainSpecialization Specialization { get; set; }
    public CaptainPersonality Personality { get; set; }
    
    // Stats (0-100)
    public int CombatSkill { get; set; }
    public int TradingSkill { get; set; }
    public int MiningSkill { get; set; }
    public int NavigationSkill { get; set; }
    public int LeadershipSkill { get; set; }
    
    // Status
    public bool IsHired { get; set; } = false;
    public string CurrentShipId { get; set; } = "";
    public int HireCost { get; set; }
    public int DailySalary { get; set; }
    public int Morale { get; set; } = 100;
    public int Experience { get; set; } = 0;
    public int Level { get; set; } = 1;
    
    /// <summary>
    /// Generate a random captain with stats appropriate to specialization
    /// </summary>
    public static Captain GenerateRandom(Random random, CaptainSpecialization? forcedSpec = null)
    {
        var captain = new Captain();
        
        // Generate name
        string[] firstNames = { "James", "Sarah", "Marcus", "Elena", "Viktor", "Aria", "Chen", "Zara", "Kai", "Nova" };
        string[] lastNames = { "Drake", "Steele", "Voss", "Reyes", "Kaine", "Storm", "West", "Cross", "Hart", "Vale" };
        captain.Name = $"{firstNames[random.Next(firstNames.Length)]} {lastNames[random.Next(lastNames.Length)]}";
        
        // Set specialization
        if (forcedSpec.HasValue)
        {
            captain.Specialization = forcedSpec.Value;
        }
        else
        {
            captain.Specialization = (CaptainSpecialization)random.Next(Enum.GetValues<CaptainSpecialization>().Length);
        }
        
        // Set personality
        captain.Personality = (CaptainPersonality)random.Next(Enum.GetValues<CaptainPersonality>().Length);
        
        // Generate base stats (20-60 base)
        captain.CombatSkill = 20 + random.Next(40);
        captain.TradingSkill = 20 + random.Next(40);
        captain.MiningSkill = 20 + random.Next(40);
        captain.NavigationSkill = 20 + random.Next(40);
        captain.LeadershipSkill = 20 + random.Next(40);
        
        // Boost stats based on specialization
        switch (captain.Specialization)
        {
            case CaptainSpecialization.Combat:
                captain.CombatSkill += 30 + random.Next(20);
                captain.LeadershipSkill += 10 + random.Next(15);
                break;
            case CaptainSpecialization.Trading:
                captain.TradingSkill += 30 + random.Next(20);
                captain.NavigationSkill += 10 + random.Next(15);
                break;
            case CaptainSpecialization.Mining:
                captain.MiningSkill += 30 + random.Next(20);
                break;
            case CaptainSpecialization.Salvage:
                captain.MiningSkill += 20 + random.Next(15);
                captain.NavigationSkill += 15 + random.Next(10);
                break;
            case CaptainSpecialization.Exploration:
                captain.NavigationSkill += 30 + random.Next(20);
                break;
            case CaptainSpecialization.Transport:
                captain.NavigationSkill += 20 + random.Next(15);
                captain.TradingSkill += 15 + random.Next(10);
                break;
            case CaptainSpecialization.Defense:
                captain.CombatSkill += 20 + random.Next(15);
                captain.LeadershipSkill += 20 + random.Next(15);
                break;
        }
        
        // Clamp stats to 0-100
        captain.CombatSkill = Math.Clamp(captain.CombatSkill, 0, 100);
        captain.TradingSkill = Math.Clamp(captain.TradingSkill, 0, 100);
        captain.MiningSkill = Math.Clamp(captain.MiningSkill, 0, 100);
        captain.NavigationSkill = Math.Clamp(captain.NavigationSkill, 0, 100);
        captain.LeadershipSkill = Math.Clamp(captain.LeadershipSkill, 0, 100);
        
        // Calculate hire cost based on stats
        int avgStats = (captain.CombatSkill + captain.TradingSkill + captain.MiningSkill + 
                        captain.NavigationSkill + captain.LeadershipSkill) / 5;
        captain.HireCost = 1000 + avgStats * 100;
        captain.DailySalary = 100 + avgStats * 10;
        
        return captain;
    }
    
    /// <summary>
    /// Get the captain's overall skill rating
    /// </summary>
    public int GetOverallRating()
    {
        return (CombatSkill + TradingSkill + MiningSkill + NavigationSkill + LeadershipSkill) / 5;
    }
}

/// <summary>
/// Component for stations that can hire out captains
/// </summary>
public class StationCaptainRosterComponent : IComponent
{
    public Guid EntityId { get; set; }
    public List<Captain> AvailableCaptains { get; set; } = new();
    public DateTime LastRefreshTime { get; set; } = DateTime.UtcNow;
    public int RefreshIntervalHours { get; set; } = 24;  // New captains appear daily
    
    /// <summary>
    /// Refresh the roster with new captains
    /// </summary>
    public void RefreshRoster(string stationType, Random random)
    {
        // Remove hired captains
        AvailableCaptains.RemoveAll(c => c.IsHired);
        
        // Add new captains based on station type
        int newCaptainCount = 2 + random.Next(4);  // 2-5 new captains
        
        for (int i = 0; i < newCaptainCount; i++)
        {
            CaptainSpecialization? preferredSpec = GetPreferredSpecialization(stationType, random);
            var captain = Captain.GenerateRandom(random, preferredSpec);
            AvailableCaptains.Add(captain);
        }
        
        LastRefreshTime = DateTime.UtcNow;
    }
    
    /// <summary>
    /// Get preferred captain specialization based on station type
    /// </summary>
    private CaptainSpecialization? GetPreferredSpecialization(string stationType, Random random)
    {
        // 60% chance to get station-appropriate captain
        if (random.Next(100) < 60)
        {
            return stationType.ToLower() switch
            {
                "military" => CaptainSpecialization.Combat,
                "trading" => CaptainSpecialization.Trading,
                "mining" => CaptainSpecialization.Mining,
                "shipyard" => CaptainSpecialization.Combat,
                "refinery" => CaptainSpecialization.Transport,
                _ => null
            };
        }
        return null;  // Random specialization
    }
    
    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            ["EntityId"] = EntityId.ToString(),
            ["LastRefreshTime"] = LastRefreshTime.ToString("o"),
            ["RefreshIntervalHours"] = RefreshIntervalHours,
            ["CaptainCount"] = AvailableCaptains.Count
        };
    }
}
