using System;
using System.Text.Json.Serialization;

namespace Top5.Models
{
    public class DefectHistoryEntry
    {
        public Guid Id { get; set; }
        public string IdDms { get; set; } = string.Empty;
        public string Machine { get; set; } = string.Empty;
        public string DateDms { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Heure { get; set; } = string.Empty;
        public string Utilisateur { get; set; } = string.Empty;
        public string TypeDefaut { get; set; } = string.Empty;
        public string Gravite { get; set; } = string.Empty;
        public string Commentaire { get; set; } = string.Empty;
        public string NumeroNoyau { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;

        [JsonIgnore]
        public string DateInitiale { get; set; } = string.Empty;

        // Formattage pour le tableau (HH:mm)
        [JsonIgnore]
        public string DateAffichee => (string.IsNullOrEmpty(Heure) || Heure.Length < 5) ? $"{Date} {Heure}" : $"{Date} {Heure.Substring(0, 5)}";

        [JsonIgnore]
        public ControlState StateValue => Enum.TryParse<ControlState>(Gravite, out var s) ? s : ControlState.NonRenseigne;
    }
}