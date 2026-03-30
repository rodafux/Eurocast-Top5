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
    public class MainViewModel : ViewModelBase
    {
        private DateTime _viewingDate = DateTime.Today;
        private string _controllerMatin = "";
        private string _controllerApresMidi = "";
        private string _controllerNuit = "";

        private string _teamCommentMatin = "";
        private string _teamCommentApresMidi = "";
        private string _teamCommentNuit = "";

        public ObservableCollection<ProductionRow> ProductionRows { get; set; } = new ObservableCollection<ProductionRow>();

        #region Propriétés
        public DateTime ViewingDate
        {
            get => _viewingDate;
            set
            {
                if (_viewingDate != value)
                {
                    ForceSave();
                    _viewingDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCurrentDay));
                    OnPropertyChanged(nameof(ViewingDayOfYear));
                    LoadDateData();
                }
            }
        }

        public int ViewingDayOfYear
        {
            get => ViewingDate.DayOfYear;
            set { try { ViewingDate = new DateTime(ViewingDate.Year, 1, 1).AddDays(value - 1); } catch { } }
        }

        public bool IsCurrentDay => ViewingDate.Date == DateTime.Today;

        public string ControllerMatin { get => _controllerMatin; set { _controllerMatin = value; OnPropertyChanged(); } }
        public string ControllerApresMidi { get => _controllerApresMidi; set { _controllerApresMidi = value; OnPropertyChanged(); } }
        public string ControllerNuit { get => _controllerNuit; set { _controllerNuit = value; OnPropertyChanged(); } }

        public string TeamCommentMatin { get => _teamCommentMatin; set { _teamCommentMatin = value; OnPropertyChanged(); } }
        public string TeamCommentApresMidi { get => _teamCommentApresMidi; set { _teamCommentApresMidi = value; OnPropertyChanged(); } }
        public string TeamCommentNuit { get => _teamCommentNuit; set { _teamCommentNuit = value; OnPropertyChanged(); } }
        #endregion

        #region Logique des Shifts avec BATTEMENT DE 10 MINUTES
        // 1. Détection visuelle (Fond bleu) : Change à l'heure pile
        public bool IsMatinActive => IsCurrentDay && IsTimeBetween(new TimeSpan(4, 30, 0), new TimeSpan(12, 30, 0));
        public bool IsApresMidiActive => IsCurrentDay && IsTimeBetween(new TimeSpan(12, 30, 0), new TimeSpan(20, 30, 0));
        public bool IsNuitActive => IsCurrentDay && (IsTimeBetween(new TimeSpan(20, 30, 0), new TimeSpan(23, 59, 59)) || IsTimeBetween(new TimeSpan(0, 0, 0), new TimeSpan(4, 30, 0)));

        // 2. Verrouillage des saisies : Laisse 10 minutes de plus à l'équipe sortante
        public bool IsMatinEnabled => IsCurrentDay && IsTimeBetween(new TimeSpan(4, 30, 0), new TimeSpan(12, 40, 0));
        public bool IsApresMidiEnabled => IsCurrentDay && IsTimeBetween(new TimeSpan(12, 30, 0), new TimeSpan(20, 40, 0));
        public bool IsNuitEnabled => IsCurrentDay && (IsTimeBetween(new TimeSpan(20, 30, 0), new TimeSpan(23, 59, 59)) || IsTimeBetween(new TimeSpan(0, 0, 0), new TimeSpan(4, 40, 0)));

        // 3. Verrouillage des Noms (Même règle des 10 minutes)
        public bool IsMatinTimeWindow => IsMatinEnabled;
        public bool IsApresMidiTimeWindow => IsApresMidiEnabled;
        public bool IsNuitTimeWindow => IsNuitEnabled;

        private bool IsTimeBetween(TimeSpan start, TimeSpan end)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (start <= end) return now >= start && now <= end;
            return now >= start || now <= end;
        }
        #endregion

        #region Commands
        public ICommand PreviousDayCommand => new RelayCommand(_ => ViewingDate = ViewingDate.AddDays(-1));
        public ICommand NextDayCommand => new RelayCommand(_ => ViewingDate = ViewingDate.AddDays(1));
        public ICommand GoToTodayCommand => new RelayCommand(_ => ViewingDate = DateTime.Today);

        public ICommand PrintReportCommand => new RelayCommand(_ =>
        {
            var doc = PdfReportService.CreateDocument(this);
            var win = new Top5.Views.PdfPreviewWindow(doc) { Owner = Application.Current.MainWindow };
            win.ShowDialog();
        });

        public ICommand OpenDMSCommand => new RelayCommand(param =>
        {
            if (param is ProductionContext context)
            {
                var vm = new DMSViewModel(context);
                var win = new Top5.Views.DMSWindow { DataContext = vm, Owner = Application.Current.MainWindow };
                win.ShowDialog();
            }
        });

        public ICommand EditTeamCommentCommand => new RelayCommand(param =>
        {
            string shift = param?.ToString() ?? "";
            string current = shift switch { "Matin" => TeamCommentMatin, "ApresMidi" => TeamCommentApresMidi, "Nuit" => TeamCommentNuit, _ => "" };

            // Vérification de sécurité : Seule l'équipe en cours (avec ses 10 min de battement) peut modifier
            bool isReadOnly = true;
            if (shift == "Matin") isReadOnly = !IsMatinTimeWindow;
            else if (shift == "ApresMidi") isReadOnly = !IsApresMidiTimeWindow;
            else if (shift == "Nuit") isReadOnly = !IsNuitTimeWindow;

            // On ouvre notre belle nouvelle fenêtre
            var vm = new TeamCommentViewModel(shift, current, isReadOnly);
            var win = new Top5.Views.TeamCommentWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            win.ShowDialog();

            // On ne sauvegarde QUE si l'utilisateur a cliqué sur le bouton "Enregistrer" (Corrige le bug d'effacement)
            if (vm.IsSaved)
            {
                if (shift == "Matin") TeamCommentMatin = vm.Comment;
                else if (shift == "ApresMidi") TeamCommentApresMidi = vm.Comment;
                else if (shift == "Nuit") TeamCommentNuit = vm.Comment;
                ForceSave();
            }
        });

        // --- COMMANDES DU MENU ---

        public ICommand OpenDailyProductionCommand => new RelayCommand(_ => {
            var vm = new DailyProductionViewModel(ProductionRows);
            var win = new Top5.Views.DailyProductionWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            win.ShowDialog();
            ForceSave(); LoadDateData();
        });

        public ICommand OpenConfigurationCommand => new RelayCommand(_ => {
            var vm = new ConfigurationViewModel();
            var win = new Top5.Views.ConfigurationWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            win.ShowDialog();
        });

        public ICommand OpenDefectTypesCommand => new RelayCommand(_ => {
            var vm = new DefectTypesViewModel();
            var win = new Top5.Views.DefectTypesWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            win.ShowDialog();
        });

        public ICommand OpenProductionHistoryCommand => new RelayCommand(_ => {
            var win = new Top5.Views.ProductionHistoryWindow { Owner = Application.Current.MainWindow };
            win.ShowDialog();
        });

        public ICommand OpenDefectHistoryCommand => new RelayCommand(_ => {
            var vm = new DefectHistoryViewModel();
            var win = new Top5.Views.DefectHistoryWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            win.ShowDialog();
        });

        public ICommand OpenCatalogCommand => new RelayCommand(_ => {
            var win = new Top5.Views.CatalogWindow { Owner = Application.Current.MainWindow };
            win.ShowDialog();
        });
        #endregion

        public MainViewModel()
        {
            LoadDateData();
        }

        private void LoadDateData()
        {
            // 1. On charge TOUJOURS le catalogue des machines en premier pour construire la grille
            ProductionRows.Clear();
            var catalog = ProductionDataService.Load();
            if (catalog != null && catalog.Machines != null)
            {
                foreach (var machine in catalog.Machines)
                {
                    ProductionRows.Add(new ProductionRow { Production = new ProductionContext { Machine = machine } });
                }
            }

            // 2. On utilise le VRAI service pour charger le fichier TOP5-Jour...
            bool fileLoaded = Top5HistoryService.LoadDailyReport(this, ViewingDate);

            // 3. S'il n'y a pas de fichier (nouveau jour), on met les variables à zéro et on cherche les dernières pièces produites
            if (!fileLoaded)
            {
                ControllerMatin = "";
                ControllerApresMidi = "";
                ControllerNuit = "";
                TeamCommentMatin = "";
                TeamCommentApresMidi = "";
                TeamCommentNuit = "";

                var latestStates = ProductionHistoryService.GetLatestProductions();
                foreach (var row in ProductionRows)
                {
                    if (latestStates.ContainsKey(row.Production.Machine))
                    {
                        row.Production.Piece = latestStates[row.Production.Machine].Piece;
                        row.Production.Moule = latestStates[row.Production.Machine].Moule;
                        row.Production.RefreshDMS();
                    }
                }
            }

            // 4. Sécurité des noms
            foreach (var row in ProductionRows)
            {
                if (row.ReportMatin != null) row.ReportMatin.GetControllerName = () => ControllerMatin;
                if (row.ReportApresMidi != null) row.ReportApresMidi.GetControllerName = () => ControllerApresMidi;
                if (row.ReportNuit != null) row.ReportNuit.GetControllerName = () => ControllerNuit;
            }
        }

        public void ForceSave()
        {
            // On utilise le VRAI service de sauvegarde
            Top5HistoryService.SaveDailyReport(this, ViewingDate);
        }
    }
}