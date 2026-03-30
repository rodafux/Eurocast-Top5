using System.Windows;
using Top5.ViewModels;

namespace Top5.Views
{
    public partial class DMSWindow : Window
    {
        public DMSWindow()
        {
            InitializeComponent();

            // On lie la fermeture de la fenêtre au ViewModel
            if (DataContext is DMSViewModel vm)
            {
                vm.CloseAction = new System.Action(this.Close);
            }
        }

        private void BtnAnnuler_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}