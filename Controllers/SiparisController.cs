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

        // GET: /Siparis/Satin/5  VEYA  /Siparis/Satin?artworkId=5
        public async Task<IActionResult> Satin(int? id, int? artworkId)
        {
            // Hem /Siparis/Satin/5 hem de /Siparis/Satin?artworkId=5 destekle
            var eserID = id ?? artworkId ?? 0;

            if (HttpContext.Session.GetInt32("UserID") == null)
            {
                TempData["ErrorMessage"] = "Satın almak için giriş yapmanız gerekiyor.";
                return RedirectToAction("Login", "Users");
            }
            if (HttpContext.Session.GetString("UserRole") != "Musteri")
            {
                TempData["ErrorMessage"] = "Sadece Müşteri hesabı olanlar eser satın alabilir.";
                return RedirectToAction("Index", "Artwork");
            }
            var artwork = await _context.Artworks.Include(a => a.Artist).FirstOrDefaultAsync(a => a.ArtworkID == eserID);
            if (artwork == null) return NotFound();

            // Kullanıcı rolüne uygun aktif kampanyaları getir
            var userId = HttpContext.Session.GetInt32("UserID");
            var user = await _context.Users.FindAsync(userId);
            var kampanyalar = await _context.Kampanyalar
                .Where(k => k.Aktif && k.BaslangicTarihi.Date <= DateTime.UtcNow.Date && k.BitisTarihi.Date >= DateTime.UtcNow.Date)
                .Where(k => k.HedefRolID == null || k.HedefRolID == (user != null ? user.RolID : 0))
                .ToListAsync();
            ViewBag.Kampanyalar = kampanyalar;

            return View(artwork);
        }

        // POST: /Siparis/Onayla — Siparişi onaylama
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int artworkId, string odemeYontemi, string? adres, string? kuponKodu)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Users");
            
            if (HttpContext.Session.GetString("UserRole") != "Musteri")
            {
                TempData["ErrorMessage"] = "Sadece Müşteri hesabı olanlar eser satın alabilir.";
                return RedirectToAction("Index", "Artwork");
            }
            var artwork = await _context.Artworks.FindAsync(artworkId);
            if (artwork == null) return NotFound();
            var tutar = artwork.Price;
            int? kampanyaId = null;

            // İndirim kuponu uygulama
            if (!string.IsNullOrEmpty(kuponKodu))
            {
                var normalizedKupon = kuponKodu.Trim().ToUpper();
                var user = await _context.Users.FindAsync(userId);
                var k = await _context.Kampanyalar.FirstOrDefaultAsync(k =>
                    k.KuponKodu.ToUpper() == normalizedKupon && k.Aktif &&
                    k.BaslangicTarihi.Date <= DateTime.UtcNow.Date && k.BitisTarihi.Date >= DateTime.UtcNow.Date &&
                    (k.HedefRolID == null || k.HedefRolID == (user != null ? user.RolID : 0)));
                if (k != null)
                {
                    tutar = tutar * (1 - k.IndirimOrani / 100);
                    kampanyaId = k.KampanyaID;
                }
            }

            // Ödeme yöntemine göre durum belirleme
            var durum = odemeYontemi switch
            {
                "KrediKarti" => "Tamamlandi",     // Kart ödemesi anında tamamlanır
                "HavaleEFT" => "OdemeBekleniyor",  // Havale/EFT onay bekler
                "KapidaOdeme" => "Onaylandi",       // Kapıda ödeme onaylanır, ödeme teslimatta
                _ => "Beklemede"
            };

            _context.Siparisler.Add(new Siparis
            {
                UserID = userId.Value,
                ArtworkID = artworkId,
                Tutar = tutar,
                OdemeYontemi = odemeYontemi,
                Adres = adres,
                KampanyaID = kampanyaId,
                Durum = durum,
                SiparisTarihi = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            var durumMesaj = durum switch
            {
                "Tamamlandi" => "Ödemeniz alındı ve siparişiniz tamamlandı!",
                "OdemeBekleniyor" => "Siparişiniz oluşturuldu. Havale/EFT onayı bekleniyor.",
                "Onaylandi" => "Siparişiniz oluşturuldu. Kapıda ödeme yapacaksınız.",
                _ => "Siparişiniz alındı."
            };
            TempData["SuccessMessage"] = $"{durumMesaj} Tutar: {tutar:N0} ₺";
            return RedirectToAction("Siparislerim", "Users");
        }

        // GET: /Siparis/Detay/5 — Sipariş durumu kontrol
        public async Task<IActionResult> Detay(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Users");

            var siparis = await _context.Siparisler
                .Include(s => s.Artwork).ThenInclude(a => a!.Artist)
                .Include(s => s.Kampanya)
                .FirstOrDefaultAsync(s => s.SiparisID == id && s.UserID == userId);

            if (siparis == null) return NotFound();
            return View(siparis);
        }
    }
}
