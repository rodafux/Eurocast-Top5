using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Top5.Models;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class ShiftReportControl : UserControl
    {
        public ShiftReportControl()
        {
            InitializeComponent();
        }

        // ====================================================================
        // NIVEAU 1 : INTERCEPTION SYSTÈME (La méthode absolue)
        // Intercepte le clic de souris AVANT même qu'il ne rentre dans le contrôle
        // ====================================================================
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsSecurityBlocked())
            {
                e.Handled = true; // Bloque le clic
                return;
            }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        // Intercepte la navigation au clavier (Touche TAB)
        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (IsSecurityBlocked())
            {
                e.Handled = true; // Bloque le focus
                Keyboard.ClearFocus(); // Éjecte le clavier de la zone
                return;
            }
            base.OnPreviewGotKeyboardFocus(e);
        }

        // La logique de blocage centralisée
        private bool IsSecurityBlocked()
        {
            if (DataContext is ShiftReport report)
            {
                string rawName = report.GetControllerName?.Invoke() ?? "";
                string cleanName = rawName.Trim();

                if (string.IsNullOrEmpty(cleanName) ||
                    cleanName.Equals("Inconnu", StringComparison.OrdinalIgnoreCase) ||
                    cleanName == "?")
                {
                    MessageBox.Show(
                        "Action refusée.\n\nVeuillez d'abord renseigner votre nom en haut de la colonne pour identifier votre équipe avant de pouvoir saisir ou modifier des données.",
                        "Identification requise",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);

                    return true; // Le bouclier s'active
                }
            }
            return false; // Laisse passer
        }

        // ====================================================================
        // NIVEAU 2 : SÉCURITÉ DU BOUTON "+"
        // ====================================================================
        private void BtnAddDefect_Click(object sender, RoutedEventArgs e)
        {
            // Ultime vérification de sécurité au cas où un clic passerait
            if (IsSecurityBlocked()) return;

            var vm = new DefectDialogViewModel();
            var window = new DefectDialogWindow { DataContext = vm, Owner = Window.GetWindow(this) };

            if (window.ShowDialog() == true)
            {
                var newDefect = new Defect
                {
                    Id = Guid.NewGuid(),
                    DefectType = vm.SelectedDefectType,
                    State = vm.SelectedState,
                    Comment = vm.Comment,
                    CoreNumber = vm.CoreNumber
                };

                if (DataContext is ShiftReport report)
                {
                    var parentRow = GetParentProductionRow();
                    if (parentRow != null)
                    {
                        var context = parentRow.Production;

                        if (!string.IsNullOrWhiteSpace(newDefect.CoreNumber))
                        {
                            var alertHistory = Top5.Services.DefectHistoryService.CheckCoreHistory(context.Piece, context.Moule, newDefect.CoreNumber);

                            if (alertHistory.Count > 0)
                            {
                                var alertWin = new Top5.Views.CoreAlertWindow(alertHistory)
                                {
                                    Owner = Application.Current.MainWindow
                                };
                                alertWin.ShowDialog();
                            }
                        }

                        report.Defects.Add(newDefect);
                        Top5.Services.DefectHistoryService.LogDefectAction(context, report.GetControllerName?.Invoke() ?? "Inconnu", newDefect, "Création");
                    }
                }
            }
        }

        private ProductionRow? GetParentProductionRow()
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null && !(parent is DataGridRow))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return (parent as DataGridRow)?.Item as ProductionRow;
        }
    }
}