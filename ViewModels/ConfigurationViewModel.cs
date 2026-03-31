using System;
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

        // NOUVEAU : Propriétés pour le PDF
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
            ConfigurationService.Save(_config);
            CloseAction?.Invoke();
        }
    }
}