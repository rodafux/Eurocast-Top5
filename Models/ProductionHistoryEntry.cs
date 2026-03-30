using System;

namespace Top5.Models
{
    public class ProductionHistoryEntry
    {
        public DateTime Timestamp { get; set; }
        public string Machine { get; set; } = string.Empty;
        public string Piece { get; set; } = string.Empty;
        public string Moule { get; set; } = string.Empty;
        public string Action { get; set; } = "Affectation"; // Indique si c'est un ajout, une modification, ou un arrêt
    }
}