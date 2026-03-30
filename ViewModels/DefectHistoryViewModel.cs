using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Top5.Models;
using Top5.Services;
using Top5.Utils;

namespace Top5.ViewModels
{
    public class DefectHistoryViewModel : ViewModelBase
    {
        private List<DefectHistoryEntry> _rawHistory = new List<DefectHistoryEntry>();

        public ObservableCollection<string> AvailablePieces { get; set; }
        public ObservableCollection<string> AvailableMoules { get; set; }

        public ObservableCollection<DefectHistoryEntry> FilteredDefects { get; set; }
        public ObservableCollection<DefectHistoryEntry> SelectedDefectTimeline { get; set; }

        private string? _selectedPiece;
        public string? SelectedPiece
        {
            get => _selectedPiece;
            set { _selectedPiece = value; OnPropertyChanged(); LoadFileAndApplyFilters(); }
        }

        private string? _selectedMoule;
        public string? SelectedMoule
        {
            get => _selectedMoule;
            set { _selectedMoule = value; OnPropertyChanged(); LoadFileAndApplyFilters(); }
        }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set { _startDate = value; OnPropertyChanged(); LoadFileAndApplyFilters(); }
        }

        private DateTime _endDate;
        public DateTime EndDate
        {
            get => _endDate;
            set { _endDate = value; OnPropertyChanged(); LoadFileAndApplyFilters(); }
        }

        private string _searchDefectType = string.Empty;
        public string SearchDefectType
        {
            get => _searchDefectType;
            set { _searchDefectType = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _searchCoreNumber = string.Empty;
        public string SearchCoreNumber
        {
            get => _searchCoreNumber;
            set { _searchCoreNumber = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private string _searchComment = string.Empty;
        public string SearchComment
        {
            get => _searchComment;
            set { _searchComment = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private DefectHistoryEntry? _selectedDefect;
        public DefectHistoryEntry? SelectedDefect
        {
            get => _selectedDefect;
            set { _selectedDefect = value; OnPropertyChanged(); UpdateTimeline(); }
        }

        public DefectHistoryViewModel()
        {
            FilteredDefects = new ObservableCollection<DefectHistoryEntry>();
            SelectedDefectTimeline = new ObservableCollection<DefectHistoryEntry>();

            // 30 derniers jours par défaut
            EndDate = DateTime.Today;
            StartDate = DateTime.Today.AddDays(-30);

            var catalog = ProductionDataService.Load();
            var comparer = new AlphanumericComparer();
            AvailablePieces = new ObservableCollection<string>(catalog.Pieces.OrderBy(x => x, comparer));
            AvailableMoules = new ObservableCollection<string>(catalog.Moules.OrderBy(x => x, comparer));
        }

        private void LoadFileAndApplyFilters()
        {
            if (string.IsNullOrEmpty(SelectedPiece) || string.IsNullOrEmpty(SelectedMoule))
            {
                _rawHistory.Clear();
                ApplyFilters();
                return;
            }

            // Charge le fichier physique une seule fois lors du changement de pièce/moule
            _rawHistory = DefectHistoryService.LoadHistory(SelectedPiece, SelectedMoule);
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            SelectedDefect = null; // Réinitialise la timeline

            if (_rawHistory.Count == 0)
            {
                FilteredDefects.Clear();
                return;
            }

            var filtered = new List<DefectHistoryEntry>();

            // 1. Grouper par ID
            var groupedById = _rawHistory.GroupBy(x => x.Id);

            foreach (var group in groupedById)
            {
                // On prend le dernier événement de cet ID (état actuel)
                var latestState = group.Last();

                // NOUVEAU : On prend le TOUT PREMIER événement de cet ID (Création)
                var initialState = group.First();

                // On affecte la date initiale à notre objet pour l'affichage
                latestState.DateInitiale = initialState.Date;

                // 2. Filtre de Date (sur le dernier état)
                if (DateTime.TryParseExact(latestState.Date, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime defectDate))
                {
                    if (defectDate.Date < StartDate.Date || defectDate.Date > EndDate.Date)
                        continue;
                }

                // 3. Filtres Textuels
                if (!string.IsNullOrWhiteSpace(SearchDefectType) && !latestState.TypeDefaut.Contains(SearchDefectType, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrWhiteSpace(SearchCoreNumber) && !latestState.NumeroNoyau.Contains(SearchCoreNumber, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (!string.IsNullOrWhiteSpace(SearchComment) && !latestState.Commentaire.Contains(SearchComment, StringComparison.OrdinalIgnoreCase))
                    continue;

                filtered.Add(latestState);
            }

            FilteredDefects.Clear();
            foreach (var item in filtered.OrderByDescending(x => x.Date).ThenByDescending(x => x.Heure))
            {
                FilteredDefects.Add(item);
            }
        }

        private void UpdateTimeline()
        {
            SelectedDefectTimeline.Clear();

            if (SelectedDefect != null)
            {
                // On récupère TOUTES les entrées de l'historique brut ayant cet ID exact
                var timeline = _rawHistory.Where(x => x.Id == SelectedDefect.Id).ToList();

                foreach (var entry in timeline)
                {
                    SelectedDefectTimeline.Add(entry);
                }
            }
        }
    }
}