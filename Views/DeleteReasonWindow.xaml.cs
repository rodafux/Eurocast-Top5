using System.Windows;

namespace Top5.Views
{
    public partial class DeleteReasonWindow : Window
    {
        public string ReasonText => TxtReason.Text;

        public DeleteReasonWindow()
        {
            InitializeComponent();
            TxtReason.Focus();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtReason.Text))
            {
                MessageBox.Show("La saisie d'une raison est obligatoire pour procéder à la suppression.", "Information manquante", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}