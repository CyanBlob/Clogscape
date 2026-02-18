using Godot;
using System;
using System.Diagnostics;

public partial class Tile : Sprite2D
{
    static Button TooltipButton;

    static bool ButtonLocked = false;

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
        TooltipButton.Position = Position + new Vector2(50, -64);
        TooltipButton.Visible = true;
        Modulate = Color.Color8(128, 128, 128, 255);
    }

    public void _on_mouse_exited()
    {
        if (ButtonLocked)
        {
            return;
        }
        TooltipButton.Disabled = true;
        TooltipButton.Visible = false;
        Modulate = Color.Color8(255, 255, 255, 255);
    }

    public void _on_input_event(Node viewport, InputEvent inputEvent, int shape_idx)
    {
        if (inputEvent.IsPressed())
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
