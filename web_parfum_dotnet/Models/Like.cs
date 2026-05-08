namespace WebParfum.Models;

public class Like
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PerfumeId { get; set; }
    public Perfume Perfume { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
