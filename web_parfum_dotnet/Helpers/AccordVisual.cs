namespace WebParfum.Helpers;

// Akor isimlerini renk ve Bootstrap-Icons sınıflarına eşler. Filtre chip'leri,
// takvim hücreleri ve detay rozetlerinde tutarlı görsel kimlik için ortak kaynak.
public static class AccordVisual
{
    public record Style(string Hex, string TextHex, string Icon);

    private static readonly Style Default = new("#e5dccd", "#5b4f3c", "bi-droplet");

    private static readonly Dictionary<string, Style> Map = new(StringComparer.OrdinalIgnoreCase)
    {
        // Odunsu / sıcak
        ["woody"]      = new("#8b5e3c", "#fff", "bi-tree"),
        ["oud"]        = new("#5c3a1e", "#fff", "bi-tree"),
        ["amber"]      = new("#c97f2e", "#fff", "bi-sun"),
        ["leather"]    = new("#6b4423", "#fff", "bi-bag"),
        ["earthy"]     = new("#7a5c3a", "#fff", "bi-globe"),
        ["mossy"]      = new("#5f7345", "#fff", "bi-tree-fill"),
        ["patchouli"]  = new("#6a5b3c", "#fff", "bi-flower3"),
        ["balsamic"]   = new("#8c6230", "#fff", "bi-droplet-fill"),
        ["animalic"]   = new("#4a3424", "#fff", "bi-bug"),

        // Çiçeksi
        ["floral"]      = new("#e688b5", "#fff", "bi-flower1"),
        ["white floral"]= new("#f3e7df", "#7a5c4a", "bi-flower2"),
        ["yellow floral"]= new("#f6d96a", "#5b4500", "bi-flower2"),
        ["rose"]        = new("#d23360", "#fff", "bi-flower1"),
        ["tuberose"]    = new("#f5e0e6", "#7a3c52", "bi-flower2"),
        ["violet"]      = new("#7a5fbf", "#fff", "bi-flower3"),
        ["iris"]        = new("#a394d3", "#fff", "bi-flower3"),
        ["lavender"]    = new("#9a87c4", "#fff", "bi-flower3"),

        // Meyvemsi / tatlı
        ["fruity"]   = new("#e25c4a", "#fff", "bi-apple"),
        ["tropical"] = new("#ffb347", "#fff", "bi-sun-fill"),
        ["coconut"]  = new("#e6d3a7", "#5b4500", "bi-circle"),
        ["sweet"]    = new("#e8a4c9", "#fff", "bi-heart-fill"),
        ["vanilla"]  = new("#e0c084", "#5b4500", "bi-egg-fried"),
        ["caramel"]  = new("#b87333", "#fff", "bi-cup-hot"),
        ["almond"]   = new("#cda77a", "#fff", "bi-circle-fill"),
        ["lactonic"] = new("#f0e7d4", "#7a5c4a", "bi-cup"),

        // Ferah / narenciye
        ["citrus"]      = new("#f5b441", "#fff", "bi-lemon"),
        ["fresh"]       = new("#7ec8e3", "#fff", "bi-wind"),
        ["fresh spicy"] = new("#5fa9a3", "#fff", "bi-stars"),
        ["green"]       = new("#7bb661", "#fff", "bi-tree"),
        ["herbal"]      = new("#8aa861", "#fff", "bi-flower1"),
        ["aromatic"]    = new("#6b9a78", "#fff", "bi-wind"),
        ["aldehydic"]   = new("#cfd9e0", "#3a4a55", "bi-stars"),

        // Su / okyanus
        ["aquatic"] = new("#4aa3df", "#fff", "bi-water"),
        ["marine"]  = new("#2e86ab", "#fff", "bi-water"),
        ["ozonic"]  = new("#9ec5e0", "#1f3a4a", "bi-cloud"),

        // Baharatlı
        ["warm spicy"] = new("#c84630", "#fff", "bi-fire"),
        ["soft spicy"] = new("#d98a5f", "#fff", "bi-fire"),

        // Pudralı / misk
        ["powdery"] = new("#efd9d4", "#7a5c4a", "bi-circle"),
        ["musky"]   = new("#b9a98e", "#fff", "bi-droplet-half"),
    };

    public static Style Get(string? accord)
    {
        if (string.IsNullOrWhiteSpace(accord)) return Default;
        return Map.TryGetValue(accord.Trim(), out var s) ? s : Default;
    }

    // İlk dolu accord'u verir (Accord1..5) — takvim/feed kartlarında ana renk için.
    public static Style ForPerfume(string? a1, string? a2 = null, string? a3 = null, string? a4 = null, string? a5 = null)
    {
        foreach (var a in new[] { a1, a2, a3, a4, a5 })
            if (!string.IsNullOrWhiteSpace(a)) return Get(a);
        return Default;
    }
}
