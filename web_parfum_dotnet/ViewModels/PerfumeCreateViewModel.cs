using System.ComponentModel.DataAnnotations;

namespace WebParfum.ViewModels;

public class PerfumeCreateViewModel
{
    [Required, MaxLength(200), Display(Name = "Parfüm Adı")]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(100), Display(Name = "Marka")]
    public string Brand { get; set; } = string.Empty;

    [MaxLength(100), Display(Name = "Ülke")]
    public string? Country { get; set; }

    [MaxLength(50), Display(Name = "Cinsiyet")]
    public string? Gender { get; set; }

    [Range(1800, 2100), Display(Name = "Yıl")]
    public int? Year { get; set; }

    [Display(Name = "Üst Notalar")]
    public string? TopNotes { get; set; }

    [Display(Name = "Orta Notalar")]
    public string? MiddleNotes { get; set; }

    [Display(Name = "Alt Notalar")]
    public string? BaseNotes { get; set; }

    [MaxLength(50), Display(Name = "Akor 1")]
    public string? Accord1 { get; set; }

    [MaxLength(50), Display(Name = "Akor 2")]
    public string? Accord2 { get; set; }

    [MaxLength(50), Display(Name = "Akor 3")]
    public string? Accord3 { get; set; }

    [MaxLength(50), Display(Name = "Akor 4")]
    public string? Accord4 { get; set; }

    [MaxLength(50), Display(Name = "Akor 5")]
    public string? Accord5 { get; set; }

    [Url, Display(Name = "Resim URL")]
    public string? ImageUrl { get; set; }
}
