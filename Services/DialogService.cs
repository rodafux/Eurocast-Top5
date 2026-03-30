using Top5.Models;
using Top5.ViewModels;
using Top5.Views;

namespace Top5.Services
{
    public class DialogService : IDialogService
    {
        public static DialogService Instance { get; } = new DialogService();

        private DialogService() { }

        public (bool Validated, bool Deleted, Defect? Data) ShowDefectDialog(Defect? existingDefect = null)
        {
            var viewModel = new DefectDialogViewModel(existingDefect);
            var window = new DefectDialogWindow
            {
                DataContext = viewModel
            };

            if (window.ShowDialog() == true)
            {
                if (window.IsDeleted)
                {
                    return (true, true, null);
                }

                return (true, false, new Defect
                {
                    DefectType = viewModel.SelectedDefectType,
                    State = viewModel.SelectedState,
                    Comment = viewModel.Comment,
                    CoreNumber = viewModel.CoreNumber // Prise en compte du noyau
                });
            }

            return (false, false, null);
        }
    }
}