using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

public class ClogData
{
    public Clog data {get; set;}
}

/// Begin player clog API
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
    public Dictionary<string, List<Item>> sources { get; set; }
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

/// End player clog API

/// Begin clog categories API
public class ClogCategories {
    public Dictionary<string, List<int>> bosses { get; set; }
    public Dictionary<string, List<int>> raids { get; set; }
    public Dictionary<string, List<int>> clues { get; set; }
    public Dictionary<string, List<int>> minigames { get; set; }
    public Dictionary<string, List<int>> other { get; set; }
}

public class ClogCategory {
    public Dictionary<String, List<int>> CategoryEntry {get; set;}
}
/// End clog categories API

/// Begin clog items API
public class ClogItems {
    public Dictionary<String, String> items {get; set;}
}

public class ItemOverview
{
    public string batchcomplete { get; set; }

    public ItemOverviewQuery query { get; set; }

    public string GetFirstThumbnailUrl()
    {
        if (query?.pages == null) return null;
        foreach (var p in query.pages.Values)
        {
            if (p?.thumbnail?.source != null) return p.thumbnail.source;
        }
        return null;
    }
}

public class ItemOverviewQuery
{
    public List<ItemOverviewNormalized> normalized { get; set; }

    public Dictionary<string, ItemOverviewPage> pages { get; set; }
}

public class ItemOverviewNormalized
{
    public string from { get; set; }

    public string to { get; set; }
}

public class ItemOverviewPage
{
    public int pageid { get; set; }

    public int ns { get; set; }

    public string title { get; set; }

    public ItemOverviewThumbnail thumbnail { get; set; }

    public string pageimage { get; set; }
}

public class ItemOverviewThumbnail
{
    public string source { get; set; }

    public int width { get; set; }

    public int height { get; set; }
}
/// End item overview
public class ClogItemsIntId {
    public Dictionary<int, String> items {get; set;}
}
/// End clog items API
/// Begin item thumbnail API
/// Begin item thumbnail API
// Duplicate/empty placeholder removed — thumbnail types are defined above.
/// End item thumbnail API
