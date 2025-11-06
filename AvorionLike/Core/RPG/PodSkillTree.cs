using System.Text.Json;
using AvorionLike.Core.ECS;
using AvorionLike.Core.Persistence;

namespace AvorionLike.Core.RPG;

/// <summary>
/// Represents a skill category in the pod skill tree
/// </summary>
public enum SkillCategory
{
    Combat,      // Weapon damage, accuracy, critical hits
    Defense,     // Shield regeneration, armor, damage reduction
    Engineering, // Power efficiency, system repair, thrust
    Exploration, // Scanner range, jump drive, resource detection
    Leadership   // Fleet bonuses, trade, experience gain
}

/// <summary>
/// Represents an individual skill in the pod skill tree
/// </summary>
public class PodSkill
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public SkillCategory Category { get; set; }
    public int MaxRank { get; set; } = 5;
    public int CurrentRank { get; set; } = 0;
    public int SkillPointCost { get; set; } = 1;
    
    // Prerequisites
    public List<string> RequiredSkills { get; set; } = new();
    public int RequiredLevel { get; set; } = 1;
    
    // Skill effects (value per rank)
    public float EffectValue { get; set; } = 0f;
    
    public PodSkill(string id, string name, string description, SkillCategory category, 
                    float effectValue, int maxRank = 5, int skillPointCost = 1)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
        EffectValue = effectValue;
        MaxRank = maxRank;
        SkillPointCost = skillPointCost;
    }
    
    /// <summary>
    /// Check if the skill can be learned
    /// </summary>
    public bool CanLearn(int availableSkillPoints, int currentLevel, Dictionary<string, PodSkill> learnedSkills)
    {
        if (CurrentRank >= MaxRank) return false;
        if (availableSkillPoints < SkillPointCost) return false;
        if (currentLevel < RequiredLevel) return false;
        
        // Check prerequisites
        foreach (var requiredSkillId in RequiredSkills)
        {
            if (!learnedSkills.ContainsKey(requiredSkillId) || learnedSkills[requiredSkillId].CurrentRank == 0)
            {
                return false;
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get the total effect value for current rank
    /// </summary>
    public float GetTotalEffect()
    {
        return EffectValue * CurrentRank;
    }
}

/// <summary>
/// Component managing the pod's skill tree
/// </summary>
public class PodSkillTreeComponent : IComponent, ISerializable
{
    public Guid EntityId { get; set; }
    
    // Available skills organized by category
    public Dictionary<string, PodSkill> AllSkills { get; set; } = new();
    
    // Learned skills
    public Dictionary<string, PodSkill> LearnedSkills { get; set; } = new();
    
    public PodSkillTreeComponent()
    {
        InitializeSkills();
    }
    
    /// <summary>
    /// Initialize all available skills
    /// </summary>
    private void InitializeSkills()
    {
        // Combat Skills
        AddSkill(new PodSkill(
            "combat_weapon_damage",
            "Weapon Mastery",
            "Increases weapon damage by 10% per rank",
            SkillCategory.Combat,
            0.10f,
            5,
            1
        ));
        
        AddSkill(new PodSkill(
            "combat_critical_hit",
            "Critical Strike",
            "Increases critical hit chance by 5% per rank",
            SkillCategory.Combat,
            0.05f,
            5,
            1
        ) { RequiredSkills = new List<string> { "combat_weapon_damage" } });
        
        AddSkill(new PodSkill(
            "combat_fire_rate",
            "Rapid Fire",
            "Increases weapon fire rate by 8% per rank",
            SkillCategory.Combat,
            0.08f,
            5,
            1
        ) { RequiredLevel = 3 });
        
        // Defense Skills
        AddSkill(new PodSkill(
            "defense_shield_capacity",
            "Shield Fortification",
            "Increases shield capacity by 15% per rank",
            SkillCategory.Defense,
            0.15f,
            5,
            1
        ));
        
        AddSkill(new PodSkill(
            "defense_shield_regen",
            "Shield Regeneration",
            "Increases shield regeneration by 20% per rank",
            SkillCategory.Defense,
            0.20f,
            5,
            1
        ) { RequiredSkills = new List<string> { "defense_shield_capacity" } });
        
        AddSkill(new PodSkill(
            "defense_armor",
            "Reinforced Armor",
            "Reduces incoming damage by 5% per rank",
            SkillCategory.Defense,
            0.05f,
            5,
            1
        ) { RequiredLevel = 5 });
        
        // Engineering Skills
        AddSkill(new PodSkill(
            "engineering_thrust",
            "Advanced Propulsion",
            "Increases thrust power by 12% per rank",
            SkillCategory.Engineering,
            0.12f,
            5,
            1
        ));
        
        AddSkill(new PodSkill(
            "engineering_power",
            "Power Optimization",
            "Increases power generation by 15% per rank",
            SkillCategory.Engineering,
            0.15f,
            5,
            1
        ));
        
        AddSkill(new PodSkill(
            "engineering_efficiency",
            "System Efficiency",
            "Reduces pod efficiency penalty by 5% per rank",
            SkillCategory.Engineering,
            0.05f,
            5,
            2
        ) { RequiredSkills = new List<string> { "engineering_power" }, RequiredLevel = 5 });
        
        // Exploration Skills
        AddSkill(new PodSkill(
            "exploration_scanner",
            "Enhanced Scanners",
            "Increases scanner range by 25% per rank",
            SkillCategory.Exploration,
            0.25f,
            5,
            1
        ));
        
        AddSkill(new PodSkill(
            "exploration_resource",
            "Resource Detection",
            "Increases chance to find rare resources by 10% per rank",
            SkillCategory.Exploration,
            0.10f,
            5,
            1
        ) { RequiredSkills = new List<string> { "exploration_scanner" } });
        
        AddSkill(new PodSkill(
            "exploration_jump",
            "Jump Drive Mastery",
            "Reduces jump drive cooldown by 15% per rank",
            SkillCategory.Exploration,
            0.15f,
            3,
            2
        ) { RequiredLevel = 8 });
        
        // Leadership Skills
        AddSkill(new PodSkill(
            "leadership_experience",
            "Fast Learner",
            "Increases experience gain by 10% per rank",
            SkillCategory.Leadership,
            0.10f,
            5,
            1
        ));
        
        AddSkill(new PodSkill(
            "leadership_trade",
            "Trade Expertise",
            "Improves trade prices by 5% per rank",
            SkillCategory.Leadership,
            0.05f,
            5,
            1
        ));
        
        AddSkill(new PodSkill(
            "leadership_fleet",
            "Fleet Commander",
            "Provides 8% bonus to fleet ship stats per rank",
            SkillCategory.Leadership,
            0.08f,
            5,
            2
        ) { RequiredLevel = 10 });
    }
    
    /// <summary>
    /// Add a skill to the available skills
    /// </summary>
    private void AddSkill(PodSkill skill)
    {
        AllSkills[skill.Id] = skill;
    }
    
    /// <summary>
    /// Learn or upgrade a skill
    /// </summary>
    public bool LearnSkill(string skillId, int currentLevel, ref int availableSkillPoints)
    {
        if (!AllSkills.ContainsKey(skillId))
        {
            return false;
        }
        
        var skill = AllSkills[skillId];
        
        if (!skill.CanLearn(availableSkillPoints, currentLevel, LearnedSkills))
        {
            return false;
        }
        
        // Deduct skill points
        availableSkillPoints -= skill.SkillPointCost;
        
        // Add or upgrade skill
        if (!LearnedSkills.ContainsKey(skillId))
        {
            LearnedSkills[skillId] = new PodSkill(
                skill.Id, skill.Name, skill.Description, skill.Category,
                skill.EffectValue, skill.MaxRank, skill.SkillPointCost
            )
            {
                RequiredSkills = skill.RequiredSkills,
                RequiredLevel = skill.RequiredLevel,
                CurrentRank = 1
            };
        }
        else
        {
            LearnedSkills[skillId].CurrentRank++;
        }
        
        // Update the skill in AllSkills too
        AllSkills[skillId].CurrentRank = LearnedSkills[skillId].CurrentRank;
        
        return true;
    }
    
    /// <summary>
    /// Get the total bonus for a specific skill
    /// </summary>
    public float GetSkillBonus(string skillId)
    {
        if (!LearnedSkills.ContainsKey(skillId))
        {
            return 0f;
        }
        
        return LearnedSkills[skillId].GetTotalEffect();
    }
    
    /// <summary>
    /// Get all learned skills in a category
    /// </summary>
    public List<PodSkill> GetSkillsByCategory(SkillCategory category)
    {
        return LearnedSkills.Values.Where(s => s.Category == category).ToList();
    }
    
    /// <summary>
    /// Get total skill points invested
    /// </summary>
    public int GetTotalSkillPointsSpent()
    {
        return LearnedSkills.Values.Sum(s => s.CurrentRank * s.SkillPointCost);
    }
    
    /// <summary>
    /// Serialize the component
    /// </summary>
    public Dictionary<string, object> Serialize()
    {
        var learnedSkillsData = new List<Dictionary<string, object>>();
        foreach (var skill in LearnedSkills.Values)
        {
            learnedSkillsData.Add(new Dictionary<string, object>
            {
                ["Id"] = skill.Id,
                ["CurrentRank"] = skill.CurrentRank
            });
        }
        
        return new Dictionary<string, object>
        {
            ["EntityId"] = EntityId.ToString(),
            ["LearnedSkills"] = learnedSkillsData
        };
    }
    
    /// <summary>
    /// Deserialize the component
    /// </summary>
    public void Deserialize(Dictionary<string, object> data)
    {
        EntityId = Guid.Parse(SerializationHelper.GetValue(data, "EntityId", Guid.Empty.ToString()));
        
        LearnedSkills.Clear();
        if (data.ContainsKey("LearnedSkills"))
        {
            var skillsData = data["LearnedSkills"];
            
            if (skillsData is JsonElement jsonElement && jsonElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var skillElement in jsonElement.EnumerateArray())
                {
                    var skillId = skillElement.GetProperty("Id").GetString() ?? "";
                    var currentRank = skillElement.GetProperty("CurrentRank").GetInt32();
                    
                    if (AllSkills.ContainsKey(skillId))
                    {
                        var originalSkill = AllSkills[skillId];
                        var learnedSkill = new PodSkill(
                            originalSkill.Id, originalSkill.Name, originalSkill.Description,
                            originalSkill.Category, originalSkill.EffectValue,
                            originalSkill.MaxRank, originalSkill.SkillPointCost
                        )
                        {
                            RequiredSkills = originalSkill.RequiredSkills,
                            RequiredLevel = originalSkill.RequiredLevel,
                            CurrentRank = currentRank
                        };
                        
                        LearnedSkills[skillId] = learnedSkill;
                        AllSkills[skillId].CurrentRank = currentRank;
                    }
                }
            }
        }
    }
}
