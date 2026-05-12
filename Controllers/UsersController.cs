using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        public UsersController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.Include(u => u.Rol).ToListAsync();
            return View(users);
        }

        // ---- KAYIT ----
        public IActionResult Register() => View(new RegisterViewModel());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            var gecerliTipler = new[] { "Musteri", "Sanatci", "Egitmen" };
            if (!gecerliTipler.Contains(model.KullaniciTipi))
                ModelState.AddModelError("KullaniciTipi", "Geçersiz kullanıcı tipi.");
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı.");
            if (!ModelState.IsValid) return View(model);

            var rol = await _context.Roller.FirstOrDefaultAsync(r => r.RolAdi == model.KullaniciTipi);
            if (rol == null) { ModelState.AddModelError("", "Sistem hatası: Rol bulunamadı."); return View(model); }

            var user = new User { Ad = model.Ad, Soyad = model.Soyad, Email = model.Email, Sifre = model.Sifre, RolID = rol.RolID, CreatedAt = DateTime.UtcNow };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            switch (model.KullaniciTipi)
            {
                case "Sanatci":
                    _context.SanatciProfiller.Add(new SanatciProfil { UserID = user.UserID, PortfolyoLinki = model.PortfolyoLinki, SanatDali = model.SanatDali, Ozgecmis = model.Ozgecmis });
                    break;
                case "Egitmen":
                    _context.EgitmenProfiller.Add(new EgitmenProfil { UserID = user.UserID, UzmanlikAlani = model.UzmanlikAlani, DeneyimYili = model.DeneyimYili, Biyografi = model.Biyografi });
                    break;
                case "Musteri":
                    _context.MusteriProfiller.Add(new MusteriProfil { UserID = user.UserID });
                    break;
            }
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kayıt başarılı! Giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        public IActionResult Create() => RedirectToAction("Register");

        // ---- GİRİŞ ----
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("UserID") != null)
                return RedirectByRole(HttpContext.Session.GetString("UserRole") ?? "");
            return View(new LoginViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);
            var user = await _context.Users.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == model.Email && u.Sifre == model.Sifre);
            if (user == null) { ModelState.AddModelError("", "E-posta veya şifre hatalı!"); return View(model); }
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("UserAd", user.Ad);
            HttpContext.Session.SetString("UserSoyad", user.Soyad);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Rol?.RolAdi ?? "Musteri");
            TempData["SuccessMessage"] = $"Hoş geldiniz, {user.Ad} {user.Soyad}!";
            return RedirectByRole(user.Rol?.RolAdi ?? "Musteri");
        }

        public IActionResult Logout() { HttpContext.Session.Clear(); TempData["SuccessMessage"] = "Başarıyla çıkış yaptınız."; return RedirectToAction("Login"); }

        // ---- MÜŞTERİ DASHBOARD ----
        public async Task<IActionResult> MusteriDashboard()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var artworks = await _context.Artworks.Include(a => a.Artist).Where(a => a.Aktif).OrderByDescending(a => a.BegeniSayisi).Take(8).ToListAsync();
            var etkinlikler = await _context.Etkinlikler.Include(e => e.Egitmen).Where(e => e.Aktif && e.Tarih >= DateTime.UtcNow).OrderBy(e => e.Tarih).Take(4).ToListAsync();
            var kampanyalar = await _context.Kampanyalar.Where(k => k.Aktif && k.BaslangicTarihi <= DateTime.UtcNow && k.BitisTarihi >= DateTime.UtcNow).ToListAsync();
            ViewBag.UserName = $"{HttpContext.Session.GetString("UserAd")} {HttpContext.Session.GetString("UserSoyad")}";
            ViewBag.Etkinlikler = etkinlikler;
            ViewBag.Kampanyalar = kampanyalar;
            return View(artworks);
        }

        // ---- YÖNETİCİ DASHBOARD ----
        public async Task<IActionResult> YoneticiDashboard()
        {
            if (!IsLoggedIn() || HttpContext.Session.GetString("UserRole") != "Yonetici") return RedirectToAction("Login");
            ViewBag.UserName = $"{HttpContext.Session.GetString("UserAd")} {HttpContext.Session.GetString("UserSoyad")}";
            ViewBag.ToplamKullanici = await _context.Users.CountAsync();
            ViewBag.ToplamEser = await _context.Artworks.CountAsync();
            ViewBag.ToplamSanatci = await _context.Users.CountAsync(u => u.RolID == 4);
            ViewBag.ToplamEgitmen = await _context.Users.CountAsync(u => u.RolID == 3);
            ViewBag.ToplamMusteri = await _context.Users.CountAsync(u => u.RolID == 1);
            ViewBag.ToplamRezervasyonlar = await _context.Rezervasyonlar.CountAsync();
            ViewBag.ToplamSiparisler = await _context.Siparisler.CountAsync(s => s.Durum == "Tamamlandi");
            ViewBag.ToplamGelir = await _context.Siparisler.Where(s => s.Durum == "Tamamlandi").SumAsync(s => s.Tutar);
            ViewBag.AcikDestekler = await _context.DestekTalepleri.CountAsync(d => d.Durum == "Acik");
            ViewBag.Eserler = await _context.Artworks.Include(a => a.Artist).OrderByDescending(a => a.GoruntulenmeSayisi).Take(10).ToListAsync();
            ViewBag.Etkinlikler = await _context.Etkinlikler.Include(e => e.Egitmen).OrderByDescending(e => e.RezervasyonSayisi).Take(10).ToListAsync();
            ViewBag.SonYorumlar = await _context.Yorumlar.Include(y => y.User).Include(y => y.Artwork).Include(y => y.Etkinlik).OrderByDescending(y => y.OlusturulmaTarihi).Take(10).ToListAsync();
            ViewBag.Kampanyalar = await _context.Kampanyalar.Include(k => k.HedefRol).OrderByDescending(k => k.Aktif).ToListAsync();
            ViewBag.DestekTalepleri = await _context.DestekTalepleri.Include(d => d.User).OrderByDescending(d => d.OlusturulmaTarihi).Take(10).ToListAsync();
            ViewBag.TumKullanicilar = await _context.Users.Include(u => u.Rol).OrderByDescending(u => u.CreatedAt).Take(15).ToListAsync();
            return View();
        }

        // ---- EĞİTMEN DASHBOARD ----
        public async Task<IActionResult> EgitmenDashboard()
        {
            if (!IsLoggedIn() || HttpContext.Session.GetString("UserRole") != "Egitmen") return RedirectToAction("Login");
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var etkinlikler = await _context.Etkinlikler.Where(e => e.EgitmenID == userId).ToListAsync();
            
            ViewBag.EtkinlikYorumlari = await _context.Yorumlar
                .Include(y => y.User)
                .Include(y => y.Etkinlik)
                .Where(y => y.Etkinlik != null && y.Etkinlik.EgitmenID == userId)
                .OrderByDescending(y => y.OlusturulmaTarihi)
                .ToListAsync();

            ViewBag.UserName = $"{HttpContext.Session.GetString("UserAd")} {HttpContext.Session.GetString("UserSoyad")}";
            ViewBag.ToplamEtkinlik = etkinlikler.Count;
            ViewBag.ToplamRezervasyonlar = etkinlikler.Sum(e => e.RezervasyonSayisi);
            ViewBag.OrtalamaPuan = etkinlikler.Any() ? etkinlikler.Average(e => e.OrtalamaPuan) : 0;
            return View(etkinlikler);
        }

        // ---- SANATÇI DASHBOARD ----
        public async Task<IActionResult> SanatciDashboard()
        {
            if (!IsLoggedIn() || HttpContext.Session.GetString("UserRole") != "Sanatci") return RedirectToAction("Login");
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var artworks = await _context.Artworks.Where(a => a.ArtistID == userId).ToListAsync();

            ViewBag.EserYorumlari = await _context.Yorumlar
                .Include(y => y.User)
                .Include(y => y.Artwork)
                .Where(y => y.Artwork != null && y.Artwork.ArtistID == userId)
                .OrderByDescending(y => y.OlusturulmaTarihi)
                .ToListAsync();

            ViewBag.UserName = $"{HttpContext.Session.GetString("UserAd")} {HttpContext.Session.GetString("UserSoyad")}";
            ViewBag.ToplamEser = artworks.Count;
            ViewBag.ToplamGoruntulenme = artworks.Sum(a => a.GoruntulenmeSayisi);
            ViewBag.ToplamBegeni = artworks.Sum(a => a.BegeniSayisi);
            ViewBag.ToplamYorum = artworks.Sum(a => a.YorumSayisi);
            return View(artworks);
        }

        // ---- REZERVASYONLARIM ----
        public async Task<IActionResult> Rezervasyonlarim()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var rezervasyonlar = await _context.Rezervasyonlar.Include(r => r.Etkinlik).Where(r => r.UserID == userId).OrderByDescending(r => r.RezervasyonTarihi).ToListAsync();
            return View(rezervasyonlar);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RezervasyonIptal(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var rez = await _context.Rezervasyonlar.Include(r => r.Etkinlik).FirstOrDefaultAsync(r => r.RezervasyonID == id && r.UserID == userId);
            if (rez != null)
            {
                rez.Durum = "Iptal";
                if (rez.Etkinlik != null) rez.Etkinlik.RezervasyonSayisi -= rez.KatilimciSayisi;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Rezervasyon iptal edildi.";
            }
            return RedirectToAction("Rezervasyonlarim");
        }

        // ---- REZERVASYON GÜNCELLE ----
        public async Task<IActionResult> RezervasyonGuncelle(int id)
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var rez = await _context.Rezervasyonlar.Include(r => r.Etkinlik).FirstOrDefaultAsync(r => r.RezervasyonID == id && r.UserID == userId);
            if (rez == null || rez.Durum == "Iptal") return NotFound();

            // Aynı kategorideki veya aynı isimdeki diğer aktif etkinlikleri bul (tarih değişimi için)
            ViewBag.DigerTarihler = await _context.Etkinlikler
                .Where(e => e.Ad == rez.Etkinlik!.Ad && e.EtkinlikID != rez.EtkinlikID && e.Aktif && e.Tarih >= DateTime.UtcNow)
                .ToListAsync();

            return View(rez);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RezervasyonGuncelle(int id, int katilimciSayisi, int? yeniEtkinlikId, string? notlar)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");
            var rez = await _context.Rezervasyonlar.Include(r => r.Etkinlik).FirstOrDefaultAsync(r => r.RezervasyonID == id && r.UserID == userId);
            if (rez == null || rez.Durum == "Iptal") return NotFound();

            var eskiEtkinlik = rez.Etkinlik;
            var hedefEtkinlik = eskiEtkinlik;

            if (yeniEtkinlikId.HasValue && yeniEtkinlikId != rez.EtkinlikID)
            {
                hedefEtkinlik = await _context.Etkinlikler.FindAsync(yeniEtkinlikId.Value);
                if (hedefEtkinlik == null || !hedefEtkinlik.Aktif) return NotFound();
            }

            if (hedefEtkinlik == null) return NotFound();

            // Kontenjan kontrolü
            int gerekenYer = (hedefEtkinlik.EtkinlikID == rez.EtkinlikID) 
                ? (katilimciSayisi - rez.KatilimciSayisi) 
                : katilimciSayisi;

            if (hedefEtkinlik.KalanKapasite < gerekenYer)
            {
                TempData["ErrorMessage"] = "Hedef etkinlikte yeterli kontenjan yok.";
                return RedirectToAction("RezervasyonGuncelle", new { id });
            }

            // Eski etkinlikten düş, yeniye ekle
            if (eskiEtkinlik != null) eskiEtkinlik.RezervasyonSayisi -= rez.KatilimciSayisi;
            hedefEtkinlik.RezervasyonSayisi += katilimciSayisi;

            rez.EtkinlikID = hedefEtkinlik.EtkinlikID;
            rez.KatilimciSayisi = katilimciSayisi;
            rez.ToplamTutar = hedefEtkinlik.Ucret * katilimciSayisi;
            if (notlar != null) rez.Notlar = notlar;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Rezervasyon başarıyla güncellendi!";
            return RedirectToAction("Rezervasyonlarim");
        }

        // ---- SİPARİŞLERİM ----
        public async Task<IActionResult> Siparislerim()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var siparisler = await _context.Siparisler.Include(s => s.Artwork).ThenInclude(a => a!.Artist).Where(s => s.UserID == userId).OrderByDescending(s => s.SiparisTarihi).ToListAsync();
            return View(siparisler);
        }

        // ---- PROFİL ----
        public async Task<IActionResult> Profil()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");
            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var user = await _context.Users.Include(u => u.Rol).Include(u => u.MusteriProfil).Include(u => u.SanatciProfil).Include(u => u.EgitmenProfil).FirstOrDefaultAsync(u => u.UserID == userId);
            return View(user);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ProfilGuncelle(string ad, string soyad, string? telefon, string? adres)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");
            var user = await _context.Users.Include(u => u.MusteriProfil).FirstOrDefaultAsync(u => u.UserID == userId);
            if (user != null)
            {
                user.Ad = ad; user.Soyad = soyad;
                if (user.MusteriProfil != null) { user.MusteriProfil.Telefon = telefon; user.MusteriProfil.Adres = adres; }
                HttpContext.Session.SetString("UserAd", ad);
                HttpContext.Session.SetString("UserSoyad", soyad);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Profil güncellendi!";
            }
            return RedirectToAction("Profil");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> SifreDegistir(string eskiSifre, string yeniSifre)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login");
            var user = await _context.Users.FindAsync(userId.Value);
            if (user == null || user.Sifre != eskiSifre) { TempData["ErrorMessage"] = "Mevcut şifre hatalı."; return RedirectToAction("Profil"); }
            user.Sifre = yeniSifre;
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Şifre başarıyla değiştirildi!";
            return RedirectToAction("Profil");
        }

        // ---- DESTEK ----
        public IActionResult Destek() => View(new DestekTalebi { Email = HttpContext.Session.GetString("UserEmail") ?? "" });

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Destek(DestekTalebi model)
        {
            model.UserID = HttpContext.Session.GetInt32("UserID");
            model.OlusturulmaTarihi = DateTime.UtcNow;
            ModelState.Remove("User");
            if (!ModelState.IsValid) return View(model);
            _context.DestekTalepleri.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Destek talebiniz iletildi. En kısa sürede dönüş yapılacak.";
            return RedirectToAction("Destek");
        }

        // ---- YARDIMCI ----
        private bool IsLoggedIn() => HttpContext.Session.GetInt32("UserID") != null;

        private IActionResult RedirectByRole(string rolAdi) => rolAdi switch
        {
            "Yonetici" => RedirectToAction("YoneticiDashboard"),
            "Sanatci"  => RedirectToAction("SanatciDashboard"),
            "Egitmen"  => RedirectToAction("EgitmenDashboard"),
            "Musteri"  => RedirectToAction("MusteriDashboard"),
            _ => RedirectToAction("Index", "Home")
        };
    }
}