using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Etiquetadora.Models;

namespace Etiquetadora.Services;

public interface IPresetService
{
    public PresetAttributes GetCurrentPreset();
    public ObservableCollection<PresetAttributes> LoadPresets();
    public Task SaveFromCollection(ObservableCollection<PresetAttributes> presets);
    public void AddPreset(PresetAttributes preset);
}