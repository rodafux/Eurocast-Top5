using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Top5.Models;
using Top5.Services;
using Top5.Utils;

namespace Top5.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private DateTime _viewingDate;
        private string _controllerMatin = "";
        private string _controllerApresMidi = "";
        private string _controllerNuit = "";

        private string _teamCommentMatin = "";
        private string _teamCommentApresMidi = "";
        private string _teamCommentNuit = "";

        // Timers d'arrière-plan
        private DispatcherTimer _shiftCheckTimer;
        private DispatcherTimer _autoSaveTimer;
        private string _currentActiveShiftName = "";

        // Mémoire du jour logique pour ne pas perturber la navigation dans les archives
        private DateTime _lastKnownLogicalToday;

        public ObservableCollection<ProductionRow> ProductionRows { get; set; } = new ObservableCollection<ProductionRow>();

        #region Propriétés
        public DateTime ViewingDate
        {
            get => _viewingDate;
            set
            {
                var todayLogical = Top5HistoryService.GetLogicalProductionDate(DateTime.Now);
                if (value.Date > todayLogical) value = todayLogical;

                if (_viewingDate != value)
                {
                    ForceSave();
                    _viewingDate = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCurrentDay));
                    OnPropertyChanged(nameof(ViewingDayOfYear));

                    RefreshUIBindings();
                    LoadDateData();
                }
            }
        }

        public int ViewingDayOfYear
        {
            get => ViewingDate.DayOfYear;
            set
            {
                try
                {
                    var newDate = new DateTime(ViewingDate.Year, 1, 1).AddDays(value - 1);
                    var todayLogical = Top5HistoryService.GetLogicalProductionDate(DateTime.Now);
                    if (newDate.Date <= todayLogical) ViewingDate = newDate;
                }
                catch { }
            }
        }

        public bool IsCurrentDay => ViewingDate.Date == Top5HistoryService.GetLogicalProductionDate(DateTime.Now).Date;

        public string ControllerMatin { get => _controllerMatin; set { _controllerMatin = value; OnPropertyChanged(); } }
        public string ControllerApresMidi { get => _controllerApresMidi; set { _controllerApresMidi = value; OnPropertyChanged(); } }
        public string ControllerNuit { get => _controllerNuit; set { _controllerNuit = value; OnPropertyChanged(); } }

        public string TeamCommentMatin { get => _teamCommentMatin; set { _teamCommentMatin = value; OnPropertyChanged(); } }
        public string TeamCommentApresMidi { get => _teamCommentApresMidi; set { _teamCommentApresMidi = value; OnPropertyChanged(); } }
        public string TeamCommentNuit { get => _teamCommentNuit; set { _teamCommentNuit = value; OnPropertyChanged(); } }
        #endregion

        #region Logique Dynamique des Shifts (Configuration)

        private TimeSpan GetShiftStart(string shiftName)
        {
            var config = ConfigurationService.Load();
            if (shiftName == "Matin" && TimeSpan.TryParse(config.ShiftMatinStart, out var tm)) return tm;
            if (shiftName == "ApresMidi" && TimeSpan.TryParse(config.ShiftApresMidiStart, out var ta)) return ta;
            if (shiftName == "Nuit" && TimeSpan.TryParse(config.ShiftNuitStart, out var tn)) return tn;

            return shiftName switch { "Matin" => new TimeSpan(4, 30, 0), "ApresMidi" => new TimeSpan(12, 30, 0), _ => new TimeSpan(20, 30, 0) };
        }

        private bool IsTimeBetween(TimeSpan start, TimeSpan end)
        {
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (start <= end) return now >= start && now < end;
            return now >= start || now < end;
        }

        private TimeSpan NormalizeTime(TimeSpan time) => new TimeSpan(time.Hours, time.Minutes, time.Seconds);

        // NOUVEAU : Règle spéciale qui détecte si on est dans les 10 min de battement du matin (04:30 - 04:40)
        public bool IsMorningGracePeriod => IsTimeBetween(GetShiftStart("Matin"), NormalizeTime(GetShiftStart("Matin").Add(TimeSpan.FromMinutes(10))));

        // NOUVEAU : Vérifie si la feuille consultée est celle d'hier
        public bool IsPreviousDay => ViewingDate.Date == Top5HistoryService.GetLogicalProductionDate(DateTime.Now).AddDays(-1).Date;

        // 1. Équipe réellement en cours (Détermine le fond bleu sur la feuille du jour)
        public bool IsMatinActive => IsCurrentDay && IsTimeBetween(GetShiftStart("Matin"), GetShiftStart("ApresMidi"));
        public bool IsApresMidiActive => IsCurrentDay && IsTimeBetween(GetShiftStart("ApresMidi"), GetShiftStart("Nuit"));
        public bool IsNuitActive => IsCurrentDay && IsTimeBetween(GetShiftStart("Nuit"), GetShiftStart("Matin"));

        // 2. Déverrouillage des cases de données (Avec les 10 minutes de battement)
        public bool IsMatinEnabled => IsCurrentDay && IsTimeBetween(GetShiftStart("Matin"), NormalizeTime(GetShiftStart("ApresMidi").Add(TimeSpan.FromMinutes(10))));
        public bool IsApresMidiEnabled => IsCurrentDay && IsTimeBetween(GetShiftStart("ApresMidi"), NormalizeTime(GetShiftStart("Nuit").Add(TimeSpan.FromMinutes(10))));

        // CORRECTION MAJEURE : La Nuit est modifiable sur le jour actuel (jusqu'à 04h30) OU sur la feuille de la veille (pendant le battement de 04h30 à 04h40)
        public bool IsNuitEnabled => (IsCurrentDay && IsTimeBetween(GetShiftStart("Nuit"), GetShiftStart("Matin"))) ||
                                     (IsPreviousDay && IsMorningGracePeriod);

        // 3. Verrouillage des noms de contrôleurs à L'HEURE PILE (pas de battement)
        public bool IsMatinTimeWindow => IsMatinActive;
        public bool IsApresMidiTimeWindow => IsApresMidiActive;

        // Le nom du contrôleur de nuit reste modifiable sur la veille pendant le battement, pour corriger une erreur de dernière minute
        public bool IsNuitTimeWindow => IsNuitActive || (IsPreviousDay && IsMorningGracePeriod);

        private ShiftReport GetActiveShift(ProductionRow row)
        {
            if (IsMatinActive) return row.ReportMatin;
            if (IsApresMidiActive) return row.ReportApresMidi;
            if (IsNuitActive) return row.ReportNuit;
            return row.ReportMatin;
        }

        private string GetActiveShiftName()
        {
            if (IsMatinActive) return "Matin";
            if (IsApresMidiActive) return "ApresMidi";
            if (IsNuitActive) return "Nuit";
            return "";
        }
        #endregion

        #region Commands
        public ICommand PreviousDayCommand => new RelayCommand(_ => ViewingDate = ViewingDate.AddDays(-1));

        public ICommand NextDayCommand => new RelayCommand(_ => {
            if (ViewingDate.Date < Top5HistoryService.GetLogicalProductionDate(DateTime.Now)) ViewingDate = ViewingDate.AddDays(1);
        });

        public ICommand GoToTodayCommand => new RelayCommand(_ => ViewingDate = Top5HistoryService.GetLogicalProductionDate(DateTime.Now));

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

            // La consigne profite du battement de 10 min
            bool isReadOnly = true;
            if (shift == "Matin") isReadOnly = !IsMatinEnabled;
            else if (shift == "ApresMidi") isReadOnly = !IsApresMidiEnabled;
            else if (shift == "Nuit") isReadOnly = !IsNuitEnabled;

            var vm = new TeamCommentViewModel(shift, current, isReadOnly);
            var win = new Top5.Views.TeamCommentWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            win.ShowDialog();

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

            RefreshUIBindings();
            ForceSave();
            LoadDateData();
        });

        public ICommand OpenConfigurationCommand => new RelayCommand(_ => {
            var vm = new ConfigurationViewModel();
            var win = new Top5.Views.ConfigurationWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            win.ShowDialog();

            RefreshUIBindings();
            LoadDateData();
        });

        public ICommand OpenDefectTypesCommand => new RelayCommand(_ => {
            var vm = new DefectTypesViewModel();
            var win = new Top5.Views.DefectTypesWindow { DataContext = vm, Owner = Application.Current.MainWindow };
            win.ShowDialog();
        });

        public ICommand OpenProductionHistoryCommand => new RelayCommand(_ => {
            var vm = new ProductionHistoryViewModel();
            var win = new Top5.Views.ProductionHistoryWindow { DataContext = vm, Owner = Application.Current.MainWindow };
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

        // --- CONSTRUCTEUR STANDARD (UI) ---
        public MainViewModel()
        {
            // 1. INITIALISATION IMMÉDIATE (Corrige les avertissements de nullabilité)
            _shiftCheckTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _shiftCheckTimer.Tick += CheckShiftChange;

            _autoSaveTimer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(10) };
            _autoSaveTimer.Tick += (s, e) =>
            {
                ForceSave();
            };

            // 2. CHARGEMENT DES DONNÉES
            _lastKnownLogicalToday = Top5HistoryService.GetLogicalProductionDate(DateTime.Now);
            _viewingDate = _lastKnownLogicalToday;
            LoadDateData();

            _currentActiveShiftName = GetActiveShiftName();

            // 3. DÉMARRAGE DES TIMERS
            _shiftCheckTimer.Start();
            _autoSaveTimer.Start();

            // 4. TÂCHES DE FOND (PDF)
            _ = RunStartupTasksAsync();
        }

        // ==============================================================================
        // MOTEUR DE CHANGEMENT D'ÉQUIPE ET DE JOUR AUTOMATIQUE (SÉCURISÉ)
        // ==============================================================================
        private void CheckShiftChange(object? sender, EventArgs e)
        {
            // CORRECTION : Forcer l'interface à se mettre à jour toutes les 30 secondes
            // Cela permet de détecter et verrouiller instantanément la fin des 10 min de battement !
            RefreshUIBindings();

            var currentLogicalToday = Top5HistoryService.GetLogicalProductionDate(DateTime.Now);

            if (currentLogicalToday > _lastKnownLogicalToday)
            {
                if (_viewingDate.Date == _lastKnownLogicalToday.Date)
                {
                    Top5HistoryService.SaveDailyReport(this, _viewingDate);
                    ViewingDate = currentLogicalToday;
                }

                _lastKnownLogicalToday = currentLogicalToday;
                _currentActiveShiftName = GetActiveShiftName();
                return;
            }

            if (!IsCurrentDay) return;

            string newShift = GetActiveShiftName();
            if (newShift != _currentActiveShiftName && !string.IsNullOrEmpty(newShift))
            {
                ForceSave();

                _currentActiveShiftName = newShift;
                TransferUnresolvedDefects();

                ForceSave();
            }
        }

        private void RefreshUIBindings()
        {
            OnPropertyChanged(nameof(IsMatinActive)); OnPropertyChanged(nameof(IsApresMidiActive)); OnPropertyChanged(nameof(IsNuitActive));
            OnPropertyChanged(nameof(IsMatinEnabled)); OnPropertyChanged(nameof(IsApresMidiEnabled)); OnPropertyChanged(nameof(IsNuitEnabled));
            OnPropertyChanged(nameof(IsMatinTimeWindow)); OnPropertyChanged(nameof(IsApresMidiTimeWindow)); OnPropertyChanged(nameof(IsNuitTimeWindow));
        }

        private async Task RunStartupTasksAsync()
        {
            await Task.Delay(2000);
            Top5HistoryService.ExportMissingPdfsAsync(DateTime.Today, startAtPastDay: false);
        }

        // --- CONSTRUCTEUR SILENCIEUX (BACKGROUND PDF) ---
        public MainViewModel(DateTime backgroundDate)
        {
            // Initialisation d'urgence pour calmer le compilateur
            _shiftCheckTimer = new DispatcherTimer();
            _autoSaveTimer = new DispatcherTimer();

            _viewingDate = backgroundDate;
            LoadDateData();
        }

        private void LoadDateData()
        {
            ProductionRows.Clear();
            var catalog = ProductionDataService.Load();
            if (catalog != null && catalog.Machines != null)
            {
                foreach (var machine in catalog.Machines)
                {
                    ProductionRows.Add(new ProductionRow { Production = new ProductionContext { Machine = machine } });
                }
            }

            bool fileLoaded = Top5HistoryService.LoadDailyReport(this, _viewingDate);

            // CORRECTION : Si la journée est Vierge (Nouveau jour sans sauvegarde),
            // on force l'effacement des variables de la veille restées en mémoire !
            if (!fileLoaded)
            {
                ControllerMatin = "";
                ControllerApresMidi = "";
                ControllerNuit = "";
                TeamCommentMatin = "";
                TeamCommentApresMidi = "";
                TeamCommentNuit = "";
            }

            var latestStates = ProductionHistoryService.GetLatestProductions();
            foreach (var row in ProductionRows)
            {
                if (!fileLoaded && latestStates.ContainsKey(row.Production.Machine))
                {
                    row.Production.Piece = latestStates[row.Production.Machine].Piece;
                    row.Production.Moule = latestStates[row.Production.Machine].Moule;
                    row.Production.Priority = Top5HistoryService.GetLastKnownPriority(row.Production.Machine, _viewingDate);
                    row.Production.RefreshDMS();
                }
            }

            if (IsCurrentDay)
            {
                TransferUnresolvedDefects();
            }

            foreach (var row in ProductionRows)
            {
                if (row.ReportMatin != null) row.ReportMatin.GetControllerName = () => ControllerMatin;
                if (row.ReportApresMidi != null) row.ReportApresMidi.GetControllerName = () => ControllerApresMidi;
                if (row.ReportNuit != null) row.ReportNuit.GetControllerName = () => ControllerNuit;
            }
        }

        private void TransferUnresolvedDefects()
        {
            foreach (var row in ProductionRows)
            {
                var unresolvedDefects = DefectHistoryService.GetUnresolvedDefects(row.Production.Piece, row.Production.Moule);
                if (unresolvedDefects.Count > 0)
                {
                    var activeShift = GetActiveShift(row);
                    foreach (var defect in unresolvedDefects)
                    {
                        if (!activeShift.Defects.Any(d => d.Id == defect.Id))
                        {
                            activeShift.Defects.Add(new Defect
                            {
                                Id = defect.Id,
                                DefectType = defect.DefectType,
                                CoreNumber = defect.CoreNumber,
                                Comment = defect.Comment,
                                State = defect.State
                            });
                        }
                    }
                }
            }
        }

        public void ForceSave()
        {
            if (IsCurrentDay)
            {
                Top5HistoryService.SaveDailyReport(this, _viewingDate);
            }
        }
    }
}