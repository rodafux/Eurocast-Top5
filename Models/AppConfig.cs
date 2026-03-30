namespace Top5.Models
{
    public class AppConfig // (Le tien s'appelle comme ça)
    {
        public string DatabasePath { get; set; } = string.Empty;

        // NOUVEAU : Ajoute juste cette ligne !
        public int NoyauAlertDays { get; set; } = 7;
    }
}