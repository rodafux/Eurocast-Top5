using System.Windows;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class ConfigurationWindow : Window
    {
        public ConfigurationWindow()
        {
            InitializeComponent();

            // Correction EDR / MVVM : Attendre que le DataContext soit injecté
            DataContextChanged += (s, e) =>
            {
                if (DataContext is ConfigurationViewModel viewModel)
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