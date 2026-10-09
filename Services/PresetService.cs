using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Avalonia.Controls;
using Etiquetadora.Models;
using Etiquetadora.ViewModels;

namespace Etiquetadora.Services;

public class PresetService : IPresetService
{
    private readonly Window _target;
    public PresetService(Window target)
    {
        _target = target;
    }
    
    public PresetAttributes GetCurrentPreset()
    {
        if (!File.Exists(_presetFilePath))
        {
            CreatePresetFile();
        }

        ObservableCollection<PresetAttributes> presets =
            JsonSerializer.Deserialize<ObservableCollection<PresetAttributes>>(File.ReadAllText(_presetFilePath));
        return presets.FirstOrDefault();
    }

    public ObservableCollection<PresetAttributes?> LoadPresets()
    {
        if (!File.Exists(_presetFilePath))
        {
            CreatePresetFile();
        }

        ObservableCollection<PresetAttributes> presets = JsonSerializer.Deserialize<ObservableCollection<PresetAttributes>>(File.ReadAllText(_presetFilePath));
        return presets;
    }
    
    private readonly string _presetFilePath = Path.Combine(Directory.GetCurrentDirectory(), "presets.json");

    public async void SavePreset(PresetAttributes preset)
    {
        //si no existe el archivo
        if (!File.Exists(_presetFilePath))
        {
            CreatePresetFile();
            return;
        }
        //todo si esta con archivos
        ObservableCollection<PresetAttributes> presetsRead = JsonSerializer.Deserialize<ObservableCollection<PresetAttributes>>(File.ReadAllText(_presetFilePath))!;
        presetsRead.Add(preset);
        await File.WriteAllTextAsync(_presetFilePath, JsonSerializer.Serialize(presetsRead));
    }

    private async void CreatePresetFile()
    {
        var onlyPreset = new PresetAttributes();
        ObservableCollection<PresetAttributes> presets = [onlyPreset];
        await File.WriteAllTextAsync(_presetFilePath, JsonSerializer.Serialize(presets));
    }
}
