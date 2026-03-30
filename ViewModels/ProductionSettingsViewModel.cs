using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Top5.Models;
using Top5.Services;
using Top5.Utils;

namespace Top5.ViewModels
{
    public class ProductionSettingsViewModel : ViewModelBase
    {
        public ObservableCollection<string> Machines { get; set; }
        public ObservableCollection<string> Pieces { get; set; }
        public ObservableCollection<string> Moules { get; set; }

        private string _newMachine = string.Empty;
        public string NewMachine { get => _newMachine; set { _newMachine = value; OnPropertyChanged(); } }

        private string _newPiece = string.Empty;
        public string NewPiece { get => _newPiece; set { _newPiece = value; OnPropertyChanged(); } }

        private string _newMoule = string.Empty;
        public string NewMoule { get => _newMoule; set { _newMoule = value; OnPropertyChanged(); } }

        private string? _selectedMachine;
        public string? SelectedMachine { get => _selectedMachine; set { _selectedMachine = value; OnPropertyChanged(); } }

        private string? _selectedPiece;
        public string? SelectedPiece { get => _selectedPiece; set { _selectedPiece = value; OnPropertyChanged(); } }

        private string? _selectedMoule;
        public string? SelectedMoule { get => _selectedMoule; set { _selectedMoule = value; OnPropertyChanged(); } }

        public ICommand AddMachineCommand { get; }
        public ICommand DeleteMachineCommand { get; }
        public ICommand AddPieceCommand { get; }
        public ICommand DeletePieceCommand { get; }
        public ICommand AddMouleCommand { get; }
        public ICommand DeleteMouleCommand { get; }
        public ICommand SaveCommand { get; }

        public Action? CloseAction { get; set; }

        // Instance de notre trieur natif
        private readonly AlphanumericComparer _comparer = new AlphanumericComparer();

        public ProductionSettingsViewModel()
        {
            var catalog = ProductionDataService.Load();

            // Initialisation avec tri alphanumérique
            Machines = new ObservableCollection<string>(catalog.Machines.OrderBy(x => x, _comparer));
            Pieces = new ObservableCollection<string>(catalog.Pieces.OrderBy(x => x, _comparer));
            Moules = new ObservableCollection<string>(catalog.Moules.OrderBy(x => x, _comparer));

            AddMachineCommand = new RelayCommand(_ => AddItem(Machines, NewMachine, val => NewMachine = val));
            DeleteMachineCommand = new RelayCommand(_ => RemoveItem(Machines, SelectedMachine));

            AddPieceCommand = new RelayCommand(_ => AddItem(Pieces, NewPiece, val => NewPiece = val));
            DeletePieceCommand = new RelayCommand(_ => RemoveItem(Pieces, SelectedPiece));

            AddMouleCommand = new RelayCommand(_ => AddItem(Moules, NewMoule, val => NewMoule = val));
            DeleteMouleCommand = new RelayCommand(_ => RemoveItem(Moules, SelectedMoule));

            SaveCommand = new RelayCommand(ExecuteSave);
        }

        private void AddItem(ObservableCollection<string> collection, string newItem, Action<string> clearInputField)
        {
            string trimmedItem = newItem.Trim();

            if (string.IsNullOrWhiteSpace(trimmedItem))
                return;

            if (collection.Any(x => x.Equals(trimmedItem, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"L'élément '{trimmedItem}' existe déjà dans la liste.", "Doublon", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            collection.Add(trimmedItem);

            // Re-tri de la collection de manière alphanumérique après un ajout
            var sortedList = collection.OrderBy(x => x, _comparer).ToList();
            collection.Clear();
            foreach (var item in sortedList)
            {
                collection.Add(item);
            }

            clearInputField(string.Empty);
        }

        private void RemoveItem(ObservableCollection<string> collection, string? selectedItem)
        {
            if (!string.IsNullOrEmpty(selectedItem))
            {
                collection.Remove(selectedItem);
            }
        }

        private void ExecuteSave(object? obj)
        {
            var catalog = new ProductionCatalog
            {
                Machines = Machines.ToList(),
                Pieces = Pieces.ToList(),
                Moules = Moules.ToList()
            };

            ProductionDataService.Save(catalog);
            CloseAction?.Invoke();
        }
    }
}