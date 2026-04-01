namespace Top5.Models
{
    public class AppConfig
    {
        public string DatabasePath { get; set; } = string.Empty;
        public int NoyauAlertDays { get; set; } = 7;

        public string PdfExportPath { get; set; } = string.Empty;
        public int PdfExportDays { get; set; } = 3;

        // NOUVEAU : Horaires de début des équipes (Format HH:mm)
        // La fin d'une équipe est implicitement le début de la suivante.
        public string ShiftMatinStart { get; set; } = "04:30";
        public string ShiftApresMidiStart { get; set; } = "12:30";
        public string ShiftNuitStart { get; set; } = "20:30";
    }
}