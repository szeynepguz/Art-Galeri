using Microsoft.EntityFrameworkCore;
using art_galeri.Models;

namespace art_galeri.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Artwork> Artworks { get; set; }
        public DbSet<Rol> Roller { get; set; }
        public DbSet<SanatciProfil> SanatciProfiller { get; set; }
        public DbSet<EgitmenProfil> EgitmenProfiller { get; set; }
        public DbSet<MusteriProfil> MusteriProfiller { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Roller tablosuna seed data ekle
            modelBuilder.Entity<Rol>().HasData(
                new Rol { RolID = 1, RolAdi = "Musteri", Aciklama = "Eser inceleyen ve satın alan kullanıcılar" },
                new Rol { RolID = 2, RolAdi = "Yonetici", Aciklama = "Sistem yöneticisi - tam yetki" },
                new Rol { RolID = 3, RolAdi = "Egitmen", Aciklama = "Atölye ve workshop yöneten eğitmenler" },
                new Rol { RolID = 4, RolAdi = "Sanatci", Aciklama = "Eser yükleyen ve sergileyen sanatçılar" }
            );

            // User -> Rol ilişkisi
            modelBuilder.Entity<User>()
                .HasOne(u => u.Rol)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RolID)
                .OnDelete(DeleteBehavior.Restrict);

            // User -> SanatciProfil (1-1)
            modelBuilder.Entity<SanatciProfil>()
                .HasOne(sp => sp.User)
                .WithOne(u => u.SanatciProfil)
                .HasForeignKey<SanatciProfil>(sp => sp.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> EgitmenProfil (1-1)
            modelBuilder.Entity<EgitmenProfil>()
                .HasOne(ep => ep.User)
                .WithOne(u => u.EgitmenProfil)
                .HasForeignKey<EgitmenProfil>(ep => ep.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> MusteriProfil (1-1)
            modelBuilder.Entity<MusteriProfil>()
                .HasOne(mp => mp.User)
                .WithOne(u => u.MusteriProfil)
                .HasForeignKey<MusteriProfil>(mp => mp.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Email unique index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();
        }
    }
}