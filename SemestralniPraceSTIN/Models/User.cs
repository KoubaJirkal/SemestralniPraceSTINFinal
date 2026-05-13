namespace smestralka.Models
{
    public class User
    {
        public int Id { get; set; }

        public string BaseCurrency { get; set; }

        public string SelectedCurrencies { get; set; }

        public string PasswordHash { get; set; }
    }
}
