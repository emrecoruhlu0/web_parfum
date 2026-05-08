using System.ComponentModel.DataAnnotations;

namespace WebParfum.Models;

public class User
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required, MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    public string? Bio { get; set; }
    public string? Avatar { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Collection> Collections { get; set; } = [];
    public ICollection<DailyLog> DailyLogs { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
    public ICollection<Like> Likes { get; set; } = [];
    public ICollection<Follow> Followers { get; set; } = [];
    public ICollection<Follow> Following { get; set; } = [];
    public ICollection<Notification> ReceivedNotifications { get; set; } = [];
    public ICollection<Notification> SentNotifications { get; set; } = [];
    public ICollection<CommunityMember> CommunityMemberships { get; set; } = [];
    public ICollection<CommunityPost> CommunityPosts { get; set; } = [];
    public ICollection<Message> SentMessages { get; set; } = [];
    public ICollection<Message> ReceivedMessages { get; set; } = [];
}
