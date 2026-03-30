using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Top5.Models;

namespace Top5.Services
{
    public static class SerializationService
    {
        private static string GetFilePath(DateTime date)
        {
            string directory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            try
            {
                var config = ConfigurationService.Load();
                if (config != null && !string.IsNullOrWhiteSpace(config.DatabasePath))
                    directory = config.DatabasePath;
            }
            catch { }

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
            return Path.Combine(directory, $"Top5_{date:yyyy-MM-dd}.json");
        }

        public static DayData LoadDayData(DateTime date)
        {
            try
            {
                string path = GetFilePath(date);
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path).Trim();
                    json = json.TrimStart('\uFEFF'); // Nettoie les caractères fantômes de Windows

                    // Si c'est l'ancien format (Liste)
                    if (json.StartsWith("["))
                    {
                        var oldRows = JsonSerializer.Deserialize<List<ProductionRow>>(json);
                        return new DayData { Rows = oldRows ?? new List<ProductionRow>() };
                    }

                    // Format actuel
                    return JsonSerializer.Deserialize<DayData>(json) ?? new DayData();
                }
            }
            catch { }
            return new DayData();
        }

        public static void SaveDayData(DateTime date, DayData data)
        {
            try
            {
                string path = GetFilePath(date);
                var options = new JsonSerializerOptions { WriteIndented = true };
                File.WriteAllText(path, JsonSerializer.Serialize(data, options));
            }
            catch { }
        }
    }
}