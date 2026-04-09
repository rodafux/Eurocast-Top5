using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Top5.Models;
using Top5.Services;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class ShiftReportControl : UserControl
    {
        public ShiftReportControl()
        {
            InitializeComponent();
        }

        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (IsSecurityBlocked()) { e.Handled = true; return; }
            base.OnPreviewMouseLeftButtonDown(e);
        }

        protected override void OnPreviewGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            if (IsSecurityBlocked()) { e.Handled = true; Keyboard.ClearFocus(); return; }
            base.OnPreviewGotKeyboardFocus(e);
        }

        private bool IsSecurityBlocked()
        {
            if (DataContext is ShiftReport report)
            {
                string cleanName = (report.GetControllerName?.Invoke() ?? "").Trim();
                if (string.IsNullOrEmpty(cleanName) || cleanName.Equals("Inconnu", StringComparison.OrdinalIgnoreCase) || cleanName == "?")
                {
                    MessageBox.Show("Action refusée.\n\nVeuillez d'abord renseigner votre nom en haut de la colonne.", "Identification", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return true;
                }
            }
            return false;
        }

        private void BtnAddDefect_Click(object sender, RoutedEventArgs e)
        {
            if (IsSecurityBlocked()) return;

            var vm = new DefectDialogViewModel();
            var window = new DefectDialogWindow { DataContext = vm, Owner = Window.GetWindow(this) };

            if (window.ShowDialog() == true || vm.IsSaved)
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
                        report.Defects.Add(newDefect);
                        Top5.Services.DefectHistoryService.LogDefectAction(context, report.GetControllerName?.Invoke() ?? "Inconnu", newDefect, "Création");
                    }
                }
            }
        }

        private void EditDefect_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Defect defect)
            {
                if (this.DataContext is ShiftReport report)
                {
                    var parentRow = GetParentProductionRow();
                    if (parentRow != null)
                    {
                        string controller = (report.GetControllerName?.Invoke() ?? "").Trim();

                        // On vérifie si l'utilisateur est identifié
                        bool isBlocked = string.IsNullOrEmpty(controller) || controller.Equals("Inconnu") || controller == "?";

                        var vm = new DefectDialogViewModel(defect, parentRow.Production, controller);

                        // SI BLOQUÉ : On force le mode LECTURE SEULE
                        if (isBlocked)
                        {
                            vm.IsReadOnly = true;
                        }

                        var win = new DefectDialogWindow { DataContext = vm, Owner = Window.GetWindow(this) };

                        if (win.ShowDialog() == true)
                        {
                            if (!vm.IsReadOnly) // Sécurité supplémentaire
                            {
                                if (win.IsDeleted)
                                {
                                    report.Defects.Remove(defect);
                                    DefectHistoryService.LogDefectAction(parentRow.Production, controller, defect, "Suppression");
                                }
                                else
                                {
                                    vm.FinalizeUpdate();
                                }
                            }
                        }
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