namespace Bountyscape.Tests;

using GdUnit4;
using static GdUnit4.Assertions;
using System.Collections;
using System.Collections.Generic;

[TestSuite]
[RequireGodotRuntime]
public class GameStateTests
{
    private GameState state;

    private static Bounty MakeBounty(string name = "Test", Difficulty difficulty = Difficulty.Novice) =>
        new()
        {
            name = name,
            difficulty = difficulty,
            minKeys = 1,
            maxKeys = 2,
            keyChance = 1.0f,
            minGp = 0,
            maxGp = 1,
            maxLifetimeKeys = 10000,
            lifetimeClaimedKeys = 0,
            skipChance = 0,
            skipChancePerCompletion = 10,
            maxSkipChance = 100,
        };

    [BeforeTest]
    public void SetUp()
    {
        var bounties = new List<Bounty>
        {
            MakeBounty("A"), MakeBounty("B"), MakeBounty("C"),
            MakeBounty("D"), MakeBounty("E"), MakeBounty("F"),
        };

        state = new GameState
        {
            playerName = "TestPlayer",
            playerAllowance = 0,
            playerKeys = 0,
            completedBounties = new List<Bounty>(),
            currentBounties = new List<Bounty>(bounties),
            allBounties = bounties,
            hashedTiles = new Hashtable(),
            allowedDifficulties = new List<Difficulty>
            {
                Difficulty.Novice, Difficulty.Easy, Difficulty.Medium,
                Difficulty.Hard, Difficulty.Expert, Difficulty.Grandmaster
            },
        };

        GameManager.SetState(state);
    }

    // ── CompleteBounty ───────────────────────────────────────────────────────

    [TestCase]
    public void CompleteBounty_AddsToCompletedBounties()
    {
        var bounty = state.currentBounties[0];
        state.CompleteBounty(bounty);
        AssertThat(state.completedBounties.Contains(bounty)).IsTrue();
    }

    [TestCase]
    public void CompleteBounty_AwardsAtLeastMinKeys()
    {
        var bounty = state.currentBounties[0];
        int before = state.playerKeys;
        state.CompleteBounty(bounty);
        AssertThat(state.playerKeys).IsGreaterEqual(before + (bounty.minKeys ?? 0));
    }

    [TestCase]
    public void CompleteBounty_NeverExceedsMaxKeys()
    {
        var bounty = state.currentBounties[0];
        int before = state.playerKeys;
        state.CompleteBounty(bounty);
        AssertThat(state.playerKeys).IsLessEqual(before + (bounty.maxKeys ?? 1));
    }

    [TestCase]
    public void CompleteBounty_IncreasesSkipChance()
    {
        var bounty = MakeBounty("SkipTest");
        bounty.skipChance = 0;
        bounty.skipChancePerCompletion = 10;
        bounty.maxSkipChance = 100;
        state.allBounties.Add(bounty);
        state.currentBounties.Add(bounty);
        float before = bounty.skipChance ?? 0;
        state.CompleteBounty(bounty);
        AssertThat(bounty.skipChance ?? 0f).IsGreater(before);
    }

    [TestCase]
    public void CompleteBounty_DoesNotExceedMaxSkipChance()
    {
        var bounty = MakeBounty("CapTest");
        bounty.skipChance = 95;
        bounty.skipChancePerCompletion = 50;
        bounty.maxSkipChance = 100;
        state.allBounties.Add(bounty);
        state.currentBounties.Add(bounty);
        state.CompleteBounty(bounty);
        AssertThat(bounty.skipChance ?? 0f).IsLessEqual(bounty.maxSkipChance ?? 100f);
    }

    [TestCase]
    public void CompleteBounty_IncreasesAllowance()
    {
        var bounty = MakeBounty("Rich");
        bounty.minGp = 500;
        bounty.maxGp = 501;
        state.allBounties.Add(bounty);
        state.currentBounties.Add(bounty);

        int before = state.playerAllowance;
        state.CompleteBounty(bounty);
        AssertThat(state.playerAllowance).IsGreater(before);
    }

    [TestCase]
    public void CompleteBounty_IncrementsLifetimeClaimedKeys()
    {
        var bounty = state.currentBounties[0];
        int before = (int)(bounty.lifetimeClaimedKeys ?? 0);
        state.CompleteBounty(bounty);
        AssertThat(bounty.lifetimeClaimedKeys ?? 0).IsGreaterEqual(before + (bounty.minKeys ?? 0));
    }

    // ── GetPlayerDifficulty ──────────────────────────────────────────────────

    [TestCase]
    public void GetPlayerDifficulty_NewPlayer_ReturnsNovice()
    {
        AssertThat(state.GetPlayerDifficulty()).IsEqual(Difficulty.Novice);
    }

    [TestCase]
    public void GetPlayerDifficulty_NullCompletedBounties_ReturnsNovice()
    {
        state.completedBounties = null;
        AssertThat(state.GetPlayerDifficulty()).IsEqual(Difficulty.Novice);
        AssertThat(state.completedBounties).IsNotNull();
    }

    [TestCase]
    public void GetPlayerDifficulty_After20Novice_ReturnsEasy()
    {
        var novice = new Bounty { difficulty = Difficulty.Novice };
        for (int i = 0; i < 20; i++)
            state.completedBounties.Add(novice);

        AssertThat(state.GetPlayerDifficulty()).IsEqual(Difficulty.Easy);
    }

    [TestCase]
    public void GetPlayerDifficulty_After15Easy_ReturnsMedium()
    {
        for (int i = 0; i < 20; i++)
            state.completedBounties.Add(new Bounty { difficulty = Difficulty.Novice });
        for (int i = 0; i < 15; i++)
            state.completedBounties.Add(new Bounty { difficulty = Difficulty.Easy });

        AssertThat(state.GetPlayerDifficulty()).IsEqual(Difficulty.Medium);
    }

    [TestCase]
    public void GetPlayerDifficulty_After10Hard_ReturnsExpert()
    {
        for (int i = 0; i < 10; i++)
            state.completedBounties.Add(new Bounty { difficulty = Difficulty.Hard });

        AssertThat(state.GetPlayerDifficulty()).IsEqual(Difficulty.Expert);
    }

    [TestCase]
    public void GetPlayerDifficulty_After10Expert_ReturnsGrandmaster()
    {
        for (int i = 0; i < 10; i++)
            state.completedBounties.Add(new Bounty { difficulty = Difficulty.Expert });

        AssertThat(state.GetPlayerDifficulty()).IsEqual(Difficulty.Grandmaster);
    }
}
