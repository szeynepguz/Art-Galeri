using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class SiparisController : Controller
    {
        private readonly ApplicationDbContext _context;
        public SiparisController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Satin(int artworkId)
        {
            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                TempData["ErrorMessage"] = "Satın almak için giriş yapmanız gerekiyor.";
                return RedirectToAction("Login", "Users");
            }
            var artwork = await _context.Artworks.Include(a => a.Artist).FirstOrDefaultAsync(a => a.ArtworkID == artworkId);
            if (artwork == null) return NotFound();
            ViewBag.Kampanyalar = await _context.Kampanyalar.Where(k => k.Aktif && k.BaslangicTarihi <= DateTime.UtcNow && k.BitisTarihi >= DateTime.UtcNow).ToListAsync();
            return View(artwork);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int artworkId, string odemeYontemi, string? adres, string? kuponKodu)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Users");
            var artwork = await _context.Artworks.FindAsync(artworkId);
            if (artwork == null) return NotFound();
            var tutar = artwork.Price;
            int? kampanyaId = null;
            if (!string.IsNullOrEmpty(kuponKodu))
            {
                var k = await _context.Kampanyalar.FirstOrDefaultAsync(k => k.KuponKodu == kuponKodu && k.Aktif && k.BaslangicTarihi <= DateTime.UtcNow && k.BitisTarihi >= DateTime.UtcNow);
                if (k != null) { tutar = tutar * (1 - k.IndirimOrani / 100); kampanyaId = k.KampanyaID; }
            }
            _context.Siparisler.Add(new Siparis { UserID = userId.Value, ArtworkID = artworkId, Tutar = tutar, OdemeYontemi = odemeYontemi, Adres = adres, KampanyaID = kampanyaId, Durum = "Tamamlandi", SiparisTarihi = DateTime.UtcNow });
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Satın alma başarılı! {tutar:C2} ödeme tamamlandı.";
            return RedirectToAction("Siparislerim", "Users");
        }
    }
}
