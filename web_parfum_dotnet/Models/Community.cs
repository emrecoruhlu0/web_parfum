using System.ComponentModel.DataAnnotations;

namespace WebParfum.Models;

public class Community
{
    public int Id { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Image { get; set; }

    public int OwnerId { get; set; }
    public User Owner { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<CommunityMember> Members { get; set; } = [];
    public ICollection<CommunityPost> Posts { get; set; } = [];
}
