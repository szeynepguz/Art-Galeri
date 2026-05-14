using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class ArtworkController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ArtworkController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /Artwork  — Tüm eserler listesi
        public async Task<IActionResult> Index(string? kategori, string? arama, string? sirala)
        {
            var query = _context.Artworks
                .Include(a => a.Artist)
                .Where(a => a.Aktif);

            if (!string.IsNullOrEmpty(kategori))
                query = query.Where(a => a.Kategori == kategori);

            if (!string.IsNullOrEmpty(arama))
                query = query.Where(a => a.Title!.Contains(arama) || a.Description!.Contains(arama));

            query = sirala switch
            {
                "fiyat_asc"   => query.OrderBy(a => a.Price),
                "fiyat_desc"  => query.OrderByDescending(a => a.Price),
                "populer"     => query.OrderByDescending(a => a.BegeniSayisi),
                "yeni"        => query.OrderByDescending(a => a.UploadDate),
                _ => query.OrderByDescending(a => a.UploadDate)
            };

            ViewBag.Kategoriler = await _context.Artworks.Where(a => a.Aktif && a.Kategori != null).Select(a => a.Kategori).Distinct().ToListAsync();
            ViewBag.SecilenKategori = kategori;
            ViewBag.Arama = arama;
            ViewBag.Sirala = sirala;

            return View(await query.ToListAsync());
        }

        // GET: /Artwork/Detay/5
        public async Task<IActionResult> Detay(int id)
        {
            var artwork = await _context.Artworks
                .Include(a => a.Artist)
                .Include(a => a.Yorumlar!).ThenInclude(y => y.User)
                .FirstOrDefaultAsync(a => a.ArtworkID == id);

            if (artwork == null) return NotFound();

            // Görüntülenme artır
            artwork.GoruntulenmeSayisi++;
            await _context.SaveChangesAsync();

            // Kullanıcı favoriye eklemiş mi?
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId.HasValue)
            {
                ViewBag.FavoriMi = await _context.Favoriler.AnyAsync(f => f.UserID == userId && f.ArtworkID == id);
                ViewBag.SatinAlindiMi = await _context.Siparisler.AnyAsync(s => s.UserID == userId && s.ArtworkID == id && s.Durum == "Tamamlandi");
            }

            ViewBag.BenzerEserler = await _context.Artworks
                .Where(a => a.Kategori == artwork.Kategori && a.ArtworkID != id && a.Aktif)
                .Take(4).ToListAsync();

            return View(artwork);
        }

        // POST: /Artwork/Begen/5
        [HttpPost]
        public async Task<IActionResult> Begen(int id)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });

            var artwork = await _context.Artworks.FindAsync(id);
            if (artwork == null) return Json(new { success = false });

            artwork.BegeniSayisi++;
            await _context.SaveChangesAsync();
            return Json(new { success = true, count = artwork.BegeniSayisi });
        }

        // GET: /Artwork/Yukle — Sanatçı eser yükleme
        public IActionResult Yukle()
        {
            if (HttpContext.Session.GetString("UserRole") != "Sanatci")
                return RedirectToAction("Login", "Users");
            return View();
        }

        // POST: /Artwork/Yukle
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Yukle(Artwork artwork, IFormFile? imageFile)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null || HttpContext.Session.GetString("UserRole") != "Sanatci")
                return RedirectToAction("Login", "Users");

            artwork.ArtistID = userId.Value;
            artwork.UploadDate = DateTime.UtcNow;

            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(_env.WebRootPath, "uploads", fileName);
                using var fs = new FileStream(path, FileMode.Create);
                await imageFile.CopyToAsync(fs);
                artwork.ImageUrl = "/uploads/" + fileName;
            }

            ModelState.Remove("Artist");
            ModelState.Remove("Yorumlar");
            ModelState.Remove("Favoriler");
            ModelState.Remove("Siparisler");

            if (!ModelState.IsValid)
                return View(artwork);

            _context.Artworks.Add(artwork);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Eser başarıyla yüklendi!";
            return RedirectToAction("SanatciDashboard", "Users");
        }

        // GET: /Artwork/Karsilastir?ids=1,2,3
        public async Task<IActionResult> Karsilastir(string? ids)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId != null)
            {
                var kayitli = await _context.Karsilastirmalar
                    .Where(k => k.UserID == userId && k.Tip == "Artwork")
                    .OrderByDescending(k => k.KayitTarihi)
                    .ToListAsync();
                ViewBag.KayitliKarsilastirmalar = kayitli;
            }

            if (string.IsNullOrEmpty(ids)) return View(new List<Artwork>());
            var idList = ids.Split(',').Select(int.Parse).ToList();
            var eserler = await _context.Artworks.Include(a => a.Artist).Where(a => idList.Contains(a.ArtworkID)).ToListAsync();

            return View(eserler);
        }

        // POST: /Artwork/KarsilastirmaKaydet — Karşılaştırma sonuçlarını kaydetme
        [HttpPost]
        public async Task<IActionResult> KarsilastirmaKaydet(string ids, string baslik)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });

            var yeniKayit = new Karsilastirma
            {
                UserID = userId.Value,
                Tip = "Artwork",
                Baslik = baslik,
                IDler = ids,
                KayitTarihi = DateTime.UtcNow
            };

            _context.Karsilastirmalar.Add(yeniKayit);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Eser karşılaştırması kalıcı olarak kaydedildi!" });
        }

        // GET: /Artwork/KayitliKarsilastirmalar — Kayıtlı karşılaştırmaları listele
        public IActionResult KayitliKarsilastirmalar()
        {
            var kayitli = HttpContext.Session.GetString("KayitliEserKarsilastirma") ?? "";
            var list = kayitli.Split(";;;", StringSplitOptions.RemoveEmptyEntries)
                .Select(k => {
                    var parts = k.Split('|');
                    return new { Baslik = parts[0], Ids = parts.Length > 1 ? parts[1] : "", Tarih = parts.Length > 2 ? parts[2] : "" };
                }).ToList();
            ViewBag.Kayitlar = list;
            return View();
        }
    }
}
