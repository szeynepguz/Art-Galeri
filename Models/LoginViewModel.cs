using System.ComponentModel.DataAnnotations;

namespace art_galeri.Models
{
    /// <summary>
    /// Giriş Yap formu için ViewModel
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [Display(Name = "E-Posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;
    }
}
