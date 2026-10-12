using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public PresetAttributes GetCurrentPreset()
    {
        Debug.WriteLine($"PRESETSERVICE -> _presetFilePath: {_presetFilePath}");
        if (!File.Exists(_presetFilePath))
        {
            CreatePresetFile();
        }

        ObservableCollection<PresetAttributes> presets =
            JsonSerializer.Deserialize<ObservableCollection<PresetAttributes>>(File.ReadAllText(_presetFilePath));
        return presets.FirstOrDefault();
    }

    public ObservableCollection<PresetAttributes> LoadPresets()
    {
        if (!File.Exists(_presetFilePath))
        {
            CreatePresetFile();
        }

        ObservableCollection<PresetAttributes> presets = JsonSerializer.Deserialize<ObservableCollection<PresetAttributes>>(File.ReadAllText(_presetFilePath));
        if(presets == null || presets.Count == 0)
        {
            CreatePresetFile();
            presets = JsonSerializer.Deserialize<ObservableCollection<PresetAttributes>>(File.ReadAllText(_presetFilePath));
        }
        return presets;
    }
    
    private readonly string _presetFilePath = Path.Combine(Directory.GetCurrentDirectory(), "presets.json");

    public async void AddPreset(PresetAttributes preset)
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
        await File.WriteAllTextAsync(_presetFilePath, JsonSerializer.Serialize(presetsRead, new JsonSerializerOptions(){ WriteIndented = true }));
    }

    private void CreatePresetFile()
    {
        var onlyPreset = new PresetAttributes();
        ObservableCollection<PresetAttributes> presets = [onlyPreset];
        File.WriteAllTextAsync(_presetFilePath, JsonSerializer.Serialize(presets, new JsonSerializerOptions(){ WriteIndented = true }));
    }

    public async Task SaveFromCollection(ObservableCollection<PresetAttributes> presets)
    {
        await File.WriteAllTextAsync(_presetFilePath, JsonSerializer.Serialize(presets, new JsonSerializerOptions(){ WriteIndented = true }));
    }
}
