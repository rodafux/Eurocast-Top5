using System;
using System.Collections.Generic;

namespace Top5.Models
{
    public class Top5DailyReportDTO
    {
        public DateTime ProductionDate { get; set; }
        public string ControllerMatin { get; set; } = string.Empty;
        public string ControllerApresMidi { get; set; } = string.Empty;
        public string ControllerNuit { get; set; } = string.Empty;

        public string TeamCommentMatin { get; set; } = string.Empty;
        public string TeamCommentApresMidi { get; set; } = string.Empty;
        public string TeamCommentNuit { get; set; } = string.Empty;

        public List<ProductionRowDTO> Rows { get; set; } = new List<ProductionRowDTO>();
    }

    public class ProductionRowDTO
    {
        public string Machine { get; set; } = string.Empty;
        public string Piece { get; set; } = string.Empty;
        public string Moule { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;

        public ShiftReportDTO Matin { get; set; } = new ShiftReportDTO();
        public ShiftReportDTO ApresMidi { get; set; } = new ShiftReportDTO();
        public ShiftReportDTO Nuit { get; set; } = new ShiftReportDTO();
    }

    public class ShiftReportDTO
    {
        public string RXState { get; set; } = "NonRenseigne";
        public string DimensionalState { get; set; } = "NonRenseigne";
        public string AspectState { get; set; } = "NonRenseigne";
        public string GeneralComment { get; set; } = string.Empty;
        public int AncCount { get; set; } = 0;
        public bool IsSP { get; set; } = false;

        public List<DefectDTO> Defects { get; set; } = new List<DefectDTO>();
    }

    public class DefectDTO
    {
        public Guid Id { get; set; }
        public string DefectType { get; set; } = string.Empty;
        public string State { get; set; } = "NonRenseigne";
        public string Comment { get; set; } = string.Empty;
        public string CoreNumber { get; set; } = string.Empty;
        public bool IsModified { get; set; }

        // NOUVEAU : Sauvegarde de la date de création dans le fichier
        public DateTime CreationDate { get; set; }
    }
}