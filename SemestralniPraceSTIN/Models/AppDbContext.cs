using Microsoft.EntityFrameworkCore;

namespace SemestralniPraceSTIN.Models
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Log> Logs { get; set; }

        public DbSet<ExchangeRate> ExchangeRates { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
    }
}
