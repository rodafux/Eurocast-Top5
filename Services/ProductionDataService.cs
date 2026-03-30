using System;
using System.IO;
using System.Text.Json;
using Top5.Models;
using Top5.Utils;

namespace Top5.Services
{
    public static class ProductionDataService
    {
        // Renommage strict exigé par le métier
        private const string FileName = "catalogue.json";

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

        public static ProductionCatalog Load()
        {
            try
            {
                string filePath = GetFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    return JsonSerializer.Deserialize<ProductionCatalog>(json) ?? new ProductionCatalog();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la lecture du catalogue : {ex.Message}");
            }

            return new ProductionCatalog();
        }

        public static void Save(ProductionCatalog catalog)
        {
            try
            {
                string filePath = GetFilePath();
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(catalog, options);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la sauvegarde du catalogue : {ex.Message}");
                throw;
            }
        }
    }
}