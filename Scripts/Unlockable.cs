using System.Collections.Generic;
using Godot;

public abstract class Unlockable
{
    private bool Unlocked = false;

    public List<QuestUnlock> questRequirements = new();
    public List<(Skill, int)> skillRequirements = new();
    public int combatRequirement = 0;

    // The params are only used for bosses
    public virtual bool IsUnlocked(List<SkillUnlock> unlocks, List<QuestUnlock> quests, int combatLevel = 0)
    {
        return Unlocked;
    }

    public void Unlock()
    {
        Unlocked = true;
    }

    #nullable enable
    public abstract Texture2D? GetTexture();

    public abstract bool RequirementsMet(List<SkillUnlock> skillUnlocks, List<QuestUnlock> quests, int combatLevel = 0);
}