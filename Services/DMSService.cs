using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Top5.Models;
using Top5.Utils;

namespace Top5.Services
{
    public class DMSService
    {
        private const string FileName = "historique_DMS.json";

        private static string GetFilePath()
        {
            var config = ConfigurationService.Load();
            string directory = string.IsNullOrWhiteSpace(config.DatabasePath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : config.DatabasePath;

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return Path.Combine(directory, FileName);
        }

        public static void LogDMS(string machine, string piece, string moule, string utilisateur, DateTime dmsDate)
        {
            try
            {
                string filePath = GetFilePath();
                List<DMSEntry> history = new List<DMSEntry>();

                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    history = JsonSerializer.Deserialize<List<DMSEntry>>(json) ?? new List<DMSEntry>();
                }

                history.Add(new DMSEntry
                {
                    Timestamp = dmsDate,
                    Machine = machine,
                    Piece = piece,
                    Moule = moule,
                    Utilisateur = string.IsNullOrWhiteSpace(utilisateur) ? "Inconnu" : utilisateur
                });

                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(filePath, JsonSerializer.Serialize(history, options));
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la sauvegarde du DMS : {ex.Message}");
            }
        }

        public static string GetDMSColor(string machine, string piece, string moule)
        {
            if (piece == "---" || moule == "---") return "#DDDDDD";

            try
            {
                string filePath = GetFilePath();
                if (!File.Exists(filePath)) return "#E74C3C";

                string json = File.ReadAllText(filePath);
                var history = JsonSerializer.Deserialize<List<DMSEntry>>(json);

                if (history == null || history.Count == 0) return "#E74C3C";

                var lastDMS = history.Where(x => x.Machine == machine && x.Piece == piece && x.Moule == moule)
                                     .OrderByDescending(x => x.Timestamp)
                                     .FirstOrDefault();

                if (lastDMS == null) return "#E74C3C";

                double daysOld = (DateTime.Now - lastDMS.Timestamp).TotalDays;

                if (daysOld >= 30) return "#E74C3C";
                if (daysOld >= 23) return "#F39C12";

                return "#2ECC71";
            }
            catch
            {
                return "#DDDDDD";
            }
        }

        public static string GetLatestDMSId(string machine, string piece, string moule)
        {
            if (piece == "---" || moule == "---") return string.Empty;

            try
            {
                string filePath = GetFilePath();
                if (!File.Exists(filePath)) return string.Empty;

                string json = File.ReadAllText(filePath);
                var history = JsonSerializer.Deserialize<List<DMSEntry>>(json);

                if (history == null || history.Count == 0) return string.Empty;

                var lastDMS = history.Where(x => x.Machine == machine && x.Piece == piece && x.Moule == moule)
                                     .OrderByDescending(x => x.Timestamp)
                                     .FirstOrDefault();

                return lastDMS != null ? lastDMS.Id.ToString() : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static string GetLastDMSDateString(string machine, string piece, string moule)
        {
            if (piece == "---" || moule == "---") return "Non applicable";

            try
            {
                string filePath = GetFilePath();
                if (!File.Exists(filePath)) return "Aucun (Jamais fait)";

                string json = File.ReadAllText(filePath);
                var history = JsonSerializer.Deserialize<List<DMSEntry>>(json);

                if (history == null || history.Count == 0) return "Aucun (Jamais fait)";

                var lastDMS = history.Where(x => x.Machine == machine && x.Piece == piece && x.Moule == moule)
                                     .OrderByDescending(x => x.Timestamp)
                                     .FirstOrDefault();

                if (lastDMS == null) return "Aucun (Jamais fait)";

                return lastDMS.Timestamp.ToString("dd/MM/yyyy");
            }
            catch
            {
                return "Erreur de lecture";
            }
        }

        // NOUVEAU : Calcul automatique de l'expiration (+ 30 jours)
        public static string GetDMSExpirationDateString(string machine, string piece, string moule)
        {
            if (piece == "---" || moule == "---") return "N/A";

            try
            {
                string filePath = GetFilePath();
                if (!File.Exists(filePath)) return "N/A";

                string json = File.ReadAllText(filePath);
                var history = JsonSerializer.Deserialize<List<DMSEntry>>(json);

                if (history == null || history.Count == 0) return "N/A";

                var lastDMS = history.Where(x => x.Machine == machine && x.Piece == piece && x.Moule == moule)
                                     .OrderByDescending(x => x.Timestamp)
                                     .FirstOrDefault();

                if (lastDMS == null) return "N/A";

                // Le DMS expire exactement 30 jours après sa réalisation
                return lastDMS.Timestamp.AddDays(30).ToString("dd/MM/yyyy");
            }
            catch
            {
                return "Erreur";
            }
        }
    }
}