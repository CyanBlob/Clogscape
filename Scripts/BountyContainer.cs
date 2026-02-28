using Godot;
using System;

public partial class BountyContainer : NinePatchRect
{
    [Export]
    public int bountyIndex = 0;

    [Export]
    public Texture2D bossTexture;
    [Export]
    public Texture2D raidTexture;
    [Export]
    public Texture2D clueTexture;
    [Export]
    public Texture2D minigameTexture;
    [Export]
    public Texture2D grindTexture;
    [Export]
    public Texture2D skillingTexture;
    [Export]
    public Texture2D challengeTexture;
    [Export]
    public Texture2D miscTexture;
    [Export]
    public Texture2D fetchTexture;
    [Export]
    public Texture2D realLifeTexture;
    [Export]
    public Texture2D slayerTexture;

    Label title;
    Label description;
    Label rewards;
    TextureRect icon;

    TextEdit rewardsEdit;

    Bounty bounty;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var container = GetChild(0);
        title = (Label)container.FindChild("Title");
        icon = (TextureRect)container.FindChild("Icon");
        description = (Label)container.FindChild("DescriptionContainer").FindChild("Description");
        rewards = (Label)container.FindChild("Rewards");

        rewardsEdit = (TextEdit)GetParent().GetParent().FindChild("RewardsEdit");

        if (GameManager.GetState().currentBounties == null ||  GameManager.GetState().currentBounties.Count < 3)
        {
            GameManager.UpdateBounties();
        }
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Update();
    }

    public void Update()
    {
        if (GameManager.GetState().currentBounties == null ||  GameManager.GetState().currentBounties.Count < 3)
        {
            GameManager.UpdateBounties();
        }
        bounty = GameManager.GetState().currentBounties[bountyIndex];

        title.Text = "\n" + bounty.name;
        description.Text = bounty.description;
        rewards.Text = $"Keys: {bounty.minKeys} {(bounty.maxKeys == bounty.minKeys ? "" : $"-{bounty.maxKeys}")}";
        rewards.Text += $" GP: {bounty.minGp} {(bounty.maxGp == bounty.minGp ? "" : $"-{bounty.maxGp}")}";

        switch (bounty.bountyType)
        {
            case BountyType.Boss:
                icon.Texture = bossTexture;
                break;
            case BountyType.Raid:
                icon.Texture = raidTexture;
                break;
            case BountyType.Clue:
                icon.Texture = clueTexture;
                break;
            case BountyType.Minigame:
                icon.Texture = minigameTexture;
                break;
            case BountyType.Grind:
                icon.Texture = grindTexture;
                break;
            case BountyType.Skilling:
                icon.Texture = skillingTexture;
                break;
            case BountyType.Challenge:
                icon.Texture = challengeTexture;
                break;
            case BountyType.Misc:
                icon.Texture = miscTexture;
                break;
            case BountyType.Fetch:
                icon.Texture = fetchTexture;
                break;
            case BountyType.RealLife:
                icon.Texture = realLifeTexture;
                break;
            case BountyType.Slayer:
                icon.Texture = slayerTexture;
                break;
            default:
                icon.Texture = miscTexture;
                break;
        }
    }

    public void _on_complete_bounty_button_pressed()
    {
        GD.Print($"Completing: {bounty.name}");
        var rewards = GameManager.GetState().CompleteBounty(bounty);

        rewardsEdit.Text = $"+{rewards.Item1} Keys\n+{rewards.Item2}gp";

        var fade = (Fade)rewardsEdit;
        fade.visibility = 100.0f;

        GameManager.Save($"{GameManager.GetState().playerName}", $"_auto_bounty_{DateTime.Now.ToString("MM_dd_yy_HH_mm_ss")}", false);
    }
}
