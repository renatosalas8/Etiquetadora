using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Svg.Skia;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Etiquetadora.Models;
using Etiquetadora.Views;

namespace Etiquetadora.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty] public partial string? ProductName { get; set; }
    [ObservableProperty] public partial string? ProductPrice { get; set; }
    [ObservableProperty] public partial string? ProductSku { get; set; }
    [ObservableProperty] public partial bool? IsPriceInvalid { get; set; }
    static PresetAttributes _currentPreset = new();
    private static SvgSource _previewSvg = SvgTools.CreateDefaultPresetView(_currentPreset);
    [ObservableProperty] public partial SvgImage? BarcodeSvg { get; set; } = new SvgImage {Source = _previewSvg};
    
    [ObservableProperty] private PresetAttributes _temporalPreset = new(); //TODO SACAR MEIRDA
    
    [RelayCommand]
    private void NewLabel()
    {
        ProductName = string.Empty;
        ProductPrice = string.Empty;
        ProductSku = string.Empty;
        IsPriceInvalid = false;
        BarcodeSvg = new SvgImage(){Source =  _previewSvg};
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
        try
        {
            if (string.IsNullOrEmpty(ProductPrice) 
                || string.IsNullOrEmpty(ProductName) 
                || string.IsNullOrEmpty(ProductSku))
            {
                var preview = await Task.Run(() => SvgTools.CreateDefaultPresetView(_currentPreset));

                BarcodeSvg = new SvgImage() { Source = preview };
                return;
            }

            if (IsPriceInvalid == false)
            {
                var render = await Task.Run(() => SvgTools.RenderTag(ProductSku!, ProductPrice, ProductName, _currentPreset));
                BarcodeSvg = new SvgImage { Source = render };
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"error during preview barcode: {ex}");
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
    
    /*private void LoadPresets()
    {
        Presets.Clear();
        // Cargá tus presets reales acá (desde SettingsService, por ejemplo)
        Presets.Add(new TagPreset { Id = "1", Name = "Etiqueta Chica" });
        Presets.Add(new TagPreset { Id = "2", Name = "Etiqueta Grande" });

        // Siempre al final
        Presets.Add(new TagPreset { Name = "+ Crear nuevo preset", IsCreateNew = true });
    }
    
    partial void OnSelectedPresetChanged(TagPreset? value)
    {
        if (value?.IsCreateNew == true)
        {
            CreateNewPreset();
        }
        else if (value != null)
        {
            ApplyPreset(value);
        }
    }

    private void CreateNewPreset()
    {
        // Abrir diálogo/ventana para crear preset nuevo
        // Después de crear, LoadPresets() de nuevo y seleccionar el nuevo

        // Importante: resetear la selección para no dejar "Crear nuevo" seleccionado
        SelectedPreset = null;
    }

    private void ApplyPreset(TagPreset preset)
    {
        // Aplicar los valores del preset a tu estado actual
    }

    [RelayCommand]
    private void DeletePreset(TagPreset preset)
    {
        Presets.Remove(preset);
        // Guardar cambios en tu SettingsService
    }*/
} 

/*public partial class TagPreset
{
    public string Name { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;
    public bool IsCreateNew { get; set; }
    
    [RelayCommand]
    private void EditPreset()
    {
        // Abrir ventana de edición con los datos de 'preset'
    }
}*/