using System;
using Top5.Utils;
using Top5.ViewModels;

namespace Top5.Models
{
    public class Defect : ViewModelBase
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _defectType = string.Empty;
        public string DefectType { get => _defectType; set { _defectType = value; OnPropertyChanged(); } }

        private ControlState _state = ControlState.NonRenseigne;
        public ControlState State { get => _state; set { _state = value; OnPropertyChanged(); } }

        private string _comment = string.Empty;
        public string Comment { get => _comment; set { _comment = value; OnPropertyChanged(); } }

        private string _coreNumber = string.Empty;
        public string CoreNumber { get => _coreNumber; set { _coreNumber = value; OnPropertyChanged(); } }

        // NOUVEAU : Pour l'affichage de l'étoile *
        private bool _isModified;
        public bool IsModified { get => _isModified; set { _isModified = value; OnPropertyChanged(); } }
    }
}