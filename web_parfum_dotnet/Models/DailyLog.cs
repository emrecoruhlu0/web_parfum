using System.ComponentModel.DataAnnotations;

namespace WebParfum.Models;

public class DailyLog
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PerfumeId { get; set; }
    public Perfume Perfume { get; set; } = null!;

    [Required]
    public DateOnly Date { get; set; }

    public string? Note { get; set; }
    public int Sprays { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
