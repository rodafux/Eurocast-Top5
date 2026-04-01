using System;
using System.Windows;
using System.Windows.Input;
using Top5.Models;
using Top5.Services;
using Top5.Utils;

namespace Top5.ViewModels
{
    public class ConfigurationViewModel : ViewModelBase
    {
        private AppConfig _config;

        public string DatabasePath
        {
            get => _config.DatabasePath;
            set { _config.DatabasePath = value; OnPropertyChanged(); }
        }

        public int NoyauAlertDays
        {
            get => _config.NoyauAlertDays;
            set { _config.NoyauAlertDays = value; OnPropertyChanged(); }
        }

        public string PdfExportPath
        {
            get => _config.PdfExportPath;
            set { _config.PdfExportPath = value; OnPropertyChanged(); }
        }

        public int PdfExportDays
        {
            get => _config.PdfExportDays;
            set { _config.PdfExportDays = value; OnPropertyChanged(); }
        }

        // NOUVEAU : Propriétés pour les horaires
        public string ShiftMatinStart
        {
            get => _config.ShiftMatinStart;
            set { _config.ShiftMatinStart = value; OnPropertyChanged(); }
        }

        public string ShiftApresMidiStart
        {
            get => _config.ShiftApresMidiStart;
            set { _config.ShiftApresMidiStart = value; OnPropertyChanged(); }
        }

        public string ShiftNuitStart
        {
            get => _config.ShiftNuitStart;
            set { _config.ShiftNuitStart = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public Action? CloseAction { get; set; }

        public ConfigurationViewModel()
        {
            _config = ConfigurationService.Load();
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(_ => CloseAction?.Invoke());
        }

        private void ExecuteSave(object? obj)
        {
            // Sécurité anti-crash : Vérifie que l'utilisateur a bien rentré des heures valides (HH:mm)
            if (!TimeSpan.TryParse(ShiftMatinStart, out _) ||
                !TimeSpan.TryParse(ShiftApresMidiStart, out _) ||
                !TimeSpan.TryParse(ShiftNuitStart, out _))
            {
                MessageBox.Show("Le format des horaires d'équipes doit être valide (ex: 04:30).", "Format Invalide", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            ConfigurationService.Save(_config);
            CloseAction?.Invoke();
        }
    }
}