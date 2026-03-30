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

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public Action? CloseAction { get; set; } // Ajout du ?

        public ConfigurationViewModel()
        {
            _config = ConfigurationService.Load();
            SaveCommand = new RelayCommand(ExecuteSave);
            CancelCommand = new RelayCommand(_ => CloseAction?.Invoke());
        }

        private void ExecuteSave(object? obj) // Ajout du ?
        {
            ConfigurationService.Save(_config);
            CloseAction?.Invoke();
        }
    }
}