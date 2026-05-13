namespace smestralka.DTOs
{
    public class HistoricalRateResponse
    {
        public decimal Amount { get; set; }

        public string Base { get; set; } = string.Empty;

        public string Start_Date { get; set; } = string.Empty;

        public string End_Date { get; set; } = string.Empty;

        public Dictionary<string, Dictionary<string, decimal>> Rates { get; set; }
            = new();
    }
}
