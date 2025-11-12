using System.Net.Http.Json;

namespace WebApp.Services
{
    public class CryptoService
    {
        private readonly HttpClient _http;

        public CryptoService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<CryptoSignal>?> GetTopAsync()
        {
            return await _http.GetFromJsonAsync<List<CryptoSignal>>("api/crypto/top-signals");
        }
    }

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
}
