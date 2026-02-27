using Godot;
using System;
using System.Numerics;
using Vector2 = Godot.Vector2;

public partial class Camera : Camera2D
{
    [Export]
    public float ZoomSpeed = .05f;
    [Export]
    public float MinZoom = .35f;

    public float ZoomTarget = 1f;

    [Export]
    public float ZoomLerpFactor = 12f;

    [Export]
    public float PanSpeed = 1f;
    public Vector2 PanTarget = Vector2.Zero;

    [Export]
    public float PanLerpFactor = 4f;

	public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("zoom_in"))
        {
            ZoomTarget += ZoomSpeed;
        }

        if (Input.IsActionJustPressed("zoom_out") && ZoomTarget - ZoomSpeed > 0.0f)
        {
            ZoomTarget -= ZoomSpeed;

            if (ZoomTarget < MinZoom)
            {
                ZoomTarget = MinZoom;
            }
        }

        Zoom = Zoom.Lerp(new Vector2(ZoomTarget, ZoomTarget), (float)delta * ZoomLerpFactor);

        if (Input.IsActionPressed("pan_left"))
        {
            PanTarget.X -= PanSpeed;
        }
        else if (Input.IsActionPressed("pan_right"))
        {
            PanTarget.X += PanSpeed;
        }
        if (Input.IsActionPressed("pan_down"))
        {
            PanTarget.Y += PanSpeed;
        }
        else if (Input.IsActionPressed("pan_up"))
        {
            PanTarget.Y -= PanSpeed;
        }

        Position = Position.Lerp(PanTarget, (float)delta * PanLerpFactor);
    }
}
