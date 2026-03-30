namespace Top5.Models
{
    public class Configuration
    {
        // Chemin du dossier partagé réseau (par défaut vide = Mes Documents)
        public string DatabasePath { get; set; } = string.Empty;

        // NOUVEAU : Nombre de jours pour l'analyse de l'historique des noyaux (7 par défaut)
        public int NoyauAlertDays { get; set; } = 7;
    }
}