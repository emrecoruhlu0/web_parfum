using System.ComponentModel.DataAnnotations;

namespace WebParfum.Models;

public class Review
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PerfumeId { get; set; }
    public Perfume Perfume { get; set; } = null!;

    [Range(1, 5)]
    public int Rating { get; set; }

    public string? Body { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
