using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public List<SkillUnlock> skillUnlocks = new();

    private Random rand = new();

    public override void _Ready()
    {
        var allSkillUnlocks = SkillUnlock.GetSkillUnlocks();
        var allQuestUnlocks = QuestUnlock.GetQuests();
        var allDiaryUnlocks = DiaryUnlock.GetDiaries();

        GD.Print($"{allSkillUnlocks.Count} skills, {allQuestUnlocks.Count} quests, {allDiaryUnlocks.Count} diaries, {allSkillUnlocks.Count + allQuestUnlocks.Count + allDiaryUnlocks.Count} total unlocks");

        List<Unlockable> allUnlocks = GetInterleavedUnlockes(allSkillUnlocks, allQuestUnlocks, allDiaryUnlocks);
        //allUnlocks.AddRange(allSkillUnlocks);
        //allUnlocks.AddRange(allQuestUnlocks);
        //allUnlocks.AddRange(allDiaryUnlocks);



        /*int GridSizeX = (int)Math.Ceiling(Math.Sqrt(allUnlocks.Count));
        int GridSizeY = (int)Math.Ceiling(Math.Sqrt(allUnlocks.Count));

        for (int x = -GridSizeX + GridSizeX / 2; x < GridSizeX / 2; ++x)
        {
            for (int y = -GridSizeY + GridSizeY / 2; y < GridSizeY / 2; ++y)
            {

                if (allUnlocks.Count == 0)
                {
                    break;
                }
                var instance = TileScene.Instantiate<Sprite2D>();
                instance.Position = new Vector2(x * (TileSize + TileSpacing), y * (TileSize + TileSpacing));

                Tile tile = (Tile)instance;

                tile.unlockable = GetAndPopUnlockable(allUnlocks);

                tile.Texture = tile.unlockable.GetTexture();

                //GD.Print(tile.unlockable);

                AddChild(instance);
            }
        }*/

        // Generate larger and larger overlapping grids until we run out of tiles, skipping placed tiles.
        // This ensures we place lower tier unlocks towards the center.

        HashSet<Vector2> placedTiles = new();

        for (int squareSize = 1; squareSize < 100; squareSize += 2)
        {
            for (var x = -squareSize + squareSize / 2; x < squareSize / 2; x++)
            {
                for (var y = -squareSize + squareSize / 2; y < squareSize / 2; y++)
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

                    var instance = TileScene.Instantiate<Sprite2D>();
                    instance.Position = new Vector2(x * (TileSize + TileSpacing), y * (TileSize + TileSpacing));

                    Tile tile = (Tile)instance;

                    tile.unlockable = GetAndPopUnlockable(allUnlocks);

                    tile.Texture = tile.unlockable.GetTexture();

                    //GD.Print(tile.unlockable);

                    AddChild(instance);
                }
            }
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

        foreach (var diary in diaryUnlocks)
        {
            GD.Print(diary);
        }

        var mixed = skillUnlocks.Interleave<Unlockable>(questUnlocks);
        mixed = mixed.Interleave<Unlockable>(diaryUnlocks);

        //var valid = mixed.TakeWhile(e => {return e != null;}).ToList<Unlockable>();
        var valid = mixed.Where(e => { return e != null; }).ToList<Unlockable>();

        unlockables.AddRange(valid);

        return unlockables;
    }

    public Unlockable GetAndPopUnlockable(List<Unlockable> allUnlockables)
    {
        List<Range> windows = [0..12, 0..12, 0..24, 0..36];

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
