namespace WebParfum.Models;

public class CommunityPost
{
    public int Id { get; set; }

    public int CommunityId { get; set; }
    public Community Community { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int? PerfumeId { get; set; }
    public Perfume? Perfume { get; set; }

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
