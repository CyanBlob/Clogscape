using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Godot;

public enum DiaryDifficulty
{
    Easy, Medium, Hard, Elite
}

public class DiaryUnlock : Unlockable
{
    public String name {get; set; }
    public DiaryDifficulty diaryDifficulty {get; set; }

    public DiaryUnlock(String name, DiaryDifficulty diaryDifficulty)
    {
        this.name = name;
        this.diaryDifficulty = diaryDifficulty;
        unlockableType = UnlockableType.Diary;
    }

    public override bool RequirementsMet(List<SkillUnlock> unlocks, List<QuestUnlock> quests, int combatLevel = 0)
    {
        return true;
    }

    #nullable enable
    public override Texture2D? GetTexture()
    {
        try
        {
            Texture2D texture = (Texture2D)GD.Load($"res://Resources/resource-packs/quests_tab/green_achievement_diaries_icon.png");
            return texture;
        }
        catch
        {
            return null;
        }
    }

    public override string ToString()
    {
        return $"{name} {diaryDifficulty} Diary";
    }

    public override String DifficultyToString()
    {
        return $"{diaryDifficulty}";
    }

    public static List<DiaryUnlock> GetDiaries()
    {
        List<DiaryUnlock> diaryUnlocks = new();

        foreach(var diaryDifficulty in Enum.GetValues(typeof(DiaryDifficulty)))
        {
            var difficulty = (DiaryDifficulty) diaryDifficulty;
            diaryUnlocks.Add(new DiaryUnlock("Ardougne", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Desert", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Falador", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Fremennik Province", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Kandarin", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Karamja", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Kourend & Kebos", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Lumbridge & Draynore", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Morytania", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Varrock", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Western Provinces", difficulty));
            diaryUnlocks.Add(new DiaryUnlock("Wilderness", difficulty));
        }
        return diaryUnlocks;
    }
}
