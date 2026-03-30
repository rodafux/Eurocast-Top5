using Top5.Models;

namespace Top5.ViewModels
{
    public class ProductionAssignmentItem : ViewModelBase
    {
        public ProductionContext ContextRef { get; }

        public string Machine => ContextRef.Machine;

        private string _selectedPiece;
        public string SelectedPiece
        {
            get => _selectedPiece;
            set { _selectedPiece = value; OnPropertyChanged(); }
        }

        private string _selectedMoule;
        public string SelectedMoule
        {
            get => _selectedMoule;
            set { _selectedMoule = value; OnPropertyChanged(); }
        }

        public string OriginalPiece { get; }
        public string OriginalMoule { get; }

        public ProductionAssignmentItem(ProductionContext context)
        {
            ContextRef = context;
            _selectedPiece = context.Piece;
            _selectedMoule = context.Moule;
            OriginalPiece = context.Piece;
            OriginalMoule = context.Moule;
        }

        // Détermine si l'utilisateur a modifié l'affectation de cette machine
        public bool HasChanged => SelectedPiece != OriginalPiece || SelectedMoule != OriginalMoule;
    }
}