using SemestralniPraceSTIN.Models;

namespace SemestralniPraceSTIN.Services
{
    public class LoggingService
    {
        private readonly AppDbContext _db;

        public LoggingService(AppDbContext db)
        {
            _db = db;
        }

        public void LogError(string message)
        {
            var log = new Log
            {
                Level = "ERROR",
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _db.Logs.Add(log);

            _db.SaveChanges();
        }

        public void LogInfo(string message)
        {
            var log = new Log
            {
                Level = "INFO",
                Message = message,
                CreatedAt = DateTime.UtcNow
            };

            _db.Logs.Add(log);

            _db.SaveChanges();
        }
    }
}
