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
            var kampanyalar = await _context.Kampanyalar
                .Where(k => k.Aktif && k.BitisTarihi >= DateTime.UtcNow)
                .ToListAsync();
            return View(kampanyalar);
        }

        public async Task<IActionResult> Detay(int id)
        {
            var kampanya = await _context.Kampanyalar.FindAsync(id);
            if (kampanya == null) return NotFound();

            // Kampanyalı eserler (indirim oranına göre filtrelenebilir veya kategoriye göre)
            // Şimdilik tüm eserleri ve etkinlikleri kampanya detayında gösteriyoruz
            ViewBag.Eserler = await _context.Artworks.Include(a => a.Artist).Where(a => a.Aktif).Take(8).ToListAsync();
            ViewBag.Etkinlikler = await _context.Etkinlikler.Include(e => e.Egitmen).Where(e => e.Aktif).Take(4).ToListAsync();

            return View(kampanya);
        }
    }
}
