using Microsoft.AspNetCore.Mvc;

namespace art_galeri.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View(); // Views/Home/Index.cshtml dosyasını açar
        }
    }
}