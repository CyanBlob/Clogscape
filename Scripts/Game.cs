using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Godot;
using Range = System.Range;

public static class GameManager
{
    private static GameState state = new();

    private static Random rand = new();

    [JsonIgnore]
    public static UI ui { get; set; }

    public static void SetState(GameState state)
    {
        GameManager.state = state;
    }

    public static GameState GetState()
    {
        state ??= GetDefaultState();

        state.hashedTiles ??= new();
        return state;
    }

    public static GameState GetDefaultState()
    {
        GameState state = new();

        state.allowedDifficulties = [Difficulty.Novice, Difficulty.Easy, Difficulty.Medium, Difficulty.Hard, Difficulty.Expert, Difficulty.Grandmaster];
        state.bountyDifficulty = Difficulty.Novice;
        state.playerAllowance = 0;

        state.allBounties = new();
        state.currentBounties = new();
        state.completedBounties = new();

        state.hashedTiles = new();

        LoadBounties("");

        return state;
    }

    public static void Save(String playerName, String suffix = "", bool saveTiles = true)
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new RangeSystemTextJsonConverter());

        state.playerName = playerName;

        String json = JsonSerializer.Serialize(state, options);

        var lines = new List<String>
        {
            json
        };
        File.WriteAllLines(PlayerFile(playerName + suffix), lines);

        if (saveTiles)
        {
            File.Delete(TilesFile(playerName));

            foreach (Tile tile in state.hashedTiles.Values)
            {
                var tileJson = new List<String> {
                tile.Serialize()
            };
                File.AppendAllLines(TilesFile(playerName + suffix), tileJson);
            }
        }

        if (state.allBounties.Count() < 3)
        {
            File.Delete(PossibleBountiesFile(playerName + suffix));
            LoadBounties(playerName);
        }

        String allBountiesJson = JsonSerializer.Serialize(state.allBounties, options);

        var allBountiesLines = new List<String>
        {
            allBountiesJson
        };
        File.WriteAllLines(PossibleBountiesFile(playerName + suffix), allBountiesLines);
    }

    public static bool Load(String player, TileGenerator tileGenerator)
    {
        GD.Print($"Loading player: {player}");
        LoadBounties(player);

        if (!File.Exists(PlayerFile(player)) || !File.Exists(TilesFile(player)))
        {
            return false;
        }

        JsonSerializerOptions options = new();
        options.Converters.Add(new RangeSystemTextJsonConverter());

        var file = File.OpenRead(PlayerFile(player));
        byte[] bytes = new byte[file.Length];
        file.ReadExactly(bytes, 0, (int)file.Length);

        state = JsonSerializer.Deserialize<GameState>(bytes, options);

        state.hashedTiles = new();

        tileGenerator.ClearTiles();

        foreach (var line in File.ReadLines(TilesFile(player)))
        {
            var split = line.Split("|");

            int x = Int32.Parse(split[0]);
            int y = Int32.Parse(split[1]);
            UnlockableType type = (UnlockableType)Int32.Parse(split[2]);

            switch (type)
            {
                case UnlockableType.Skill:
                    SkillUnlock skill = JsonSerializer.Deserialize<SkillUnlock>(split[3], options);
                    tileGenerator.AddTileFromUnlock(skill);
                    break;
                case UnlockableType.Quest:
                    QuestUnlock quest = JsonSerializer.Deserialize<QuestUnlock>(split[3], options);
                    tileGenerator.AddTileFromUnlock(quest);
                    break;
                case UnlockableType.Diary:
                    DiaryUnlock diary = JsonSerializer.Deserialize<DiaryUnlock>(split[3], options);
                    tileGenerator.AddTileFromUnlock(diary);
                    break;
                case UnlockableType.Free:
                    FreeTile free = JsonSerializer.Deserialize<FreeTile>(split[3], options);
                    tileGenerator.AddTileFromUnlock(free);
                    break;
            }
        }

        tileGenerator.UpdateState();

        return true;
    }

    public static void LoadBounties(String player)
    {

        String[] lines = [];
        JsonSerializerOptions options = new();
        options.Converters.Add(new RangeSystemTextJsonConverter());

        if (File.Exists(PossibleBountiesFile(player)))
        {
            lines = File.ReadAllLines(PossibleBountiesFile(player));
        }
        else
        {
            lines = File.ReadAllLines(DefaultPossibleBountiesFile());
        }

        var singleLine = String.Join(" ", lines);

        var combinedLines = new List<String>
        {
            singleLine
        };

        // Bounties should be on one line for now
        state.allBounties = JsonSerializer.Deserialize<List<Bounty>>(combinedLines[0], options);

        if (state.allBounties.Count < 3)
        {
            GD.Print("Could not load bounties");
        }
        else
        {
            GD.Print($"Loaded bounties");
        }
    }

    public static void LoadFromFile(String filePath)
    {

    }

    public static void UpdateAllowance(int gp)
    {
        state.playerAllowance += gp;
        ui.UpdateAllowance();
    }

    public static List<Bounty> UpdateBounties()
    {
        List<Bounty> bounties = new();

        var playerDifficulty = state.GetPlayerDifficulty();

        if (state.allBounties == null || state.allBounties.Count < 3)
        {
            LoadBounties(state.playerName);
        }

        var newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        while (rerollBounty(newBounty, playerDifficulty) == true)
        {
            newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        }

        bounties.Add(newBounty);

        newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        while (bounties.Contains(newBounty) || newBounty == null || rerollBounty(newBounty, playerDifficulty))
        {
            newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        }

        bounties.Add(newBounty);

        newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        while (bounties.Contains(newBounty) || newBounty == null || rerollBounty(newBounty, playerDifficulty))
        {
            newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        }

        bounties.Add(newBounty);

        state.currentBounties = bounties;

        return bounties;
    }

    public static bool rerollBounty(Bounty bounty, Difficulty playerDifficulty)
    {
        var randSingle = rand.NextSingle();
        var chance = bounty.skipChance / 100.0f;

        if (randSingle <= chance)
        {
            GD.Print($"Re-rolling {bounty.name}. {bounty.skipChance}%");
            return true;
        }

        if (bounty.lifetimeClaimedKeys >= bounty.maxLifetimeKeys)
        {
            GD.Print($"Re-rolling {bounty.name}. {bounty.lifetimeClaimedKeys}/{bounty.maxLifetimeKeys} keys");
            return true;
        }

        if (bounty.difficulty > playerDifficulty)
        {
            GD.Print($"Re-rolling {bounty.name}. {bounty.difficulty} > {playerDifficulty}");
            return true;
        }

        if (bounty.difficulty < playerDifficulty)
        {
            // As the player difficulty increases we should get fewer easy tasks
            bool skip = rand.Next((int)bounty.difficulty, (int)playerDifficulty) == (int)playerDifficulty;
            GD.Print($"Re-rolling {bounty.name}. {bounty.difficulty} < {playerDifficulty}");
            return skip;
        }

        return false;
    }

    public static String PlayerFile(String playerName)
    {
        CreateSavePath(playerName);
        return $"saves/{playerName}.json";
    }
    public static String TilesFile(String playerName)
    {
        CreateSavePath(playerName);
        return $"saves/{playerName}_tiles.json";
    }
    public static String PossibleBountiesFile(String playerName)
    {
        CreateSavePath(playerName);
        return $"saves/{playerName}_possible_bounties.json";
    }

    public static String DefaultPossibleBountiesFile()
    {
        return $"default_possible_bounties.json";
    }

    public static void CreateSavePath(String playerName)
    {
        Directory.CreateDirectory($"saves/");
    }
}

public class GameState
{
    public String playerName { get; set; }
    public int playerAllowance { get; set; }
    public int playerKeys { get; set; }

    public Difficulty bountyDifficulty { get; set; }
    public List<Difficulty> allowedDifficulties { get; set; }

    public List<Bounty> currentBounties { get; set; }
    public List<Bounty> completedBounties { get; set; }

    [JsonIgnore] // Serialized to a different file
    public List<Bounty> allBounties { get; set; }

    [JsonIgnore]
    public Hashtable hashedTiles { get; set; }

    [JsonIgnore]
    private static Random rand = new();

    public (int, int) CompleteBounty(Bounty bounty)
    {
        completedBounties.Add(bounty);
        if (bounty.skipChance < bounty.maxSkipChance)
        {
            bounty.skipChance += bounty.skipChancePerCompletion;
        }
        var bountyKeys = bounty.minKeys;

        for (int i = (int)bounty.minKeys; i < bounty.maxKeys; ++i)
        {
            if (bountyKeys + bounty.lifetimeClaimedKeys >= bounty.maxLifetimeKeys)
            {
                break;
            }
            bountyKeys += rand.NextDouble() <= bounty.keyChance ? 1 : 0;
        }

        playerKeys += (int)bountyKeys;

        bounty.lifetimeClaimedKeys += bountyKeys;

        var allowance = rand.Next((int)bounty.minGp, (int)bounty.maxGp);

        GameManager.UpdateAllowance(allowance);

        GameManager.UpdateBounties();

        return ((int)bountyKeys, allowance);
    }

    public Difficulty GetPlayerDifficulty()
    {
        var completedExpert = completedBounties.Where(p => { return p.difficulty == Difficulty.Expert; }).Count();
        var completedHard = completedBounties.Where(p => { return p.difficulty == Difficulty.Hard; }).Count();
        var completedMedium = completedBounties.Where(p => { return p.difficulty == Difficulty.Medium; }).Count();
        var completedEasy = completedBounties.Where(p => { return p.difficulty == Difficulty.Easy; }).Count();
        var completedNovice = completedBounties.Where(p => { return p.difficulty == Difficulty.Novice; }).Count();

        GD.Print($"{completedNovice}, {completedEasy}, {completedMedium}, {completedHard}, {completedExpert}");
        if (completedExpert >= 10)
        {
            GD.Print("Grandmaster");
            return Difficulty.Grandmaster;
        }
        if (completedHard >= 10)
        {
            GD.Print("Expert");
            return Difficulty.Expert;
        }
        if (completedMedium >= 15)
        {
            GD.Print("Hard");
            return Difficulty.Hard;
        }
        if (completedEasy >= 15)
        {
            GD.Print("Medium");
            return Difficulty.Medium;
        }
        if (completedNovice >= 20)
        {
            GD.Print("Easy");
            return Difficulty.Easy;
        }
        GD.Print("Novice");
        return Difficulty.Novice;
    }
}

// Required to correctly deserialize Ranges
public class RangeSystemTextJsonConverter : JsonConverter<System.Range>
{
    public override Range Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;
        var start = root.GetProperty("Start").GetProperty("Value").GetInt32();
        var end = root.GetProperty("End").GetProperty("Value").GetInt32();
        return new Range(start, end);
    }

    public override void Write(Utf8JsonWriter writer, Range value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteStartObject("Start");
        writer.WriteNumber("Value", value.Start.Value);
        writer.WriteBoolean("IsFromEnd", value.Start.IsFromEnd);
        writer.WriteEndObject();
        writer.WriteStartObject("End");
        writer.WriteNumber("Value", value.End.Value);
        writer.WriteBoolean("IsFromEnd", value.End.IsFromEnd);
        writer.WriteEndObject();
        writer.WriteEndObject();
    }
}