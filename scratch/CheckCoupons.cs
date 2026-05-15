using Microsoft.EntityFrameworkCore;
using art_galeri.Data;
using art_galeri.Models;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var coupons = await context.Kampanyalar.ToListAsync();
    
    Console.WriteLine("--- COUPONS IN DATABASE ---");
    foreach (var c in coupons)
    {
        Console.WriteLine($"ID: {c.KampanyaID}, Name: {c.Ad}, Code: {c.KuponKodu}, Active: {c.Aktif}, Start: {c.BaslangicTarihi}, End: {c.BitisTarihi}, Role: {c.HedefRolID}");
    }
}
