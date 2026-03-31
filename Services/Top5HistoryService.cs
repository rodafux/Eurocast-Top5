using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Top5.Models;
using Top5.Utils;
using Top5.ViewModels;

namespace Top5.Services
{
    /// <summary>
    /// Service responsable de la sauvegarde, du chargement et de l'exportation des historiques de production.
    /// Entièrement Thread-Safe (Compatible avec QuestPDF pour la génération en arrière-plan).
    /// </summary>
    public static class Top5HistoryService
    {
        // Gère le décalage horaire (la journée de production commence à 04h30)
        public static DateTime GetLogicalProductionDate(DateTime realTime)
        {
            return realTime.AddHours(-4).AddMinutes(-30).Date;
        }

        private static string GetFilePath(DateTime prodDate)
        {
            var config = ConfigurationService.Load();
            string dir = string.IsNullOrWhiteSpace(config.DatabasePath)
                ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                : config.DatabasePath;

            string historyDir = Path.Combine(dir, "HistoriqueTOP5");
            if (!Directory.Exists(historyDir)) Directory.CreateDirectory(historyDir);

            string fileName = $"TOP5-Jour-{prodDate.DayOfYear}-{prodDate:dd_MM_yyyy}.json";
            return Path.Combine(historyDir, fileName);
        }

        public static void SaveDailyReport(MainViewModel vm, DateTime prodDate)
        {
            try
            {
                var dto = new Top5DailyReportDTO
                {
                    ProductionDate = prodDate,
                    ControllerMatin = vm.ControllerMatin,
                    ControllerApresMidi = vm.ControllerApresMidi,
                    ControllerNuit = vm.ControllerNuit,
                    TeamCommentMatin = vm.TeamCommentMatin,
                    TeamCommentApresMidi = vm.TeamCommentApresMidi,
                    TeamCommentNuit = vm.TeamCommentNuit,
                    Rows = vm.ProductionRows.Select(row => new ProductionRowDTO
                    {
                        Machine = row.Production.Machine,
                        Piece = row.Production.Piece,
                        Moule = row.Production.Moule,
                        Matin = MapShift(row.ReportMatin),
                        ApresMidi = MapShift(row.ReportApresMidi),
                        Nuit = MapShift(row.ReportNuit)
                    }).ToList()
                };

                string path = GetFilePath(prodDate);
                var opts = new JsonSerializerOptions { WriteIndented = true };

                // 1. Sauvegarde physique des données (JSON)
                File.WriteAllText(path, JsonSerializer.Serialize(dto, opts));

                // 2. Génération immédiate et garantie du PDF pour le jour actuel
                ExportPdfSync(vm, prodDate);

                // 3. Déclenchement de la vérification asynchrone pour les archives PDF manquantes
                ExportMissingPdfsAsync(prodDate, startAtPastDay: true);
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur de sauvegarde auto du TOP5 : {ex.Message}");
            }
        }

        public static bool LoadDailyReport(MainViewModel vm, DateTime prodDate)
        {
            string path = GetFilePath(prodDate);
            if (!File.Exists(path)) return false;

            try
            {
                string json = File.ReadAllText(path);
                var dto = JsonSerializer.Deserialize<Top5DailyReportDTO>(json);
                if (dto == null) return false;

                vm.ControllerMatin = dto.ControllerMatin ?? "";
                vm.ControllerApresMidi = dto.ControllerApresMidi ?? "";
                vm.ControllerNuit = dto.ControllerNuit ?? "";
                vm.TeamCommentMatin = dto.TeamCommentMatin ?? "";
                vm.TeamCommentApresMidi = dto.TeamCommentApresMidi ?? "";
                vm.TeamCommentNuit = dto.TeamCommentNuit ?? "";

                foreach (var rowDto in dto.Rows)
                {
                    var rowVm = vm.ProductionRows.FirstOrDefault(r => r.Production.Machine == rowDto.Machine);
                    if (rowVm != null)
                    {
                        rowVm.Production.Piece = rowDto.Piece;
                        rowVm.Production.Moule = rowDto.Moule;
                        rowVm.Production.RefreshDMS();

                        ApplyShift(rowVm.ReportMatin, rowDto.Matin);
                        ApplyShift(rowVm.ReportApresMidi, rowDto.ApresMidi);
                        ApplyShift(rowVm.ReportNuit, rowDto.Nuit);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur de chargement du TOP5 : {ex.Message}");
                return false;
            }
        }

        // --- EXPORT PDF SYNCHRONE (Jour Courant) ---
        private static void ExportPdfSync(MainViewModel vm, DateTime prodDate)
        {
            var config = ConfigurationService.Load();
            if (string.IsNullOrWhiteSpace(config.PdfExportPath) || config.PdfExportDays <= 0) return;

            string fileName = $"TOP5_{prodDate:yy_MM_dd}-J{prodDate.DayOfYear}.pdf";
            string fullPath = Path.Combine(config.PdfExportPath, fileName);

            try
            {
                // Génération ultra-rapide en RAM via QuestPDF
                PdfReportService.GeneratePdf(vm, fullPath);
            }
            catch (Exception ex)
            {
                Logger.Log($"Erreur d'export PDF synchrone : {ex.Message}");
            }
        }

        // --- EXPORT PDF ASYNCHRONE PURE (Archives et Démarrage) ---
        public static void ExportMissingPdfsAsync(DateTime currentProdDate, bool startAtPastDay = false)
        {
            var config = ConfigurationService.Load();
            if (string.IsNullOrWhiteSpace(config.PdfExportPath) || config.PdfExportDays <= 0) return;

            if (!Directory.Exists(config.PdfExportPath))
            {
                try { Directory.CreateDirectory(config.PdfExportPath); }
                catch { return; }
            }

            // Exécution dans un véritable Thread d'arrière-plan sans lien avec l'UI
            Task.Run(() =>
            {
                int startIndex = startAtPastDay ? 1 : 0;

                for (int i = startIndex; i < config.PdfExportDays; i++)
                {
                    DateTime dateToProcess = currentProdDate.AddDays(-i);
                    string fileName = $"TOP5_{dateToProcess:yy_MM_dd}-J{dateToProcess.DayOfYear}.pdf";
                    string fullPath = Path.Combine(config.PdfExportPath, fileName);

                    // Si le PDF d'archive existe déjà, on l'ignore
                    if (i > 0 && File.Exists(fullPath)) continue;

                    // Si nous n'avons pas la donnée source JSON pour cette date, on l'ignore
                    if (!File.Exists(GetFilePath(dateToProcess))) continue;

                    try
                    {
                        // Instanciation "silencieuse" du ViewModel pour charger les données du passé
                        var tempVm = new MainViewModel(dateToProcess);

                        // Création du PDF (Safe : Aucune interaction avec WPF n'est nécessaire)
                        PdfReportService.GeneratePdf(tempVm, fullPath);
                    }
                    catch (IOException)
                    {
                        // Le fichier PDF est peut-être ouvert par quelqu'un d'autre sur le réseau. On l'ignore.
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Erreur export PDF background ({fileName}) : {ex.Message}");
                    }
                }
            });
        }

        // --- UTILITAIRES DE MAPPING (DTO) ---
        private static ShiftReportDTO MapShift(ShiftReport shift)
        {
            return new ShiftReportDTO
            {
                RXState = shift.RXState.ToString(),
                DimensionalState = shift.DimensionalState.ToString(),
                AspectState = shift.AspectState.ToString(),
                GeneralComment = shift.GeneralComment,
                AncCount = shift.AncCount,
                Defects = shift.Defects.Select(d => new DefectDTO
                {
                    Id = d.Id,
                    DefectType = d.DefectType,
                    State = d.State.ToString(),
                    Comment = d.Comment,
                    CoreNumber = d.CoreNumber
                }).ToList()
            };
        }

        private static void ApplyShift(ShiftReport shift, ShiftReportDTO dto)
        {
            if (Enum.TryParse(dto.RXState, out ControlState rx)) shift.RXState = rx;
            if (Enum.TryParse(dto.DimensionalState, out ControlState dim)) shift.DimensionalState = dim;
            if (Enum.TryParse(dto.AspectState, out ControlState asp)) shift.AspectState = asp;

            shift.GeneralComment = dto.GeneralComment;
            shift.AncCount = dto.AncCount;

            shift.Defects.Clear();
            foreach (var d in dto.Defects)
            {
                var def = new Defect { Id = d.Id, DefectType = d.DefectType, Comment = d.Comment, CoreNumber = d.CoreNumber };
                if (Enum.TryParse(d.State, out ControlState st)) def.State = st;
                shift.Defects.Add(def);
            }
        }
    }
}