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

        private bool _isModified;
        public bool IsModified { get => _isModified; set { _isModified = value; OnPropertyChanged(); } }

        // Date de création pour l'infobulle de traçabilité
        private DateTime _creationDate = DateTime.Now;
        public DateTime CreationDate
        {
            get => _creationDate;
            set
            {
                _creationDate = value;
                OnPropertyChanged();
                // Notification pour que WPF mette à jour la propriété calculée liée
                OnPropertyChanged(nameof(FormattedCreationDate));
            }
        }

        // NOUVEAU : Propriété calculée qui force le format exact en C# pour contourner la culture XAML
        public string FormattedCreationDate => $"Date de création : {CreationDate.ToString("dd/MM/yy HH:mm")}";
    }
}