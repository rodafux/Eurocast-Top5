using System.Windows;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class CatalogWindow : Window
    {
        public CatalogWindow()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                if (DataContext is CatalogViewModel vm)
                {
                    vm.CloseAction = () => this.Close();
                }
            };
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}