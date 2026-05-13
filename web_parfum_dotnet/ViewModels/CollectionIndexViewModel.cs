using WebParfum.Models;

namespace WebParfum.ViewModels;

public class CollectionIndexViewModel
{
    public string Tab { get; set; } = "owned"; // owned | wishlist | tried
    public List<Collection> Items { get; set; } = [];

    public int OwnedCount { get; set; }
    public int WishlistCount { get; set; }
    public int TriedCount { get; set; }
}
