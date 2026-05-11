using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class Kampanya
    {
        [Key]
        public int KampanyaID { get; set; }

        [Required, StringLength(200)]
        public string Ad { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Aciklama { get; set; }

        // İndirim oranı (0-100)
        [Column(TypeName = "decimal(5,2)")]
        public decimal IndirimOrani { get; set; }

        [Required, StringLength(50)]
        public string KuponKodu { get; set; } = string.Empty;

        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }

        public bool Aktif { get; set; } = true;

        // Belirli rol için mi? (null = herkese)
        public int? HedefRolID { get; set; }
        [ForeignKey("HedefRolID")]
        public Rol? HedefRol { get; set; }
    }
}
