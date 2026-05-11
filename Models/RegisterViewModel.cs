using System.ComponentModel.DataAnnotations;

namespace art_galeri.Models
{
    /// <summary>
    /// Kayıt Ol formu için ViewModel - Seçilen role göre dinamik alanlar içerir
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")]
        [Display(Name = "Ad")]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50, ErrorMessage = "Soyad en fazla 50 karakter olabilir.")]
        [Display(Name = "Soyad")]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
        [Display(Name = "E-Posta")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string Sifre { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre tekrar alanı zorunludur.")]
        [Compare("Sifre", ErrorMessage = "Şifreler eşleşmiyor.")]
        [DataType(DataType.Password)]
        [Display(Name = "Şifre Tekrar")]
        public string SifreTekrar { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kullanıcı tipi seçimi zorunludur.")]
        [Display(Name = "Kullanıcı Tipi")]
        public string KullaniciTipi { get; set; } = string.Empty;

        // --- Sanatçı Ek Alanları ---
        [StringLength(200)]
        [Display(Name = "Portfolyo Linki")]
        public string? PortfolyoLinki { get; set; }

        [StringLength(100)]
        [Display(Name = "Sanat Dalı")]
        public string? SanatDali { get; set; }

        [StringLength(500)]
        [Display(Name = "Kısa Özgeçmiş")]
        public string? Ozgecmis { get; set; }

        // --- Eğitmen Ek Alanları ---
        [StringLength(100)]
        [Display(Name = "Uzmanlık Alanı")]
        public string? UzmanlikAlani { get; set; }

        [Display(Name = "Deneyim Yılı")]
        public int? DeneyimYili { get; set; }

        [StringLength(500)]
        [Display(Name = "Biyografi")]
        public string? Biyografi { get; set; }
    }
}
