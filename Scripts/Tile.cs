using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text.Json;

public partial class Tile : Node2D
{
    static Button TooltipButton;
    static Button UnlockButton;
    static Button ClaimButton;
    static Control UnlockContainer;
    static Control ClaimContainer;

#nullable enable
    static Tile? ButtonLocked = null;
#nullable disable

    public Unlockable unlockable;

    private Sprite2D icon;
    private Node2D lockIcon;
    private Node2D checkIcon;
    private Label text;
    private Node2D background;

    public Vector2 gridPos;

    public bool hidden = true;

    public FogRect fogRect;

    // TODO: Use a signal instead
    public TileGenerator tileGenerator;

    private AudioStreamPlayer selectAudioPlayer;
    private AudioStreamPlayer unlockAudioPlayer;
    private AudioStreamPlayer claimAudioPlayer;
    private AudioStreamPlayer errorAudioPlayer;

    public override void _Ready()
    {
        selectAudioPlayer = (AudioStreamPlayer)GetChild(0);
        unlockAudioPlayer = (AudioStreamPlayer)GetChild(1);
        claimAudioPlayer = (AudioStreamPlayer)GetChild(2);
        errorAudioPlayer = (AudioStreamPlayer)GetChild(3);

        fogRect = (FogRect)GetParent().GetParent().FindChild("FogRect");

        TooltipButton = GetParent().GetChild<Button>(0);
        UnlockContainer = (Control)TooltipButton.FindChild("UnlockContainer");
        ClaimContainer = (Control)TooltipButton.FindChild("ClaimContainer");

        UnlockButton = UnlockContainer.GetChild(0).GetChild<Button>(0);
        ClaimButton = ClaimContainer.GetChild(0).GetChild<Button>(0);

        icon = (Sprite2D)FindChild("Icon");
        lockIcon = (Node2D)FindChild("Locks");
        checkIcon = (Node2D)FindChild("Checks");

        text = (Label)FindChild("Label");
        background = (Node2D)FindChild("Background");

        if (text == null)
        {
            GD.Print($"Could not find label for {unlockable}");
        }
        else if (unlockable == null)
        {
            GD.Print($"Could not find unlockable for tile at {Position}");
        }
        else
        {
            text.Text = unlockable.DifficultyToString();
        }
    }

    public override void _Process(double delta)
    {
    }

    public void Unlock()
    {
        if (ButtonLocked != this)
        {
            UnlockButton.Pressed -= Unlock;
            return;
        }

        if (GameManager.GetState().playerKeys <= 0)
        {
            GD.Print("No keys! Can't unlock");
            errorAudioPlayer.Play();
            return;
        }

        unlockAudioPlayer.Play();

        GameManager.GetState().playerKeys -= 1;

        unlockable.Unlock();
        tileGenerator.UpdateState();
        _on_mouse_entered();
        UnlockButton.Pressed -= Unlock;
        ClaimButton.Pressed += Claim;

        GameManager.Save($"{GameManager.GetState().playerName}", $"_auto_unlock_{DateTime.Now.ToString("MM_dd_yy_HH_mm_ss")}");
    }

    public void Claim()
    {
        if (ButtonLocked != this)
        {
            ClaimButton.Pressed -= Claim;
            return;
        }

        claimAudioPlayer.Play();

        unlockable.Claim();
        tileGenerator.UpdateState();
        _on_mouse_entered();
        ClaimButton.Pressed -= Claim;
        ButtonLocked = null;
        TooltipButton.Visible = false;

        GameManager.Save($"{GameManager.GetState().playerName}", $"_auto_claim_{DateTime.Now.ToString("MM_dd_yy_HH_mm_ss")}");
    }

    public void _on_mouse_entered()
    {
        if (ButtonLocked == this /*|| ButtonLocked == null*/)
        {
            if (unlockable.IsUnlocked() && !unlockable.IsClaimed())
            {
                ClaimContainer.Visible = true;
                UnlockContainer.Visible = false;
            }
            else if (unlockable.IsClaimed())
            {
                ClaimContainer.Visible = false;
                UnlockContainer.Visible = false;
            }
            else
            {
                UnlockContainer.Visible = true;
                ClaimContainer.Visible = false;
            }

            if (unlockable.IsClaimed())
            {
            }
        }

        if (ButtonLocked != null || hidden)
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
        if (ButtonLocked != null)
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
            if (ButtonLocked == null && !hidden)
            {
                if (!unlockable.IsUnlocked())
                {
                    UnlockButton.Pressed += Unlock;
                }
                else if (!unlockable.IsClaimed())
                {
                    ClaimButton.Pressed += Claim;
                }

                ButtonLocked = this;
                selectAudioPlayer.Play();
                _on_mouse_entered();
                return;
            }

            ButtonLocked = null;

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

        if (unlockable.IsClaimed())
        {
            //fogRect.SetVisibleWorldPos(Position, true);
            fogRect.RevealCircle(new Vector2(Position.X, Position.Y), .5f, 1.0f);
        }

        lockIcon.Visible = !unlockable.IsUnlocked();// && !(unlockable is FreeTile);
        checkIcon.Visible = unlockable.IsClaimed() && unlockable.IsUnlocked();// && !(unlockable is FreeTile);

        if (!(unlockable is FreeTile))
        {
            if (unlockable.IsClaimed() || unlockable.IsUnlocked())
            hidden = false;
        }
        // Unlocked tiles can't be hidden
        if (!unlockable.IsUnlocked()/* || unlockable is FreeTile*/)
        {
            var leftTile = (Tile)tiles[new Vector2((int)gridPos.X - 1, (int)gridPos.Y)];
            var rightTile = (Tile)tiles[new Vector2((int)gridPos.X + 1, (int)gridPos.Y)];
            var upTile = (Tile)tiles[new Vector2((int)gridPos.X, (int)gridPos.Y - 1)];
            var downTile = (Tile)tiles[new Vector2((int)gridPos.X, (int)gridPos.Y + 1)];

            if (leftTile != null && leftTile.unlockable.IsClaimed())
            {
                hidden = false;
            }
            else if (rightTile != null && rightTile.unlockable.IsClaimed())
            {
                hidden = false;
            }
            else if (upTile != null && upTile.unlockable.IsClaimed())
            {
                hidden = false;
            }
            else if (downTile != null && downTile.unlockable.IsClaimed())
            {
                hidden = false;
            }

            icon.Visible = !hidden;
            text.Visible = !hidden;

            if (unlockable is FreeTile && !hidden)
            {
                unlockable.Unlock();
                unlockable.Claim();
                tileGenerator.UpdateState();
            }

            background.Modulate = hidden ? new Color(.5f, .5f, .5f, .75f) : new Color(1, 1, 1, 1);
        }
    }

    public String Serialize()
    {
        String str = $"{gridPos.X}|{gridPos.Y}|{(int)unlockable.unlockableType}|";

        string json = "";
        JsonSerializerOptions options = new();
        options.Converters.Add(new RangeSystemTextJsonConverter());

        switch (unlockable.unlockableType)
        {
            case UnlockableType.Skill:
                json = JsonSerializer.Serialize((SkillUnlock)unlockable, options);
                break;
            case UnlockableType.Quest:
                json = JsonSerializer.Serialize((QuestUnlock)unlockable, options);
                break;
            case UnlockableType.Diary:
                json = JsonSerializer.Serialize((DiaryUnlock)unlockable, options);
                break;
            case UnlockableType.Free:
                json = JsonSerializer.Serialize((FreeTile)unlockable, options);
                break;
        }

        str += json;

        return str;
    }
}
