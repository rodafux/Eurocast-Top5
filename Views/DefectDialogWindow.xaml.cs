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
                // 1. Vérification du Type de défaut
                if (string.IsNullOrWhiteSpace(vm.SelectedDefectType))
                {
                    MessageBox.Show("Veuillez sélectionner ou taper un type de défaut.", "Saisie invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. Vérification du Noyau (si requis)
                if (vm.IsCoreNumberRequired && string.IsNullOrWhiteSpace(vm.CoreNumber))
                {
                    MessageBox.Show("Veuillez saisir un numéro de noyau pour ce type de défaut.", "Information manquante", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 3. Vérification de l'état obligatoire
                if (vm.SelectedState == ControlState.NonRenseigne)
                {
                    MessageBox.Show("Veuillez obligatoirement sélectionner un état pour ce défaut :\n- Conforme\n- À Améliorer\n- Non Conforme",
                                    "État manquant", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                vm.SaveCommand.Execute(null);
            }
        }

        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûr de vouloir supprimer ce défaut ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
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