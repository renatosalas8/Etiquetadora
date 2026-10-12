using System;

namespace Etiquetadora.Services;

public class PresetAttributes
{
    public string Uuid => Guid.NewGuid().ToString();
    public string Name { get; set; } = "Default";
    public double TagWidth { get; set; } = 44.7;
    public double TagHeight { get; set; } = 64;
    public double BarcodeHeight { get; set; } = 6;
    public double BarcodeWidth => TagWidth - 2*SideMargin;
    public double SideMargin { get; set; } = 6;
    public double BottomMargin { get; set; } = 8.2;
}