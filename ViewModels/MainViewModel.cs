using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Svg.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Etiquetadora.Models;
using Etiquetadora.Services;
using Microsoft.Extensions.DependencyInjection;
using SvgImage = Avalonia.Svg.Skia.SvgImage;

namespace Etiquetadora.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(_canDeleteInputs))] public partial string? ProductName { get; set; }
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(_canDeleteInputs))] public partial string? ProductPrice { get; set; }
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(_canDeleteInputs))] public partial string? ProductSku { get; set; }
    [ObservableProperty] public partial bool IsPriceInvalid { get; set; }
    
    [ObservableProperty] private static ObservableCollection<PresetAttributes> presets;
    [ObservableProperty] private static PresetAttributes _currentPreset;

    public bool _canDeleteInputs => !string.IsNullOrWhiteSpace(ProductName) 
                                                          || !string.IsNullOrWhiteSpace(ProductPrice) 
                                                          || !string.IsNullOrWhiteSpace(ProductSku);
    
    private static string previewedSvg;
    private bool isPreviewed = true;
    private static string renderedSvg;
    private bool ChangedPreset = false;
    [ObservableProperty] public partial SvgImage? BarcodeSvg { get; set; }


    public MainViewModel()
    {
        var presetService = App.Current.Services.GetService<IPresetService>();
        if (presetService is null) throw new NullReferenceException("Missing PresetService instance.");

        presets = presetService.LoadPresets();
        _currentPreset = presetService.GetCurrentPreset();

        previewedSvg = SvgTools.CreateDefaultPresetView(_currentPreset);
        BarcodeSvg = new() { Source = SvgSource.LoadFromSvg(previewedSvg) };
    }
    
    partial void OnCurrentPresetChanged(PresetAttributes value)
    {
        ChangedPreset = true;
        _ = PreviewBarcode();
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
    
    [RelayCommand]
    private async Task PreviewBarcode()
    {
        try
        {
            if (string.IsNullOrEmpty(ProductPrice) //if previewing
                || string.IsNullOrEmpty(ProductName) 
                || string.IsNullOrEmpty(ProductSku)
                || IsPriceInvalid == true)
            {
                if (ChangedPreset == true)
                {
                    ChangedPreset = false;
                    previewedSvg = await Task.Run(() => SvgTools.CreateDefaultPresetView(_currentPreset));
                } 
                isPreviewed = true;
                BarcodeSvg = new SvgImage { Source = SvgSource.LoadFromSvg(previewedSvg) };
                return;
            }
            
            //if rendering a tag
            isPreviewed = false;
            renderedSvg = SvgTools.RenderTag(ProductSku, ProductPrice, ProductName, _currentPreset);
            BarcodeSvg = new SvgImage { Source = SvgSource.LoadFromSvg(renderedSvg) };
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
            if (isPreviewed == true) return;
            var app = (App)App.Current!;
            var filesService = app.Services?.GetService<IFilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.SaveFileAsync("svg", $"{ProductName}_{ProductSku}");
            if (file is null || string.IsNullOrEmpty(renderedSvg)) return;
            
            var stream = new MemoryStream(Encoding.Default.GetBytes(renderedSvg));
            
            await using var writeStream = await file.OpenWriteAsync();
            await stream.CopyToAsync(writeStream);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}