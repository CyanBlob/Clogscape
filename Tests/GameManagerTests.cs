namespace Bountyscape.Tests;

using GdUnit4;
using static GdUnit4.Assertions;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[TestSuite]
[RequireGodotRuntime]
public class GameManagerTests
{
    private const string TestPlayer = "_bountyscape_unit_test_";

    private static List<Bounty> MinimalBounties() =>
    [
        new() { name = "A", difficulty = Difficulty.Novice, minKeys = 0, maxKeys = 1, keyChance = 1f, minGp = 0, maxGp = 1, maxLifetimeKeys = 9999, lifetimeClaimedKeys = 0, skipChance = 0, skipChancePerCompletion = 0, maxSkipChance = 100 },
        new() { name = "B", difficulty = Difficulty.Novice, minKeys = 0, maxKeys = 1, keyChance = 1f, minGp = 0, maxGp = 1, maxLifetimeKeys = 9999, lifetimeClaimedKeys = 0, skipChance = 0, skipChancePerCompletion = 0, maxSkipChance = 100 },
        new() { name = "C", difficulty = Difficulty.Novice, minKeys = 0, maxKeys = 1, keyChance = 1f, minGp = 0, maxGp = 1, maxLifetimeKeys = 9999, lifetimeClaimedKeys = 0, skipChance = 0, skipChancePerCompletion = 0, maxSkipChance = 100 },
        new() { name = "D", difficulty = Difficulty.Novice, minKeys = 0, maxKeys = 1, keyChance = 1f, minGp = 0, maxGp = 1, maxLifetimeKeys = 9999, lifetimeClaimedKeys = 0, skipChance = 0, skipChancePerCompletion = 0, maxSkipChance = 100 },
        new() { name = "E", difficulty = Difficulty.Novice, minKeys = 0, maxKeys = 1, keyChance = 1f, minGp = 0, maxGp = 1, maxLifetimeKeys = 9999, lifetimeClaimedKeys = 0, skipChance = 0, skipChancePerCompletion = 0, maxSkipChance = 100 },
        new() { name = "F", difficulty = Difficulty.Novice, minKeys = 0, maxKeys = 1, keyChance = 1f, minGp = 0, maxGp = 1, maxLifetimeKeys = 9999, lifetimeClaimedKeys = 0, skipChance = 0, skipChancePerCompletion = 0, maxSkipChance = 100 },
    ];

    private GameState FreshState()
    {
        var bounties = MinimalBounties();
        return new GameState
        {
            playerName = TestPlayer,
            playerKeys = 0,
            playerAllowance = 0,
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
    }

    [BeforeTest]
    public void SetUp() => GameManager.SetState(FreshState());

    [AfterTest]
    public void TearDown()
    {
        var playerFile = GameManager.PlayerFile(TestPlayer);
        var tilesFile = GameManager.TilesFile(TestPlayer);
        var bountiesFile = GameManager.PossibleBountiesFile(TestPlayer);

        if (File.Exists(playerFile)) File.Delete(playerFile);
        if (File.Exists(tilesFile)) File.Delete(tilesFile);
        if (File.Exists(bountiesFile)) File.Delete(bountiesFile);

        GameManager.SetState(FreshState());
    }

    // ── GetState / SetState ──────────────────────────────────────────────────

    [TestCase]
    public void GetState_NeverReturnsNull()
    {
        AssertThat(GameManager.GetState()).IsNotNull();
    }

    [TestCase]
    public void GetState_InitializesHashedTiles()
    {
        AssertThat(GameManager.GetState().hashedTiles).IsNotNull();
    }

    [TestCase]
    public void SetState_ReplacesExistingState()
    {
        var newState = FreshState();
        newState.playerName = "NewPlayer";
        GameManager.SetState(newState);
        AssertThat(GameManager.GetState().playerName).IsEqual("NewPlayer");
    }

    // ── GetDefaultState ──────────────────────────────────────────────────────

    [TestCase]
    public void GetDefaultState_HasNonNullLists()
    {
        var state = GameManager.GetDefaultState();
        AssertThat(state.completedBounties).IsNotNull();
        AssertThat(state.currentBounties).IsNotNull();
        AssertThat(state.hashedTiles).IsNotNull();
        AssertThat(state.allowedDifficulties).IsNotNull();
    }

    [TestCase]
    public void GetDefaultState_CompletedBountiesIsEmpty()
    {
        AssertThat(GameManager.GetDefaultState().completedBounties.Count).IsEqual(0);
    }

    [TestCase]
    public void GetDefaultState_PlayerKeysIsZero()
    {
        AssertThat(GameManager.GetDefaultState().playerKeys).IsEqual(0);
    }

    // ── UpdateBounties ───────────────────────────────────────────────────────

    [TestCase]
    public void UpdateBounties_NullCompletedBounties_DoesNotThrow()
    {
        GameManager.GetState().completedBounties = null;
        AssertThat(GameManager.UpdateBounties).IsNotNull(); // just ensure it runs
        GameManager.UpdateBounties();
        AssertThat(GameManager.GetState().completedBounties).IsNotNull();
    }

    [TestCase]
    public void UpdateBounties_SetsThreeCurrentBounties()
    {
        GameManager.UpdateBounties();
        AssertThat(GameManager.GetState().currentBounties.Count).IsEqual(3);
    }

    [TestCase]
    public void UpdateBounties_CurrentBountiesAreDistinct()
    {
        GameManager.UpdateBounties();
        var bounties = GameManager.GetState().currentBounties;
        var distinct = new HashSet<Bounty>(bounties);
        AssertThat(distinct.Count).IsEqual(3);
    }

    // ── Load (missing files) ─────────────────────────────────────────────────

    [TestCase]
    public void Load_ReturnsFalse_WhenFilesDoNotExist()
    {
        var result = GameManager.Load("_player_that_does_not_exist_xyzabc_", AutoFree(new StubTileGenerator()));
        AssertThat(result).IsFalse();
    }

    // ── Save / Load round-trip ───────────────────────────────────────────────

    [TestCase]
    public void SaveLoad_PreservesPlayerName()
    {
        GameManager.GetState().playerName = TestPlayer;
        GameManager.Save(TestPlayer);

        GameManager.SetState(FreshState());
        GameManager.GetState().playerName = "";

        GameManager.Load(TestPlayer, AutoFree(new StubTileGenerator()));
        AssertThat(GameManager.GetState().playerName).IsEqual(TestPlayer);
    }

    [TestCase]
    public void SaveLoad_PreservesPlayerKeys()
    {
        GameManager.GetState().playerKeys = 7;
        GameManager.Save(TestPlayer);

        GameManager.SetState(FreshState());
        GameManager.Load(TestPlayer, AutoFree(new StubTileGenerator()));

        AssertThat(GameManager.GetState().playerKeys).IsEqual(7);
    }

    [TestCase]
    public void SaveLoad_PreservesPlayerAllowance()
    {
        GameManager.GetState().playerAllowance = 12345;
        GameManager.Save(TestPlayer);

        GameManager.SetState(FreshState());
        GameManager.Load(TestPlayer, AutoFree(new StubTileGenerator()));

        AssertThat(GameManager.GetState().playerAllowance).IsEqual(12345);
    }

    [TestCase]
    public void SaveLoad_PreservesCompletedBountyCount()
    {
        GameManager.GetState().completedBounties.Add(new Bounty { name = "Done", difficulty = Difficulty.Novice });
        GameManager.GetState().completedBounties.Add(new Bounty { name = "Done2", difficulty = Difficulty.Easy });
        GameManager.Save(TestPlayer);

        GameManager.SetState(FreshState());
        GameManager.Load(TestPlayer, AutoFree(new StubTileGenerator()));

        AssertThat(GameManager.GetState().completedBounties.Count).IsEqual(2);
    }

    [TestCase]
    public void Save_CreatesPlayerFile()
    {
        GameManager.Save(TestPlayer);
        AssertThat(File.Exists(GameManager.PlayerFile(TestPlayer))).IsTrue();
    }

    [TestCase]
    public void Save_CreatesTilesFile()
    {
        GameManager.Save(TestPlayer);
        AssertThat(File.Exists(GameManager.TilesFile(TestPlayer))).IsTrue();
    }
}

/// <summary>Minimal TileGenerator stand-in for load tests — no scene needed.</summary>
public partial class StubTileGenerator : TileGenerator
{
    public override void ClearTiles()
    {
        GameManager.GetState().hashedTiles = new Hashtable();
    }

    // Suppress the real implementations: AddTileFromUnlock calls TileScene.Instantiate()
    // where TileScene is null (never assigned via editor export), which crashes Godot.
    public override void AddTileFromUnlock(Unlockable unlockable) { }

    // UpdateState would busy-loop on IsNodeReady() without a scene tree.
    public override void UpdateState() { }
}
