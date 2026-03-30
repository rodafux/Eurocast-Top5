using System.Collections.ObjectModel;
using System.Windows.Input;
using Top5.Models;
using Top5.Services;

namespace Top5.ViewModels
{
    public class DefectDialogViewModel : ViewModelBase
    {
        public ObservableCollection<string> AvailableDefects { get; set; }

        public bool IsEditMode { get; set; }

        private string _selectedDefectType = string.Empty;
        public string SelectedDefectType
        {
            get => _selectedDefectType;
            set
            {
                _selectedDefectType = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsCoreNumberRequired)); // Notifie la vue que l'état a changé
            }
        }

        // Règle métier stricte demandée
        public bool IsCoreNumberRequired =>
            SelectedDefectType == "Noyau cassé" ||
            SelectedDefectType == "Noyau plié" ||
            SelectedDefectType == "Noyau HS";

        private string _coreNumber = string.Empty;
        public string CoreNumber
        {
            get => _coreNumber;
            set { _coreNumber = value; OnPropertyChanged(); }
        }

        private ControlState _selectedState = ControlState.NC;
        public ControlState SelectedState
        {
            get => _selectedState;
            set { _selectedState = value; OnPropertyChanged(); }
        }

        private string _comment = string.Empty;
        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(); }
        }

        public ICommand SetStateCommand { get; }

        public DefectDialogViewModel(Defect? existingDefect = null)
        {
            // Chargement de la liste dynamique
            AvailableDefects = new ObservableCollection<string>(DefectTypeDataService.Load());

            SetStateCommand = new RelayCommand(ExecuteSetState);

            if (existingDefect != null)
            {
                IsEditMode = true;
                SelectedDefectType = existingDefect.DefectType;
                SelectedState = existingDefect.State;
                Comment = existingDefect.Comment;
                CoreNumber = existingDefect.CoreNumber;
            }
            else
            {
                IsEditMode = false;
                if (AvailableDefects.Count > 0)
                    SelectedDefectType = AvailableDefects[0];
            }
        }

        private void ExecuteSetState(object? parameter)
        {
            if (parameter is string stateStr && System.Enum.TryParse(stateStr, out ControlState parsedState))
            {
                SelectedState = parsedState;
            }
        }
    }
}