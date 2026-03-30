using System.Windows;

namespace Top5
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Utils.Logger.Log("Démarrage de l'application Top5.");
        }
    }
}