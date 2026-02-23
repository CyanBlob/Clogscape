using System.Collections.Generic;
using Godot;

public class FreeTile : Unlockable
{
    public override string DifficultyToString()
    {
        return "Free!";
    }

    public override string ToString()
    {
        return "It's a free tile!";
    }

    public override Texture2D GetTexture()
    {
        return null;
    }

    public override bool RequirementsMet(List<SkillUnlock> skillUnlocks, List<QuestUnlock> quests, int combatLevel = 0)
    {
        return true;
    }

    public override bool IsUnlocked()
    {
        return true;
    }
}