using System;
using System.Collections.Generic;
using Godot;

public abstract class Unlockable
{
    private bool Unlocked = false;

    public List<QuestUnlock> questRequirements = new();
    public List<(Skill, int)> skillRequirements = new();
    public int combatRequirement = 0;

    public virtual bool IsUnlocked()
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

    public abstract String DifficultyToString();
}