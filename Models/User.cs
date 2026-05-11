using System.ComponentModel.DataAnnotations;

namespace art_galeri.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string? Name { get; set; } // Soru işareti hatayı siler
        public string? Email { get; set; }
        public string? Password { get; set; }
        public string? Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
