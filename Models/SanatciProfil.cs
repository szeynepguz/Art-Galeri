using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    /// <summary>
    /// Sanatçı rolüne sahip kullanıcıların ek profil bilgileri
    /// </summary>
    public class SanatciProfil
    {
        [Key]
        public int ProfilID { get; set; }

        [Required]
        public int UserID { get; set; }

        [ForeignKey("UserID")]
        public User? User { get; set; }

        [StringLength(500)]
        public string? Ozgecmis { get; set; }

        [StringLength(200)]
        public string? PortfolyoLinki { get; set; }

        [StringLength(100)]
        public string? SanatDali { get; set; }

        [StringLength(200)]
        public string? ProfilFotoUrl { get; set; }

        public DateTime? KatilimTarihi { get; set; } = DateTime.UtcNow;
    }
}
