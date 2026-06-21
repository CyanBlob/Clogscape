namespace Bountyscape.Tests;

using GdUnit4;
using static GdUnit4.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Range = System.Range;

[TestSuite]
[RequireGodotRuntime]
public class SkillUnlockTests
{
    [Before]
    public void SetUp() => GameManager.Log = _ => { };

    // 18 skills in enum, Prayer excluded = 17; 13 level ranges
    private const int SkillCount = 17;
    private const int RangeCount = 13;
    private const int TotalUnlocks = SkillCount * RangeCount; // 221

    // ── GetSkillUnlocks ──────────────────────────────────────────────────────

    [TestCase]
    public void GetSkillUnlocks_ReturnsCorrectCount()
    {
        var unlocks = SkillUnlock.GetSkillUnlocks();
        AssertThat(unlocks.Count).IsEqual(TotalUnlocks);
    }

    [TestCase]
    public void GetSkillUnlocks_ExcludesPrayer()
    {
        var unlocks = SkillUnlock.GetSkillUnlocks();
        AssertThat(unlocks.Any(u => u.skill == Skill.Prayer)).IsFalse();
    }

    [TestCase]
    public void GetSkillUnlocks_AllRangesPresent()
    {
        var unlocks = SkillUnlock.GetSkillUnlocks();
        foreach (var range in SkillUnlock.standardRanges)
        {
            var count = unlocks.Count(u => u.levels.Start.Value == range.Start.Value);
            AssertThat(count).IsEqual(SkillCount);
        }
    }

    [TestCase]
    public void GetSkillUnlocks_EachSkillAppearsInEveryRange()
    {
        var unlocks = SkillUnlock.GetSkillUnlocks();
        foreach (Skill skill in Enum.GetValues(typeof(Skill)))
        {
            if (skill == Skill.Prayer) continue;
            var count = unlocks.Count(u => u.skill == skill);
            AssertThat(count).IsEqual(RangeCount);
        }
    }

    [TestCase]
    public void GetSkillUnlocks_AllStartAsLocked()
    {
        var unlocks = SkillUnlock.GetSkillUnlocks();
        AssertThat(unlocks.All(u => !u.IsUnlocked())).IsTrue();
    }

    // ── GetRandomizedSkillUnlocks ────────────────────────────────────────────

    [TestCase]
    public void GetRandomizedSkillUnlocks_ReturnsSameCount()
    {
        var unlocks = SkillUnlock.GetRandomizedSkillUnlocks(new Random(42));
        AssertThat(unlocks.Count).IsEqual(TotalUnlocks);
    }

    [TestCase]
    public void GetRandomizedSkillUnlocks_ContainsSameSkillsAndRanges()
    {
        var ordered = SkillUnlock.GetSkillUnlocks();
        var shuffled = SkillUnlock.GetRandomizedSkillUnlocks(new Random(42));

        foreach (var range in SkillUnlock.standardRanges)
        {
            var orderedInRange = ordered.Where(u => u.levels.Start.Value == range.Start.Value)
                                        .Select(u => u.skill).OrderBy(s => s).ToList();
            var shuffledInRange = shuffled.Where(u => u.levels.Start.Value == range.Start.Value)
                                          .Select(u => u.skill).OrderBy(s => s).ToList();
            AssertThat(orderedInRange.SequenceEqual(shuffledInRange)).IsTrue();
        }
    }

    [TestCase]
    public void GetRandomizedSkillUnlocks_ShufflesWithinEachBracket()
    {
        // Two different seeds should produce different orderings within at least one bracket
        var shuffleA = SkillUnlock.GetRandomizedSkillUnlocks(new Random(1));
        var shuffleB = SkillUnlock.GetRandomizedSkillUnlocks(new Random(99999));

        var orderA = shuffleA.Select(u => u.skill.ToString()).ToList();
        var orderB = shuffleB.Select(u => u.skill.ToString()).ToList();

        AssertThat(orderA.SequenceEqual(orderB)).IsFalse();
    }

    [TestCase]
    public void GetRandomizedSkillUnlocks_TiersRemainsOrdered()
    {
        // Lower level ranges must all appear before higher level ranges
        var unlocks = SkillUnlock.GetRandomizedSkillUnlocks(new Random(42));

        int lastRangeIndex = -1;
        int currentRangeIndex = -1;

        foreach (var unlock in unlocks)
        {
            currentRangeIndex = SkillUnlock.standardRanges
                .FindIndex(r => r.Start.Value == unlock.levels.Start.Value);

            // Range index may stay the same (within same bracket) or advance
            AssertThat(currentRangeIndex).IsGreaterEqual(lastRangeIndex);

            if (currentRangeIndex > lastRangeIndex)
                lastRangeIndex = currentRangeIndex;
        }
    }

    // ── RequirementsMet ──────────────────────────────────────────────────────

    [TestCase]
    public void RequirementsMet_FirstRange_AlwaysTrue()
    {
        var unlock = new SkillUnlock(Skill.Agility, new Range(1, 10), false);
        AssertThat(unlock.RequirementsMet(new List<SkillUnlock>(), new List<QuestUnlock>())).IsTrue();
    }

    [TestCase]
    public void RequirementsMet_SecondRange_FalseWhenPreviousNotUnlocked()
    {
        var first = new SkillUnlock(Skill.Agility, new Range(1, 10), false);
        var second = new SkillUnlock(Skill.Agility, new Range(11, 20), false);

        AssertThat(second.RequirementsMet(new List<SkillUnlock> { first }, new List<QuestUnlock>())).IsFalse();
    }

    [TestCase]
    public void RequirementsMet_SecondRange_TrueWhenPreviousUnlocked()
    {
        var first = new SkillUnlock(Skill.Agility, new Range(1, 10), true);
        var second = new SkillUnlock(Skill.Agility, new Range(11, 20), false);

        AssertThat(second.RequirementsMet(new List<SkillUnlock> { first }, new List<QuestUnlock>())).IsTrue();
    }

    [TestCase]
    public void RequirementsMet_IgnoresOtherSkills()
    {
        // A Mining unlock should not satisfy Agility requirements
        var miningFirst = new SkillUnlock(Skill.Mining, new Range(1, 10), true);
        var agilitySecond = new SkillUnlock(Skill.Agility, new Range(11, 20), false);

        AssertThat(agilitySecond.RequirementsMet(new List<SkillUnlock> { miningFirst }, new List<QuestUnlock>())).IsFalse();
    }
}
