namespace WebParfum.Models;

/// <summary>
/// Kullanıcının "Sana Özel Öneriler" kartlarına verdiği beğendim/beğenmedim geri bildirimi.
/// Bu explicit sinyal, koku profili puanlamasındaki review/koleksiyon eksiğini tamamlar
/// ve ileride ML/öneri modeli için etiketlenmiş eğitim verisi sağlar.
/// </summary>
public class RecommendationFeedback
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PerfumeId { get; set; }
    public Perfume Perfume { get; set; } = null!;

    /// <summary>true = beğendim (öneri isabetli), false = beğenmedim (öneri yanlış)</summary>
    public bool Liked { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
