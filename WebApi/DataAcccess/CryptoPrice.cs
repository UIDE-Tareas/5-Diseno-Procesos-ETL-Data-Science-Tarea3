using System.ComponentModel.DataAnnotations;

namespace WebApi.DataAcccess
{
    public class CryptoPrice
    {
        [Key]
        public int Id { get; set; }

        public string Symbol { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal MarketCap { get; set; }
        public decimal Change24h { get; set; }
        public DateTime LastUpdated { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
        public string ImageUrl { get; set; } = string.Empty;
    }
}
