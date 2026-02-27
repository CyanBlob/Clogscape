using Godot;
using System;

public partial class Fade : TextEdit
{
    [Export]
    public float fadeSpeed = 45f;

    private AudioStreamPlayer audioPlayer;

    public float visibility = 0.0f;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
    {
        audioPlayer = (AudioStreamPlayer)GetChild(0);
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
    {
        if (visibility < 0)
        {
            return;
        }
        else if (visibility == 100.0f)
        {
            audioPlayer.Play();
        }

        visibility -= (float)(fadeSpeed * delta);
        var color = Modulate;
        color.A = visibility / 100f;
        Modulate = color;

    }
}
