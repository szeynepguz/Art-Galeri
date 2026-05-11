using System.ComponentModel.DataAnnotations;

namespace art_galeri.Models
{
    public class Artwork
    {
        
            public int ArtworkID { get; set; }
            public string? Title { get; set; }
            public string? Description { get; set; }
            public decimal Price { get; set; }
            public string? ImageUrl { get; set; }
            public int ArtistID { get; set; }
            public DateTime UploadDate { get; set; }
        
    }
}