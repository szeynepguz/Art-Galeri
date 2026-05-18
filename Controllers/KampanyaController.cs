using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class KampanyaController : Controller
    {
        private readonly ApplicationDbContext _context;
        public KampanyaController(ApplicationDbContext context) { _context = context; }

        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var user = userId.HasValue ? await _context.Users.FindAsync(userId) : null;
            var userRolId = user?.RolID;

            var kampanyalar = await _context.Kampanyalar
                .Include(k => k.HedefRol)
                .Where(k => k.Aktif && k.BitisTarihi.Date >= DateTime.UtcNow.Date)
                .ToListAsync();

            // Sadece yetkili/belirlenmiş kişiye gösterilecek şekilde filtrele
            var filtrelenmis = kampanyalar
                .Where(k => (k.TargetUserID == null || k.TargetUserID == userId) && 
                            (k.HedefRolID == null || k.HedefRolID == userRolId))
                .ToList();

            ViewBag.GenelKampanyalar = filtrelenmis.Where(k => k.HedefRolID == null && k.TargetUserID == null).ToList();
            ViewBag.OzelKampanyalar = filtrelenmis.Where(k => k.HedefRolID != null || k.TargetUserID != null).ToList();

            return View(filtrelenmis);
        }

        public async Task<IActionResult> Detay(int id)
        {
            var kampanya = await _context.Kampanyalar
                .Include(k => k.HedefRol)
                .FirstOrDefaultAsync(k => k.KampanyaID == id);
            if (kampanya == null) return NotFound();

            // Kampanyalı eserler ve etkinlikler
            ViewBag.Eserler = await _context.Artworks.Include(a => a.Artist).Where(a => a.Aktif).Take(8).ToListAsync();
            ViewBag.Etkinlikler = await _context.Etkinlikler.Include(e => e.Egitmen).Where(e => e.Aktif).Take(4).ToListAsync();

            return View(kampanya);
        }

        // Yönetici kampanya oluşturma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(string ad, string aciklama, decimal indirimOrani, string kuponKodu,
            DateTime baslangicTarihi, DateTime bitisTarihi, int? hedefRolId, int? targetUserId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index");
            }

            // Kupon kodu benzersiz mi kontrol et
            var normalizedKupon = kuponKodu.Trim().ToUpper();
            if (await _context.Kampanyalar.AnyAsync(k => k.KuponKodu.ToUpper() == normalizedKupon))
            {
                TempData["ErrorMessage"] = "Bu kupon kodu zaten mevcut.";
                return RedirectToAction("Index", "Users", new { }, "kampanyalar");
            }

            var kampanya = new Kampanya
            {
                Ad = ad,
                Aciklama = aciklama,
                IndirimOrani = indirimOrani,
                KuponKodu = normalizedKupon,
                BaslangicTarihi = baslangicTarihi.Date.ToUniversalTime(),
                BitisTarihi = bitisTarihi.Date.AddDays(1).AddSeconds(-1).ToUniversalTime(),
                HedefRolID = hedefRolId,
                TargetUserID = targetUserId,
                Aktif = true
            };

            _context.Kampanyalar.Add(kampanya);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kampanya başarıyla oluşturuldu!";
            return RedirectToAction("YoneticiDashboard", "Users");
        }
        [HttpGet]
        public async Task<IActionResult> GetCouponInfo(string code)
        {
            if (string.IsNullOrEmpty(code)) return Json(new { valid = false, message = "Lütfen kupon kodu girin." });

            var normalizedCode = code.Trim().ToUpper();
            var userId = HttpContext.Session.GetInt32("UserID");
            var user = userId.HasValue ? await _context.Users.FindAsync(userId) : null;
            var isAdmin = HttpContext.Session.GetString("UserRole") == "Yonetici";

            // Kuponu bul (Sadece koda göre)
            var k = await _context.Kampanyalar.FirstOrDefaultAsync(k => k.KuponKodu.ToUpper() == normalizedCode);

            if (k == null)
            {
                return Json(new { valid = false, message = "Böyle bir kupon kodu bulunamadı." });
            }

            if (!k.Aktif)
            {
                return Json(new { valid = false, message = "Bu kupon şu an aktif değil." });
            }

            var now = DateTime.UtcNow.Date;
            if (k.BaslangicTarihi.Date > now)
            {
                return Json(new { valid = false, message = $"Bu kupon henüz başlamadı. Başlangıç: {k.BaslangicTarihi.ToLocalTime():dd.MM.yyyy}" });
            }

            if (k.BitisTarihi.Date < now)
            {
                return Json(new { valid = false, message = "Bu kuponun süresi dolmuş." });
            }

            // Rol kontrolü (Yönetici değilse)
            if (!isAdmin)
            {
                if (k.TargetUserID != null)
                {
                    if (k.TargetUserID != userId)
                    {
                        return Json(new { valid = false, message = "Bu kupon sadece belirlenmiş bir kullanıcıya özeldir." });
                    }
                }
                else if (k.HedefRolID != null)
                {
                    var userRolId = user != null ? user.RolID : 0;
                    if (k.HedefRolID != userRolId)
                    {
                        return Json(new { valid = false, message = "Bu kupon sizin hesabınız için geçerli değil." });
                    }
                }
            }

            return Json(new { valid = true, discount = k.IndirimOrani, name = k.Ad });
        }
    }
}
