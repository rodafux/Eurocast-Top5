using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Top5.Models;
using Top5.Services;
using Top5.Utils;

namespace Top5.ViewModels
{
    public class DefectDialogViewModel : ViewModelBase
    {
        private Defect? _defect;
        private ProductionContext? _context;
        private string _controller = "Inconnu";

        public bool IsEditMode { get; set; }
        public bool IsCreationMode => !IsEditMode;
        public bool IsSaved { get; private set; }

        private bool _isReadOnly;
        public bool IsReadOnly { get => _isReadOnly; set { _isReadOnly = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsEditable)); } }
        public bool IsEditable => !IsReadOnly;

        public ObservableCollection<string> AvailableDefects { get; set; } = new ObservableCollection<string>(DefectTypeDataService.Load());
        public ObservableCollection<DefectHistoryEntry> History { get; set; } = new ObservableCollection<DefectHistoryEntry>();

        private string _selectedDefectType = string.Empty;
        public string SelectedDefectType { get => _selectedDefectType; set { _selectedDefectType = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsCoreNumberRequired)); } }

        public bool IsCoreNumberRequired => SelectedDefectType == "Noyau cassé" || SelectedDefectType == "Noyau plié" || SelectedDefectType == "Noyau HS" || SelectedDefectType == "Casse goupille";

        public ControlState SelectedState { get; set; }
        public string Comment { get; set; } = string.Empty;
        public string CoreNumber { get; set; } = string.Empty;

        public ICommand SaveCommand { get; }
        public Action? CloseAction { get; set; }

        public DefectDialogViewModel(Defect? existingDefect = null, ProductionContext? context = null, string controller = "Inconnu")
        {
            SaveCommand = new RelayCommand(_ => { IsSaved = true; CloseAction?.Invoke(); });

            if (existingDefect != null)
            {
                IsEditMode = true;
                _defect = existingDefect;
                _context = context;
                _controller = controller;
                SelectedDefectType = existingDefect.DefectType;
                SelectedState = existingDefect.State;
                Comment = existingDefect.Comment;
                CoreNumber = existingDefect.CoreNumber;

                if (context != null)
                {
                    var list = DefectHistoryService.GetHistory(context.Piece, context.Moule, existingDefect.Id);
                    foreach (var h in list) History.Add(h);
                }
            }
        }

        public void FinalizeUpdate()
        {
            if (_defect != null && _context != null && !IsReadOnly)
            {
                _defect.DefectType = SelectedDefectType;
                _defect.State = SelectedState;
                _defect.Comment = Comment;
                _defect.CoreNumber = CoreNumber;
                _defect.IsModified = true; // Allume l'étoile
                DefectHistoryService.LogDefectAction(_context, _controller, _defect, "Modification");
            }
        }
    }
}