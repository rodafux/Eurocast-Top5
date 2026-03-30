using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Top5.Models;
using Top5.Services;
using Top5.Utils;

namespace Top5.ViewModels
{
    public class CatalogViewModel : ViewModelBase
    {
        public ObservableCollection<string> Machines { get; set; }
        public ObservableCollection<string> Pieces { get; set; }
        public ObservableCollection<string> Moules { get; set; }

        private string _newMachine = "";
        public string NewMachine { get => _newMachine; set { _newMachine = value; OnPropertyChanged(); } }
        private string _newPiece = "";
        public string NewPiece { get => _newPiece; set { _newPiece = value; OnPropertyChanged(); } }
        private string _newMoule = "";
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

        public CatalogViewModel()
        {
            var catalog = ProductionDataService.Load();
            var comparer = new AlphanumericComparer();

            Machines = new ObservableCollection<string>(catalog.Machines.OrderBy(x => x, comparer));
            Pieces = new ObservableCollection<string>(catalog.Pieces.OrderBy(x => x, comparer));
            Moules = new ObservableCollection<string>(catalog.Moules.OrderBy(x => x, comparer));

            AddMachineCommand = new RelayCommand(_ => {
                if (!string.IsNullOrWhiteSpace(NewMachine) && !Machines.Contains(NewMachine.Trim()))
                { Machines.Add(NewMachine.Trim()); NewMachine = ""; Sort(Machines); }
            });
            DeleteMachineCommand = new RelayCommand(_ => { if (SelectedMachine != null) Machines.Remove(SelectedMachine); });

            AddPieceCommand = new RelayCommand(_ => {
                if (!string.IsNullOrWhiteSpace(NewPiece) && !Pieces.Contains(NewPiece.Trim()))
                { Pieces.Add(NewPiece.Trim()); NewPiece = ""; Sort(Pieces); }
            });
            DeletePieceCommand = new RelayCommand(_ => { if (SelectedPiece != null) Pieces.Remove(SelectedPiece); });

            AddMouleCommand = new RelayCommand(_ => {
                if (!string.IsNullOrWhiteSpace(NewMoule) && !Moules.Contains(NewMoule.Trim()))
                { Moules.Add(NewMoule.Trim()); NewMoule = ""; Sort(Moules); }
            });
            DeleteMouleCommand = new RelayCommand(_ => { if (SelectedMoule != null) Moules.Remove(SelectedMoule); });

            SaveCommand = new RelayCommand(_ => ExecuteSave());
        }

        private void Sort(ObservableCollection<string> collection)
        {
            var sorted = collection.OrderBy(x => x, new AlphanumericComparer()).ToList();
            collection.Clear();
            foreach (var item in sorted) collection.Add(item);
        }

        private void ExecuteSave()
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