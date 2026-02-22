using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

public partial class TileGenerator : Node
{
    [Export]
    PackedScene TileScene;

    [Export]
    int TileSize = 128;

    [Export]
    int TileSpacing = 0;

    [Export]
    int GridSize = 10;

    public List<SkillUnlock> skillUnlocks = new();

    public override void _Ready()
    {
        var allSkillUnlocks = SkillUnlock.GetSkillUnlocks();
        var allQuestUnlocks = QuestUnlock.GetQuests();
        List<Unlockable> allUnlocks = new();
        allUnlocks.AddRange(allSkillUnlocks);
        allUnlocks.AddRange(allQuestUnlocks);

        GD.Print($"{allSkillUnlocks.Count} skills, {allQuestUnlocks.Count} quests, {allSkillUnlocks.Count + allQuestUnlocks.Count} total unlocks");

        var rand = new Random();

        for (int x = -GridSize + GridSize / 2; x < GridSize / 2; ++x)
        {
            for (int y = -GridSize + GridSize / 2; y < GridSize / 2; ++y)
            {
                var instance = TileScene.Instantiate<Sprite2D>();
                instance.Position = new Vector2(x * (TileSize + TileSpacing), y * (TileSize + TileSpacing));

                Tile tile = (Tile)instance;

                int unlockIndex = rand.Next(0, allUnlocks.Count);
                tile.unlockable = allUnlocks[unlockIndex];
                allUnlocks.RemoveAt(unlockIndex);

                tile.Texture = tile.unlockable.GetTexture();

                GD.Print(tile.unlockable);

                AddChild(instance);
            }
        }




    }

    public override void _Process(double delta)
    {
    }
}
