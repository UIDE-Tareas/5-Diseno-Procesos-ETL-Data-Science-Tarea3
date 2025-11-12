using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace WebApi.DataAcccess
{
    public class AppDbContext : DbContext
    {
        public DbSet<CryptoPrice> CryptoPrices => Set<CryptoPrice>();

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var dbPath = Path.Combine(AppContext.BaseDirectory, "crypto.db");

                optionsBuilder
                    .UseSqlite($"Data Source={dbPath}")
                    .UseLoggerFactory(LoggerFactory.Create(builder => { }))
                    .LogTo(_ => { }); 
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CryptoPrice>().ToTable("CryptoPrices");
        }
    }
}
