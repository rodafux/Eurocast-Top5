using Top5.Models;

namespace Top5.Services
{
    public interface IDialogService
    {
        // Retourne un tuple : Validé (bool), Supprimé (bool), et les nouvelles données du défaut
        (bool Validated, bool Deleted, Defect? Data) ShowDefectDialog(Defect? existingDefect = null);
    }
}