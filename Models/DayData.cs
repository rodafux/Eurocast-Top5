using System.Collections.Generic;

namespace Top5.Models
{
    public class DayData
    {
        public List<ProductionRow> Rows { get; set; } = new List<ProductionRow>();
        public string ControllerMatin { get; set; } = "";
        public string ControllerApresMidi { get; set; } = "";
        public string ControllerNuit { get; set; } = "";
        public string TeamCommentMatin { get; set; } = "";
        public string TeamCommentApresMidi { get; set; } = "";
        public string TeamCommentNuit { get; set; } = "";
    }
}