using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Top5.Models;
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

        public static List<DefectTypeModel> Load()
        {
            try
            {
                string filePath = GetFilePath();
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);

                    // Gestion sécurisée de la rétrocompatibilité via JsonDocument
                    using (JsonDocument doc = JsonDocument.Parse(json))
                    {
                        if (doc.RootElement.ValueKind == JsonValueKind.Array && doc.RootElement.GetArrayLength() > 0)
                        {
                            // Si le vieux fichier contient de simples strings
                            if (doc.RootElement[0].ValueKind == JsonValueKind.String)
                            {
                                var stringList = JsonSerializer.Deserialize<List<string>>(json);
                                if (stringList != null)
                                {
                                    // On migre silencieusement vers le nouveau modèle (on coche AC et 3D par défaut par sécurité)
                                    return stringList.Select(s => new DefectTypeModel { Name = s, AffectsAC = true, Affects3D = true }).ToList();
                                }
                            }
                        }
                    }

                    // Format actuel normalisé
                    return JsonSerializer.Deserialize<List<DefectTypeModel>>(json) ?? GetDefaultList();
                }
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur lors de la lecture des types de défauts : {ex.Message}");
            }
            return GetDefaultList();
        }

        public static void Save(List<DefectTypeModel> defectTypes)
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

        private static List<DefectTypeModel> GetDefaultList()
        {
            return new List<DefectTypeModel>
            {
                new DefectTypeModel { Name = "Porosité", AffectsRX = true, Affects3D = false, AffectsAC = true },
                new DefectTypeModel { Name = "Fissure / Crique", AffectsRX = true, Affects3D = false, AffectsAC = true },
                new DefectTypeModel { Name = "Rayure", AffectsRX = false, Affects3D = false, AffectsAC = true },
                new DefectTypeModel { Name = "Manque matière", AffectsRX = false, Affects3D = true, AffectsAC = true },
                new DefectTypeModel { Name = "Ébavurage incomplet", AffectsRX = false, Affects3D = true, AffectsAC = true },
                new DefectTypeModel { Name = "Déformation", AffectsRX = false, Affects3D = true, AffectsAC = true },
                new DefectTypeModel { Name = "Coup / Choc", AffectsRX = false, Affects3D = true, AffectsAC = true },
                new DefectTypeModel { Name = "Noyau cassé", AffectsRX = false, Affects3D = true, AffectsAC = true },
                new DefectTypeModel { Name = "Noyau plié", AffectsRX = false, Affects3D = true, AffectsAC = true },
                new DefectTypeModel { Name = "Noyau HS", AffectsRX = false, Affects3D = true, AffectsAC = true },
                new DefectTypeModel { Name = "Autre", AffectsRX = true, Affects3D = true, AffectsAC = true }
            };
        }
    }
}