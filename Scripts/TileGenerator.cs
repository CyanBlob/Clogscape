using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;

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
        for (int x = -GridSize + GridSize / 2; x < GridSize / 2; ++x)
        {
            for (int y = -GridSize + GridSize / 2; y < GridSize / 2; ++y)
            {
                var instance = TileScene.Instantiate<Sprite2D>();
                instance.Position = new Vector2(x * (TileSize + TileSpacing), y * (TileSize + TileSpacing));

                AddChild(instance);
            }
        }

        skillUnlocks.Add(new SkillUnlock(Skill.Prayer, SkillUnlock.standardRanges[0], true));
        skillUnlocks.Add(new SkillUnlock(Skill.Prayer, SkillUnlock.standardRanges[1], false));

        GD.Print(skillUnlocks[0].RequirementsMet(skillUnlocks));
        GD.Print(skillUnlocks[1].RequirementsMet(skillUnlocks));
    }

    public override void _Process(double delta)
    {
    }
}
