using Top5.Services;
using Top5.ViewModels;

namespace Top5.Models
{
    public class ProductionContext : ViewModelBase
    {
        private string _machine = string.Empty;
        private string _piece = "---";
        private string _moule = "---";
        private string _dmsColor = "#DDDDDD";
        private string _lastDMSDateString = "Inconnu";

        // NOUVEAU : Variable de priorité (0 = aucune, 1, 2, 3)
        private int _priority = 0;

        public string Machine { get => _machine; set { _machine = value; OnPropertyChanged(); RefreshDMS(); } }
        public string Piece { get => _piece; set { _piece = value; OnPropertyChanged(); RefreshDMS(); } }
        public string Moule { get => _moule; set { _moule = value; OnPropertyChanged(); RefreshDMS(); } }
        public string DMSColor { get => _dmsColor; set { _dmsColor = value; OnPropertyChanged(); } }
        public string LastDMSDateString { get => _lastDMSDateString; set { _lastDMSDateString = value; OnPropertyChanged(); } }

        // NOUVEAU : Gestion de la priorité
        public int Priority
        {
            get => _priority;
            set
            {
                _priority = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(PriorityText));
                OnPropertyChanged(nameof(PriorityColor));
            }
        }

        // Renvoie une étoile vide si 0, sinon le chiffre
        public string PriorityText => Priority == 0 ? "☆" : Priority.ToString();

        // Renvoie la couleur selon le niveau
        public string PriorityColor => Priority switch
        {
            1 => "#E74C3C", // Rouge (Urgente)
            2 => "#E67E22", // Orange (Moyenne)
            3 => "#F1C40F", // Jaune (Basse)
            _ => "#DDDDDD"  // Gris (Aucune)
        };

        public void RefreshDMS()
        {
            DMSColor = DMSService.GetDMSColor(Machine, Piece, Moule);
            LastDMSDateString = DMSService.GetLastDMSDateString(Machine, Piece, Moule);
        }
    }
}