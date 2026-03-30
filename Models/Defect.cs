using System;
using Top5.ViewModels;

namespace Top5.Models
{
    public class Defect : ViewModelBase
    {
        // Identifiant unique pour la traçabilité (ne change jamais même après modification)
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _defectType = string.Empty;
        private ControlState _state = ControlState.NonRenseigne;
        private string _comment = string.Empty;
        private string _coreNumber = string.Empty;

        public string DefectType
        {
            get => _defectType;
            set { _defectType = value; OnPropertyChanged(); }
        }

        public ControlState State
        {
            get => _state;
            set { _state = value; OnPropertyChanged(); }
        }

        public string Comment
        {
            get => _comment;
            set { _comment = value; OnPropertyChanged(); }
        }

        public string CoreNumber
        {
            get => _coreNumber;
            set { _coreNumber = value; OnPropertyChanged(); }
        }
    }
}