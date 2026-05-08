using System.ComponentModel.DataAnnotations;

namespace WebParfum.Models;

public class Collection
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PerfumeId { get; set; }
    public Perfume Perfume { get; set; } = null!;

    [Required, MaxLength(20)]
    public string Status { get; set; } = "owned"; // owned, wishlist, tried

    public int BottleLevel { get; set; } = 100;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
