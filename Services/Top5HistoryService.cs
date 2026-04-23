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
    public static class Top5HistoryService
    {
        public static DateTime GetLogicalProductionDate(DateTime realTime)
        {
            var config = ConfigurationService.Load();
            if (TimeSpan.TryParse(config.ShiftMatinStart, out TimeSpan startMatin))
            {
                return realTime.AddHours(-startMatin.Hours).AddMinutes(-startMatin.Minutes).Date;
            }
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

        public static int GetLastKnownPriority(string machine, DateTime currentDate)
        {
            try
            {
                for (int i = 1; i <= 3; i++)
                {
                    DateTime pastDate = currentDate.AddDays(-i);
                    string path = GetFilePath(pastDate);
                    if (File.Exists(path))
                    {
                        var dto = JsonSerializer.Deserialize<Top5DailyReportDTO>(File.ReadAllText(path));
                        var row = dto?.Rows.FirstOrDefault(r => r.Machine == machine);
                        if (row != null) return row.Priority;
                    }
                }
            }
            catch { }
            return 0;
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
                        Priority = row.Production.Priority,
                        Matin = MapShift(row.ReportMatin),
                        ApresMidi = MapShift(row.ReportApresMidi),
                        Nuit = MapShift(row.ReportNuit)
                    }).ToList()
                };

                string path = GetFilePath(prodDate);
                var opts = new JsonSerializerOptions { WriteIndented = true };

                File.WriteAllText(path, JsonSerializer.Serialize(dto, opts));
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
                        rowVm.Production.Priority = rowDto.Priority;
                        rowVm.Production.RefreshDMS();

                        ApplyShift(rowVm.ReportMatin, rowDto.Matin, rowDto.Piece, rowDto.Moule);
                        ApplyShift(rowVm.ReportApresMidi, rowDto.ApresMidi, rowDto.Piece, rowDto.Moule);
                        ApplyShift(rowVm.ReportNuit, rowDto.Nuit, rowDto.Piece, rowDto.Moule);
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

        public static void ExportMissingPdfsAsync(DateTime currentProdDate, bool startAtPastDay = false)
        {
            var config = ConfigurationService.Load();
            if (string.IsNullOrWhiteSpace(config.PdfExportPath) || config.PdfExportDays <= 0) return;

            if (!Directory.Exists(config.PdfExportPath))
            {
                try { Directory.CreateDirectory(config.PdfExportPath); }
                catch { return; }
            }

            Task.Run(() =>
            {
                int startIndex = startAtPastDay ? 1 : 0;
                for (int i = startIndex; i < config.PdfExportDays; i++)
                {
                    DateTime dateToProcess = currentProdDate.AddDays(-i);
                    string fileName = $"TOP5_{dateToProcess:yy_MM_dd}-J{dateToProcess.DayOfYear}.pdf";
                    string fullPath = Path.Combine(config.PdfExportPath, fileName);

                    if (i > 0 && File.Exists(fullPath)) continue;
                    if (!File.Exists(GetFilePath(dateToProcess))) continue;

                    try
                    {
                        var tempVm = new MainViewModel(dateToProcess);
                        PdfReportService.GeneratePdf(tempVm, fullPath);
                    }
                    catch (IOException) { }
                    catch (Exception ex) { Logger.Log($"Erreur export PDF background ({fileName}) : {ex.Message}"); }
                }
            });
        }

        private static ShiftReportDTO MapShift(ShiftReport shift)
        {
            return new ShiftReportDTO
            {
                RXState = shift.RXState.ToString(),
                DimensionalState = shift.DimensionalState.ToString(),
                AspectState = shift.AspectState.ToString(),
                GeneralComment = shift.GeneralComment,
                AncCount = shift.AncCount,
                IsSP = shift.IsSP,
                Defects = shift.Defects.Select(d => new DefectDTO
                {
                    Id = d.Id,
                    DefectType = d.DefectType,
                    State = d.State.ToString(),
                    Comment = d.Comment,
                    CoreNumber = d.CoreNumber,
                    IsModified = d.IsModified,
                    CreationDate = d.CreationDate
                }).ToList()
            };
        }

        private static void ApplyShift(ShiftReport shift, ShiftReportDTO dto, string piece, string moule)
        {
            if (Enum.TryParse(dto.RXState, out ControlState rx)) shift.RXState = rx;
            if (Enum.TryParse(dto.DimensionalState, out ControlState dim)) shift.DimensionalState = dim;
            if (Enum.TryParse(dto.AspectState, out ControlState asp)) shift.AspectState = asp;

            shift.GeneralComment = dto.GeneralComment;
            shift.AncCount = dto.AncCount;
            shift.IsSP = dto.IsSP;

            shift.Defects.Clear();

            var fullHistory = DefectHistoryService.LoadHistory(piece, moule);

            foreach (var d in dto.Defects)
            {
                DateTime actualDate = d.CreationDate;

                // ==============================================================================
                // AUTO-RÉPARATION POKA-YOKE
                // On ne fait plus confiance au JSON journalier pour la date !
                // On va lire le fichier de traçabilité historique qui est la source absolue.
                // ==============================================================================
                if (fullHistory != null)
                {
                    var historicalRecords = fullHistory.Where(h => h.Id == d.Id).ToList();
                    if (historicalRecords.Any())
                    {
                        // Le premier enregistrement de la liste est la création initiale du défaut
                        var originalRecord = historicalRecords.First();

                        string timeString = originalRecord.Heure.Length >= 5 ? originalRecord.Heure.Substring(0, 5) : "00:00";
                        if (DateTime.TryParseExact($"{originalRecord.Date} {timeString}", "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime parsed))
                        {
                            actualDate = parsed; // L'écrasement corrige les JSON corrompus par le précédent lancement
                        }
                    }
                }

                if (actualDate == default)
                {
                    actualDate = DateTime.Now;
                }

                var def = new Defect
                {
                    Id = d.Id,
                    DefectType = d.DefectType,
                    Comment = d.Comment,
                    CoreNumber = d.CoreNumber,
                    IsModified = d.IsModified,
                    CreationDate = actualDate
                };

                if (!def.IsModified && fullHistory != null && fullHistory.Any(h => h.Id == d.Id && h.Action == "Modification"))
                {
                    def.IsModified = true;
                }

                if (Enum.TryParse(d.State, out ControlState st)) def.State = st;
                shift.Defects.Add(def);
            }
        }
    }
}