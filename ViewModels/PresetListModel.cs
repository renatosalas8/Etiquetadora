using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Svg.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Etiquetadora.Models;
using Etiquetadora.Services;
using Etiquetadora.Views;

namespace Etiquetadora.ViewModels;

public partial class PresetListModel : ViewModelBase
{
    [ObservableProperty] public partial ObservableCollection<PresetAttributes> Presets { get; set; }

    [ObservableProperty] private int _selectedPresetIndex;
    
    [ObservableProperty] private SvgImage? _previewedSvg;
    
    private MainViewModel _mainViewModel;
    public PresetListModel(ObservableCollection<PresetAttributes> presets, int selectedPresetIndex, MainViewModel mainViewModel)
    {
        Presets = presets;
        _selectedPresetIndex = selectedPresetIndex;
        _mainViewModel = mainViewModel;
        
        Debug.WriteLine($"PRESETLISTMODEL -> presets.count: {Presets.Count}");

        _ = PreviewBarcode();
    }

    partial void OnSelectedPresetIndexChanged(int e)
    {
        _ = PreviewBarcode();
        // _mainViewModel.CurrentPresetIndex = SelectedPresetIndex;
    }
    
    [RelayCommand]
    private void DeleteSelectedPreset()
    {
        if (Presets.Count == 1) return;
        Presets.RemoveAt(SelectedPresetIndex);
        
        // SelectedPresetIndex = SelectedPresetIndex >= Presets.Count - 1 ? Presets.Count - 1 : SelectedPresetIndex;
        SelectedPresetIndex = 0;
        _mainViewModel.CurrentPresetIndex = SelectedPresetIndex;
        
        //SelectedPreset = index == Presets.Count - 1 ? Presets.Last() : Presets[index + 1];
        
        
        Debug.WriteLine($"PRESETLISTMODEL -> presets.count: {Presets.Count}");
        // _ = PreviewBarcode();
    }

    // partial void OnSelectedPresetChanged(PresetAttributes? preset)
    // {
    //     _ =  PreviewBarcode();
    // }
    
    [RelayCommand]
    private void NewPreset()
    {
        Presets.Add(new PresetAttributes());
        SelectedPresetIndex = Presets.Count - 1;
        // _ = PreviewBarcode();
        // _mainViewModel.CurrentPreset = SelectedPreset;
    }

    [RelayCommand]
    private async Task EditPreset()
    {
        var window = new PresetEditor()
        {
            DataContext =  new PresetEditorModel(Presets[SelectedPresetIndex], out var result)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainW = desktop.MainWindow.OwnedWindows[0]; 
            await window.ShowDialog(mainW);
            
            var index = Presets.IndexOf(result);
            
            if(index >= 0)
            {
                Presets[index] = result;
            }

            SelectedPresetIndex = Presets.IndexOf(result);
            // _mainViewModel.CurrentPreset = SelectedPreset;
            // _ = PreviewBarcode();
        }
    }
    
    private async Task PreviewBarcode()
    {
        try
        {
            PreviewedSvg = new SvgImage(){ Source = SvgSource.LoadFromSvg(SvgTools.CreateDefaultPresetView(Presets[SelectedPresetIndex]))};
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MAINVIEWMODEL -> error during preview barcode: {ex}");
        }
    }

    [RelayCommand]
    private void NextPreset()
    {
        Debug.WriteLine($"PRESETLISTMODEL -> {SelectedPresetIndex} / {Presets.Count}");
        SelectedPresetIndex = SelectedPresetIndex == Presets.Count - 1 ? 0 : SelectedPresetIndex + 1;
        // _ = PreviewBarcode();
    }

    [RelayCommand]
    private void PreviousPreset()
    {
        SelectedPresetIndex = SelectedPresetIndex == 0 ? Presets.Count - 1 : SelectedPresetIndex - 1;
        // _ = PreviewBarcode();
    }
}