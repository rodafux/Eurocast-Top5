using Top5.Models;
using Top5.ViewModels;
using Top5.Views;

namespace Top5.Services
{
    public class DialogService : IDialogService
    {
        public static DialogService Instance { get; } = new DialogService();

        private DialogService() { }

        public (bool Validated, bool Deleted, Defect? Data) ShowDefectDialog(Defect? existingDefect = null, ProductionContext? context = null, string controller = "Inconnu", bool isReadOnly = false)
        {
            var viewModel = new DefectDialogViewModel(existingDefect, context, controller, isReadOnly);
            var window = new DefectDialogWindow
            {
                DataContext = viewModel
            };

            if (window.ShowDialog() == true)
            {
                // NOUVEAU : On transmet quand même l'objet Defect avec le commentaire modifié lors d'une suppression
                if (window.IsDeleted)
                {
                    return (true, true, new Defect
                    {
                        Comment = viewModel.Comment
                    });
                }

                return (true, false, new Defect
                {
                    DefectType = viewModel.SelectedDefectType,
                    State = viewModel.SelectedState,
                    Comment = viewModel.Comment,
                    CoreNumber = viewModel.CoreNumber
                });
            }

            return (false, false, null);
        }
    }
}