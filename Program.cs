using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritabani Baglantisi (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session Destegi
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ===================== SEED DATA =====================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // Kullanicilar
    if (!context.Users.Any())
    {
        var admin    = new User { Ad = "Admin",  Soyad = "Yonetici",  Email = "admin@art.com",    Sifre = "123", RolID = 2 };
        var musteri1 = new User { Ad = "Ahmet",  Soyad = "Kaya",      Email = "musteri1@art.com", Sifre = "123", RolID = 1 };
        var musteri2 = new User { Ad = "Ayse",   Soyad = "Demir",     Email = "musteri2@art.com", Sifre = "123", RolID = 1 };
        var egitmen  = new User { Ad = "Mehmet", Soyad = "Yilmaz",    Email = "egitmen@art.com",  Sifre = "123", RolID = 3 };
        var sanatci  = new User { Ad = "Zeynep", Soyad = "Arslan",    Email = "sanatci@art.com",  Sifre = "123", RolID = 4 };

        context.Users.AddRange(admin, musteri1, musteri2, egitmen, sanatci);
        context.SaveChanges();

        context.MusteriProfiller.AddRange(
            new MusteriProfil { UserID = musteri1.UserID, Telefon = "0555-111-2233", Adres = "Istanbul" },
            new MusteriProfil { UserID = musteri2.UserID, Telefon = "0555-444-5566", Adres = "Ankara" }
        );
        context.EgitmenProfiller.Add(new EgitmenProfil
        {
            UserID = egitmen.UserID, UzmanlikAlani = "Resim & Suluboya",
            DeneyimYili = 8, Biyografi = "15 yıllık sanat egitmenliği deneyimine sahip uzman ressam."
        });
        context.SanatciProfiller.Add(new SanatciProfil
        {
            UserID = sanatci.UserID, SanatDali = "Heykel & Resim",
            Ozgecmis = "Uluslararası sergilerde yer alan ödüllü bir sanatçı."
        });
        context.SaveChanges();

        // Eserler
        var eserler = new[]
        {
            new Artwork { Title = "Denizin Huzuru", Description = "Ege kıyılarından ilham alan suluboya eser.", Price = 2500, ArtistID = sanatci.UserID, Kategori = "Suluboya", GoruntulenmeSayisi = 142, BegeniSayisi = 38, YorumSayisi = 5, ImageUrl = "/img/artwork1.jpg" },
            new Artwork { Title = "Şehrin Ritmi",   Description = "İstanbul sokaklarının dinamik yorumu.", Price = 3800, ArtistID = sanatci.UserID, Kategori = "Yağlıboya", GoruntulenmeSayisi = 97, BegeniSayisi = 24, YorumSayisi = 3, ImageUrl = "/img/artwork2.jpg" },
            new Artwork { Title = "Sonsuzluk",       Description = "Soyut bir evren yolculuğu.", Price = 5200, ArtistID = sanatci.UserID, Kategori = "Soyut", GoruntulenmeSayisi = 210, BegeniSayisi = 67, YorumSayisi = 12, ImageUrl = "/img/artwork3.jpg" },
            new Artwork { Title = "Baharın Sesi",    Description = "Doğanın uyanışını betimleyen çalışma.", Price = 1800, ArtistID = sanatci.UserID, Kategori = "Natürmort", GoruntulenmeSayisi = 65, BegeniSayisi = 19, YorumSayisi = 2, ImageUrl = "/img/artwork4.jpg" },
        };
        context.Artworks.AddRange(eserler);
        context.SaveChanges();

        // Etkinlikler
        var etkinlikler = new[]
        {
            new Etkinlik { Ad = "Suluboya Atölyesi", Aciklama = "Başlangıç seviyesi suluboya dersi. Malzemeler dahil.", Tarih = DateTime.UtcNow.AddDays(10), Konum = "Trabzon Sanat Merkezi", Ucret = 350, Kapasite = 15, RezervasyonSayisi = 9, OrtalamaPuan = 4.7, EgitmenID = egitmen.UserID, Kategori = "Resim", GorselUrl = "/img/etkinlik1.jpg" },
            new Etkinlik { Ad = "Fotoğraf Sanatı",   Aciklama = "Kompozisyon ve ışık teknikleri.", Tarih = DateTime.UtcNow.AddDays(20), Konum = "Online (Zoom)", Ucret = 200, Kapasite = 30, RezervasyonSayisi = 22, OrtalamaPuan = 4.5, EgitmenID = egitmen.UserID, Kategori = "Fotograf", GorselUrl = "/img/etkinlik2.jpg" },
            new Etkinlik { Ad = "Heykel Workshopu",  Aciklama = "Kil ve seramik çalışmaları.", Tarih = DateTime.UtcNow.AddDays(5), Konum = "Ankara Kültür Merkezi", Ucret = 500, Kapasite = 10, RezervasyonSayisi = 10, OrtalamaPuan = 4.9, EgitmenID = egitmen.UserID, Kategori = "Heykel", GorselUrl = "/img/etkinlik3.jpg" },
        };
        context.Etkinlikler.AddRange(etkinlikler);
        context.SaveChanges();

        // Rezervasyonlar
        context.Rezervasyonlar.AddRange(
            new Rezervasyon { UserID = musteri1.UserID, EtkinlikID = etkinlikler[0].EtkinlikID, KatilimciSayisi = 2, ToplamTutar = 700, Durum = "Onaylandi" },
            new Rezervasyon { UserID = musteri2.UserID, EtkinlikID = etkinlikler[1].EtkinlikID, KatilimciSayisi = 1, ToplamTutar = 200, Durum = "Onaylandi" }
        );

        // Siparisler
        context.Siparisler.AddRange(
            new Siparis { UserID = musteri1.UserID, ArtworkID = eserler[0].ArtworkID, Tutar = eserler[0].Price, OdemeYontemi = "KrediKarti", Durum = "Tamamlandi" },
            new Siparis { UserID = musteri2.UserID, ArtworkID = eserler[2].ArtworkID, Tutar = eserler[2].Price, OdemeYontemi = "HavaleEFT",  Durum = "Tamamlandi" }
        );

        // Yorumlar
        context.Yorumlar.AddRange(
            new Yorum { UserID = musteri1.UserID, ArtworkID = eserler[0].ArtworkID, Icerik = "Muhteşem bir eser! Renk uyumu harika.", Puan = 5, Dogrulanmis = true, FaydaliBulma = 12 },
            new Yorum { UserID = musteri2.UserID, ArtworkID = eserler[0].ArtworkID, Icerik = "Beklentilerimi karşıladı, çok beğendim.", Puan = 4, Dogrulanmis = true, FaydaliBulma = 7 },
            new Yorum { UserID = musteri1.UserID, EtkinlikID = etkinlikler[0].EtkinlikID, Icerik = "Eğitmen çok sabırlı ve yardımsever.", Puan = 5, Dogrulanmis = true, FaydaliBulma = 5 }
        );

        // Favoriler
        context.Favoriler.AddRange(
            new Favori { UserID = musteri1.UserID, ArtworkID = eserler[1].ArtworkID },
            new Favori { UserID = musteri1.UserID, ArtworkID = eserler[2].ArtworkID },
            new Favori { UserID = musteri2.UserID, ArtworkID = eserler[0].ArtworkID }
        );

        // Kampanya
        context.Kampanyalar.Add(new Kampanya
        {
            Ad = "Yaz Festivali İndirimi",
            Aciklama = "Tüm eserlerde %15 indirim!",
            IndirimOrani = 15,
            KuponKodu = "YAZ2026",
            BaslangicTarihi = DateTime.UtcNow,
            BitisTarihi = DateTime.UtcNow.AddMonths(2),
            Aktif = true
        });

        // Destek talebi
        context.DestekTalepleri.Add(new DestekTalebi
        {
            UserID = musteri1.UserID,
            Konu = "Sipariş hakkında bilgi",
            Mesaj = "Siparişimin durumunu öğrenmek istiyorum.",
            Email = musteri1.Email,
            Durum = "Acik"
        });

        context.SaveChanges();
    }
}

app.Run();
