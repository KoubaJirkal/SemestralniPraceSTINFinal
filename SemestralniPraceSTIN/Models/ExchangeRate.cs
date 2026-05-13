namespace SemestralniPraceSTIN.Models
{
    public class ExchangeRate
    {
        public int Id { get; set; }

        public string BaseCurrency { get; set; } = string.Empty;

        public string Currency { get; set; } = string.Empty;

        public decimal Rate { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
