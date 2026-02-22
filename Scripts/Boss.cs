using System;
using System.Collections.Generic;
using Godot;

public class BossUnlock()
{
    public String name;
    public int recommendedCombatLevel;

    public bool IsUnlocked(List<SkillUnlock> unlocks, List<QuestUnlock> quests, int combatLevel)
    {
        return RequirementsMet(unlocks, quests, combatLevel);
    }

    public bool RequirementsMet(List<SkillUnlock> unlocks, List<QuestUnlock> quests, int combatLevel)
    {
        return true;
    }

    #nullable enable
    public Texture2D? GetTexture()
    {
        return null;
    }

    public String DifficultyToString()
    {
        return $"Boss";
    }
}
