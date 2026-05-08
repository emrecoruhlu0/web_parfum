using System.ComponentModel.DataAnnotations;

namespace WebParfum.Models;

public class CommunityMember
{
    public int Id { get; set; }

    public int CommunityId { get; set; }
    public Community Community { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    [MaxLength(20)]
    public string Role { get; set; } = "member"; // member, admin

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
