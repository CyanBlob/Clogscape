using System;
using System.Collections.Generic;
using Godot;

public class BossUnlock() : Unlockable
{
    public String name;
    public int recommendedCombatLevel;

    public override bool IsUnlocked(List<SkillUnlock> unlocks, List<QuestUnlock> quests, int combatLevel)
    {
        return RequirementsMet(unlocks, quests, combatLevel);
    }

    public override bool RequirementsMet(List<SkillUnlock> unlocks, List<QuestUnlock> quests, int combatLevel)
    {
        return true;
    }

    #nullable enable
    public override Texture2D? GetTexture()
    {
        return null;
    }
}
