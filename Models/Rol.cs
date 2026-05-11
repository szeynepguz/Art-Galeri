using System.ComponentModel.DataAnnotations;

namespace art_galeri.Models
{
    public class Rol
    {
        [Key]
        public int RolID { get; set; }

        [Required]
        [StringLength(50)]
        public string RolAdi { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Aciklama { get; set; }

        // Navigation property
        public ICollection<User>? Users { get; set; }
    }
}
