using System;
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
using Svg;
using SvgDocument = SvgLib.SvgDocument;
using SvgImage = Avalonia.Svg.Skia.SvgImage;

namespace Etiquetadora.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] public partial string? ProductName { get; set; }
    [ObservableProperty] public partial string? ProductPrice { get; set; }
    [ObservableProperty] public partial string? ProductSku { get; set; }
    [ObservableProperty] public partial bool? IsPriceInvalid { get; set; }
    static PresetAttributes _currentPreset = new();
    private static string? renderedSvg = SvgTools.CreateDefaultPresetView(_currentPreset);
    [ObservableProperty] public partial SvgImage? BarcodeSvg { get; set; } = new() {Source = SvgSource.LoadFromSvg(renderedSvg)};
    
    [ObservableProperty] private PresetAttributes _temporalPreset = new(); //TODO SACAR MEIRDA
    
    
    [RelayCommand]
    private void NewLabel()
    {
        ProductName = string.Empty;
        ProductPrice = string.Empty;
        ProductSku = string.Empty;
        IsPriceInvalid = false;
        BarcodeSvg = new SvgImage(){Source =  SvgSource.LoadFromSvg(renderedSvg)};
    }

    partial void OnProductPriceChanged(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            IsPriceInvalid = false;
            return;
        }

        // Strip out all non-digit characters
        string rawDigits = new string(value.Where(char.IsDigit).ToArray());

        if (string.IsNullOrEmpty(rawDigits))
        {
            IsPriceInvalid = true;
            return;
        }

        if (long.TryParse(rawDigits, out long numericValue))
        {
            IsPriceInvalid = false;
            string formattedPrice = $"{numericValue:C0}";

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
    
    public async Task PreviewBarcode()
    {
        try //todo cuando se cambia el texto y no hay mas inputs, se genera un nuevo preview igual. corregir para que solo se cree cuando se borran entradas o es invalido
        {
            
            if (string.IsNullOrEmpty(ProductPrice) 
                || string.IsNullOrEmpty(ProductName) 
                || string.IsNullOrEmpty(ProductSku))
            {
                renderedSvg = await Task.Run(() => SvgTools.CreateDefaultPresetView(_currentPreset));

                BarcodeSvg = new SvgImage() { Source = SvgSource.LoadFromSvg(renderedSvg) };
                return;
            }

            if (IsPriceInvalid == false)
            {
                renderedSvg = SvgTools.RenderTag(ProductSku, ProductPrice, ProductName, _currentPreset);
                BarcodeSvg = new SvgImage { Source = SvgSource.LoadFromSvg(renderedSvg) };
            }
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
            var app = (App)App.Current!;
            var filesService = app.Services?.GetService<IFilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.SaveFileAsync("svg", $"{ProductName}_{ProductSku}");
            if (file is null) return;
            
            var stream = new MemoryStream(Encoding.Default.GetBytes((string)renderedSvg)); //todo barcodesvg que sea string y no la otra wea xd :'v
            
            await using var writeStream = await file.OpenWriteAsync();
            await stream.CopyToAsync(writeStream);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }
    }
}