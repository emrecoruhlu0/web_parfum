using WebParfum.Models;

namespace WebParfum.ViewModels;

public class FeedItem
{
    public string Type { get; set; } = ""; // "review" | "log"
    public DateTime CreatedAt { get; set; }
    public User Actor { get; set; } = null!;
    public Perfume Perfume { get; set; } = null!;

    // type=review
    public int? Rating { get; set; }
    public string? Body { get; set; }

    // type=log
    public DateOnly? LogDate { get; set; }
    public int? Sprays { get; set; }
    public string? Note { get; set; }
}

public class FeedViewModel
{
    public List<FeedItem> Items { get; set; } = [];
    public bool HasFollows { get; set; }
    public List<Perfume> FallbackPopular { get; set; } = [];
}
