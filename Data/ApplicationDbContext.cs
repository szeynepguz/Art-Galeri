using Microsoft.EntityFrameworkCore;
using art_galeri.Models;

namespace art_galeri.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        // BU SATIRI EKLE:
        public DbSet<Artwork> Artworks { get; set; }
    }
}