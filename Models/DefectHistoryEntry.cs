using System;
using System.Text.Json.Serialization;

namespace Top5.Models
{
    public class DefectHistoryEntry
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public string ShortId => $"{Id.ToString().Substring(0, 8)}...";

        // NOUVEAU : Sauvegarde de l'ID du DMS lié
        public string IdDms { get; set; } = string.Empty;

        // NOUVEAU : Formatage visuel de l'ID du DMS
        [JsonIgnore]
        public string ShortIdDms => string.IsNullOrEmpty(IdDms) ? "Aucun" : $"{IdDms.Substring(0, 8)}...";

        public string Date { get; set; } = string.Empty;
        public string Heure { get; set; } = string.Empty;

        [JsonIgnore]
        public string DateInitiale { get; set; } = string.Empty;

        public string Utilisateur { get; set; } = string.Empty;
        public string TypeDefaut { get; set; } = string.Empty;
        public string Gravite { get; set; } = string.Empty;
        public string Commentaire { get; set; } = string.Empty;
        public string NumeroNoyau { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
    }
}