using CommunityToolkit.Mvvm.ComponentModel;
using Etiquetadora.Services;

namespace Etiquetadora.ViewModels;

public partial class PresetEditorModel : ViewModelBase
{
    [ObservableProperty]
    public partial PresetAttributes Preset { get; set; }

    public PresetEditorModel(PresetAttributes preset, out PresetAttributes result)
    {
        Preset = preset;
        result = Preset;
    }
}