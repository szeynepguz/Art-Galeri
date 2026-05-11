using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    /// <summary>
    /// Müşteri rolüne sahip kullanıcıların ek profil bilgileri
    /// </summary>
    public class MusteriProfil
    {
        [Key]
        public int ProfilID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }

        [StringLength(200)]
        public string? Adres { get; set; }

        [StringLength(20)]
        public string? Telefon { get; set; }

        [StringLength(500)]
        public string? IlgiAlanlari { get; set; }

        public DateTime? KatilimTarihi { get; set; } = DateTime.UtcNow;
    }
}
