using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Etiquetadora.Models;

public static class PresetService
{
    private static string PresetsFile => Path.Combine(AppContext.BaseDirectory,"presets.json");

    public static List<PresetAttributes> LoadPresets()
    {
        if (!File.Exists(PresetsFile) || string.IsNullOrWhiteSpace(File.ReadAllText(PresetsFile)))
        {
            var defaults =  new List<PresetAttributes>() {new PresetAttributes()};
            SavePresets(defaults);
            return defaults;
        }

        var presets = File.ReadAllText(PresetsFile);
        return JsonSerializer.Deserialize<List<PresetAttributes>>(presets) ?? [];
    }

    public static PresetAttributes GetPreset()
    {
        var presets = LoadPresets();
        return presets.FirstOrDefault() ?? new PresetAttributes();
    }

    public static void SavePreset(PresetAttributes preset)
    {
        var presets = LoadPresets();
        presets.Add(preset);
        SavePresets(presets);
    }

    public static void SavePresets(List<PresetAttributes> presets)
    {
        var json = JsonSerializer.Serialize(presets, options: new JsonSerializerOptions(){WriteIndented = true });
        File.WriteAllText(PresetsFile, json);
    }
}