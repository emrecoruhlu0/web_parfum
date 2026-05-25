namespace WebParfum.Helpers;

// Tekil nota isimlerini (rose, vanilla, bergamot, ...) Bootstrap-Icons sınıfı
// ve renk koduna eşler. Detay sayfasındaki üst/orta/alt nota chip'leri için.
public static class NoteVisual
{
    public record Style(string Icon, string Hex);

    private static readonly Style Default = new("bi-droplet", "#b8a382");

    private static readonly Dictionary<string, Style> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        // Narenciye
        ["bergamot"]     = new("bi-lemon", "#f5b441"),
        ["lemon"]        = new("bi-lemon-fill", "#f5d442"),
        ["orange"]       = new("bi-lemon-fill", "#f59042"),
        ["mandarin"]     = new("bi-lemon-fill", "#f59042"),
        ["mandarin orange"] = new("bi-lemon-fill", "#f59042"),
        ["grapefruit"]   = new("bi-lemon", "#f5816a"),
        ["lime"]         = new("bi-lemon", "#9bcc3e"),
        ["yuzu"]         = new("bi-lemon", "#f5c441"),
        ["citruses"]     = new("bi-lemon", "#f5b441"),
        ["citrus"]       = new("bi-lemon", "#f5b441"),
        ["neroli"]       = new("bi-flower2", "#f5e6c4"),
        ["petitgrain"]   = new("bi-tree", "#9bcc3e"),

        // Çiçekler
        ["rose"]              = new("bi-flower1", "#d23360"),
        ["bulgarian rose"]    = new("bi-flower1", "#d23360"),
        ["damask rose"]       = new("bi-flower1", "#c41e4e"),
        ["jasmine"]           = new("bi-flower2", "#f8f0e0"),
        ["egyptian jasmine"]  = new("bi-flower2", "#f8f0e0"),
        ["sambac jasmine"]    = new("bi-flower2", "#f8f0e0"),
        ["tuberose"]          = new("bi-flower2", "#f5e0e6"),
        ["ylang-ylang"]       = new("bi-flower2", "#f5d96a"),
        ["ylang ylang"]       = new("bi-flower2", "#f5d96a"),
        ["lily-of-the-valley"]= new("bi-flower2", "#f0f5e8"),
        ["lily of the valley"]= new("bi-flower2", "#f0f5e8"),
        ["lily"]              = new("bi-flower2", "#f0f5e8"),
        ["violet"]            = new("bi-flower3", "#7a5fbf"),
        ["iris"]              = new("bi-flower3", "#a394d3"),
        ["orris"]             = new("bi-flower3", "#a394d3"),
        ["lavender"]          = new("bi-flower3", "#9a87c4"),
        ["orange blossom"]    = new("bi-flower2", "#fbe9d0"),
        ["peony"]              = new("bi-flower1", "#f3a5c2"),
        ["magnolia"]          = new("bi-flower2", "#f9efe5"),
        ["gardenia"]          = new("bi-flower2", "#f5f0e1"),
        ["geranium"]          = new("bi-flower1", "#e07a8e"),
        ["mimosa"]            = new("bi-flower1", "#f5d35a"),
        ["narcissus"]         = new("bi-flower2", "#f9e8a1"),
        ["honeysuckle"]       = new("bi-flower1", "#f9d976"),
        ["freesia"]           = new("bi-flower2", "#f6dfe6"),
        ["heliotrope"]        = new("bi-flower1", "#b89cd0"),

        // Meyveler
        ["apple"]        = new("bi-apple", "#d23a3a"),
        ["pear"]         = new("bi-apple", "#c8d96a"),
        ["peach"]        = new("bi-apple", "#f5a877"),
        ["plum"]         = new("bi-apple", "#7a2e58"),
        ["raspberry"]    = new("bi-heart-fill", "#c41e4e"),
        ["strawberry"]   = new("bi-heart-fill", "#e63946"),
        ["blackcurrant"] = new("bi-circle-fill", "#3d1a3d"),
        ["black currant"]= new("bi-circle-fill", "#3d1a3d"),
        ["pineapple"]    = new("bi-sun-fill", "#f5c441"),
        ["coconut"]      = new("bi-circle-fill", "#e6d3a7"),
        ["fig"]          = new("bi-apple", "#7a4f6a"),
        ["cherry"]       = new("bi-heart-fill", "#c41e4e"),
        ["melon"]        = new("bi-circle-fill", "#f5b07a"),
        ["watermelon"]   = new("bi-circle-fill", "#f06a7a"),
        ["mango"]        = new("bi-sun-fill", "#f5a141"),

        // Baharat
        ["pink pepper"]  = new("bi-fire", "#e8869a"),
        ["pepper"]       = new("bi-fire", "#5a4a3a"),
        ["black pepper"] = new("bi-fire", "#2a2018"),
        ["cinnamon"]     = new("bi-fire", "#a0522d"),
        ["cardamom"]     = new("bi-fire", "#7a9166"),
        ["nutmeg"]       = new("bi-fire", "#8b5a3c"),
        ["clove"]        = new("bi-fire", "#5a3820"),
        ["saffron"]      = new("bi-fire", "#e8a541"),
        ["ginger"]       = new("bi-fire", "#d9a066"),
        ["anise"]        = new("bi-fire", "#9bb085"),
        ["star anise"]   = new("bi-stars", "#9bb085"),
        ["coriander"]    = new("bi-tree", "#a8b87a"),
        ["cumin"]        = new("bi-fire", "#a87a3c"),

        // Odunsu
        ["sandalwood"]   = new("bi-tree-fill", "#c8956a"),
        ["cedar"]        = new("bi-tree-fill", "#8b5e3c"),
        ["cedarwood"]    = new("bi-tree-fill", "#8b5e3c"),
        ["oud"]          = new("bi-tree-fill", "#5c3a1e"),
        ["agarwood"]     = new("bi-tree-fill", "#5c3a1e"),
        ["vetiver"]      = new("bi-tree", "#7a6a3c"),
        ["patchouli"]    = new("bi-flower3", "#6a5b3c"),
        ["guaiac wood"]  = new("bi-tree-fill", "#7a5a3c"),
        ["pine"]         = new("bi-tree-fill", "#3a6a3c"),
        ["birch"]        = new("bi-tree", "#c8a87a"),
        ["woody notes"]  = new("bi-tree-fill", "#8b5e3c"),
        ["woods"]        = new("bi-tree-fill", "#8b5e3c"),
        ["blonde woods"] = new("bi-tree-fill", "#d4b48a"),
        ["white woods"]  = new("bi-tree-fill", "#e8d8b8"),

        // Reçineler / balsamik
        ["amber"]        = new("bi-sun", "#c97f2e"),
        ["ambergris"]    = new("bi-sun", "#c97f2e"),
        ["benzoin"]      = new("bi-droplet-fill", "#a8763a"),
        ["frankincense"] = new("bi-droplet-fill", "#b8966a"),
        ["incense"]      = new("bi-fire", "#7a5a3a"),
        ["myrrh"]        = new("bi-droplet-fill", "#8b5a3c"),
        ["labdanum"]     = new("bi-droplet-fill", "#7a4a2c"),
        ["styrax"]       = new("bi-droplet-fill", "#a8763a"),
        ["opoponax"]     = new("bi-droplet-fill", "#a87a3c"),

        // Tatlı / gurme
        ["vanilla"]      = new("bi-egg-fried", "#e0c084"),
        ["tonka bean"]   = new("bi-circle-fill", "#a87a3c"),
        ["tonka"]        = new("bi-circle-fill", "#a87a3c"),
        ["caramel"]      = new("bi-cup-hot", "#b87333"),
        ["honey"]        = new("bi-droplet-fill", "#f5b041"),
        ["chocolate"]    = new("bi-cup-hot-fill", "#5a3820"),
        ["cocoa"]        = new("bi-cup-hot-fill", "#5a3820"),
        ["coffee"]       = new("bi-cup-hot-fill", "#3e2418"),
        ["almond"]       = new("bi-circle-fill", "#cda77a"),
        ["praline"]      = new("bi-cup-hot", "#a87833"),
        ["sugar"]        = new("bi-circle", "#f5f0e1"),

        // Misk / hayvansal
        ["musk"]         = new("bi-droplet-half", "#b9a98e"),
        ["white musk"]   = new("bi-droplet-half", "#e8dec8"),
        ["civet"]        = new("bi-droplet-half", "#7a5a3c"),
        ["castoreum"]    = new("bi-droplet-half", "#5c3a1e"),
        ["leather"]      = new("bi-bag-fill", "#6b4423"),
        ["suede"]        = new("bi-bag", "#a87a5a"),

        // Yeşil / aromatik
        ["mint"]         = new("bi-tree", "#7ec881"),
        ["basil"]        = new("bi-tree", "#5a8a3c"),
        ["rosemary"]     = new("bi-tree", "#6a8a5c"),
        ["thyme"]        = new("bi-tree", "#7a9a5c"),
        ["sage"]         = new("bi-tree", "#9ab07a"),
        ["green notes"]  = new("bi-tree", "#7bb661"),
        ["grass"]        = new("bi-tree", "#7bb661"),
        ["green leaves"] = new("bi-tree", "#5a9a4a"),
        ["tea"]          = new("bi-cup", "#a8b87a"),
        ["green tea"]    = new("bi-cup", "#9bb56a"),
        ["bamboo"]       = new("bi-tree", "#9bb085"),
        ["eucalyptus"]   = new("bi-tree", "#7a9a8c"),

        // Diğer
        ["aldehydes"]    = new("bi-stars", "#cfd9e0"),
        ["sea notes"]    = new("bi-water", "#4aa3df"),
        ["sea salt"]     = new("bi-water", "#7ec0e0"),
        ["marine notes"] = new("bi-water", "#2e86ab"),
        ["water"]        = new("bi-water", "#7ec8e3"),
        ["ozone"]        = new("bi-cloud", "#9ec5e0"),
        ["smoke"]        = new("bi-cloud-fog", "#5a5a5a"),
        ["tobacco"]      = new("bi-droplet-fill", "#7a4a2c"),
        ["powdery notes"]= new("bi-circle", "#efd9d4"),
        ["fruity notes"] = new("bi-apple", "#e25c4a"),
        ["spicy notes"]  = new("bi-fire", "#c84630"),
        ["floral notes"] = new("bi-flower1", "#e688b5"),
    };

    public static Style Get(string? note)
    {
        if (string.IsNullOrWhiteSpace(note)) return Default;
        var key = note.Trim().ToLowerInvariant();
        if (Map.TryGetValue(key, out var s)) return s;

        // Kısmi eşleşme: "egyptian jasmine" -> "jasmine"
        foreach (var kv in Map)
            if (key.Contains(kv.Key)) return kv.Value;

        return Default;
    }

    // CSV içindeki virgülle ayrılmış nota listesini ayrıştırır.
    public static IEnumerable<string> Split(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) yield break;
        foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            if (!string.IsNullOrWhiteSpace(part)) yield return part;
    }
}
