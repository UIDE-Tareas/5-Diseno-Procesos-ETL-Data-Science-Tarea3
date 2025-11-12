using Microsoft.EntityFrameworkCore;
using WebApi.DataAcccess;
using WebApi.Hubs;
using WebApi.Services;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("https://localhost:7374");
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddSignalR();
builder.Services.AddHostedService<CryptoPriceService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorWasm", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();
app.UseCors("AllowBlazorWasm");

app.MapGet("/api/crypto/top-signals", (AppDbContext db) =>
{
    var unaHoraAtras = DateTime.UtcNow.AddHours(-1);

    // Procesamos en memoria después del select para evitar errores de SQLite
    var rawData = db.CryptoPrices
        .Where(x => x.RecordedAt >= unaHoraAtras)
        .AsEnumerable() // 👈 fuerza ejecución en memoria, evita problemas con ORDER BY/decimal
        .GroupBy(x => x.Symbol)
        .Select(g => new CryptoSignal
        {
            Symbol = g.Key,
            Name = g.First().Name,
            ImageUrl = g.First().ImageUrl,
            Current = g.OrderByDescending(x => x.RecordedAt).First().Price,
            Highest1H = g.Max(x => x.Price),
            Lower1H = g.Min(x => x.Price),
            Avg1H = g.Average(x => x.Price),
            Signal = g.OrderByDescending(x => x.RecordedAt).First().Price > g.Average(x => x.Price) ? "BUY" : "SELL",
            LastUpdated = g.Max(x => x.RecordedAt)
        })
        .OrderByDescending(x => x.Current) // ahora se hace en memoria, no en SQL
        .ToList();

    return Results.Ok(rawData);
});

app.MapDelete("/api/crypto/clear", async (AppDbContext db) =>
{
    db.CryptoPrices.RemoveRange(db.CryptoPrices);
    await db.SaveChangesAsync();
    return Results.Ok("Datos borrados.");
});

app.MapHub<CryptoHub>("/hubs/crypto");

app.Run();


public class CryptoSignal
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Current { get; set; }
    public decimal Highest1H { get; set; }
    public decimal Lower1H { get; set; }
    public decimal Avg1H { get; set; }
    public string Signal { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}