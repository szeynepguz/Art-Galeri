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
        public DbSet<Etkinlik> Etkinlikler { get; set; }
        public DbSet<Rezervasyon> Rezervasyonlar { get; set; }
        public DbSet<Favori> Favoriler { get; set; }
        public DbSet<Yorum> Yorumlar { get; set; }
        public DbSet<Siparis> Siparisler { get; set; }
        public DbSet<Kampanya> Kampanyalar { get; set; }
        public DbSet<DestekTalebi> DestekTalepleri { get; set; }
        public DbSet<Karsilastirma> Karsilastirmalar { get; set; }
        public DbSet<EtkinlikTarih> EtkinlikTarihler { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Roller seed
            modelBuilder.Entity<Rol>().HasData(
                new Rol { RolID = 1, RolAdi = "Musteri", Aciklama = "Eser inceleyen ve satın alan kullanıcılar" },
                new Rol { RolID = 2, RolAdi = "Yonetici", Aciklama = "Sistem yöneticisi - tam yetki" },
                new Rol { RolID = 3, RolAdi = "Egitmen", Aciklama = "Atölye ve workshop yöneten eğitmenler" },
                new Rol { RolID = 4, RolAdi = "Sanatci", Aciklama = "Eser yükleyen ve sergileyen sanatçılar" }
            );

            // User -> Rol
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

            // Artwork -> Artist (User)
            modelBuilder.Entity<Artwork>()
                .HasOne(a => a.Artist)
                .WithMany()
                .HasForeignKey(a => a.ArtistID)
                .OnDelete(DeleteBehavior.SetNull);

            // Yorum -> User
            modelBuilder.Entity<Yorum>()
                .HasOne(y => y.User)
                .WithMany()
                .HasForeignKey(y => y.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Yorum -> Artwork (nullable)
            modelBuilder.Entity<Yorum>()
                .HasOne(y => y.Artwork)
                .WithMany(a => a.Yorumlar)
                .HasForeignKey(y => y.ArtworkID)
                .OnDelete(DeleteBehavior.Cascade);

            // Yorum -> Etkinlik (nullable)
            modelBuilder.Entity<Yorum>()
                .HasOne(y => y.Etkinlik)
                .WithMany(e => e.Yorumlar)
                .HasForeignKey(y => y.EtkinlikID)
                .OnDelete(DeleteBehavior.Cascade);

            // Favori -> User
            modelBuilder.Entity<Favori>()
                .HasOne(f => f.User)
                .WithMany()
                .HasForeignKey(f => f.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Favori -> Artwork
            modelBuilder.Entity<Favori>()
                .HasOne(f => f.Artwork)
                .WithMany(a => a.Favoriler)
                .HasForeignKey(f => f.ArtworkID)
                .OnDelete(DeleteBehavior.Cascade);

            // Rezervasyon -> User
            modelBuilder.Entity<Rezervasyon>()
                .HasOne(r => r.User)
                .WithMany()
                .HasForeignKey(r => r.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Rezervasyon -> Etkinlik
            modelBuilder.Entity<Rezervasyon>()
                .HasOne(r => r.Etkinlik)
                .WithMany(e => e.Rezervasyonlar)
                .HasForeignKey(r => r.EtkinlikID)
                .OnDelete(DeleteBehavior.Cascade);

            // Siparis -> User
            modelBuilder.Entity<Siparis>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            // Siparis -> Artwork
            modelBuilder.Entity<Siparis>()
                .HasOne(s => s.Artwork)
                .WithMany(a => a.Siparisler)
                .HasForeignKey(s => s.ArtworkID)
                .OnDelete(DeleteBehavior.SetNull);

            // Siparis -> Kampanya (nullable)
            modelBuilder.Entity<Siparis>()
                .HasOne(s => s.Kampanya)
                .WithMany()
                .HasForeignKey(s => s.KampanyaID)
                .OnDelete(DeleteBehavior.SetNull);

            // Etkinlik -> Egitmen (nullable)
            modelBuilder.Entity<Etkinlik>()
                .HasOne(e => e.Egitmen)
                .WithMany()
                .HasForeignKey(e => e.EgitmenID)
                .OnDelete(DeleteBehavior.SetNull);

            // EtkinlikTarih -> Etkinlik
            modelBuilder.Entity<EtkinlikTarih>()
                .HasOne(et => et.Etkinlik)
                .WithMany(e => e.Tarihler)
                .HasForeignKey(et => et.EtkinlikID)
                .OnDelete(DeleteBehavior.Cascade);

            // Rezervasyon -> EtkinlikTarih (nullable)
            modelBuilder.Entity<Rezervasyon>()
                .HasOne(r => r.EtkinlikTarih)
                .WithMany()
                .HasForeignKey(r => r.EtkinlikTarihID)
                .OnDelete(DeleteBehavior.SetNull);

            // Kampanya -> HedefRol (nullable)
            modelBuilder.Entity<Kampanya>()
                .HasOne(k => k.HedefRol)
                .WithMany()
                .HasForeignKey(k => k.HedefRolID)
                .OnDelete(DeleteBehavior.SetNull);

            // DestekTalebi -> User (nullable)
            modelBuilder.Entity<DestekTalebi>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserID)
                .OnDelete(DeleteBehavior.SetNull);

            // Email unique index
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // KuponKodu unique
            modelBuilder.Entity<Kampanya>()
                .HasIndex(k => k.KuponKodu)
                .IsUnique();
        }
    }
}