using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class YorumController : Controller
    {
        private readonly ApplicationDbContext _context;

        public YorumController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: /Yorum/EkleEser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EkleEser(int artworkId, string icerik, int puan)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Yorum yapmak için giriş yapmanız gerekiyor.";
                return RedirectToAction("Detay", "Artwork", new { id = artworkId });
            }

            // Doğrulanmış satın alma kontrolü
            var satinAlindi = await _context.Siparisler
                .AnyAsync(s => s.UserID == userId && s.ArtworkID == artworkId && s.Durum == "Tamamlandi");

            var yorum = new Yorum
            {
                UserID = userId.Value,
                ArtworkID = artworkId,
                Icerik = icerik,
                Puan = Math.Clamp(puan, 1, 5),
                OlusturulmaTarihi = DateTime.UtcNow,
                Onaylandi = true,
                Dogrulanmis = satinAlindi
            };

            _context.Yorumlar.Add(yorum);

            // YorumSayisi güncelle
            var artwork = await _context.Artworks.FindAsync(artworkId);
            if (artwork != null) artwork.YorumSayisi++;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Yorumunuz eklendi!";
            return RedirectToAction("Detay", "Artwork", new { id = artworkId });
        }

        // POST: /Yorum/EkleEtkinlik
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EkleEtkinlik(int etkinlikId, string icerik, int puan)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                TempData["ErrorMessage"] = "Yorum yapmak için giriş yapmanız gerekiyor.";
                return RedirectToAction("Detay", "Etkinlik", new { id = etkinlikId });
            }

            // Katılım kontrolü
            var katildi = await _context.Rezervasyonlar
                .AnyAsync(r => r.UserID == userId && r.EtkinlikID == etkinlikId && r.Durum == "Onaylandi");

            if (!katildi)
            {
                TempData["ErrorMessage"] = "Yalnızca katıldığınız etkinlikler için yorum yapabilirsiniz.";
                return RedirectToAction("Detay", "Etkinlik", new { id = etkinlikId });
            }

            var yorum = new Yorum
            {
                UserID = userId.Value,
                EtkinlikID = etkinlikId,
                Icerik = icerik,
                Puan = Math.Clamp(puan, 1, 5),
                OlusturulmaTarihi = DateTime.UtcNow,
                Onaylandi = true,
                Dogrulanmis = true
            };

            _context.Yorumlar.Add(yorum);

            // Ortalama puan güncelle
            var etkinlik = await _context.Etkinlikler.FindAsync(etkinlikId);
            if (etkinlik != null)
            {
                var puanlar = await _context.Yorumlar.Where(y => y.EtkinlikID == etkinlikId).Select(y => y.Puan).ToListAsync();
                puanlar.Add(puan);
                etkinlik.OrtalamaPuan = puanlar.Average();
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Değerlendirmeniz eklendi!";
            return RedirectToAction("Detay", "Etkinlik", new { id = etkinlikId });
        }

        // POST: /Yorum/Faydali/5
        [HttpPost]
        public async Task<IActionResult> Faydali(int id)
        {
            var yorum = await _context.Yorumlar.FindAsync(id);
            if (yorum == null) return Json(new { success = false });
            yorum.FaydaliBulma++;
            await _context.SaveChangesAsync();
            return Json(new { success = true, count = yorum.FaydaliBulma });
        }

        // POST: /Yorum/YoneticiYanit  (Yalnızca Yönetici)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YoneticiYanit(int yorumId, string yanit, string? donusUrl)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici")
                return Forbid();

            var yorum = await _context.Yorumlar.FindAsync(yorumId);
            if (yorum == null) return NotFound();

            yorum.YoneticiYaniti = yanit;
            yorum.YanitTarihi = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yanıt eklendi.";
            return Redirect(donusUrl ?? "/Users/YoneticiDashboard");
        }

        // POST: /Yorum/Sil/5  (Yalnızca Yönetici)
        [HttpPost]
        public async Task<IActionResult> Sil(int id, string? donusUrl)
        {
            if (HttpContext.Session.GetString("UserRole") != "Yonetici")
                return Forbid();

            var yorum = await _context.Yorumlar.FindAsync(id);
            if (yorum != null)
            {
                _context.Yorumlar.Remove(yorum);
                await _context.SaveChangesAsync();
            }
            TempData["SuccessMessage"] = "Yorum silindi.";
            return Redirect(donusUrl ?? "/Users/YoneticiDashboard");
        }
    }
}
