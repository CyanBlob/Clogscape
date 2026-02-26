using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using HttpClient = System.Net.Http.HttpClient;

public partial class UI : Button
{
    // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
    static HttpClient client;
    static String nameFilePath = "name.txt";
    static TileGenerator tileGenerator;

    static Label keysLabel;
    static LineEdit allowanceEdit;

    static FogRect fogRect;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        GameManager.ui = this;

        fogRect = (FogRect)GetParent().GetParent().GetParent().FindChild("FogRect");

        var label = GetParent().GetParent().FindChild("Bounty Controls").FindChild("KeysLabel");
        if (label != null)
        {
            keysLabel = (Label)label;
            keysLabel.Text = GameManager.GetState().playerKeys.ToString();
        }

        var textEdit = GetParent().GetParent().FindChild("Bounty Controls").FindChild("AllowanceEdit");
        if (textEdit != null)
        {
            allowanceEdit = (LineEdit)textEdit;
            allowanceEdit.Text = GameManager.GetState().playerAllowance.ToString();
        }

        tileGenerator = (TileGenerator)GetParent().GetParent().GetParent().FindChild("Tiles");
        if (File.Exists(nameFilePath))
        {
            GameManager.GetState().playerName = File.ReadAllLines(nameFilePath)[0];

            var playerNameEdit = (LineEdit)GetParent().FindChild("PlayerName");

            playerNameEdit.Text = GameManager.GetState().playerName;
            GameManager.GetState().playerName = playerNameEdit.Text;

            if (!GameManager.Load(playerNameEdit.Text, tileGenerator))
            {
                GameManager.SetState(GameManager.GetDefaultState());
                GameManager.Save(playerNameEdit.Text);
            }
        }

        client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.ParseAdd(
            "Clogscape/1.0 (+https://github.com/CyanBlob/clogscape; andrewthomas255@duck.com)");

        //client.DefaultRequestHeaders.UserAgent.ParseAdd(
        //"Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/117.0.0.0 Safari/537.36");
        client.DefaultRequestHeaders.Accept.ParseAdd("image/*,*/*;q=0.8");
    }

    public override void _Process(double delta)
    {
        // TODO: I don't want to do this every frame
        UpdateKeys();
        //UpdateAllowance();
    }

    public async void _on_sync_pressed()
    {
        GD.Print("Sync");
        try
        {
            using HttpResponseMessage response = await client.GetAsync("https://templeosrs.com/api/collection-log/player_collection_log.php?player=MagentaBlob&categories=bosses%2Craids%2Cclues%2Cminigames%2Cother&includenames=1&dateformat=unix");
            response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();

            // Above three lines can be replaced with new helper method below
            // string responseBody = await client.GetStringAsync(uri);

            ClogData clogData = await response.Content.ReadFromJsonAsync<ClogData>();
            Clog clog = clogData.data;

            GD.Print(clog.player_name_with_capitalization);
            GD.Print(clog.ehc);
            GD.Print(clog.total_collections_finished);

            foreach (var source in clog.sources)
            {
                foreach (var item in source.Value)
                    GD.Print($"{source.Key}: {item.name}");
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    public async void _on_items_button_pressed()
    {
        GD.Print("Items");
        try
        {
            HttpResponseMessage response = await client.GetAsync("https://templeosrs.com/api/collection-log/categories.php?");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            ClogCategories categories = await response.Content.ReadFromJsonAsync<ClogCategories>();
            GD.Print(categories.bosses);

            response = await client.GetAsync("https://templeosrs.com/api/collection-log/items.php");
            response.EnsureSuccessStatusCode();
            responseBody = await response.Content.ReadAsStringAsync();

            ClogItems items = await response.Content.ReadFromJsonAsync<ClogItems>();

            ClogItemsIntId itemsIntId = new()
            {
                items = new()
            };

            foreach (var item in items.items)
            {
                itemsIntId.items.Add(Int32.Parse(item.Key), item.Value);
            }

            foreach (var boss in categories.bosses)
            {
                GD.Print($"{boss.Key}:");
                foreach (var item in boss.Value)
                {
                    GD.Print($"\t{itemsIntId.items[item]}");
                }
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    public async void _on_items_with_icons_pressed()
    {
        GD.Print("Items with icons");
        try
        {
            //HttpResponseMessage response = await client.GetAsync("https://templeosrs.com/api/collection-log/categories.php?");
            //response.EnsureSuccessStatusCode();
            //string responseBody = await response.Content.ReadAsStringAsync();

            //ClogCategories categories = await response.Content.ReadFromJsonAsync<ClogCategories>();
            //GD.Print(categories.bosses);

            HttpResponseMessage response = await client.GetAsync("https://templeosrs.com/api/collection-log/items.php");
            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            ClogItems items = await response.Content.ReadFromJsonAsync<ClogItems>();

            //var basePngUri = "https://oldschool.runescape.wiki/w/File:";

            var queryUri = "https://oldschool.runescape.wiki/api.php?action=query&titles=<ITEM_NAME>&prop=pageimages&format=json&pithumbsize=64";

            GD.Print("Downloading...");
            foreach (var item in items.items)
            {
                var query = queryUri.Replace("<ITEM_NAME>", item.Value.Replace(" ", "_"));
                String thumbnailQuery = "";
                ItemOverview overview = new();
                try
                {

                    response = await client.GetAsync(query);

                    overview = await response.Content.ReadFromJsonAsync<ItemOverview>();

                    thumbnailQuery = overview.GetFirstThumbnailUrl();
                    response = await client.GetAsync(thumbnailQuery);
                    var bytes = await response.Content.ReadAsByteArrayAsync();

                    var file = File.OpenWrite($"Resources/Items/{item.Value}.png");
                    file.Write(bytes);
                }
                catch
                {
                    GD.Print($"Warning: Failed to download thumbnail for: {item.Value}");
                    GD.Print(query);
                    GD.Print(thumbnailQuery);
                }
            }
            GD.Print("Downloading complete");

            /*var basePngUri = "https://oldschool.runescape.wiki/images/";
            var basePngUriSuffix = "_detail.png";

            foreach (var item in items.items)
            {
                var uri = $"{basePngUri}{item.Key.Replace(" ", "_")}{basePngUriSuffix}";
                GD.Print(uri);
                response = await client.GetAsync(uri);

                var file = File.OpenWrite($"Resources/Items/{item.Key}.png");
                //var bytes = await response.Content.ReadAsByteArrayAsync();
                var strbytes = await response.Content.ReadAsStringAsync();
                var bytes = Encoding.Unicode.GetBytes(strbytes);

                file.Write(bytes);
                break;
            }*/

        }
        catch (HttpRequestException e)
        {
            Console.WriteLine("\nException Caught!");
            Console.WriteLine("Message :{0} ", e.Message);
        }
    }

    public void UpdateKeys()
    {
        keysLabel.Text = GameManager.GetState().playerKeys.ToString();
    }

    public void UpdateAllowance()
    {
        allowanceEdit.Text = GameManager.GetState().playerAllowance.ToString();
    }

    public void _on_player_save()
    {
        var playerNameEdit = (LineEdit)GetParent().FindChild("PlayerName");
        GameManager.Save(playerNameEdit.Text);
    }
    public void _on_player_load()
    {
        var playerNameEdit = (LineEdit)GetParent().FindChild("PlayerName");
        if (tileGenerator == null)
        {
            tileGenerator = (TileGenerator)GetParent().GetParent().GetParent().FindChild("Tiles");
        }
        GameManager.Load(playerNameEdit.Text, tileGenerator);
    }
    public void _on_create_player()
    {
        var state = GameManager.GetDefaultState();

        var playerNameEdit = (LineEdit)GetParent().FindChild("PlayerName");
        state.playerName = playerNameEdit.Text;

        GameManager.SetState(state);
        GameManager.LoadBounties("default");
        GameManager.UpdateBounties();

        if (tileGenerator == null)
        {
            tileGenerator = (TileGenerator)GetParent().GetParent().GetParent().FindChild("Tiles");
        }

        tileGenerator._Ready();
        UpdateAllowance();
        fogRect._Ready();
    }

    public void _on_player_text_changed(String name)
    {
        var lines = new List<String>
        {
            name
        };
        File.WriteAllLines(nameFilePath, lines);
    }

    public void _on_roll_bounties_button_pressed()
    {
        var bounties = GameManager.UpdateBounties();

        foreach (var bounty in bounties)
        {
            GD.Print($"{bounty.name}");
        }
    }

    public void _on_complete_bounty_button_pressed()
    {
        if (GameManager.GetState().currentBounties.Count == 0)
        {
            GD.Print("No bounties. Re-rolling");
            _on_roll_bounties_button_pressed();
            return;
        }
        GameManager.GetState().CompleteBounty(GameManager.GetState().currentBounties[0]);
        keysLabel.Text = GameManager.GetState().playerKeys.ToString();
        allowanceEdit.Text = GameManager.GetState().playerAllowance.ToString();

        _on_roll_bounties_button_pressed();
    }

    public void _on_allowance_edit_text_changed(String allowance)
    {
        int newAllowance;

        if (Int32.TryParse(allowance, out newAllowance))
        {
            GD.Print(newAllowance);
            GameManager.GetState().playerAllowance = newAllowance;
        }
    }
}
