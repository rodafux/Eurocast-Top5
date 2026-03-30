using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Top5.Utils;

namespace Top5.Services
{
    public static class DefectTypeDataService
    {
        private const string FileName = "defect_types.json";

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

        public static List<string> Load()
        {
            try
            {
                string filePath = GetFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<List<string>>(json) ?? GetDefaultList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la lecture des types de défauts : {ex.Message}");
            }
            return GetDefaultList();
        }

        public static void Save(List<string> defectTypes)
        {
            try
            {
                string filePath = GetFilePath();
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(defectTypes, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la sauvegarde des types de défauts : {ex.Message}");
                throw;
            }
        }

        private static List<string> GetDefaultList()
        {
            return new List<string>
            {
                "Porosité", "Fissure / Crique", "Rayure", "Manque matière",
                "Ébavurage incomplet", "Déformation", "Coup / Choc",
                "Noyau cassé", "Noyau plié", "Noyau HS", "Autre"
            };
        }
    }
}