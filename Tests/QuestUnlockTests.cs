namespace Bountyscape.Tests;

using GdUnit4;
using static GdUnit4.Assertions;
using System;
using System.Collections.Generic;
using System.Linq;

[TestSuite]
[RequireGodotRuntime]
public class QuestUnlockTests
{
    [Before]
    public void SetUp() => GameManager.Log = _ => { };

    // ── GetQuests ────────────────────────────────────────────────────────────

    [TestCase]
    public void GetQuests_ReturnsNonEmptyList()
    {
        var quests = QuestUnlock.GetQuests();
        AssertThat(quests).IsNotNull();
        AssertThat(quests.Count).IsGreater(0);
    }

    [TestCase]
    public void GetQuests_ContainsExpectedNoviceQuests()
    {
        var quests = QuestUnlock.GetQuests();
        var names = quests.Select(q => q.name).ToHashSet();

        AssertThat(names.Contains("Druidic Ritual")).IsTrue();
        AssertThat(names.Contains("Rune Mysteries")).IsTrue();
        AssertThat(names.Contains("Pandemonium")).IsTrue();
        AssertThat(names.Contains("Cook's Assistant")).IsTrue();
    }

    [TestCase]
    public void GetQuests_ContainsGrandmasterQuests()
    {
        var quests = QuestUnlock.GetQuests();
        var names = quests.Select(q => q.name).ToHashSet();

        AssertThat(names.Contains("Dragon Slayer II")).IsTrue();
        AssertThat(names.Contains("Song of the Elves")).IsTrue();
        AssertThat(names.Contains("While Guthix Sleeps")).IsTrue();
    }

    [TestCase]
    public void GetQuests_BiohazardRequiresPlagueCity()
    {
        var quests = QuestUnlock.GetQuests();
        var biohazard = quests.First(q => q.name == "Biohazard");
        var plagueCity = quests.First(q => q.name == "Plague City");

        AssertThat(biohazard.questRequirements.Contains(plagueCity)).IsTrue();
    }

    [TestCase]
    public void GetQuests_JunglePotionRequiresDruidicRitual()
    {
        var quests = QuestUnlock.GetQuests();
        var junglePotion = quests.First(q => q.name == "Jungle Potion");
        var druidicRitual = quests.First(q => q.name == "Druidic Ritual");

        AssertThat(junglePotion.questRequirements.Contains(druidicRitual)).IsTrue();
    }

    [TestCase]
    public void GetQuests_AllDifficultiesRepresented()
    {
        var quests = QuestUnlock.GetQuests();

        AssertThat(quests.Any(q => q.questDifficulty == QuestDifficulty.Novice)).IsTrue();
        AssertThat(quests.Any(q => q.questDifficulty == QuestDifficulty.Intermediate)).IsTrue();
        AssertThat(quests.Any(q => q.questDifficulty == QuestDifficulty.Experienced)).IsTrue();
        AssertThat(quests.Any(q => q.questDifficulty == QuestDifficulty.Master)).IsTrue();
        AssertThat(quests.Any(q => q.questDifficulty == QuestDifficulty.Grandmaster)).IsTrue();
    }

    [TestCase]
    public void GetQuests_IdesOfMilkIsNovice()
    {
        // Previously a bug set DruidicRitual.questDifficulty instead of IdesOfMilk.questDifficulty
        var quests = QuestUnlock.GetQuests();
        var idesOfMilk = quests.First(q => q.name == "The Ides of Milk");
        AssertThat(idesOfMilk.questDifficulty).IsEqual(QuestDifficulty.Novice);
    }

    [TestCase]
    public void GetQuests_NoDuplicateNames()
    {
        var quests = QuestUnlock.GetQuests();
        var uniqueNames = quests.Select(q => q.name).Distinct().Count();
        AssertThat(uniqueNames).IsEqual(quests.Count);
    }

    // ── GetRandomizedQuests ──────────────────────────────────────────────────

    [TestCase]
    public void GetRandomizedQuests_ReturnsSameCount()
    {
        var all = QuestUnlock.GetQuests();
        var shuffled = QuestUnlock.GetRandomizedQuests(new Random(42));
        AssertThat(shuffled.Count).IsEqual(all.Count);
    }

    [TestCase]
    public void GetRandomizedQuests_PreservesDifficultyGroupOrder()
    {
        var shuffled = QuestUnlock.GetRandomizedQuests(new Random(42));

        // All Novice quests should appear before all Intermediate quests, etc.
        var difficulties = shuffled.Select(q => (int)q.questDifficulty).ToList();
        for (int i = 1; i < difficulties.Count; i++)
            AssertThat(difficulties[i]).IsGreaterEqual(difficulties[i - 1]);
    }

    [TestCase]
    public void GetRandomizedQuests_ShufflesWithinDifficultyGroups()
    {
        var ordered = QuestUnlock.GetQuests()
            .Where(q => q.questDifficulty == QuestDifficulty.Novice)
            .Select(q => q.name).ToList();

        var shuffledA = QuestUnlock.GetRandomizedQuests(new Random(1))
            .Where(q => q.questDifficulty == QuestDifficulty.Novice)
            .Select(q => q.name).ToList();

        var shuffledB = QuestUnlock.GetRandomizedQuests(new Random(9999))
            .Where(q => q.questDifficulty == QuestDifficulty.Novice)
            .Select(q => q.name).ToList();

        // At least one shuffled order should differ from the original
        AssertThat(shuffledA.SequenceEqual(ordered) && shuffledB.SequenceEqual(ordered)).IsFalse();
    }
}
