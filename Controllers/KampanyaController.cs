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
            var userRolId = HttpContext.Session.GetInt32("UserID") != null
                ? (await _context.Users.FindAsync(HttpContext.Session.GetInt32("UserID")))?.RolID
                : (int?)null;

            var kampanyalar = await _context.Kampanyalar
                .Include(k => k.HedefRol)
                .Where(k => k.Aktif && k.BitisTarihi >= DateTime.UtcNow)
                .ToListAsync();

            // Belirli kullanıcılara özel fırsatlar: HedefRolID null = herkese, aksi halde sadece o role
            var filtrelenmis = kampanyalar
                .Where(k => k.HedefRolID == null || k.HedefRolID == userRolId)
                .ToList();

            ViewBag.GenelKampanyalar = filtrelenmis.Where(k => k.HedefRolID == null).ToList();
            ViewBag.OzelKampanyalar = filtrelenmis.Where(k => k.HedefRolID != null).ToList();

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
            DateTime baslangicTarihi, DateTime bitisTarihi, int? hedefRolId)
        {
            var userRole = HttpContext.Session.GetString("UserRole");
            if (userRole != "Yonetici")
            {
                TempData["ErrorMessage"] = "Bu işlem için yetkiniz yok.";
                return RedirectToAction("Index");
            }

            // Kupon kodu benzersiz mi kontrol et
            if (await _context.Kampanyalar.AnyAsync(k => k.KuponKodu == kuponKodu))
            {
                TempData["ErrorMessage"] = "Bu kupon kodu zaten mevcut.";
                return RedirectToAction("Index", "Users", new { }, "kampanyalar");
            }

            var kampanya = new Kampanya
            {
                Ad = ad,
                Aciklama = aciklama,
                IndirimOrani = indirimOrani,
                KuponKodu = kuponKodu,
                BaslangicTarihi = baslangicTarihi.ToUniversalTime(),
                BitisTarihi = bitisTarihi.ToUniversalTime(),
                HedefRolID = hedefRolId,
                Aktif = true
            };

            _context.Kampanyalar.Add(kampanya);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Kampanya başarıyla oluşturuldu!";
            return RedirectToAction("YoneticiDashboard", "Users");
        }
    }
}
