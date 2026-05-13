namespace SemestralniPraceSTIN.Models
{
    public class Log
    {
        public int Id { get; set; }

        public string Level { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}
