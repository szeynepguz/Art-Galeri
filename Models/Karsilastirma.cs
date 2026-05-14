using System.ComponentModel.DataAnnotations;

namespace art_galeri.Models
{
    public class Karsilastirma
    {
        [Key]
        public int KarsilastirmaID { get; set; }
        
        [Required]
        public int UserID { get; set; }
        
        [Required, StringLength(100)]
        public string Tip { get; set; } = "Etkinlik"; // "Etkinlik" veya "Artwork"
        
        [Required, StringLength(200)]
        public string Baslik { get; set; } = string.Empty;
        
        [Required]
        public string IDler { get; set; } = string.Empty; // Virgülle ayrılmış ID'ler
        
        public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
    }
}
