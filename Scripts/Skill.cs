using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using Godot;
using Range = System.Range;

public enum Skill
{
    Agility,
    //Attack,
    Construction,
    Cooking,
    Crafting,
    //Defence,
    Farming,
    Firemaking,
    Fishing,
    Fletching,
    Herblore,
    //Hitpoints,
    Hunter,
    //Magic,
    Mining,
    Prayer,
    //Ranged,
    Runecraft,
    Sailing,
    Slayer,
    Smithing,
    //Strength,
    Thieving,
    Woodcutting,
}


public class SkillUnlock : Unlockable, IComparable
{
    public Skill skill;

    public static List<Range> standardRanges = [new Range(1, 10), new Range(11, 20), new Range(21, 30), new Range(31, 40), new Range(41, 50), new Range(51, 60), new Range(61, 70), new Range(71, 75), new Range(76, 80), new Range(81, 85), new Range(86, 90), new Range(91, 95), new Range(96, 99)];

    public Range levels;

    public SkillUnlock(Skill skill, Range levels, bool unlocked)
    {
        this.skill = skill;
        this.levels = levels;

        if (unlocked)
        {
            Unlock();
        }
    }

#nullable enable
    public override Texture2D? GetTexture()
    {
        try
        {
            ImageTexture texture = new ImageTexture();
            Image image = new Image();
            image.Load($"Resources/resource-packs/skill/{skill}.png");
            texture.SetImage(image);
            return texture;
        }
        catch
        {
            return null;
        }
    }

    // TODO: Unit tests
    public override bool RequirementsMet(List<SkillUnlock> unlocks, List<QuestUnlock> quests, int combatLevel = 0)
    {
        List<SkillUnlock> levelsUnlocked = unlocks.TakeWhile(unlock =>
        {
            return unlock.skill == skill;
        }).ToList<SkillUnlock>();

        levelsUnlocked.Sort();

        var levelsIndex = standardRanges.FindIndex(l => { return l.Start.Value == levels.Start.Value; });

        // First levels have no requirements
        // TODO: Some skills have quest requirements!
        if (levelsIndex == 0)
        {
            return true;
        }

        var previousLevelsIndex = levelsUnlocked.FindIndex(unlock => { return unlock.levels.Start.Value == standardRanges[levelsIndex - 1].Start.Value; });

        if (previousLevelsIndex > 0)
        {
            if (levelsUnlocked[previousLevelsIndex].IsUnlocked())
            {
                GD.Print($"Requirements met! {skill}: {levels}");
                return true;
            }
            return false;
        }

        return false;
    }


    public int CompareTo(object? obj)
    {
        if (obj == null)
        {
            return -1;
        }

        var other = (SkillUnlock)obj;
        if (other.levels.Start.Value > levels.Start.Value)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    public static List<SkillUnlock> GetSkillUnlocks()
    {
        List<SkillUnlock> skillUnlocks = new();

        foreach (var range in standardRanges)
        {
            foreach (var skill in Enum.GetValues(typeof(Skill)))
            {
               SkillUnlock newUnlock = new SkillUnlock((Skill)skill, range, false); 
               skillUnlocks.Add(newUnlock);
            }
        }

        return skillUnlocks;
    }

    public override string ToString()
    {
        return $"{skill}: {levels}";
    }

    public override String DifficultyToString()
    {
        return $"{levels.Start.Value} - {levels.End.Value}";
    }
}