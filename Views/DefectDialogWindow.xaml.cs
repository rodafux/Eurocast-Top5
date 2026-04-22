using System.Windows;
using Top5.Models;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class DefectDialogWindow : Window
    {
        public bool IsDeleted { get; private set; }

        public DefectDialogWindow()
        {
            InitializeComponent();
            IsDeleted = false;

            DataContextChanged += (s, e) => {
                if (DataContext is DefectDialogViewModel vm)
                {
                    vm.CloseAction = () => { this.DialogResult = true; };
                }
            };
        }

        private void BtnValider_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is DefectDialogViewModel vm)
            {
                if (string.IsNullOrWhiteSpace(vm.SelectedDefectType))
                {
                    MessageBox.Show("Veuillez sélectionner ou taper un type de défaut.", "Saisie invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (vm.IsCoreNumberRequired && string.IsNullOrWhiteSpace(vm.CoreNumber))
                {
                    MessageBox.Show("Veuillez saisir un numéro de noyau pour ce type de défaut.", "Information manquante", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (vm.SelectedState == ControlState.NonRenseigne)
                {
                    MessageBox.Show("Veuillez obligatoirement sélectionner un état pour ce défaut :\n- Validé\n- À Améliorer\n- Non Conforme",
                                    "État manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                vm.SaveCommand.Execute(null);
            }
        }

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            // Appel de la fenêtre personnalisée de suppression
            var reasonWindow = new DeleteReasonWindow { Owner = this };

            if (reasonWindow.ShowDialog() == true)
            {
                if (DataContext is DefectDialogViewModel vm)
                {
                    string cause = reasonWindow.ReasonText.Trim();
                    string prefix = string.IsNullOrWhiteSpace(vm.Comment) ? "" : vm.Comment.Trim() + "\n";

                    // On modifie le commentaire en temps réel avant de renvoyer le signal de suppression
                    vm.Comment = $"{prefix}(Cause de la suppression : {cause})";
                }

                IsDeleted = true;
                this.DialogResult = true;
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}