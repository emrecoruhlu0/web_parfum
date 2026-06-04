using System.ComponentModel.DataAnnotations;

namespace WebParfum.ViewModels;

public class EditProfileViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
    [MinLength(3, ErrorMessage = "Kullanıcı adı en az 3 karakter olmalıdır")]
    [MaxLength(50)]
    [RegularExpression(@"^[a-zA-Z0-9_.]+$",
        ErrorMessage = "Kullanıcı adı yalnızca harf, rakam, alt çizgi ve nokta içerebilir")]
    [Display(Name = "Kullanıcı Adı")]
    public string Username { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "Bio en fazla 500 karakter olabilir")]
    [Display(Name = "Hakkında")]
    public string? Bio { get; set; }
}
