using System.Threading.Tasks;

namespace OnlyVThemeCreator.Services
{
    public interface IDialogService
    {
        Task<bool?> ShouldSaveDirtyDataAsync();

        bool IsDialogVisible();
    }
}
