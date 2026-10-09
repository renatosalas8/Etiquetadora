using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Etiquetadora.ViewModels;

namespace Etiquetadora.Services;

public class PresetAttributes
{
    public string Name { get; set; } = "Default";
    public double TagWidth { get; set; } = 44.7;
    public double TagHeight { get; set; } = 64;
    public double BarcodeHeight { get; set; } = 6;
    public double SideMargin { get; set; } = 6;
    public double BottomMargin { get; set; } = 8.2;
}