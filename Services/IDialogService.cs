using Top5.Models;

namespace Top5.Services
{
    public interface IDialogService
    {
        (bool Validated, bool Deleted, Defect? Data) ShowDefectDialog(Defect? existingDefect = null, ProductionContext? context = null, string controller = "Inconnu", bool isReadOnly = false);
    }
}