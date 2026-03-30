using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Top5.Services;

namespace Top5.ViewModels
{
    public class DefectTypesViewModel : ViewModelBase
    {
        public ObservableCollection<string> DefectTypes { get; set; }

        private string _newType = string.Empty;
        public string NewType { get => _newType; set { _newType = value; OnPropertyChanged(); } }

        private string? _selectedType;
        public string? SelectedType { get => _selectedType; set { _selectedType = value; OnPropertyChanged(); } }

        public ICommand AddCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SaveCommand { get; }

        public Action? CloseAction { get; set; }

        public DefectTypesViewModel()
        {
            var types = DefectTypeDataService.Load();
            DefectTypes = new ObservableCollection<string>(types.OrderBy(x => x));

            AddCommand = new RelayCommand(ExecuteAdd);
            DeleteCommand = new RelayCommand(ExecuteDelete);
            SaveCommand = new RelayCommand(ExecuteSave);
        }

        private void ExecuteAdd(object? obj)
        {
            string trimmed = NewType.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return;

            if (DefectTypes.Any(x => x.Equals(trimmed, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Ce type de défaut existe déjà.", "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DefectTypes.Add(trimmed);
            var sorted = DefectTypes.OrderBy(x => x).ToList();
            DefectTypes.Clear();
            foreach (var t in sorted) DefectTypes.Add(t);

            NewType = string.Empty;
        }

        private void ExecuteDelete(object? obj)
        {
            if (!string.IsNullOrEmpty(SelectedType))
            {
                DefectTypes.Remove(SelectedType);
            }
        }

        private void ExecuteSave(object? obj)
        {
            DefectTypeDataService.Save(DefectTypes.ToList());
            CloseAction?.Invoke();
        }
    }
}