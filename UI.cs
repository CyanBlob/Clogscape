using Godot;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using HttpClient = System.Net.Http.HttpClient;

public partial class UI : Button
{
    // HttpClient is intended to be instantiated once per application, rather than per-use. See Remarks.
    static readonly HttpClient client = new HttpClient();

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }

    public async void _on_sync_pressed()
    {
        GD.Print("Sync");
        // Call asynchronous network methods in a try/catch block to handle exceptions.
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
        // Call asynchronous network methods in a try/catch block to handle exceptions.
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
}
