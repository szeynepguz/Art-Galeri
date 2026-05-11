using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class DestekTalebi
    {
        [Key]
        public int TalepID { get; set; }

        public int? UserID { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }

        [Required, StringLength(200)]
        public string Konu { get; set; } = string.Empty;

        [Required, StringLength(5000)]
        public string Mesaj { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Email { get; set; } = string.Empty;

        // Durum: Acik, Cevaplandi, Kapali
        [StringLength(50)]
        public string Durum { get; set; } = "Acik";

        [StringLength(5000)]
        public string? YoneticiYaniti { get; set; }

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;
        public DateTime? YanitTarihi { get; set; }
    }
}
