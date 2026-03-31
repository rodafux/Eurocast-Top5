namespace Top5.Models
{
    public class AppConfig
    {
        // Chemin du dossier partagé réseau (par défaut vide = Mes Documents)
        public string DatabasePath { get; set; } = string.Empty;

        // Nombre de jours pour l'analyse de l'historique des noyaux (7 par défaut)
        public int NoyauAlertDays { get; set; } = 7;

        // NOUVEAU : Configuration de l'export PDF automatique
        public string PdfExportPath { get; set; } = string.Empty;
        public int PdfExportDays { get; set; } = 3;
    }
}