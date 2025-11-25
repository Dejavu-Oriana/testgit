using System.Threading.Tasks;

namespace DailyPoetryA.Library.Services;

public interface IFileDialogService
{
    Task<string[]?> OpenFileDialogAsync(string title, string filters, bool allowMultiple = false);
    Task<string?> SaveFileDialogAsync(string title, string fileName, string filters);
}     