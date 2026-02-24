using System;
using Godot;

public enum BountyType
{
    Boss,
    Raid,
    Clue,
    Minigame,
    Grind,
    Skilling,
    Misc
}

public enum Difficulty
{
    Novice,
    Easy,
    Medium,
    Hard,
    Expert,
    Grandmaster
}

public class Bounty
{
    public String name;
    public Texture2D texture;
    public int keyValue;
    public String description;
    public String help;

    public int combatLevel = 0;
    public bool isWildy = false;

    public bool requirementLocked = false;
    public bool completedLocked = true; // Used for tasks that can no longer be completed
}