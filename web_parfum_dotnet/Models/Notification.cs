using System.ComponentModel.DataAnnotations;

namespace WebParfum.Models;

public class Notification
{
    public int Id { get; set; }

    public int RecipientId { get; set; }
    public User Recipient { get; set; } = null!;

    public int ActorId { get; set; }
    public User Actor { get; set; } = null!;

    [Required, MaxLength(50)]
    public string Type { get; set; } = string.Empty; // like, follow, review, community_invite

    public int? PerfumeId { get; set; }
    public int? CommunityId { get; set; }

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
