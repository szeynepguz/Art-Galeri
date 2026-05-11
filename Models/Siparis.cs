using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class Siparis
    {
        [Key]
        public int SiparisID { get; set; }

        [Required]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }

        public int? ArtworkID { get; set; }
        [ForeignKey("ArtworkID")]
        public Artwork? Artwork { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Tutar { get; set; }

        // OdemeYontemi: KrediKarti, HavaleEFT, KapidaOdeme
        [StringLength(50)]
        public string OdemeYontemi { get; set; } = "KrediKarti";

        // Durum: Beklemede, OdemeBekleniyor, Onaylandi, Kargolandi, Tamamlandi, Iptal
        [StringLength(50)]
        public string Durum { get; set; } = "Tamamlandi";

        public DateTime SiparisTarihi { get; set; } = DateTime.UtcNow;

        [StringLength(500)]
        public string? Adres { get; set; }

        // İndirim kuponu
        public int? KampanyaID { get; set; }
        [ForeignKey("KampanyaID")]
        public Kampanya? Kampanya { get; set; }
    }
}
