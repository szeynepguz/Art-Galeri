using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Ad alanı zorunludur.")]
        [StringLength(50)]
        public string Ad { get; set; } = string.Empty;

        [Required(ErrorMessage = "Soyad alanı zorunludur.")]
        [StringLength(50)]
        public string Soyad { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Şifre alanı zorunludur.")]
        [StringLength(255)]
        public string Sifre { get; set; } = string.Empty;

        // Foreign Key - Roller tablosuna
        [Required]
        public int RolID { get; set; }

        [ForeignKey("RolID")]
        public Rol? Rol { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Eski alanlar (geriye uyumluluk için)
        [NotMapped]
        public string? Name
        {
            get => $"{Ad} {Soyad}";
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var parts = value.Split(' ', 2);
                    Ad = parts[0];
                    Soyad = parts.Length > 1 ? parts[1] : "";
                }
            }
        }

        [NotMapped]
        public string? Password
        {
            get => Sifre;
            set => Sifre = value ?? string.Empty;
        }

        [NotMapped]
        public string? Role
        {
            get => Rol?.RolAdi;
        }

        // Navigation properties
        public SanatciProfil? SanatciProfil { get; set; }
        public EgitmenProfil? EgitmenProfil { get; set; }
        public MusteriProfil? MusteriProfil { get; set; }
    }
}
