using System.Windows;
using QuestPDF.Infrastructure; // NOUVEAU

namespace Top5
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // NOUVEAU : Configuration de QuestPDF
            QuestPDF.Settings.License = LicenseType.Community;

            base.OnStartup(e);
        }
    }
}