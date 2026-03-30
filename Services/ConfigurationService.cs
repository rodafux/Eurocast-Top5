using System;
using System.IO;
using System.Text.Json;
using Top5.Models;
using Top5.Utils;

namespace Top5.Services
{
    public static class ConfigurationService
    {
        private static readonly string AppDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string AppFolder = Path.Combine(AppDataFolder, "Eurocast - Top5");
        private static readonly string ConfigFilePath = Path.Combine(AppFolder, "config.json");

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    string json = File.ReadAllText(ConfigFilePath);
                    return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la lecture de la configuration : {ex.Message}");
            }

            // Retourne une configuration par défaut si le fichier n'existe pas ou est corrompu
            return new AppConfig();
        }

        public static void Save(AppConfig config)
        {
            try
            {
                if (!Directory.Exists(AppFolder))
                {
                    Directory.CreateDirectory(AppFolder);
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(config, options);
                File.WriteAllText(ConfigFilePath, json);

                Logger.Log("Configuration sauvegardée avec succès.");
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la sauvegarde de la configuration : {ex.Message}");
                throw; // On relance l'exception pour pouvoir l'afficher à l'utilisateur
            }
        }
    }
}