using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Top5.Models;
using Top5.Services;
using Top5.Utils;

namespace Top5.ViewModels
{
    public class DailyProductionViewModel : ViewModelBase
    {
        public ObservableCollection<ProductionAssignmentItem> AssignmentItems { get; }

        public ObservableCollection<string> AvailablePieces { get; }
        public ObservableCollection<string> AvailableMoules { get; }

        public ICommand SaveCommand { get; }

        public Action? CloseAction { get; set; }

        public DailyProductionViewModel(IEnumerable<ProductionRow> currentWorkshopState)
        {
            AvailablePieces = new ObservableCollection<string> { "---" };
            AvailableMoules = new ObservableCollection<string> { "---" };

            var catalog = ProductionDataService.Load();
            var comparer = new AlphanumericComparer();

            foreach (var p in catalog.Pieces.OrderBy(x => x, comparer)) AvailablePieces.Add(p);
            foreach (var m in catalog.Moules.OrderBy(x => x, comparer)) AvailableMoules.Add(m);

            AssignmentItems = new ObservableCollection<ProductionAssignmentItem>();
            foreach (var row in currentWorkshopState)
            {
                AssignmentItems.Add(new ProductionAssignmentItem(row.Production));
            }

            SaveCommand = new RelayCommand(ExecuteSave);
        }

        private void ExecuteSave(object? obj)
        {
            // 1. PASSE DE VALIDATION
            // On vérifie d'abord toutes les lignes modifiées avant d'enregistrer quoi que ce soit
            foreach (var item in AssignmentItems)
            {
                if (item.HasChanged)
                {
                    bool isPieceEmpty = item.SelectedPiece == "---" || string.IsNullOrWhiteSpace(item.SelectedPiece);
                    bool isMouleEmpty = item.SelectedMoule == "---" || string.IsNullOrWhiteSpace(item.SelectedMoule);

                    // XOR logique : Si l'un est vide mais pas l'autre, c'est une erreur métier
                    if (isPieceEmpty != isMouleEmpty)
                    {
                        MessageBox.Show(
                            $"Affectation invalide sur la machine {item.Machine}.\n\n" +
                            "Règle de production : Vous devez sélectionner à la fois une Pièce ET un Moule, " +
                            "ou laisser les deux sur '---' pour indiquer un arrêt.",
                            "Validation impossible",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);

                        // On interrompt la sauvegarde, la fenêtre reste ouverte pour correction
                        return;
                    }
                }
            }

            // 2. PASSE DE SAUVEGARDE
            // Si la validation passe, on applique les modifications
            foreach (var item in AssignmentItems)
            {
                if (item.HasChanged)
                {
                    item.ContextRef.Piece = item.SelectedPiece;
                    item.ContextRef.Moule = item.SelectedMoule;

                    ProductionHistoryService.LogChange(item.Machine, item.SelectedPiece, item.SelectedMoule);
                }
            }

            CloseAction?.Invoke();
        }
    }
}