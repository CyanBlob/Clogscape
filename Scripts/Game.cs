using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Range = System.Range;

public static class GameManager
{
    private static GameState state = new();

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

        state.hashedTiles = new();

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
        File.WriteAllLines($"{playerName}.json", lines);

        File.Delete($"{playerName}_tiles.json");

        foreach (Tile tile in state.hashedTiles.Values)
        {
            var tileJson = new List<String> {
                tile.Serialize()
            };
            File.AppendAllLines($"{playerName}_tiles.json", tileJson);
        }
    }

    public static bool Load(String player, TileGenerator tileGenerator)
    {
        if (!File.Exists($"{state.playerName}.json") || !File.Exists($"{state.playerName}_tiles.json")) {
            return false;
        }

        JsonSerializerOptions options = new();
        options.Converters.Add(new RangeSystemTextJsonConverter());

        var file = File.OpenRead($"{state.playerName}.json");
        byte[] bytes = new byte[file.Length];
        file.ReadExactly(bytes, 0, (int)file.Length);

        state = JsonSerializer.Deserialize<GameState>(bytes, options);

        state.hashedTiles = new();

        tileGenerator.ClearTiles();

        foreach (var line in File.ReadLines($"{state.playerName}_tiles.json"))
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

    public static void LoadFromFile(String filePath)
    {

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