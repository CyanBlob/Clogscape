using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public partial class Tile : Node2D
{
    static Button TooltipButton;

    static bool ButtonLocked = false;

    public Unlockable unlockable;

    private Sprite2D icon;
    private Node2D lockIcon;
    private Label text;
    private Node2D background;

    public Vector2 gridPos;

    // TODO: Use a signal instead
    public TileGenerator tileGenerator;

    public override void _Ready()
    {
        TooltipButton = GetParent().GetChild<Button>(0);

        icon = (Sprite2D)FindChild("Icon");
        lockIcon = (Node2D)FindChild("Locks");

        text = (Label)FindChild("Label");
        background = (Node2D)FindChild("Background");

        if (text == null)
        {
            GD.Print($"Could not find label for {unlockable}");
        }
        else if (unlockable == null)
        {
            GD.Print($"Could not find unlocabke for tile at {Position}");
        }
        else
        {
            text.Text = unlockable.DifficultyToString();
        }
    }

    public override void _Process(double delta)
    {
    }

    public void _on_mouse_entered()
    {
        if (ButtonLocked)
        {
            return;
        }
        TooltipButton.Disabled = false;
        TooltipButton.Position = Position + new Vector2(25, -50);
        TooltipButton.Visible = true;
        TooltipButton.Icon = unlockable.GetTexture();
        TooltipButton.Text = unlockable.ToString();
        TooltipButton.ResetSize();
    }

    public void _on_mouse_exited()
    {
        if (ButtonLocked)
        {
            return;
        }
        TooltipButton.Disabled = true;
        TooltipButton.Visible = false;
    }

    public void _on_input_event(Node viewport, InputEvent inputEvent, int shape_idx)
    {
        if (inputEvent.IsPressed() && inputEvent is InputEventMouseButton mouseEvent && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            TooltipButton.MouseFilter = Control.MouseFilterEnum.Stop;

            // Ensure the button is placed properly before locking
            if (!ButtonLocked)
            {
                unlockable.Unlock();
                _on_mouse_entered();
                tileGenerator.UpdateState();
            }

            ButtonLocked = !ButtonLocked;

        }
    }

    public void _on_board_state_changed(Hashtable tiles)
    {
        if (unlockable.IsUnlocked())
        {
            icon.Modulate = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        }
        else
        {
            icon.Modulate = new Color(.5f, .5f, .5f, .5f);
        }

        lockIcon.Visible = !unlockable.IsUnlocked();

        // Locked tiles can't be hidden
        if (!unlockable.IsUnlocked())
        {
            var hidden = true;
            var leftTile = (Tile)tiles[new Vector2((int)gridPos.X - 1, (int)gridPos.Y)];
            var rightTile = (Tile)tiles[new Vector2((int)gridPos.X + 1, (int)gridPos.Y)];
            var upTile = (Tile)tiles[new Vector2((int)gridPos.X, (int)gridPos.Y - 1)];
            var downTile = (Tile)tiles[new Vector2((int)gridPos.X, (int)gridPos.Y + 1)];

            if (leftTile != null && leftTile.unlockable.IsUnlocked())
            {
                hidden = false;
            }
            else if (rightTile != null && rightTile.unlockable.IsUnlocked())
            {
                hidden = false;
            }
            else if (upTile != null && upTile.unlockable.IsUnlocked())
            {
                hidden = false;
            }
            else if (downTile != null && downTile.unlockable.IsUnlocked())
            {
                hidden = false;
            }

            icon.Visible = !hidden;
            text.Visible = !hidden;

            background.Modulate = hidden ? new Color(.5f, .5f, .5f, .75f) : new Color(1, 1, 1, 1);
        }
    }
}
