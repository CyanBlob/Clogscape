using Godot;
using System;
using System.Diagnostics;

public partial class Tile : Sprite2D
{
    static Button TooltipButton;

    static bool ButtonLocked = false;

    public Unlockable unlockable;

    public override void _Ready()
    {
        TooltipButton = GetParent().GetChild<Button>(0);
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
            GD.Print($"Input event: {inputEvent}");
            TooltipButton.MouseFilter = Control.MouseFilterEnum.Stop;

            // Ensure the button is placed properly before locking
            if (!ButtonLocked)
            {
                _on_mouse_entered();
            }

            ButtonLocked = !ButtonLocked;

        }
    }
}
