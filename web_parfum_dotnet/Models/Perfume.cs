using System.ComponentModel.DataAnnotations;

namespace WebParfum.Models;

public class Perfume
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Brand { get; set; } = string.Empty;

    public string? Country { get; set; }
    public string? Gender { get; set; }
    public int? Year { get; set; }

    public string? TopNotes { get; set; }
    public string? MiddleNotes { get; set; }
    public string? BaseNotes { get; set; }

    public string? Accord1 { get; set; }
    public string? Accord2 { get; set; }
    public string? Accord3 { get; set; }
    public string? Accord4 { get; set; }
    public string? Accord5 { get; set; }

    public double? RatingValue { get; set; }
    public int? RatingCount { get; set; }

    public string? ImageUrl { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Collection> Collections { get; set; } = [];
    public ICollection<DailyLog> DailyLogs { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<CommunityPost> CommunityPosts { get; set; } = [];
}
