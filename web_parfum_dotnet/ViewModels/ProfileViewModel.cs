using WebParfum.Models;
using WebParfum.Services;

namespace WebParfum.ViewModels;

public class ProfileViewModel
{
    public User User { get; set; } = null!;
    public bool IsOwnProfile { get; set; }
    public bool IsFollowing { get; set; }

    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }
    public int ReviewsCount { get; set; }
    public int CollectionCount { get; set; }
    public int DailyLogCount { get; set; }

    public string Section { get; set; } = "collection"; // collection | logs | reviews
    public string CollectionStatus { get; set; } = "owned"; // owned | wishlist | tried

    // Section-specific data
    public List<Collection> CollectionItems { get; set; } = [];
    public int OwnedCount { get; set; }
    public int WishlistCount { get; set; }
    public int TriedCount { get; set; }
    public List<DailyLog> RecentLogs { get; set; } = [];
    public List<Review> RecentReviews { get; set; } = [];

    // Aggregation (top notes/accords from DailyLogs)
    public List<NoteFreq> TopNotes { get; set; } = [];
    public List<NoteFreq> TopAccords { get; set; } = [];

    // Taste profile (yeni — tüm etkileşimlerden ağırlıklı hesaplanır)
    public TasteProfile? TasteProfile { get; set; }
    public List<PerfumeRecommendation> Recommendations { get; set; } = [];

    // PerfumeId → mevcut geri bildirim (true=beğendim, false=beğenmedim). Yoksa anahtar yok.
    public Dictionary<int, bool> FeedbackStates { get; set; } = [];
}

public record NoteFreq(string Name, int Count);
