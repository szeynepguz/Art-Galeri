using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class EtkinlikController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EtkinlikController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Etkinlik
        public async Task<IActionResult> Index(string? kategori, string? arama)
        {
            var query = _context.Etkinlikler
                .Include(e => e.Egitmen)
                .Where(e => e.Aktif);

            if (!string.IsNullOrEmpty(kategori))
                query = query.Where(e => e.Kategori == kategori);
            if (!string.IsNullOrEmpty(arama))
                query = query.Where(e => e.Ad.Contains(arama) || (e.Aciklama != null && e.Aciklama.Contains(arama)));

            ViewBag.Kategoriler = await _context.Etkinlikler.Where(e => e.Aktif && e.Kategori != null).Select(e => e.Kategori).Distinct().ToListAsync();
            ViewBag.SecilenKategori = kategori;

            return View(await query.OrderBy(e => e.Tarih).ToListAsync());
        }

        // GET: /Etkinlik/Detay/5
        public async Task<IActionResult> Detay(int id)
        {
            var etkinlik = await _context.Etkinlikler
                .Include(e => e.Egitmen)
                .Include(e => e.Yorumlar!).ThenInclude(y => y.User)
                .FirstOrDefaultAsync(e => e.EtkinlikID == id);

            if (etkinlik == null) return NotFound();

            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                ViewBag.RezerveEttiMi = await _context.Rezervasyonlar
                    .AnyAsync(r => r.UserID == userId && r.EtkinlikID == id && r.Durum != "Iptal");
            }

            return View(etkinlik);
        }

        // GET: /Etkinlik/Rezervasyon/5
        public async Task<IActionResult> Rezervasyon(int id)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                TempData["ErrorMessage"] = "Rezervasyon yapmak için giriş yapmanız gerekiyor.";
                return RedirectToAction("Login", "Users");
            }
            if (HttpContext.Session.GetString("UserRole") != "Musteri")
            {
                TempData["ErrorMessage"] = "Sadece Müşteri hesabı olanlar etkinliklere rezervasyon yapabilir.";
                return RedirectToAction("Index");
            }

            var etkinlik = await _context.Etkinlikler.FindAsync(id);
            if (etkinlik == null || !etkinlik.Aktif) return NotFound();
            if (etkinlik.KalanKapasite <= 0)
            {
                TempData["ErrorMessage"] = "Bu etkinlik için kapasite dolmuştur.";
                return RedirectToAction("Detay", new { id });
            }

            ViewBag.Etkinlik = etkinlik;
            return View(new Rezervasyon { EtkinlikID = id });
        }

        // POST: /Etkinlik/Rezervasyon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rezervasyon(Rezervasyon model, string? kuponKodu)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Users");

            if (HttpContext.Session.GetString("UserRole") != "Musteri")
            {
                TempData["ErrorMessage"] = "Sadece Müşteri hesabı olanlar etkinliklere rezervasyon yapabilir.";
                return RedirectToAction("Index");
            }

            var etkinlik = await _context.Etkinlikler.FindAsync(model.EtkinlikID);
            if (etkinlik == null) return NotFound();

            // Tekrar rezervasyon kontrolü
            var mevcutRez = await _context.Rezervasyonlar
                .AnyAsync(r => r.UserID == userId && r.EtkinlikID == model.EtkinlikID && r.Durum != "Iptal");
            if (mevcutRez)
            {
                TempData["ErrorMessage"] = "Bu etkinlik için zaten bir rezervasyonunuz var.";
                return RedirectToAction("Detay", new { id = model.EtkinlikID });
            }

            var tutar = etkinlik.Ucret * model.KatilimciSayisi;

            // Kupon kodu uygula
            if (!string.IsNullOrEmpty(kuponKodu))
            {
                var normalizedKupon = kuponKodu.Trim().ToUpper();
                var kampanya = await _context.Kampanyalar
                    .FirstOrDefaultAsync(k => k.KuponKodu.ToUpper() == normalizedKupon && k.Aktif && k.BaslangicTarihi.Date <= DateTime.UtcNow.Date && k.BitisTarihi.Date >= DateTime.UtcNow.Date);
                if (kampanya != null)
                {
                    tutar = tutar * (1 - kampanya.IndirimOrani / 100);
                    TempData["SuccessMessage"] = $"'{kampanya.Ad}' kuponu uygulandı! %{kampanya.IndirimOrani} indirim.";
                }
            }

            model.UserID = userId.Value;
            model.ToplamTutar = tutar;
            model.RezervasyonTarihi = DateTime.UtcNow;
            
            // Ödeme yöntemine göre durum belirle
            model.Durum = model.OdemeYontemi switch
            {
                "KrediKarti" => "Tamamlandi",
                "Havale" => "Odeme Bekleniyor",
                "Gise" => "Onaylandi", // Yerinde ödeme
                _ => "Beklemede"
            };

            etkinlik.RezervasyonSayisi += model.KatilimciSayisi;

            _context.Rezervasyonlar.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Rezervasyonunuz başarıyla oluşturuldu! Toplam: {tutar:C2}";
            return RedirectToAction("Rezervasyonlarim", "Users");
        }

        // GET: /Etkinlik/Karsilastir?ids=1,2
        public async Task<IActionResult> Karsilastir(string? ids)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId != null)
            {
                // Veritabanından kayıtlı karşılaştırmaları getir
                var kayitli = await _context.Karsilastirmalar
                    .Where(k => k.UserID == userId && k.Tip == "Etkinlik")
                    .OrderByDescending(k => k.KayitTarihi)
                    .ToListAsync();
                ViewBag.KayitliKarsilastirmalar = kayitli;
            }

            if (string.IsNullOrEmpty(ids)) return View(new List<Etkinlik>());

            var idList = ids.Split(',').Select(int.Parse).ToList();
            var etkinlikler = await _context.Etkinlikler.Include(e => e.Egitmen).Where(e => idList.Contains(e.EtkinlikID)).ToListAsync();

            return View(etkinlikler);
        }

        // POST: /Etkinlik/KarsilastirmaKaydet — Karşılaştırma sonuçlarını kaydetme
        [HttpPost]
        public async Task<IActionResult> KarsilastirmaKaydet(string ids, string baslik)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });

            var yeniKayit = new Karsilastirma
            {
                UserID = userId.Value,
                Tip = "Etkinlik",
                Baslik = baslik,
                IDler = ids,
                KayitTarihi = DateTime.UtcNow
            };

            _context.Karsilastirmalar.Add(yeniKayit);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Karşılaştırma kalıcı olarak kaydedildi!" });
        }

        // --- EĞİTMEN İŞLEMLERİ ---

        // GET: /Etkinlik/Olustur
        public IActionResult Olustur()
        {
            if (HttpContext.Session.GetString("UserRole") != "Egitmen")
                return RedirectToAction("Login", "Users");
            return View(new Etkinlik { Tarih = DateTime.Today.AddDays(7) });
        }

        // POST: /Etkinlik/Olustur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(Etkinlik model, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null || HttpContext.Session.GetString("UserRole") != "Egitmen")
                return RedirectToAction("Login", "Users");

            model.EgitmenID = userId.Value;
            model.OlusturulmaTarihi = DateTime.UtcNow;
            model.Tarih = model.Tarih.ToUniversalTime();

            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                using var fs = new FileStream(path, FileMode.Create);
                await imageFile.CopyToAsync(fs);
                model.GorselUrl = "/uploads/" + fileName;
            }

            ModelState.Remove("Egitmen");
            ModelState.Remove("Rezervasyonlar");
            ModelState.Remove("Yorumlar");

            if (!ModelState.IsValid) return View(model);

            _context.Etkinlikler.Add(model);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Etkinlik başarıyla oluşturuldu!";
            return RedirectToAction("EgitmenDashboard", "Users");
        }

        // GET: /Etkinlik/Duzenle/5
        public async Task<IActionResult> Duzenle(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var etkinlik = await _context.Etkinlikler.FirstOrDefaultAsync(e => e.EtkinlikID == id && e.EgitmenID == userId);
            if (etkinlik == null) return NotFound();
            return View(etkinlik);
        }

        // POST: /Etkinlik/Duzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, Etkinlik model, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var etkinlik = await _context.Etkinlikler.FirstOrDefaultAsync(e => e.EtkinlikID == id && e.EgitmenID == userId);
            if (etkinlik == null) return NotFound();

            etkinlik.Ad = model.Ad;
            etkinlik.Aciklama = model.Aciklama;
            etkinlik.Tarih = model.Tarih.ToUniversalTime();
            etkinlik.Saat = model.Saat;
            etkinlik.Konum = model.Konum;
            etkinlik.Ucret = model.Ucret;
            etkinlik.Kapasite = model.Kapasite;
            etkinlik.Kategori = model.Kategori;
            etkinlik.Aktif = model.Aktif;

            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                using var fs = new FileStream(path, FileMode.Create);
                await imageFile.CopyToAsync(fs);
                etkinlik.GorselUrl = "/uploads/" + fileName;
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Etkinlik güncellendi!";
            return RedirectToAction("EgitmenDashboard", "Users");
        }

        // POST: /Etkinlik/Sil/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var etkinlik = await _context.Etkinlikler.FirstOrDefaultAsync(e => e.EtkinlikID == id && e.EgitmenID == userId);
            if (etkinlik != null)
            {
                _context.Etkinlikler.Remove(etkinlik);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik silindi.";
            }
            return RedirectToAction("EgitmenDashboard", "Users");
        }
    }
}
