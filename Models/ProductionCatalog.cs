using System.Collections.Generic;

namespace Top5.Models
{
    public class ProductionCatalog
    {
        public List<string> Machines { get; set; } = new List<string>();
        public List<string> Pieces { get; set; } = new List<string>();
        public List<string> Moules { get; set; } = new List<string>();
    }
}