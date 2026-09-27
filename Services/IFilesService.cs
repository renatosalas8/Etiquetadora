using System.Threading.Tasks;
using Avalonia.Platform.Storage;

namespace Etiquetadora.Services;

public interface IFilesService
{
    public Task<IStorageFile?> OpenFileAsync();
    public Task<IStorageFile?> SaveFileAsync(string? fileType, string? fileName);
}