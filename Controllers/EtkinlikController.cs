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
                .Include(e => e.Tarihler)
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
                .Include(e => e.Tarihler)
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
            if (HttpContext.Session.GetString("UserRole") != "Musteri")
            {
                TempData["ErrorMessage"] = "Sadece Müşteri hesabı olanlar etkinliklere rezervasyon yapabilir.";
                return RedirectToAction("Index");
            }

            var etkinlik = await _context.Etkinlikler
                .Include(e => e.Tarihler)
                .Include(e => e.Egitmen)
                .FirstOrDefaultAsync(e => e.EtkinlikID == id);
            if (etkinlik == null || !etkinlik.Aktif) return NotFound();

            // Müsait tarihleri filtrele (gelecek tarihli ve kapasitesi dolu olmayan)
            var musaitTarihler = etkinlik.Tarihler?
                .Where(t => t.Aktif && t.Tarih >= DateTime.UtcNow.Date && t.KalanKapasite > 0)
                .OrderBy(t => t.Tarih)
                .ToList() ?? new List<EtkinlikTarih>();

            // Eski tekli tarih sistemiyle uyumluluk: Eğer hiç tarih eklenmemişse, ana tarihi kontrol et
            if (!musaitTarihler.Any() && etkinlik.KalanKapasite <= 0 && (etkinlik.Tarihler == null || !etkinlik.Tarihler.Any()))
            {
                TempData["ErrorMessage"] = "Bu etkinlik için kapasite dolmuştur.";
                return RedirectToAction("Detay", new { id });
            }

            ViewBag.Etkinlik = etkinlik;
            ViewBag.MusaitTarihler = musaitTarihler;
            return View(new Rezervasyon { EtkinlikID = id });
        }

        // POST: /Etkinlik/Rezervasyon
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rezervasyon(Rezervasyon model, string? kuponKodu, int? secilenTarihId)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null) return RedirectToAction("Login", "Users");

            if (HttpContext.Session.GetString("UserRole") != "Musteri")
            {
                TempData["ErrorMessage"] = "Sadece Müşteri hesabı olanlar etkinliklere rezervasyon yapabilir.";
                return RedirectToAction("Index");
            }

            var etkinlik = await _context.Etkinlikler
                .Include(e => e.Tarihler)
                .FirstOrDefaultAsync(e => e.EtkinlikID == model.EtkinlikID);
            if (etkinlik == null) return NotFound();

            // Tekrar rezervasyon kontrolü
            var mevcutRez = await _context.Rezervasyonlar
                .AnyAsync(r => r.UserID == userId && r.EtkinlikID == model.EtkinlikID && r.Durum != "Iptal");
            if (mevcutRez)
            {
                TempData["ErrorMessage"] = "Bu etkinlik için zaten bir rezervasyonunuz var.";
                return RedirectToAction("Detay", new { id = model.EtkinlikID });
            }

            // Seçilen tarihi bul
            EtkinlikTarih? secilenTarih = null;
            if (secilenTarihId.HasValue)
            {
                secilenTarih = etkinlik.Tarihler?.FirstOrDefault(t => t.EtkinlikTarihID == secilenTarihId.Value);
                if (secilenTarih == null || !secilenTarih.Aktif || secilenTarih.KalanKapasite < model.KatilimciSayisi)
                {
                    TempData["ErrorMessage"] = "Seçilen tarih için yeterli kontenjan yok.";
                    return RedirectToAction("Rezervasyon", new { id = model.EtkinlikID });
                }
            }

            var tutar = etkinlik.Ucret * model.KatilimciSayisi;

            // Kupon kodu uygula
            if (!string.IsNullOrEmpty(kuponKodu))
            {
                var normalizedKupon = kuponKodu.Trim().ToUpper();
                var user = await _context.Users.FindAsync(userId);
                var kampanya = await _context.Kampanyalar
                    .FirstOrDefaultAsync(k => k.KuponKodu.ToUpper() == normalizedKupon && k.Aktif && 
                                              k.BaslangicTarihi.Date <= DateTime.UtcNow.Date && k.BitisTarihi.Date >= DateTime.UtcNow.Date &&
                                              (k.TargetUserID == null || k.TargetUserID == userId) &&
                                              (k.HedefRolID == null || k.HedefRolID == (user != null ? user.RolID : 0)));
                if (kampanya != null)
                {
                    tutar = tutar * (1 - kampanya.IndirimOrani / 100);
                    TempData["SuccessMessage"] = $"'{kampanya.Ad}' kuponu uygulandı! %{kampanya.IndirimOrani} indirim.";
                }
            }

            model.UserID = userId.Value;
            model.ToplamTutar = tutar;
            model.RezervasyonTarihi = DateTime.UtcNow;
            
            // Seçilen tarihi kaydet
            if (secilenTarih != null)
            {
                model.EtkinlikTarihID = secilenTarih.EtkinlikTarihID;
                secilenTarih.RezervasyonSayisi += model.KatilimciSayisi;
            }

            // Ana etkinlik rezervasyon sayısını da güncelle
            etkinlik.RezervasyonSayisi += model.KatilimciSayisi;
            
            // Ödeme yöntemine göre durum belirle
            model.Durum = model.OdemeYontemi switch
            {
                "KrediKarti" => "Tamamlandi",
                "Havale" => "Odeme Bekleniyor",
                "Gise" => "Onaylandi", // Yerinde ödeme
                _ => "Beklemede"
            };

            _context.Rezervasyonlar.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Rezervasyonunuz başarıyla oluşturuldu! Toplam: {tutar:C2}";
            return RedirectToAction("Rezervasyonlarim", "Users");
        }

        // GET: /Etkinlik/Karsilastir?ids=1,2
        public async Task<IActionResult> Karsilastir(string? ids)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId != null)
            {
                // Veritabanından kayıtlı karşılaştırmaları getir
                var kayitli = await _context.Karsilastirmalar
                    .Where(k => k.UserID == userId && k.Tip == "Etkinlik")
                    .OrderByDescending(k => k.KayitTarihi)
                    .ToListAsync();
                ViewBag.KayitliKarsilastirmalar = kayitli;
            }

            if (string.IsNullOrEmpty(ids)) return View(new List<Etkinlik>());

            var idList = ids.Split(',').Select(int.Parse).ToList();
            var etkinlikler = await _context.Etkinlikler.Include(e => e.Egitmen).Include(e => e.Tarihler).Where(e => idList.Contains(e.EtkinlikID)).ToListAsync();

            return View(etkinlikler);
        }

        // POST: /Etkinlik/KarsilastirmaKaydet — Karşılaştırma sonuçlarını kaydetme
        [HttpPost]
        public async Task<IActionResult> KarsilastirmaKaydet(string ids, string baslik)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
                return Json(new { success = false, message = "Giriş yapmanız gerekiyor." });

            var yeniKayit = new Karsilastirma
            {
                UserID = userId.Value,
                Tip = "Etkinlik",
                Baslik = baslik,
                IDler = ids,
                KayitTarihi = DateTime.UtcNow
            };

            _context.Karsilastirmalar.Add(yeniKayit);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Karşılaştırma kalıcı olarak kaydedildi!" });
        }

        // --- EĞİTMEN İŞLEMLERİ ---

        // GET: /Etkinlik/Olustur
        public IActionResult Olustur()
        {
            if (HttpContext.Session.GetString("UserRole") != "Egitmen")
                return RedirectToAction("Login", "Users");
            return View(new Etkinlik { Tarih = DateTime.Today.AddDays(7) });
        }

        // POST: /Etkinlik/Olustur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Olustur(Etkinlik model, IFormFile? imageFile, string? tarihlerJson)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null || HttpContext.Session.GetString("UserRole") != "Egitmen")
                return RedirectToAction("Login", "Users");

            model.EgitmenID = userId.Value;
            model.OlusturulmaTarihi = DateTime.UtcNow;
            model.Tarih = model.Tarih.ToUniversalTime();

            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                using var fs = new FileStream(path, FileMode.Create);
                await imageFile.CopyToAsync(fs);
                model.GorselUrl = "/uploads/" + fileName;
            }

            ModelState.Remove("Egitmen");
            ModelState.Remove("Rezervasyonlar");
            ModelState.Remove("Yorumlar");
            ModelState.Remove("Tarihler");
            ModelState.Remove("tarihlerJson");

            if (!ModelState.IsValid) return View(model);

            _context.Etkinlikler.Add(model);
            await _context.SaveChangesAsync();

            // Çoklu tarihleri ekle
            if (!string.IsNullOrEmpty(tarihlerJson))
            {
                try
                {
                    var tarihListesi = System.Text.Json.JsonSerializer.Deserialize<List<TarihGiris>>(tarihlerJson);
                    if (tarihListesi != null)
                    {
                        foreach (var t in tarihListesi)
                        {
                            var etkinlikTarih = new EtkinlikTarih
                            {
                                EtkinlikID = model.EtkinlikID,
                                Tarih = DateTime.Parse(t.Tarih).ToUniversalTime(),
                                Saat = TimeSpan.TryParse(t.Saat, out var saat) ? saat : model.Saat,
                                Kapasite = t.Kapasite > 0 ? t.Kapasite : model.Kapasite,
                                Aktif = true
                            };
                            _context.EtkinlikTarihler.Add(etkinlikTarih);
                        }
                        await _context.SaveChangesAsync();
                    }
                }
                catch { /* JSON parse hatası olursa sessizce devam et */ }
            }

            TempData["SuccessMessage"] = "Etkinlik başarıyla oluşturuldu!";
            return RedirectToAction("EgitmenDashboard", "Users");
        }

        // GET: /Etkinlik/Duzenle/5
        public async Task<IActionResult> Duzenle(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var etkinlik = await _context.Etkinlikler
                .Include(e => e.Tarihler)
                .FirstOrDefaultAsync(e => e.EtkinlikID == id && e.EgitmenID == userId);
            if (etkinlik == null) return NotFound();
            return View(etkinlik);
        }

        // POST: /Etkinlik/Duzenle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duzenle(int id, Etkinlik model, IFormFile? imageFile, string? tarihlerJson)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var etkinlik = await _context.Etkinlikler
                .Include(e => e.Tarihler)
                .FirstOrDefaultAsync(e => e.EtkinlikID == id && e.EgitmenID == userId);
            if (etkinlik == null) return NotFound();

            etkinlik.Ad = model.Ad;
            etkinlik.Aciklama = model.Aciklama;
            etkinlik.Tarih = model.Tarih.ToUniversalTime();
            etkinlik.Saat = model.Saat;
            etkinlik.Konum = model.Konum;
            etkinlik.Ucret = model.Ucret;
            etkinlik.Kapasite = model.Kapasite;
            etkinlik.Kategori = model.Kategori;
            etkinlik.Aktif = model.Aktif;

            if (imageFile != null && imageFile.Length > 0)
            {
                var ext = Path.GetExtension(imageFile.FileName);
                var fileName = Guid.NewGuid() + ext;
                var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", fileName);
                using var fs = new FileStream(path, FileMode.Create);
                await imageFile.CopyToAsync(fs);
                etkinlik.GorselUrl = "/uploads/" + fileName;
            }

            // Çoklu tarihleri güncelle
            if (!string.IsNullOrEmpty(tarihlerJson))
            {
                try
                {
                    var tarihListesi = System.Text.Json.JsonSerializer.Deserialize<List<TarihGiris>>(tarihlerJson);
                    if (tarihListesi != null)
                    {
                        // Gelen ID'leri al
                        var gelenIdler = tarihListesi.Where(t => t.Id > 0).Select(t => t.Id).ToList();

                        // Artık listede olmayan mevcut tarihleri sil (sadece rezervasyonu olmayanlar)
                        var silinecekler = etkinlik.Tarihler?
                            .Where(t => !gelenIdler.Contains(t.EtkinlikTarihID) && t.RezervasyonSayisi == 0)
                            .ToList();
                        if (silinecekler != null)
                        {
                            _context.EtkinlikTarihler.RemoveRange(silinecekler);
                        }

                        foreach (var t in tarihListesi)
                        {
                            if (t.Id > 0)
                            {
                                // Mevcut tarihi güncelle
                                var mevcutTarih = etkinlik.Tarihler?.FirstOrDefault(et => et.EtkinlikTarihID == t.Id);
                                if (mevcutTarih != null)
                                {
                                    mevcutTarih.Tarih = DateTime.Parse(t.Tarih).ToUniversalTime();
                                    mevcutTarih.Saat = TimeSpan.TryParse(t.Saat, out var saat) ? saat : etkinlik.Saat;
                                    mevcutTarih.Kapasite = t.Kapasite > 0 ? t.Kapasite : etkinlik.Kapasite;
                                }
                            }
                            else
                            {
                                // Yeni tarih ekle
                                var yeniTarih = new EtkinlikTarih
                                {
                                    EtkinlikID = etkinlik.EtkinlikID,
                                    Tarih = DateTime.Parse(t.Tarih).ToUniversalTime(),
                                    Saat = TimeSpan.TryParse(t.Saat, out var saat) ? saat : etkinlik.Saat,
                                    Kapasite = t.Kapasite > 0 ? t.Kapasite : etkinlik.Kapasite,
                                    Aktif = true
                                };
                                _context.EtkinlikTarihler.Add(yeniTarih);
                            }
                        }
                    }
                }
                catch { /* JSON parse hatası olursa sessizce devam et */ }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Etkinlik güncellendi!";
            return RedirectToAction("EgitmenDashboard", "Users");
        }

        // POST: /Etkinlik/Sil/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserID");
            var etkinlik = await _context.Etkinlikler.FirstOrDefaultAsync(e => e.EtkinlikID == id && e.EgitmenID == userId);
            if (etkinlik != null)
            {
                _context.Etkinlikler.Remove(etkinlik);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Etkinlik silindi.";
            }
            return RedirectToAction("EgitmenDashboard", "Users");
        }
    }

    // JSON deserialization için helper sınıf
    public class TarihGiris
    {
        public int Id { get; set; }
        public string Tarih { get; set; } = "";
        public string Saat { get; set; } = "";
        public int Kapasite { get; set; }
    }
}
