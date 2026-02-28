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
    public String name { get; set; }

    public int? countMin { get; set; }
    public int? countMax { get; set; }

    public float? keyChance { get; set; }
    public float? extraKeyChance { get; set; }

    public int? minKeys { get; set; }
    public int? maxKeys { get; set; }
    public int? maxLifetimeKeys { get; set; }
    public int? lifetimeClaimedKeys { get; set; }

    public int? minGp { get; set; }
    public int? maxGp { get; set; }

    public BountyType bountyType { get; set; }

    public Difficulty difficulty { get; set; }

#nullable enable
    // Chance that the bounty will be skipped if rolled
    public float? skipChance { get; set; }
    public float? skipChancePerCompletion { get; set; }
    public float? maxSkipChance { get; set; }

    public String? description { get; set; }
    public String? help { get; set; }

    public DateTime? lastCompleted { get; set; }
    public DateTime? lastRolled { get; set; }

    public int? combatLevel { get; set; }
    public bool? isWildy { get; set; }

    public bool? requirementLocked { get; set; }
    public bool? completedLocked { get; set; } // Used for tasks that can no longer be complete

    public Bounty()
    {
        switch (difficulty)
        {
            case Difficulty.Novice:
                minKeys = minKeys == null ? 0 : minKeys;
                maxKeys = maxKeys == null ? 1 : maxKeys;
                keyChance = keyChance == null ? .75f : keyChance;
                minGp = minGp == null ? 0 : minGp;
                maxGp = maxGp == null ? 100 : maxGp;
                maxLifetimeKeys = maxLifetimeKeys == null ? 100000 : maxLifetimeKeys;
                skipChancePerCompletion = 15.0f;
                maxSkipChance = 100.0f;
                break;
            case Difficulty.Easy:
                minKeys = minKeys == null ? 0 : minKeys;
                maxKeys = maxKeys == null ? 1 : maxKeys;
                keyChance = keyChance == null ? .9f : keyChance;
                minGp = minGp == null ? 100 : minGp;
                maxGp = maxGp == null ? 500 : maxGp;
                maxLifetimeKeys = maxLifetimeKeys == null ? 100000 : maxLifetimeKeys;
                skipChancePerCompletion = 15.0f;
                maxSkipChance = 95.0f;
                break;
            case Difficulty.Medium:
                minKeys = minKeys == null ? 0 : minKeys;
                maxKeys = maxKeys == null ? 1 : maxKeys;
                keyChance = keyChance == null ? 1f : keyChance;
                minGp = minGp == null ? 500 : minGp;
                maxGp = maxGp == null ? 1000 : maxGp;
                maxLifetimeKeys = maxLifetimeKeys == null ? 100000 : maxLifetimeKeys;
                skipChancePerCompletion = 15.0f;
                maxSkipChance = 80.0f;
                break;
            case Difficulty.Hard:
                minKeys = minKeys == null ? 1 : minKeys;
                maxKeys = maxKeys == null ? 2 : maxKeys;
                keyChance = keyChance == null ? .25f : keyChance;
                minGp = minGp == null ? 1000 : minGp;
                maxGp = maxGp == null ? 10000 : maxGp;
                maxLifetimeKeys = maxLifetimeKeys == null ? 100000 : maxLifetimeKeys;
                skipChancePerCompletion = 15.0f;
                maxSkipChance = 80.0f;
                break;
            case Difficulty.Expert:
                minKeys = minKeys == null ? 1 : minKeys;
                maxKeys = maxKeys == null ? 2 : maxKeys;
                keyChance = keyChance == null ? .50f : keyChance;
                minGp = minGp == null ? 10000 : minGp;
                maxGp = maxGp == null ? 50000 : maxGp;
                maxLifetimeKeys = maxLifetimeKeys == null ? 100000 : maxLifetimeKeys;
                skipChancePerCompletion = 15.0f;
                maxSkipChance = 80.0f;
                break;
            case Difficulty.Grandmaster:
                minKeys = minKeys == null ? 2 : minKeys;
                maxKeys = maxKeys == null ? 3 : maxKeys;
                keyChance = keyChance == null ? .50f : keyChance;
                minGp = minGp == null ? 50000 : minGp;
                maxGp = maxGp == null ? 100000 : maxGp;
                maxLifetimeKeys = maxLifetimeKeys == null ? 100000 : maxLifetimeKeys;
                skipChancePerCompletion = 15.0f;
                maxSkipChance = 80.0f;
                break;
        }
    }
}