using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Svg.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Etiquetadora.Models;
using Etiquetadora.Services;
using Etiquetadora.Views;
using Microsoft.Extensions.DependencyInjection;
using SvgImage = Avalonia.Svg.Skia.SvgImage;

namespace Etiquetadora.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanDeleteInputs))] [NotifyPropertyChangedFor(nameof(CanSaveTag))] public partial string? ProductName { get; set; }
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanDeleteInputs))] [NotifyPropertyChangedFor(nameof(CanSaveTag))] public partial string? ProductPrice { get; set; }
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(CanDeleteInputs))] [NotifyPropertyChangedFor(nameof(CanSaveTag))] public partial string? ProductSku { get; set; }
    [ObservableProperty] public partial bool IsPriceInvalid { get; set; }

    // public string _presetInfo = $"{_currentPreset.TagWidth}mm x {_currentPreset.TagHeight}mm";
    [ObservableProperty] public partial string PresetInfo { get; set; } //get => _presetInfo; set => SetProperty(ref _presetInfo, value);

    private static IPresetService PresetService =>  App.Current?.Services.GetService<IPresetService>();
    [ObservableProperty] private static ObservableCollection<PresetAttributes> _presets = PresetService.LoadPresets();
    [ObservableProperty] public static int _currentPresetIndex = 0;

    public bool CanDeleteInputs => !string.IsNullOrWhiteSpace(ProductName) 
                                                          || !string.IsNullOrWhiteSpace(ProductPrice) 
                                                          || !string.IsNullOrWhiteSpace(ProductSku);
    
    public bool CanSaveTag => !string.IsNullOrWhiteSpace(ProductName) 
                              && !string.IsNullOrWhiteSpace(ProductPrice) 
                              && !string.IsNullOrWhiteSpace(ProductSku);
    
    private static string PreviewedSvg = SvgTools.CreateDefaultPresetView(_presets[_currentPresetIndex]);
    private bool _isPreviewed = true;
    private static string? _renderedSvg;
    private bool _changedPreset = false;

    public MainViewModel()
    {
        PresetInfo = $"{_presets[_currentPresetIndex].TagWidth}mm x {_presets[_currentPresetIndex].TagHeight}mm";
    }
    
    [ObservableProperty]
    public partial SvgImage? BarcodeSvg { get; set; } = new() { Source = SvgSource.LoadFromSvg(PreviewedSvg) };
    
    partial void OnCurrentPresetIndexChanged(int value)
    {
        _changedPreset = true;
        _ = PreviewBarcode();
        PresetInfo = $"{_presets[_currentPresetIndex].TagWidth}mm x {_presets[_currentPresetIndex].TagHeight}mm";
    }
    
    [RelayCommand]
    private void NewLabel()
    {
        ProductName = string.Empty;
        ProductPrice = string.Empty;
        ProductSku = string.Empty;
        IsPriceInvalid = false;

        _ = PreviewBarcode();
    }

    partial void OnProductPriceChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            IsPriceInvalid = false;
            return;
        }

        // Strip out all non-digit characters
        var rawDigits = new string(value.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(rawDigits))
        {
            IsPriceInvalid = true;
            return;
        }

        if (long.TryParse(rawDigits, out long numericValue))
        {
            IsPriceInvalid = false;
            var formattedPrice = $"{numericValue:C0}";

            // Guard against recursive re-entry loops
            if (ProductPrice != formattedPrice)
            {
                ProductPrice = formattedPrice;
            }
        }
        else
        {
            IsPriceInvalid = true;
        }

        _ = PreviewBarcode();
    }
    
    private async Task PreviewBarcode()
    {
        try
        {
            if (string.IsNullOrEmpty(ProductPrice) //if previewing
                || string.IsNullOrEmpty(ProductName) 
                || string.IsNullOrEmpty(ProductSku)
                || IsPriceInvalid == true)
            {
                if (_changedPreset == true)
                {
                    _changedPreset = false;
                    PreviewedSvg = await Task.Run(() => SvgTools.CreateDefaultPresetView(_presets[_currentPresetIndex]));
                } 
                _isPreviewed = true;
                BarcodeSvg = new SvgImage { Source = SvgSource.LoadFromSvg(PreviewedSvg) };
                return;
            }
            
            //if rendering a tag
            _isPreviewed = false;
            _renderedSvg = SvgTools.RenderTag(ProductSku, ProductPrice, ProductName, _presets[_currentPresetIndex]);
            BarcodeSvg = new SvgImage { Source = SvgSource.LoadFromSvg(_renderedSvg) };
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MAINVIEWMODEL -> error during preview barcode: {ex}");
        }
    }

    partial void OnProductSkuChanged(string? value)
    {
        _ = PreviewBarcode();
    }

    partial void OnProductNameChanged(string? value)
    {
        _ = PreviewBarcode();
    }
    
    [RelayCommand]
    private async Task SaveFile()
    {
        try
        {
            if (_isPreviewed == true) return;
            var app = (App)App.Current!;
            var filesService = app.Services?.GetService<IFilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.SaveFileAsync("svg", $"{ProductName}_{ProductSku}");
            if (file is null || string.IsNullOrEmpty(_renderedSvg)) return;
            
            var stream = new MemoryStream(Encoding.Default.GetBytes(_renderedSvg));
            
            await using var writeStream = await file.OpenWriteAsync();
            await stream.CopyToAsync(writeStream);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
    
    [RelayCommand]
    private async Task ShowPresetsWindow()
    {
        var window = new PresetList()
        {
            DataContext = new PresetListModel(Presets,  CurrentPresetIndex, this)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainW = desktop.MainWindow; 
            if (mainW != null) 
            {
                await window.ShowDialog(mainW);
                await PresetService.SaveFromCollection(Presets);
            }
        }
    }

    [RelayCommand]
    private async Task EditPresetWindow()
    {
        var window = new PresetEditor()
        {
            DataContext = new PresetEditorModel(_presets[_currentPresetIndex], out var result)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainW = desktop.MainWindow; 
            if (mainW != null) 
            {
                await window.ShowDialog(mainW);
                
                Presets[CurrentPresetIndex] =  result;
                
                // var index = Presets.IndexOf(_presets[_currentPresetIndex]);
                // Presets[index] = result;
                // // CurrentPreset = result;
                await PresetService.SaveFromCollection(Presets);
            }
        }
    }

    [RelayCommand]
    private async Task NewPresetWindow()
    {
        var window = new PresetEditor()
        {
            DataContext = new PresetEditorModel(new(){ Name = "Nueva plantilla"}, out var result)
        };

        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var mainW = desktop.MainWindow; 
            if (mainW != null) 
            {
                await window.ShowDialog(mainW);
                Presets.Add(result);
                _currentPresetIndex = Presets.Count - 1;
            }
        }
    }
}