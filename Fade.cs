using Godot;
using System;

public partial class Fade : TextEdit
{
    [Export]
    public float fadeSpeed = 45f;

    public float visibility = 0.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (visibility < 0)
        {
            return;
        }

        visibility -= (float)(fadeSpeed * delta);
        var color = Modulate;
        color.A = visibility / 100f;
        Modulate = color;

    }
}
