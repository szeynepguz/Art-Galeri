using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class Artwork
    {
        [Key]
        public int ArtworkID { get; set; }

        [Required, StringLength(200)]
        public string? Title { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        public int ArtistID { get; set; }

        [ForeignKey("ArtistID")]
        public User? Artist { get; set; }

        [StringLength(100)]
        public string? Kategori { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public int GoruntulenmeSayisi { get; set; } = 0;
        public int BegeniSayisi { get; set; } = 0;
        public int YorumSayisi { get; set; } = 0;

        public double OrtalamaPuan { get; set; } = 0.0;

        public bool Aktif { get; set; } = true;

        // Navigation
        public ICollection<Yorum>? Yorumlar { get; set; }
        public ICollection<Favori>? Favoriler { get; set; }
        public ICollection<Siparis>? Siparisler { get; set; }
    }
}