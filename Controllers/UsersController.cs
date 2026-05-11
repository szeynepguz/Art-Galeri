using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Kullanicilari listeleyen sayfa: /Users
        public async Task<IActionResult> Index()
        {
            var users = await _context.Users.Include(u => u.Rol).ToListAsync();
            return View(users);
        }

        // ==========================================
        //  KAYIT OL (Sign-Up)
        // ==========================================

        // GET: /Users/Register
        public IActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        // POST: /Users/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Kullanici tipi validasyonu
            var gecerliTipler = new[] { "Musteri", "Sanatci", "Egitmen" };
            if (!gecerliTipler.Contains(model.KullaniciTipi))
            {
                ModelState.AddModelError("KullaniciTipi", "Gecersiz kullanici tipi.");
            }

            // Email benzersizlik kontrolu
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayitli.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Rolu bul
            var rol = await _context.Roller.FirstOrDefaultAsync(r => r.RolAdi == model.KullaniciTipi);
            if (rol == null)
            {
                ModelState.AddModelError("", "Sistem hatasi: Rol bulunamadi.");
                return View(model);
            }

            // Yeni kullanici olustur
            var user = new User
            {
                Ad = model.Ad,
                Soyad = model.Soyad,
                Email = model.Email,
                Sifre = model.Sifre, // Not: Production'da hash'lenmeli
                RolID = rol.RolID,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Role gore ek profil olustur
            switch (model.KullaniciTipi)
            {
                case "Sanatci":
                    var sanatciProfil = new SanatciProfil
                    {
                        UserID = user.UserID,
                        PortfolyoLinki = model.PortfolyoLinki,
                        SanatDali = model.SanatDali,
                        Ozgecmis = model.Ozgecmis
                    };
                    _context.SanatciProfiller.Add(sanatciProfil);
                    break;

                case "Egitmen":
                    var egitmenProfil = new EgitmenProfil
                    {
                        UserID = user.UserID,
                        UzmanlikAlani = model.UzmanlikAlani,
                        DeneyimYili = model.DeneyimYili,
                        Biyografi = model.Biyografi
                    };
                    _context.EgitmenProfiller.Add(egitmenProfil);
                    break;

                case "Musteri":
                    var musteriProfil = new MusteriProfil
                    {
                        UserID = user.UserID
                    };
                    _context.MusteriProfiller.Add(musteriProfil);
                    break;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Kayit basarili! Giris yapabilirsiniz.";
            return RedirectToAction("Login");
        }

        // Eski Create action - Register'a yonlendir
        public IActionResult Create()
        {
            return RedirectToAction("Register");
        }

        // ==========================================
        //  GIRIS YAP (Login)
        // ==========================================

        // GET: /Users/Login
        public IActionResult Login()
        {
            // Zaten giris yapmis kullaniciyi yonlendir
            if (HttpContext.Session.GetInt32("UserID") != null)
            {
                return RedirectByRole(HttpContext.Session.GetString("UserRole") ?? "");
            }
            return View(new LoginViewModel());
        }

        // POST: /Users/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email == model.Email && u.Sifre == model.Sifre);

            if (user == null)
            {
                ModelState.AddModelError("", "E-posta veya sifre hatali!");
                return View(model);
            }

            // Session'a kullanici bilgilerini kaydet
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetString("UserAd", user.Ad);
            HttpContext.Session.SetString("UserSoyad", user.Soyad);
            HttpContext.Session.SetString("UserEmail", user.Email);
            HttpContext.Session.SetString("UserRole", user.Rol?.RolAdi ?? "Musteri");

            TempData["SuccessMessage"] = $"Hos geldiniz, {user.Ad} {user.Soyad}!";

            // Role gore yonlendirme
            return RedirectByRole(user.Rol?.RolAdi ?? "Musteri");
        }

        // ==========================================
        //  CIKIS YAP (Logout)
        // ==========================================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Basariyla cikis yaptiniz.";
            return RedirectToAction("Login");
        }

        // ==========================================
        //  DASHBOARD SAYFALARI
        // ==========================================

        // Musteri: Eser inceleme ve favori listesi
        public async Task<IActionResult> MusteriDashboard()
        {
            if (!IsLoggedIn()) return RedirectToAction("Login");

            var artworks = await _context.Artworks.ToListAsync();
            ViewBag.UserName = $"{HttpContext.Session.GetString("UserAd")} {HttpContext.Session.GetString("UserSoyad")}";
            return View(artworks);
        }

        // Yonetici: Ozet rapor ve yorum paneli
        public async Task<IActionResult> YoneticiDashboard()
        {
            if (!IsLoggedIn() || HttpContext.Session.GetString("UserRole") != "Yonetici")
                return RedirectToAction("Login");

            ViewBag.UserName = $"{HttpContext.Session.GetString("UserAd")} {HttpContext.Session.GetString("UserSoyad")}";
            ViewBag.ToplamKullanici = await _context.Users.CountAsync();
            ViewBag.ToplamEser = await _context.Artworks.CountAsync();
            ViewBag.ToplamSanatci = await _context.Users.CountAsync(u => u.RolID == 4);
            ViewBag.ToplamEgitmen = await _context.Users.CountAsync(u => u.RolID == 3);
            ViewBag.ToplamMusteri = await _context.Users.CountAsync(u => u.RolID == 1);

            return View();
        }

        // Egitmen: Atolye kontenjan ve rezervasyon yonetimi
        public IActionResult EgitmenDashboard()
        {
            if (!IsLoggedIn() || HttpContext.Session.GetString("UserRole") != "Egitmen")
                return RedirectToAction("Login");

            ViewBag.UserName = $"{HttpContext.Session.GetString("UserAd")} {HttpContext.Session.GetString("UserSoyad")}";
            return View();
        }

        // Sanatci: Eser istatistik ve goruntuleme paneli
        public async Task<IActionResult> SanatciDashboard()
        {
            if (!IsLoggedIn() || HttpContext.Session.GetString("UserRole") != "Sanatci")
                return RedirectToAction("Login");

            var userId = HttpContext.Session.GetInt32("UserID") ?? 0;
            var artworks = await _context.Artworks.Where(a => a.ArtistID == userId).ToListAsync();

            ViewBag.UserName = $"{HttpContext.Session.GetString("UserAd")} {HttpContext.Session.GetString("UserSoyad")}";
            ViewBag.ToplamEser = artworks.Count;

            return View(artworks);
        }

        // ==========================================
        //  YARDIMCI METODLAR
        // ==========================================

        private bool IsLoggedIn()
        {
            return HttpContext.Session.GetInt32("UserID") != null;
        }

        private IActionResult RedirectByRole(string rolAdi)
        {
            return rolAdi switch
            {
                "Yonetici" => RedirectToAction("YoneticiDashboard"),
                "Sanatci" => RedirectToAction("SanatciDashboard"),
                "Egitmen" => RedirectToAction("EgitmenDashboard"),
                "Musteri" => RedirectToAction("MusteriDashboard"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}