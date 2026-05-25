using WebParfum.Models;
using WebParfum.Services;

namespace WebParfum.ViewModels;

public class FeedItem
{
    public string Type { get; set; } = ""; // "review" | "log" | "community"
    public DateTime CreatedAt { get; set; }
    public User Actor { get; set; } = null!;
    public Perfume? Perfume { get; set; }

    // type=review
    public int? Rating { get; set; }
    public string? Body { get; set; }

    // type=log
    public DateOnly? LogDate { get; set; }
    public int? Sprays { get; set; }
    public string? Note { get; set; }

    // type=community
    public Community? Community { get; set; }
}

public class FeedViewModel
{
    public List<FeedItem> Items { get; set; } = [];
    public bool HasFollows { get; set; }
    public List<Perfume> FallbackPopular { get; set; } = [];
    public List<PerfumeRecommendation> Recommendations { get; set; } = [];
}
