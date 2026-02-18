using Godot;
using System;
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

    public override void _Ready()
    {
        for (int x = -GridSize + GridSize / 2; x < GridSize / 2; ++x)
        {
            for (int y = -GridSize + GridSize / 2; y < GridSize / 2; ++y)
            {
                var instance = TileScene.Instantiate<Node2D>();
                instance.Position = new Vector2(x * 128, y * 128);
                AddChild(instance);
            }
        }
    }

    public override void _Process(double delta)
    {
    }
}
