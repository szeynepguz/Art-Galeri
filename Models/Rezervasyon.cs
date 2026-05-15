using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class Rezervasyon
    {
        [Key]
        public int RezervasyonID { get; set; }

        [Required]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }

        [Required]
        public int EtkinlikID { get; set; }
        [ForeignKey("EtkinlikID")]
        public Etkinlik? Etkinlik { get; set; }

        public int KatilimciSayisi { get; set; } = 1;

        public DateTime RezervasyonTarihi { get; set; } = DateTime.UtcNow;

        // Durum: Beklemede, Onaylandi, Iptal
        [StringLength(50)]
        public string Durum { get; set; } = "Onaylandi";

        [StringLength(500)]
        public string? Notlar { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ToplamTutar { get; set; }

        [StringLength(50)]
        public string OdemeYontemi { get; set; } = "KrediKarti";
    }
}
