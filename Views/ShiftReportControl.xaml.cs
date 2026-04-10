using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Top5.Models;

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
                // AUTORISATION DE CONSULTATION : Si l'équipe est verrouillée, on ignore les blocages de contrôleur 
                // pour permettre à l'utilisateur de cliquer sur un défaut et l'ouvrir en mode Lecture Seule.
                if (!report.IsEditable) return false;

                string cleanName = (report.GetControllerName?.Invoke() ?? "").Trim();
                if (string.IsNullOrEmpty(cleanName) || cleanName.Equals("Inconnu", StringComparison.OrdinalIgnoreCase) || cleanName == "?")
                {
                    MessageBox.Show("Action refusée.\n\nVeuillez d'abord renseigner votre nom en haut de la colonne.", "Identification", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return true;
                }
            }
            return false;
        }
    }
}