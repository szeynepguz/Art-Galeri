using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class Yorum
    {
        [Key]
        public int YorumID { get; set; }

        [Required]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }

        // Eser yorumu mu, etkinlik yorumu mu?
        public int? ArtworkID { get; set; }
        [ForeignKey("ArtworkID")]
        public Artwork? Artwork { get; set; }

        public int? EtkinlikID { get; set; }
        [ForeignKey("EtkinlikID")]
        public Etkinlik? Etkinlik { get; set; }

        [Required, StringLength(2000)]
        public string Icerik { get; set; } = string.Empty;

        // 1-5 puan
        public int Puan { get; set; } = 5;

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;

        public bool Onaylandi { get; set; } = true;

        // Yönetici yanıtı
        [StringLength(2000)]
        public string? YoneticiYaniti { get; set; }

        public DateTime? YanitTarihi { get; set; }

        public int FaydaliBulma { get; set; } = 0;

        // Doğrulanmış satın alma / rezervasyon mu?
        public bool Dogrulanmis { get; set; } = false;
    }
}
