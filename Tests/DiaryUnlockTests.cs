namespace Bountyscape.Tests;

using GdUnit4;
using static GdUnit4.Assertions;
using System;
using System.Linq;

[TestSuite]
[RequireGodotRuntime]
public class DiaryUnlockTests
{
    [Before]
    public void SetUp() => GameManager.Log = _ => { };

    // 4 difficulties × 12 regions = 48
    private const int DiaryCount = 48;

    private static readonly string[] ExpectedRegions =
    [
        "Ardougne", "Desert", "Falador", "Fremennik Province",
        "Kandarin", "Karamja", "Kourend & Kebos", "Lumbridge & Draynore",
        "Morytania", "Varrock", "Western Provinces", "Wilderness"
    ];

    [TestCase]
    public void GetDiaries_ReturnsCorrectCount()
    {
        AssertThat(DiaryUnlock.GetDiaries().Count).IsEqual(DiaryCount);
    }

    [TestCase]
    public void GetDiaries_AllRegionsPresent()
    {
        var diaries = DiaryUnlock.GetDiaries();
        foreach (var region in ExpectedRegions)
            AssertThat(diaries.Any(d => d.name == region)).IsTrue();
    }

    [TestCase]
    public void GetDiaries_AllDifficultiesPresent()
    {
        var diaries = DiaryUnlock.GetDiaries();
        foreach (DiaryDifficulty diff in Enum.GetValues(typeof(DiaryDifficulty)))
            AssertThat(diaries.Any(d => d.diaryDifficulty == diff)).IsTrue();
    }

    [TestCase]
    public void GetDiaries_EachRegionHasAllDifficulties()
    {
        var diaries = DiaryUnlock.GetDiaries();
        foreach (var region in ExpectedRegions)
        {
            foreach (DiaryDifficulty diff in Enum.GetValues(typeof(DiaryDifficulty)))
            {
                var exists = diaries.Any(d => d.name == region && d.diaryDifficulty == diff);
                AssertThat(exists).IsTrue();
            }
        }
    }

    [TestCase]
    public void GetDiaries_AllStartAsLocked()
    {
        AssertThat(DiaryUnlock.GetDiaries().All(d => !d.IsUnlocked())).IsTrue();
    }
}
