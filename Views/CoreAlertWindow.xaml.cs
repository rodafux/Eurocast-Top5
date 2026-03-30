using System.Collections.Generic;
using System.Windows;
using Top5.Models;

namespace Top5.Views
{
    public partial class CoreAlertWindow : Window
    {
        public CoreAlertWindow(List<DefectHistoryEntry> history)
        {
            InitializeComponent();
            HistoryGrid.ItemsSource = history;
            System.Media.SystemSounds.Exclamation.Play(); // Joue l'alerte sonore de Windows
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}