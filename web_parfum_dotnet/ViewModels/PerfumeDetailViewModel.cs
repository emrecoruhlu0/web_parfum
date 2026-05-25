using WebParfum.Models;

namespace WebParfum.ViewModels;

public class PerfumeDetailViewModel
{
    public Perfume Perfume { get; set; } = null!;
    public List<Review> Reviews { get; set; } = [];
    public int LikeCount { get; set; }
    public int CollectionCount { get; set; }

    // Current user state
    public bool IsLiked { get; set; }
    public HashSet<string> UserCollectionStatuses { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public Review? UserReview { get; set; }
}
