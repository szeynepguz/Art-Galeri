using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    /// <summary>
    /// Eğitmen rolüne sahip kullanıcıların ek profil bilgileri
    /// </summary>
    public class EgitmenProfil
    {
        [Key]
        public int ProfilID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }

        [StringLength(100)]
        public string? UzmanlikAlani { get; set; }

        [StringLength(500)]
        public string? Biyografi { get; set; }

        public int? DeneyimYili { get; set; }

        [StringLength(200)]
        public string? SertifikaUrl { get; set; }

        public DateTime? KatilimTarihi { get; set; } = DateTime.UtcNow;
    }
}
