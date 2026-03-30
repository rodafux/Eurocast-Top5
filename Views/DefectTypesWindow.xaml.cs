using System.Windows;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class DefectTypesWindow : Window
    {
        public DefectTypesWindow()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                if (DataContext is DefectTypesViewModel viewModel)
                {
                    viewModel.CloseAction = () => this.Close();
                }
            };
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}