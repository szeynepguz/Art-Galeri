using art_galeri.Data;
using art_galeri.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Veritabani Baglantisi (PostgreSQL)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session Destegi
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// ===================== SEED DATA =====================
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    context.Database.Migrate();

    // Sadece uygulamanın çalışması için gerekli olan Admin kullanıcısını oluştur (Eğer db boşsa)
    if (!context.Users.Any())
    {
        var admin = new User { Ad = "Admin", Soyad = "Yönetici", Email = "admin@art.com", Sifre = "123", RolID = 2, CreatedAt = DateTime.UtcNow };
        context.Users.Add(admin);
        context.SaveChanges();

        // Açılışa özel bir karşılama kampanyası
        context.Kampanyalar.Add(new Kampanya
        {
            Ad = "Açılışa Özel Fırsat",
            Aciklama = "Galeri açılışımıza özel %10 indirim kuponu!",
            IndirimOrani = 10,
            KuponKodu = "HOSGELDIN10",
            BaslangicTarihi = DateTime.UtcNow,
            BitisTarihi = DateTime.UtcNow.AddMonths(1),
            Aktif = true
        });
        context.SaveChanges();
    }
}

app.Run();
