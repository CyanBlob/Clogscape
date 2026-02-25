using Godot;
using System;

public partial class BountyContainer : NinePatchRect
{
    [Export]
    public int bountyIndex = 0;

    [Export]
    public Texture2D bossTexture;
    [Export]
    public Texture2D combatTexture;

    Label title;
    Label description;
    TextureRect icon;

    Bounty bounty;
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        var container = GetChild(0);
        title = (Label)container.FindChild("Title");
        icon = (TextureRect)container.FindChild("Icon");
        description = (Label)container.FindChild("DescriptionContainer").FindChild("Description");
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        Update();
    }

    public void Update()
    {
        bounty = GameManager.GetState().currentBounties[bountyIndex];

        title.Text = "\n" + bounty.name;
        description.Text = bounty.description;

        switch (bounty.bountyType)
        {
            case BountyType.Boss:
                icon.Texture = bossTexture;
                break;
            default:
                icon.Texture = combatTexture;
                break;
        }
    }

    public void _on_complete_bounty_button_pressed()
    {
        GameManager.GetState().CompleteBounty(bounty);
    }
}
