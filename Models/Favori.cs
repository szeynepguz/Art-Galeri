using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace art_galeri.Models
{
    public class Favori
    {
        [Key]
        public int FavoriID { get; set; }

        [Required]
        public int UserID { get; set; }
        [ForeignKey("UserID")]
        public User? User { get; set; }

        public int? ArtworkID { get; set; }
        [ForeignKey("ArtworkID")]
        public Artwork? Artwork { get; set; }

        public DateTime EklenmeTarihi { get; set; } = DateTime.UtcNow;
    }
}
