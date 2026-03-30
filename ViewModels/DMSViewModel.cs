using System;
using System.Windows;
using System.Windows.Input;
using Top5.Models;
using Top5.Services;

namespace Top5.ViewModels
{
    public class DMSViewModel : ViewModelBase
    {
        private readonly ProductionContext _context;

        public string Title => $"Démarrage Série : {_context.Machine} | {_context.Piece} | {_context.Moule}";

        // ==========================================
        // NOUVEAU : Propriété pour le choix de la date
        // ==========================================
        private DateTime _selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); }
        }

        // Les 8 points de contrôle
        private bool _pt1, _pt2, _pt3, _pt4, _pt5, _pt6, _pt7, _pt8;
        public bool Pt1 { get => _pt1; set { _pt1 = value; OnPropertyChanged(); CheckValidation(); } }
        public bool Pt2 { get => _pt2; set { _pt2 = value; OnPropertyChanged(); CheckValidation(); } }
        public bool Pt3 { get => _pt3; set { _pt3 = value; OnPropertyChanged(); CheckValidation(); } }
        public bool Pt4 { get => _pt4; set { _pt4 = value; OnPropertyChanged(); CheckValidation(); } }
        public bool Pt5 { get => _pt5; set { _pt5 = value; OnPropertyChanged(); CheckValidation(); } }
        public bool Pt6 { get => _pt6; set { _pt6 = value; OnPropertyChanged(); CheckValidation(); } }
        public bool Pt7 { get => _pt7; set { _pt7 = value; OnPropertyChanged(); CheckValidation(); } }
        public bool Pt8 { get => _pt8; set { _pt8 = value; OnPropertyChanged(); CheckValidation(); } }

        private string _valideur = string.Empty;
        public string Valideur
        {
            get => _valideur;
            set { _valideur = value; OnPropertyChanged(); CheckValidation(); }
        }

        private bool _canValidate;
        public bool CanValidate
        {
            get => _canValidate;
            set { _canValidate = value; OnPropertyChanged(); }
        }

        public ICommand ValidateCommand { get; }
        public Action? CloseAction { get; set; }

        public DMSViewModel(ProductionContext context)
        {
            _context = context;
            ValidateCommand = new RelayCommand(ExecuteValidate);
        }

        private void CheckValidation()
        {
            // Le bouton Valider ne s'allume que si TOUT est coché ET qu'un nom est entré
            CanValidate = Pt1 && Pt2 && Pt3 && Pt4 && Pt5 && Pt6 && Pt7 && Pt8 && !string.IsNullOrWhiteSpace(Valideur);
        }

        private void ExecuteValidate(object? obj)
        {
            if (!CanValidate) return;

            // ==========================================
            // NOUVEAU : On envoie la date sélectionnée au service
            // ==========================================
            DMSService.LogDMS(_context.Machine, _context.Piece, _context.Moule, Valideur, SelectedDate);

            // On force le rafraîchissement de la pastille de couleur sur la fenêtre principale
            _context.RefreshDMS();

            CloseAction?.Invoke();
        }
    }
}