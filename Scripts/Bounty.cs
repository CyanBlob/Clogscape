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
    Challenge,
    Misc,
    Fetch,
    RealLife,
    Slayer
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
    public String name {get; set; }
    public String imagePath {get; set; }
    public float keyChance {get; set; }
    public float extraKeyChance {get; set;}

    public int minKeys {get; set; }
    public int maxKeys {get; set; }
    //public int? maxLifetimeKeys {get; set;}
    public int? maxLifetimeKeys = 2;
    public int? lifetimeClaimedKeys {get; set;}

    public int minGp {get; set; }
    public int maxGp {get; set; }

    public BountyType bountyType {get; set; }

    public Difficulty difficulty {get; set; }

#nullable enable
    // Chance that the bounty will be skipped if rolled
    public float skipChance {get; set; }
    public String? description {get; set; }
    public String? help {get; set; }

    public DateTime? lastCompleted {get; set; }
    public DateTime? lastRolled {get; set; }

    public int combatLevel {get; set; }
    public bool isWildy {get; set; }

    public bool? requirementLocked {get; set; }
    public bool? completedLocked {get; set; } // Used for tasks that can no longer be complete
}