using System.ComponentModel;
using System.Windows;
using Top5.Models; // NOUVEAU
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.ForceSave();
            }
        }

        // NOUVEAU : Le clic sur la pastille de priorité
        private void BtnPriority_Click(object sender, RoutedEventArgs e)
        {
            // On récupère la ligne sur laquelle on vient de cliquer
            if (sender is System.Windows.Controls.Button btn && btn.DataContext is ProductionRow row)
            {
                // On fait tourner la priorité : 0 -> 1 -> 2 -> 3 -> 0
                row.Production.Priority = row.Production.Priority >= 3 ? 0 : row.Production.Priority + 1;
            }
        }

        private void DataGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {

        }
    }
}