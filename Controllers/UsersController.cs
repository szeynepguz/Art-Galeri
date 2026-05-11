using Microsoft.AspNetCore.Mvc;
using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

namespace art_galeri.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Kullanıcıları listeleyen sayfa: /Users
        public async Task<IActionResult> Index()
        {
            return View(await _context.Users.ToListAsync());
        }

        // 2. Yeni kayıt sayfası (Görüntüleme): /Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // 3. Kaydet butonu (Post): /Users/Create
        [HttpPost]
        public async Task<IActionResult> Create(User user)
        {
            user.Role = "Musteri";
            user.CreatedAt = DateTime.UtcNow;

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return Content("Hata: " + ex.Message);
            }
        }

        // 4. Giriş Sayfası (Görüntüleme): /Users/Login
        public IActionResult Login()
        {
            return View();
        }

        // 5. Giriş Yap Butonu (Post): /Users/Login
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

            if (user != null)
            {
                // Giriş başarılı! Rolüne göre yönlendiriyoruz:
                return user.Role switch
                {
                    "Admin" => RedirectToAction("AdminDashboard", "Admin"),
                    "Sanatci" => RedirectToAction("ArtistPanel", "Artist"),
                    "Egitmen" => RedirectToAction("WorkshopManager", "Workshop"),
                    _ => RedirectToAction("Index", "Home") // Müşteriler ana sayfaya
                };
            }

            ViewBag.Error = "E-posta veya şifre hatalı!";
            return View();
        }
    }
}