using System.Windows;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class DMSWindow : Window
    {
        public DMSWindow()
        {
            InitializeComponent();

            DataContextChanged += (s, e) =>
            {
                if (DataContext is DMSViewModel vm)
                {
                    vm.CloseAction = () => this.Close();
                }
            };
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}