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

    [ObservableProperty] private PresetAttributes _selectedPreset;
    
    [ObservableProperty] private SvgImage? _previewedSvg;
    
    private MainViewModel _mainViewModel;
    public PresetListModel(ObservableCollection<PresetAttributes> presets, PresetAttributes selectedPreset, MainViewModel mainViewModel)
    {
        Presets = presets;
        _selectedPreset = selectedPreset;
        _mainViewModel = mainViewModel;
        
        Debug.WriteLine($"PRESETLISTMODEL -> presets.count: {Presets.Count}");

        _ = PreviewBarcode();
    }

    partial void OnSelectedPresetChanged(PresetAttributes e)
    {
        _ = PreviewBarcode();
        _mainViewModel.CurrentPreset = SelectedPreset;
    }
    
    [RelayCommand]
    private void DeleteSelectedPreset()
    {
        if (Presets.Count == 1) return;

        var index = Presets.IndexOf(SelectedPreset);
        Presets.RemoveAt(index);
        
        SelectedPreset = index == Presets.Count ? Presets.Last() : Presets[index];
        // _mainViewModel.CurrentPreset = SelectedPreset;
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
        SelectedPreset = Presets.Last();
        // _ = PreviewBarcode();
        // _mainViewModel.CurrentPreset = SelectedPreset;
    }

    [RelayCommand]
    private async Task EditPreset()
    {
        var window = new PresetEditor()
        {
            DataContext =  new PresetEditorModel(SelectedPreset, out var result)
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

            SelectedPreset = Presets[index];
            // _mainViewModel.CurrentPreset = SelectedPreset;
            // _ = PreviewBarcode();
        }
    }
    
    private async Task PreviewBarcode()
    {
        try
        {
            PreviewedSvg = new SvgImage(){ Source = SvgSource.LoadFromSvg(SvgTools.CreateDefaultPresetView(SelectedPreset))};
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MAINVIEWMODEL -> error during preview barcode: {ex}");
        }
    }

    [RelayCommand]
    private void NextPreset()
    {
        Debug.WriteLine($"PRESETLISTMODEL -> {Presets.IndexOf(SelectedPreset)} / {Presets.Count}");
        var index = Presets.IndexOf(SelectedPreset) == Presets.Count - 1 ? 0 : Presets.IndexOf(SelectedPreset) + 1;
        SelectedPreset = Presets[index];
        // _ = PreviewBarcode();
    }

    [RelayCommand]
    private void PreviousPreset()
    {
        var index = Presets.IndexOf(SelectedPreset) == 0 ? Presets.Count - 1 : Presets.IndexOf(SelectedPreset) - 1;
        SelectedPreset = Presets[index];  
        // _ = PreviewBarcode();
    }
}