using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class YorumController : Controller
    {
        private readonly ApplicationDbContext _context;
        public YorumController(ApplicationDbContext context) { _context = context; }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Ekle(int? artworkId, int? etkinlikId, string icerik, int puan)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Json(new { success = false, message = "Yorum yapmak için giriş yapmalısınız." });

            if (string.IsNullOrWhiteSpace(icerik))
                return Json(new { success = false, message = "Yorum içeriği boş olamaz." });

            // Doğrulama ve Güvenilirlik: Etkinlik yorumu yapabilmek için kullanıcının ilgili etkinliğe katılmış olması
            if (etkinlikId.HasValue)
            {
                var katildiMi = await _context.Rezervasyonlar
                    .AnyAsync(r => r.UserID == userId && r.EtkinlikID == etkinlikId && r.Durum != "Iptal");
                if (!katildiMi)
                    return Json(new { success = false, message = "Sadece etkinliğe katılan kullanıcılar değerlendirme yapabilir." });
            }

            var yorum = new Yorum
            {
                UserID = userId.Value,
                ArtworkID = artworkId,
                EtkinlikID = etkinlikId,
                Icerik = icerik,
                Puan = puan,
                OlusturulmaTarihi = DateTime.UtcNow,
                Onaylandi = true // Varsayılan olarak onaylı, istenirse admin onayına düşürülebilir
            };

            // Doğrulama Kontrolü (Satın almış mı veya rezerve etmiş mi?)
            if (artworkId.HasValue)
            {
                yorum.Dogrulanmis = await _context.Siparisler.AnyAsync(s => s.UserID == userId && s.ArtworkID == artworkId && s.Durum == "Tamamlandi");
            }
            else if (etkinlikId.HasValue)
            {
                yorum.Dogrulanmis = await _context.Rezervasyonlar.AnyAsync(r => r.UserID == userId && r.EtkinlikID == etkinlikId && r.Durum != "Iptal");
            }

            _context.Yorumlar.Add(yorum);
            
            // Ortalama puanı güncelle (Etkinlik için)
            if (etkinlikId.HasValue)
            {
                var etkinlik = await _context.Etkinlikler.FindAsync(etkinlikId);
                if (etkinlik != null)
                {
                    var yorumlar = await _context.Yorumlar.Where(y => y.EtkinlikID == etkinlikId).Select(y => y.Puan).ToListAsync();
                    yorumlar.Add(puan);
                    etkinlik.OrtalamaPuan = yorumlar.Average();
                }
            }
            
            // Ortalama puanı güncelle (Artwork için)
            if (artworkId.HasValue)
            {
                var artwork = await _context.Artworks.FindAsync(artworkId);
                if (artwork != null)
                {
                    var yorumlar = await _context.Yorumlar.Where(y => y.ArtworkID == artworkId).Select(y => y.Puan).ToListAsync();
                    yorumlar.Add(puan);
                    artwork.OrtalamaPuan = yorumlar.Average();
                    artwork.YorumSayisi = yorumlar.Count;
                }
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Yorumunuz başarıyla eklendi." });
        }

        [HttpPost]
        public async Task<IActionResult> Faydali(int id)
        {
            var yorum = await _context.Yorumlar.FindAsync(id);
            if (yorum == null) return Json(new { success = false });
            yorum.FaydaliBulma++;
            await _context.SaveChangesAsync();
            return Json(new { success = true, count = yorum.FaydaliBulma });
        }

        // AJAX tabanlı yanıtla (Artwork/Etkinlik detay sayfalarından)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Yanitla(int yorumId, string yanit)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Yonetici" && userRole != "Egitmen" && userRole != "Sanatci")
                return Json(new { success = false, message = "Yetkiniz yok." });

            var yorum = await _context.Yorumlar.FindAsync(yorumId);
            if (yorum == null) return Json(new { success = false, message = "Yorum bulunamadı." });

            yorum.YoneticiYaniti = yanit;
            yorum.YanitTarihi = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        // Form tabanlı yönetici yanıtı (YoneticiDashboard'dan)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YoneticiYanit(int yorumId, string yanit, string? donusUrl)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Yonetici" && userRole != "Egitmen")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return Redirect(donusUrl ?? "/Users/YoneticiDashboard");
            }

            var yorum = await _context.Yorumlar.FindAsync(yorumId);
            if (yorum == null)
            {
                TempData["ErrorMessage"] = "Yorum bulunamadı.";
                return Redirect(donusUrl ?? "/Users/YoneticiDashboard");
            }

            yorum.YoneticiYaniti = yanit;
            yorum.YanitTarihi = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Yanıt başarıyla eklendi.";
            return Redirect(donusUrl ?? "/Users/YoneticiDashboard");
        }

        // Yorum silme (Yönetici yetkisi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id, string? donusUrl)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return Redirect(donusUrl ?? "/Users/YoneticiDashboard");
            }

            var yorum = await _context.Yorumlar.FindAsync(id);
            if (yorum != null)
            {
                _context.Yorumlar.Remove(yorum);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Yorum başarıyla silindi.";
            }

            return Redirect(donusUrl ?? "/Users/YoneticiDashboard");
        }
    }
}
