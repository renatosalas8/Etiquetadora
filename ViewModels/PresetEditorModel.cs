using CommunityToolkit.Mvvm.ComponentModel;
using Etiquetadora.Services;

namespace Etiquetadora.ViewModels;

public partial class PresetEditorModel : ViewModelBase
{
    [ObservableProperty] private PresetAttributes _preset;

    public PresetEditorModel(PresetAttributes preset, out PresetAttributes result)
    {
        _preset = preset;
        result = _preset;
    }
}