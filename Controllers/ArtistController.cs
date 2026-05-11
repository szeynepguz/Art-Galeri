using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;

namespace art_galeri.Controllers
{
    public class ArtistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public ArtistController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // Sanatçı Paneli: Yüklediği eserleri görür
        public IActionResult ArtistPanel()
        {
            var artworks = _context.Artworks.ToList(); // Şimdilik hepsini çekelim
            return View(artworks);
        }

        // Yeni Eser Yükleme Sayfası
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Upload(Artwork artwork, IFormFile imageFile)
        {
            if (imageFile != null)
            {
                // Resmin kaydedileceği klasörü belirle
                string wwwRootPath = _hostEnvironment.WebRootPath;
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                string path = Path.Combine(wwwRootPath + "/uploads/", fileName);

                // Dosyayı klasöre kaydet
                using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                artwork.ImageUrl = "/uploads/" + fileName;
                _context.Artworks.Add(artwork);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(ArtistPanel));
            }
            return View();
        }
    }
}