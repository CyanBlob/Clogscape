using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ClogData
{
    public Clog data {get; set;}
}

public class Clog
{
    public String player { get; set; }
    public String player_name_with_capitalization { get; set; }
    public int game_mode { get; set; }
    public int last_checked { get; set; }
    public int last_changed { get; set; }
    public int total_collections_finished { get; set; }
    public int total_collections_available { get; set; }
    public int total_collections_in_response { get; set; }
    public float ehc { get; set; }
    public float ehc_gilded { get; set; }
    public float ehc_im { get; set; }
    public float ehc_gilded_im { get; set; }
    public int total_categories_finished { get; set; }
    public int total_categories_available { get; set; }
    public int? collections_hiscores_rank { get; set; }
    // Items
    [JsonPropertyName("items")]
    public Dictionary<String, List<Item>> sources {get; set;}
}

public class Source
{
    public IList<Item> items {get; set;}
}

public class Item
{
    public int id{get; set;}
    public int count{get; set;}
    public int date{get; set;}
    public String name{get; set;}
}