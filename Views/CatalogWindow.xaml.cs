using System.Windows;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class CatalogWindow : Window
    {
        public CatalogWindow()
        {
            InitializeComponent();

            // Permet au ViewModel de fermer la fenêtre tout seul après la sauvegarde
            if (DataContext is CatalogViewModel vm)
            {
                vm.CloseAction = () => this.Close();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}