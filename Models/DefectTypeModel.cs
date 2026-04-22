namespace Top5.Models
{
    public class DefectTypeModel
    {
        public string Name { get; set; } = string.Empty;

        // Indicateurs d'impact du défaut sur les points de contrôle
        public bool AffectsRX { get; set; }
        public bool Affects3D { get; set; }
        public bool AffectsAC { get; set; }
    }
}