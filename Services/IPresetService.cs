using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Etiquetadora.Models;

namespace Etiquetadora.Services;

public interface IPresetService
{
    public PresetAttributes GetCurrentPreset();
    public ObservableCollection<PresetAttributes> LoadPresets();
    public void SavePreset(PresetAttributes preset);
}