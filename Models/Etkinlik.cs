using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class Etkinlik
    {
        [Key]
        public int EtkinlikID { get; set; }

        [Required, StringLength(200)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Aciklama { get; set; }

        [Required]
        public DateTime Tarih { get; set; }

        public TimeSpan Saat { get; set; } = new TimeSpan(10, 0, 0);

        [Required, StringLength(200)]
        public string? Konum { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Ucret { get; set; }

        public int Kapasite { get; set; } = 20;

        public int RezervasyonSayisi { get; set; } = 0;

        public double OrtalamaPuan { get; set; } = 0.0;

        public string? GorselUrl { get; set; }

        [StringLength(100)]
        public string? Kategori { get; set; }

        public bool Aktif { get; set; } = true;

        // Eğitmen / oluşturan
        public int? EgitmenID { get; set; }
        [ForeignKey("EgitmenID")]
        public User? Egitmen { get; set; }

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Rezervasyon>? Rezervasyonlar { get; set; }
        public ICollection<Yorum>? Yorumlar { get; set; }

        [NotMapped]
        public double DolulukOrani => Kapasite > 0 ? (double)RezervasyonSayisi / Kapasite * 100 : 0;

        [NotMapped]
        public int KalanKapasite => Kapasite - RezervasyonSayisi;
    }
}
