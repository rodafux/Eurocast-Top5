using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Top5.Models;
using Top5.Utils;

namespace Top5.Services
{
    public static class ProductionHistoryService
    {
        private const string FileName = "historique_productions.json";

        private static string GetFilePath()
        {
            var config = ConfigurationService.Load();
            string directory = string.IsNullOrWhiteSpace(config.DatabasePath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : config.DatabasePath;

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return Path.Combine(directory, FileName);
        }

        public static void LogChange(string machine, string piece, string moule)
        {
            try
            {
                string filePath = GetFilePath();
                List<ProductionHistoryEntry> history = new List<ProductionHistoryEntry>();

                if (File.Exists(filePath))
                {
                    string existingJson = File.ReadAllText(filePath);
                    history = JsonSerializer.Deserialize<List<ProductionHistoryEntry>>(existingJson) ?? new List<ProductionHistoryEntry>();
                }

                string actionType = (piece == "---" && moule == "---") ? "Arrêt Production" : "Affectation / Modification";

                var newEntry = new ProductionHistoryEntry
                {
                    Timestamp = DateTime.Now,
                    Machine = machine,
                    Piece = piece,
                    Moule = moule,
                    Action = actionType
                };

                history.Add(newEntry);

                var options = new JsonSerializerOptions { WriteIndented = true };
                string jsonToWrite = JsonSerializer.Serialize(history, options);
                File.WriteAllText(filePath, jsonToWrite);
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de l'écriture dans l'historique de production : {ex.Message}");
            }
        }

        public static Dictionary<string, (string Piece, string Moule)> GetLatestProductions()
        {
            var latestProductions = new Dictionary<string, (string, string)>();

            try
            {
                var history = LoadAllHistory();
                if (history.Count > 0)
                {
                    var groupedByMachine = history.GroupBy(h => h.Machine);
                    foreach (var group in groupedByMachine)
                    {
                        var latestEntry = group.OrderByDescending(h => h.Timestamp).First();
                        latestProductions[latestEntry.Machine] = (latestEntry.Piece, latestEntry.Moule);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la lecture des derniers états de production : {ex.Message}");
            }

            return latestProductions;
        }

        // NOUVELLE MÉTHODE : Chargement de l'historique complet
        public static List<ProductionHistoryEntry> LoadAllHistory()
        {
            try
            {
                string filePath = GetFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<ProductionHistoryEntry>>(json) ?? new List<ProductionHistoryEntry>();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la lecture de l'historique complet : {ex.Message}");
            }

            return new List<ProductionHistoryEntry>();
        }
    }
}