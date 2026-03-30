namespace Top5.Models
{
    public class ProductionRow
    {
        private ProductionContext _production;

        public ProductionContext Production
        {
            get => _production;
            set
            {
                _production = value;
                // On s'assure que les 3 équipes pointent vers la MÊME instance mémoire
                ReportMatin.Production = _production;
                ReportApresMidi.Production = _production;
                ReportNuit.Production = _production;
            }
        }

        public ShiftReport ReportMatin { get; set; } = new ShiftReport { Shift = ShiftType.Matin };
        public ShiftReport ReportApresMidi { get; set; } = new ShiftReport { Shift = ShiftType.ApresMidi };
        public ShiftReport ReportNuit { get; set; } = new ShiftReport { Shift = ShiftType.Nuit };

        public ProductionRow()
        {
            // Initialisation de base
            _production = new ProductionContext();
            ReportMatin.Production = _production;
            ReportApresMidi.Production = _production;
            ReportNuit.Production = _production;
        }
    }
}