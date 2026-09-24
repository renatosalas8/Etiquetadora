using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Etiquetadora.Models;
using Etiquetadora.Views;

namespace Etiquetadora.ViewModels;

public partial class OptionsViewModel : ViewModelBase
{
    [ObservableProperty] public partial string? TagWidthOption { get; set; }
    [ObservableProperty] public partial string? TagHeightOption { get; set; }
    [ObservableProperty] public partial string? SideMarginOption { get; set; }
    [ObservableProperty] public partial string? BottomMarginOption { get; set; }
    
    public OptionsViewModel()
    {
        /*_global = _opt.Load();
         
        TagWidthOption = _global.TagWidth.ToString();
        TagHeightOption = _global.TagHeight.ToString();
        SideMarginOption = _global.SideMargin.ToString();
        BottomMarginOption = _global.BottomMargin.ToString();*/
    }

    partial void OnTagWidthOptionChanged(string? value)
    {
        if (int.TryParse(value, out var x))
        {
        }
    }
    partial void OnTagHeightOptionChanged(string? value)
    {
        if (int.TryParse(value, out var x))
        {
        }
    }
    partial void OnSideMarginOptionChanged(string? value)
    {
        if (int.TryParse(value, out var x))
        {
        }
    }
    partial void OnBottomMarginOptionChanged(string? value)
    {
        if (int.TryParse(value, out var x))
        {
        }
    }
}