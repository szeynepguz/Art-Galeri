using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class FavoriController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FavoriController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Favori
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Users");

            var favoriler = await _context.Favoriler
                .Include(f => f.Artwork).ThenInclude(a => a!.Artist)
                .Where(f => f.UserID == userId)
                .OrderByDescending(f => f.EklenmeTarihi)
                .ToListAsync();

            return View(favoriler);
        }

        // POST: /Favori/Ekle/5
        [HttpPost]
        public async Task<IActionResult> Ekle(int artworkId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });

            var mevcut = await _context.Favoriler
                .AnyAsync(f => f.UserID == userId && f.ArtworkID == artworkId);

            if (mevcut)
                return Json(new { success = false, message = "Zaten favorilerde." });

            _context.Favoriler.Add(new Favori { UserID = userId.Value, ArtworkID = artworkId });
            var artwork = await _context.Artworks.FindAsync(artworkId);
            if (artwork != null) artwork.BegeniSayisi++;
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Favorilere eklendi!" });
        }

        // POST: /Favori/Cikar/5
        [HttpPost]
        public async Task<IActionResult> Cikar(int artworkId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    return Json(new { success = false });
                return RedirectToAction("Login", "Users");
            }

            var favori = await _context.Favoriler
                .FirstOrDefaultAsync(f => f.UserID == userId && f.ArtworkID == artworkId);

            if (favori != null)
            {
                _context.Favoriler.Remove(favori);
                var artwork = await _context.Artworks.FindAsync(artworkId);
                if (artwork != null && artwork.BegeniSayisi > 0) artwork.BegeniSayisi--;
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Favorilerden çıkarıldı.";

            // AJAX çağrısı ise JSON, değilse redirect
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return Json(new { success = true });

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer)) return Redirect(referer);
            return RedirectToAction("Index");
        }
    }
}
