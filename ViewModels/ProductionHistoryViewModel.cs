using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Top5.Models;
using Top5.Services;

namespace Top5.ViewModels
{
    public class ProductionHistoryViewModel : ViewModelBase
    {
        // Conserve la liste complète en mémoire pour éviter de relire le JSON à chaque frappe
        private readonly List<ProductionHistoryEntry> _allHistory;

        public ObservableCollection<ProductionHistoryEntry> FilteredHistory { get; set; }

        private string _searchMachine = string.Empty;
        public string SearchMachine
        {
            get => _searchMachine;
            set { _searchMachine = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _searchPiece = string.Empty;
        public string SearchPiece
        {
            get => _searchPiece;
            set { _searchPiece = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _searchMoule = string.Empty;
        public string SearchMoule
        {
            get => _searchMoule;
            set { _searchMoule = value; OnPropertyChanged(); ApplyFilters(); }
        }

        public ProductionHistoryViewModel()
        {
            // Chargement de l'historique et tri par défaut : du plus récent au plus ancien
            _allHistory = ProductionHistoryService.LoadAllHistory()
                            .OrderByDescending(h => h.Timestamp)
                            .ToList();

            FilteredHistory = new ObservableCollection<ProductionHistoryEntry>(_allHistory);
        }

        // Appliqué instantanément à chaque lettre tapée par l'utilisateur
        private void ApplyFilters()
        {
            var filtered = _allHistory.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(SearchMachine))
            {
                filtered = filtered.Where(h => h.Machine.Contains(SearchMachine, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SearchPiece))
            {
                filtered = filtered.Where(h => h.Piece.Contains(SearchPiece, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(SearchMoule))
            {
                filtered = filtered.Where(h => h.Moule.Contains(SearchMoule, StringComparison.OrdinalIgnoreCase));
            }

            FilteredHistory.Clear();
            foreach (var item in filtered)
            {
                FilteredHistory.Add(item);
            }
        }
    }
}