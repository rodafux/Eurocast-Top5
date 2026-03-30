using System.Windows;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class DailyProductionWindow : Window
    {
        public DailyProductionWindow()
        {
            InitializeComponent();

            if (DataContext is DailyProductionViewModel viewModel)
            {
                viewModel.CloseAction = new System.Action(this.Close);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}