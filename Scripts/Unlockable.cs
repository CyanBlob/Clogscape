using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Godot;

public enum UnlockableType
{
    Skill,
    Quest,
    Diary,
    Free
}

public abstract class Unlockable
{
    [JsonInclude]
    public bool Unlocked = false;

    [JsonInclude]
    public bool Claimed = false;

    public List<QuestUnlock> questRequirements = new();
    public List<(Skill, int)> skillRequirements = new();
    public int combatRequirement = 0;

    public UnlockableType unlockableType {get; set;}

    public int gridPosX {get; set; }
    public int gridPosY {get; set; }

    public virtual bool IsUnlocked()
    {
        return Unlocked;
    }

    public void Unlock()
    {
        Unlocked = true;
    }

    public virtual bool IsClaimed()
    {
        return Claimed;
    }

    public void Claim()
    {
        Claimed = true;
    }

    #nullable enable
    public abstract Texture2D? GetTexture();

    public abstract bool RequirementsMet(List<SkillUnlock> skillUnlocks, List<QuestUnlock> quests, int combatLevel = 0);

    public abstract String DifficultyToString();
}