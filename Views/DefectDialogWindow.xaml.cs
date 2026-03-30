using System.Windows;

namespace Top5.Views
{
    public partial class DefectDialogWindow : Window
    {
        // LA PROPRIÉTÉ MANQUANTE EST ICI :
        public bool IsDeleted { get; private set; }

        public DefectDialogWindow()
        {
            InitializeComponent();
            IsDeleted = false;
        }

        private void BtnValider_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is Top5.ViewModels.DefectDialogViewModel vm)
            {
                if (string.IsNullOrWhiteSpace(vm.SelectedDefectType) || !vm.AvailableDefects.Contains(vm.SelectedDefectType))
                {
                    MessageBox.Show("Veuillez sélectionner un type de défaut figurant dans la liste officielle.", "Saisie invalide", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (vm.IsCoreNumberRequired && string.IsNullOrWhiteSpace(vm.CoreNumber))
                {
                    MessageBox.Show("Veuillez saisir un numéro de noyau pour ce type de défaut.", "Information manquante", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            this.DialogResult = true;
        }

        // LA FONCTION DU BOUTON SUPPRIMER :
        private void BtnSupprimer_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Êtes-vous sûr de vouloir supprimer ce défaut ?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                IsDeleted = true;
                this.DialogResult = true;
            }
        }
    }
}