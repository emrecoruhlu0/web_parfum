using System.ComponentModel.DataAnnotations;

namespace WebParfum.ViewModels;

public class RegisterViewModel
{
    [Required(ErrorMessage = "Kullanıcı adı zorunludur")]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur")]
    [MinLength(6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
