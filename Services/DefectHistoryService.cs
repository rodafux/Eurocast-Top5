using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Top5.Models;
using Top5.Utils;

namespace Top5.Services
{
    public static class DefectHistoryService
    {
        private static string GetDirectoryPath()
        {
            var config = ConfigurationService.Load();
            string directory = string.IsNullOrWhiteSpace(config.DatabasePath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : config.DatabasePath;

            string historyDir = Path.Combine(directory, "HistoriqueDefauts");
            if (!Directory.Exists(historyDir)) Directory.CreateDirectory(historyDir);
            return historyDir;
        }

        private static string GetSafeFileName(string piece, string moule)
        {
            string safePiece = string.Join("_", piece.Split(Path.GetInvalidFileNameChars()));
            string safeMoule = string.Join("_", moule.Split(Path.GetInvalidFileNameChars()));
            return $"Defauts_{safePiece}_{safeMoule}.json";
        }

        public static List<DefectHistoryEntry> LoadHistory(string piece, string moule)
        {
            try
            {
                string filePath = Path.Combine(GetDirectoryPath(), GetSafeFileName(piece, moule));
                if (!File.Exists(filePath)) return new List<DefectHistoryEntry>();
                return JsonSerializer.Deserialize<List<DefectHistoryEntry>>(File.ReadAllText(filePath)) ?? new List<DefectHistoryEntry>();
            }
            catch (Exception ex) { Logger.Log($"Erreur LoadHistory : {ex.Message}"); return new List<DefectHistoryEntry>(); }
        }

        public static void LogDefectAction(ProductionContext context, string controllerName, Defect defect, string action)
        {
            if (context == null || context.Piece == "---" || context.Moule == "---") return;
            try
            {
                string filePath = Path.Combine(GetDirectoryPath(), GetSafeFileName(context.Piece, context.Moule));
                List<DefectHistoryEntry> history = File.Exists(filePath) ? JsonSerializer.Deserialize<List<DefectHistoryEntry>>(File.ReadAllText(filePath)) ?? new List<DefectHistoryEntry>() : new List<DefectHistoryEntry>();

                history.Add(new DefectHistoryEntry
                {
                    Id = defect.Id,
                    Date = DateTime.Now.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture),
                    Heure = DateTime.Now.ToString("HH:mm:ss"),
                    Utilisateur = string.IsNullOrWhiteSpace(controllerName) ? "Inconnu" : controllerName,
                    TypeDefaut = defect.DefectType,
                    Gravite = defect.State.ToString(),
                    Commentaire = defect.Comment,
                    NumeroNoyau = defect.CoreNumber,
                    Action = action,
                    IdDms = DMSService.GetLatestDMSId(context.Machine, context.Piece, context.Moule)
                });
                File.WriteAllText(filePath, JsonSerializer.Serialize(history, new JsonSerializerOptions { WriteIndented = true }));
            }
            catch (Exception ex) { Logger.Log($"Erreur LogDefectAction : {ex.Message}"); }
        }

        // --- MÉTHODE RÉTABLIE POUR LE TRANSFERT D'ÉQUIPE ---
        public static List<Defect> GetUnresolvedDefects(string piece, string moule)
        {
            var unresolved = new List<Defect>();
            try
            {
                string filePath = Path.Combine(GetDirectoryPath(), GetSafeFileName(piece, moule));
                if (!File.Exists(filePath)) return unresolved;

                var history = JsonSerializer.Deserialize<List<DefectHistoryEntry>>(File.ReadAllText(filePath));
                if (history == null) return unresolved;

                foreach (var group in history.GroupBy(x => x.Id))
                {
                    var latest = group.Last();
                    if (latest.Action != "Suppression" && (latest.Gravite == "AA" || latest.Gravite == "NC"))
                    {
                        var def = new Defect
                        {
                            Id = latest.Id,
                            DefectType = latest.TypeDefaut,
                            Comment = latest.Commentaire,
                            CoreNumber = latest.NumeroNoyau
                        };
                        if (Enum.TryParse(latest.Gravite, out ControlState st)) def.State = st;
                        unresolved.Add(def);
                    }
                }
            }
            catch (Exception ex) { Logger.Log($"Erreur GetUnresolvedDefects : {ex.Message}"); }
            return unresolved;
        }

        public static List<DefectHistoryEntry> CheckCoreHistory(string piece, string moule, string coreNumber)
        {
            var issues = new List<DefectHistoryEntry>();
            if (string.IsNullOrWhiteSpace(coreNumber)) return issues;

            try
            {
                var config = ConfigurationService.Load();
                int days = config.NoyauAlertDays <= 0 ? 7 : config.NoyauAlertDays;
                DateTime thresholdDate = DateTime.Now.Date.AddDays(-days);

                var history = LoadHistory(piece, moule);
                string targetCore = coreNumber.Trim();
                var groupedHistory = history.GroupBy(h => h.Id);

                foreach (var group in groupedHistory)
                {
                    var latest = group.Last();
                    if (string.Equals(latest.Action, "Suppression", StringComparison.OrdinalIgnoreCase)) continue;
                    if (string.IsNullOrWhiteSpace(latest.NumeroNoyau)) continue;

                    if (string.Equals(latest.NumeroNoyau.Trim(), targetCore, StringComparison.OrdinalIgnoreCase))
                    {
                        if (DateTime.TryParseExact(latest.Date, "dd/MM/yyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime d) || DateTime.TryParse(latest.Date, out d))
                        {
                            if (d.Date >= thresholdDate) issues.Add(latest);
                        }
                    }
                }
                return issues.OrderByDescending(x => x.Date).ThenByDescending(x => x.Heure).ToList();
            }
            catch (Exception ex) { Logger.Log($"Erreur CheckCoreHistory : {ex.Message}"); return issues; }
        }
    }
}