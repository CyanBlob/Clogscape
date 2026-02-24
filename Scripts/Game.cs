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
using Godot;
using Range = System.Range;

public static class GameManager
{
    private static GameState state = new();

    private static Random rand = new();

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
        state.playerAllowance = 100;

        state.skill = new SkillUnlock(Skill.Agility, new System.Range(69, 420), false);
        state.quest = new QuestUnlock("TEST");
        state.diary = new DiaryUnlock("DIARY", DiaryDifficulty.Medium);

        state.allBounties = new();
        state.currentBounties = new();
        state.completedBounties = new();

        state.hashedTiles = new();

        var bounty = new Bounty();
        bounty.name = "Test bounty";
        bounty.imagePath = "img";
        bounty.keyChance = 100f;
        bounty.minKeys = 1;
        bounty.maxKeys = 1;
        bounty.minGp = 100;
        bounty.maxGp = 1000;
        bounty.difficulty = Difficulty.Novice;
        bounty.skipChance = 0f;
        bounty.description = "A bounty";
        bounty.help = "Just do it";
        bounty.isWildy = false;
        bounty.requirementLocked = false;
        bounty.completedLocked = false;

        state.allBounties.Add(bounty);

        return state;
    }

    public static void Save(String playerName)
    {
        JsonSerializerOptions options = new();
        options.Converters.Add(new RangeSystemTextJsonConverter());

        state.playerName = playerName;

        String json = JsonSerializer.Serialize(state, options);

        var lines = new List<String>
        {
            json
        };
        File.WriteAllLines(PlayerFile(playerName), lines);

        File.Delete(TilesFile(playerName));

        foreach (Tile tile in state.hashedTiles.Values)
        {
            var tileJson = new List<String> {
                tile.Serialize()
            };
            File.AppendAllLines(TilesFile(playerName), tileJson);
        }

        String allBountiesJson = JsonSerializer.Serialize(state.allBounties, options);

        var allBountiesLines = new List<String>
        {
            allBountiesJson
        };
        File.WriteAllLines(PossibleBountiesFile(playerName), allBountiesLines);
    }

    public static bool Load(String player, TileGenerator tileGenerator)
    {
        if (!File.Exists(PlayerFile(player)) || !File.Exists(TilesFile(player)))
        {
            return false;
        }
        GD.Print($"Loading {player}");

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

        String[] lines = [];
        if (File.Exists(PossibleBountiesFile(player)))
        {
            lines = File.ReadAllLines(PossibleBountiesFile(player));
        }
        else
        {
            lines = File.ReadAllLines(DefaultPossibleBountiesFile());
        }

        var singleLine = String.Join(" ", lines);

        GD.Print(singleLine);

        var combinedLines = new List<String>
        {
            singleLine
        };

        // Bounties should be on one line for now
        state.allBounties = JsonSerializer.Deserialize<List<Bounty>>(combinedLines[0], options);

        return true;
    }

    public static void LoadFromFile(String filePath)
    {

    }

    public static void UpdateAllowance(int gp)
    {
        state.playerAllowance += gp;
    }

    public static List<Bounty> UpdateBounties()
    {
        List<Bounty> bounties = new();

        var newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));

        bounties.Add(newBounty);

        newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        while (bounties.Contains(newBounty))
        {
            newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        }

        bounties.Add(newBounty);

        newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        while (bounties.Contains(newBounty))
        {
            newBounty = state.allBounties.ElementAt(rand.Next(0, state.allBounties.Count));
        }

        bounties.Add(newBounty);

        state.currentBounties = bounties;

        return bounties;
    }

    public static String PlayerFile(String playerName)
    {
        return $"{playerName}.json";
    }
    public static String TilesFile(String playerName)
    {
        return $"{playerName}_tiles.json";
    }
    public static String PossibleBountiesFile(String playerName)
    {
        return $"{playerName}_possible_bounties.json";
    }

    public static String DefaultPossibleBountiesFile()
    {
        return $"default_possible_bounties.json";
    }
}

public class GameState
{
    public String playerName { get; set; }
    public int playerAllowance { get; set; }

    public Difficulty bountyDifficulty { get; set; }
    public List<Difficulty> allowedDifficulties { get; set; }

    public SkillUnlock skill { get; set; }
    public QuestUnlock quest { get; set; }
    public DiaryUnlock diary { get; set; }

    public List<Bounty> currentBounties { get; set; }
    public List<Bounty> completedBounties { get; set; }

    [JsonIgnore] // Serialized to a different file
    public List<Bounty> allBounties { get; set; }

    [JsonIgnore]
    public Hashtable hashedTiles { get; set; }

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