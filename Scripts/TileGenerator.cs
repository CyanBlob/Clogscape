using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using Range = System.Range;

public partial class TileGenerator : Node
{
    [Export]
    PackedScene TileScene;

    [Export]
    int TileSize = 128;

    [Export]
    int TileSpacing = 0;

    private Random rand { get; set; }

    public void ClearTiles()
    {
        foreach (var child in GetChildren())
        {
            if (child is Tile)
            {
                child.QueueFree();
            }
        }
    }

    public void AddTileFromUnlock(Unlockable unlockable)
    {
        int x = unlockable.gridPosX;
        int y = unlockable.gridPosY;

        var instance = TileScene.Instantiate<Node2D>();
        instance.Position = new Vector2(x * (TileSize + TileSpacing), y * (TileSize + TileSpacing));

        Tile tile = (Tile)instance;
        tile.gridPos = new Vector2(x, y);
        GameManager.GetState().hashedTiles.Add(new Vector2(x, y), tile);

        tile.tileGenerator = this;

        tile.unlockable = unlockable;

        // These are duplicated in the unlockable for easier serialization
        tile.unlockable.gridPosX = x;
        tile.unlockable.gridPosY = y;

        //tile.Texture = tile.unlockable.GetTexture();
        var sprite = (Sprite2D)tile.FindChild("Icon");
        sprite.Texture = tile.unlockable.GetTexture();
        sprite.Visible = true;

        AddChild(instance);
    }


    public override void _Ready()
    {
        GameManager.GetState().hashedTiles = new();
        rand = new();

        var allSkillUnlocks = SkillUnlock.GetSkillUnlocks();
        var allQuestUnlocks = QuestUnlock.GetQuests();
        var allDiaryUnlocks = DiaryUnlock.GetDiaries();

        GD.Print($"{allSkillUnlocks.Count} skills, {allQuestUnlocks.Count} quests, {allDiaryUnlocks.Count} diaries, {allSkillUnlocks.Count + allQuestUnlocks.Count + allDiaryUnlocks.Count} total unlocks");

        List<Unlockable> allUnlocks = GetInterleavedUnlockes(allSkillUnlocks, allQuestUnlocks, allDiaryUnlocks);

        // Generate larger and larger overlapping grids until we run out of tiles, skipping placed tiles.
        // This ensures we place lower tier unlocks towards the center.

        ClearTiles();

        HashSet<Vector2> placedTiles = new();

        for (int squareSize = 1; squareSize < 100; squareSize += 2)
        {
            for (var x = (squareSize - 1) / -2; x <= squareSize / 2; ++x)
            {
                for (var y = (squareSize - 1) / -2; y <= squareSize / 2; ++y)
                {
                    if (allUnlocks.Count == 0) // Out of tiles!
                    {
                        break;
                    }

                    if (placedTiles.Contains(new Vector2(x, y))) // Skip filled square
                    {
                        continue;
                    }

                    placedTiles.Add(new Vector2(x, y));

                    var instance = TileScene.Instantiate<Node2D>();
                    instance.Position = new Vector2(x * (TileSize + TileSpacing), y * (TileSize + TileSpacing));

                    Tile tile = (Tile)instance;
                    tile.gridPos = new Vector2(x, y);
                    GameManager.GetState().hashedTiles.Add(new Vector2(x, y), tile);

                    tile.tileGenerator = this;

                    // First tile is free
                    tile.unlockable = GetAndPopUnlockable(allUnlocks, squareSize == 1);

                    // These are duplicated in the unlockable for easier serialization
                    tile.unlockable.gridPosX = x;
                    tile.unlockable.gridPosY = y;

                    //tile.Texture = tile.unlockable.GetTexture();
                    var sprite = (Sprite2D)tile.FindChild("Icon");
                    sprite.Texture = tile.unlockable.GetTexture();

                    AddChild(instance);
                }
            }
        }

        UpdateState();

    }

    public void UpdateState()
    {
        foreach (Tile tile in GameManager.GetState().hashedTiles.Values)
        {
            tile._on_board_state_changed(GameManager.GetState().hashedTiles);
        }
    }

    public List<Unlockable> GetInterleavedUnlockes(List<SkillUnlock> skillUnlocks, List<QuestUnlock> questUnlocks, List<DiaryUnlock> diaryUnlocks)
    {
        List<Unlockable> unlockables = new();

        // All arrays should be padded to the length of the largest list
        var maxLength = Math.Max(skillUnlocks.Count, Math.Max(questUnlocks.Count, diaryUnlocks.Count));

        while (skillUnlocks.Count < maxLength)
        {
            skillUnlocks.Insert(rand.Next(0, skillUnlocks.Count), null);
        }
        while (questUnlocks.Count < maxLength)
        {
            questUnlocks.Insert(rand.Next(0, questUnlocks.Count), null);
        }
        while (diaryUnlocks.Count < maxLength * 2)
        {
            diaryUnlocks.Insert(rand.Next(0, diaryUnlocks.Count), null);
        }

        var mixed = skillUnlocks.Interleave<Unlockable>(questUnlocks);
        mixed = mixed.Interleave<Unlockable>(diaryUnlocks);

        //var valid = mixed.TakeWhile(e => {return e != null;}).ToList<Unlockable>();
        var valid = mixed.Where(e => { return e != null; }).ToList<Unlockable>();

        unlockables.AddRange(valid);

        return unlockables;
    }

    public Unlockable GetAndPopUnlockable(List<Unlockable> allUnlockables, bool firstTile)
    {
        List<Range> windows = [0..12, 0..24, 0..24, 0..36, 0..48, 0..96, 0..128];

        if (firstTile)
        {
            return new FreeTile();
        }

        Range randomWindow = windows[rand.Next(0, windows.Count)];

        var endValue = randomWindow.End.Value <= allUnlockables.Count ? randomWindow.End.Value : allUnlockables.Count;

        int unlockIndex = rand.Next(randomWindow.Start.Value, endValue);

        var unlockable = allUnlockables[unlockIndex];
        allUnlockables.RemoveAt(unlockIndex);

        return unlockable;
    }

    public override void _Process(double delta)
    {
    }
}

public static class EnumerableExtensions
{

    public static IEnumerable<TSource> Interleave<TSource>(this IEnumerable<TSource> source1, IEnumerable<TSource> source2)
    {

        if (source1 == null) { throw new ArgumentNullException(nameof(source1)); }
        if (source2 == null) { throw new ArgumentNullException(nameof(source2)); }

        using (var enumerator1 = source1.GetEnumerator())
        {
            using (var enumerator2 = source2.GetEnumerator())
            {

                var continue1 = true;
                var continue2 = true;

                do
                {

                    if (continue1 = enumerator1.MoveNext())
                    {
                        yield return enumerator1.Current;
                    }

                    if (continue2 = enumerator2.MoveNext())
                    {
                        yield return enumerator2.Current;
                    }

                }
                while (continue1 || continue2);

            }
        }

    }

}
