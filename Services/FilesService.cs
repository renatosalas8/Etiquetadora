using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Etiquetadora.Services;

public class FilesService : IFilesService
{
    private readonly Window _target;

    public FilesService(Window target)
    {
        _target = target;
    }

    public async Task<IStorageFile?> OpenFileAsync()
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            AllowMultiple = false
        });

        return files.Count >= 1 ? files[0] : null;
    }

    public async Task<IStorageFile?> SaveFileAsync(string? fileType, string? fileName)
    {
        return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            DefaultExtension = $"{fileType}",
            SuggestedFileType = new FilePickerFileType($"{fileType}")
            {
                Patterns = [$"*.{fileType}"],
                AppleUniformTypeIdentifiers = [fileType]
            },
            SuggestedFileName = $"{fileName}.{fileType}"
        });
    }
}