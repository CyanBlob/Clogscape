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
        state.playerName = playerName;

        String json = JsonSerializer.Serialize(state);

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

    public static void Load(String player, TileGenerator tileGenerator)
    {
        var file = File.OpenRead($"{state.playerName}.json");
        byte[] bytes = new byte[file.Length];
        file.ReadExactly(bytes, 0, (int)file.Length);

        state = JsonSerializer.Deserialize<GameState>(bytes);

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
                    SkillUnlock skill = JsonSerializer.Deserialize<SkillUnlock>(split[3]);
                    //state.hashedTiles.Add(new Vector2(skill.gridPosX, skill.gridPosY), skill);
                    tileGenerator.AddTileFromUnlock(skill);
                    break;
                case UnlockableType.Quest:
                    QuestUnlock quest = JsonSerializer.Deserialize<QuestUnlock>(split[3]);
                    //state.hashedTiles.Add(new Vector2(quest.gridPosX, quest.gridPosY), quest);
                    tileGenerator.AddTileFromUnlock(quest);
                    break;
                case UnlockableType.Diary:
                    DiaryUnlock diary = JsonSerializer.Deserialize<DiaryUnlock>(split[3]);
                    //state.hashedTiles.Add(new Vector2(diary.gridPosX, diary.gridPosY), diary);
                    tileGenerator.AddTileFromUnlock(diary);
                    break;
                case UnlockableType.Free:
                    FreeTile free = JsonSerializer.Deserialize<FreeTile>(split[3]);
                    //state.hashedTiles.Add(new Vector2(free.gridPosX, free.gridPosY), free);
                    tileGenerator.AddTileFromUnlock(free);
                    break;
            }
        }

        tileGenerator.UpdateState();
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