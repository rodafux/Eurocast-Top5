using System;
using System.Text.Json.Serialization;

namespace Top5.Models
{
    public class DefectHistoryEntry
    {
        public Guid Id { get; set; }

        [JsonIgnore]
        public string ShortId => $"{Id.ToString().Substring(0, 8)}...";

        // Gardé pour la compatibilité avec tes anciens fichiers JSON
        public string IdDms { get; set; } = string.Empty;

        [JsonIgnore]
        public string ShortIdDms => string.IsNullOrEmpty(IdDms) ? "Aucun" : $"{IdDms.Substring(0, 8)}...";

        // --- NOUVELLES DONNÉES ---
        private string _machine = string.Empty;
        public string Machine
        {
            get => string.IsNullOrEmpty(_machine) ? "Inconnue" : _machine;
            set => _machine = value;
        }

        private string _dateDms = string.Empty;
        public string DateDms
        {
            get => string.IsNullOrEmpty(_dateDms) ? "Inconnue" : _dateDms;
            set => _dateDms = value;
        }
        // -------------------------

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