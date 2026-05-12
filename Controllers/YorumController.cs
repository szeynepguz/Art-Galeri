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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Yanitla(int yorumId, string yanit)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Yonetici" && userRole != "Egitmen")
                return Json(new { success = false, message = "Yetkiniz yok." });

            var yorum = await _context.Yorumlar.FindAsync(yorumId);
            if (yorum == null) return Json(new { success = false, message = "Yorum bulunamadı." });

            yorum.YoneticiYaniti = yanit;
            yorum.YanitTarihi = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }
    }
}
