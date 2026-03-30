using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Top5.Models;
using Top5.Utils;
using Top5.ViewModels;

namespace Top5.Services
{
    public static class Top5HistoryService
    {
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

                    // Sauvegarde des commentaires
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
                File.WriteAllText(path, JsonSerializer.Serialize(dto, opts));
            }
            catch (Exception ex) { Logger.Log($"Erreur de sauvegarde auto du TOP5 : {ex.Message}"); }
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

                // Chargement des commentaires
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