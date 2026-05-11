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
                var kampanya = await _context.Kampanyalar
                    .FirstOrDefaultAsync(k => k.KuponKodu == kuponKodu && k.Aktif && k.BaslangicTarihi <= DateTime.UtcNow && k.BitisTarihi >= DateTime.UtcNow);
                if (kampanya != null)
                {
                    tutar = tutar * (1 - kampanya.IndirimOrani / 100);
                    TempData["SuccessMessage"] = $"'{kampanya.Ad}' kuponu uygulandı! %{kampanya.IndirimOrani} indirim.";
                }
            }

            model.UserID = userId.Value;
            model.ToplamTutar = tutar;
            model.RezervasyonTarihi = DateTime.UtcNow;
            model.Durum = "Onaylandi";

            etkinlik.RezervasyonSayisi += model.KatilimciSayisi;

            _context.Rezervasyonlar.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Rezervasyonunuz başarıyla oluşturuldu! Toplam: {tutar:C2}";
            return RedirectToAction("Rezervasyonlarim", "Users");
        }

        // GET: /Etkinlik/Karsilastir?ids=1,2
        public async Task<IActionResult> Karsilastir(string ids)
        {
            if (string.IsNullOrEmpty(ids)) return View(new List<Etkinlik>());
            var idList = ids.Split(',').Select(int.Parse).ToList();
            var etkinlikler = await _context.Etkinlikler.Include(e => e.Egitmen).Where(e => idList.Contains(e.EtkinlikID)).ToListAsync();
            return View(etkinlikler);
        }
    }
}
