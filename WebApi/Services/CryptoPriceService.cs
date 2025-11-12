using Microsoft.AspNetCore.SignalR;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using WebApi.DataAcccess;
using WebApi.Hubs;
namespace WebApi.Services
{

    public class CryptoPriceService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly IHubContext<CryptoHub> _hub;
        private readonly HttpClient _http = new();
        private string? _lastHash;

        public CryptoPriceService(IServiceProvider services, IHubContext<CryptoHub> hub)
        {
            _services = services;
            _hub = hub;
            _http = new HttpClient();
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64) CryptoPriceBot/1.0");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("[CryptoPriceService] Iniciado.");

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Database.EnsureCreatedAsync(); // Sin migrations ✅

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    bool hayCambios = await ActualizarDatosAsync();
                    if (hayCambios)
                    {
                        Console.WriteLine("[CryptoPriceService] CON cambios detectados.");
                        await _hub.Clients.All.SendAsync("TopUpdated");
                    } else
                    {
                        Console.WriteLine("[CryptoPriceService] SIN cambios detectados.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Error CryptoPriceService] {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }

        private async Task<bool> ActualizarDatosAsync()
        {
            const string url = "https://api.coingecko.com/api/v3/coins/markets?vs_currency=usd&order=market_cap_desc&per_page=10&page=1";

            List<CryptoMarket>? data = null;

            try
            {
                data = await _http.GetFromJsonAsync<List<CryptoMarket>>(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al obtener datos de CoinGecko: {ex.Message}");
                return false;
            }

            if (data is null || data.Count == 0)
            {
                Console.WriteLine("[CryptoPriceService] No se recibieron datos válidos de CoinGecko.");
                return false;
            }

            // Calcular hash para detectar cambios
            var json = System.Text.Json.JsonSerializer.Serialize(data);
            var newHash = ComputeHash(json);

            if (newHash == _lastHash)
                return false; // No hay cambios, no hacer nada

            _lastHash = newHash;

            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            foreach (var item in data)
            {
                db.CryptoPrices.Add(new CryptoPrice
                {
                    Symbol = item.symbol.ToUpper(),
                    Name = item.name,
                    Price = item.current_price,
                    MarketCap = item.market_cap,
                    Change24h = item.price_change_percentage_24h,
                    LastUpdated = item.last_updated,
                    RecordedAt = DateTime.UtcNow,
                    ImageUrl = item.image // 👈 CoinGecko ya incluye esta URL
                });
            }

            await db.SaveChangesAsync();
            Console.WriteLine($"[CryptoPriceService] Guardados {data.Count} registros en BD.");
            return true;
        }


        private static string ComputeHash(string data)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(bytes);
        }
    }

    public class CryptoMarket
    {
        public string id { get; set; } = string.Empty;
        public string symbol { get; set; } = string.Empty;
        public string name { get; set; } = string.Empty;
        public decimal current_price { get; set; }
        public decimal market_cap { get; set; }
        public decimal price_change_percentage_24h { get; set; }
        public DateTime last_updated { get; set; }
        public string image { get; set; } = string.Empty; 
    }
}
