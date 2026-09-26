using System;
using System.IO;
using Barcoder;
using Barcoder.Renderer.Svg;
using Barcoder.Renderers;
using SvgLib;

namespace Etiquetadora.Models;

public class BarcodeRenderer : IBarcodeRenderer
{
    private readonly PresetAttributes _options;
    private const double MmPerPixel = 3.7795;

    public BarcodeRenderer(PresetAttributes? options = null)
    {
        _options = options ?? new PresetAttributes();
    }

    public void Render(IBarcode barcode, Stream outputStream, out double outputWidth)
    {
        barcode = barcode ?? throw new ArgumentNullException(nameof(barcode));
        outputStream = outputStream ?? throw new ArgumentNullException(nameof(outputStream));
        if (barcode.Bounds.Y == 1)
            Render1D(barcode, outputStream, out outputWidth);
        else
            throw new NotSupportedException($"Y value of {barcode.Bounds.Y} is invalid");
    }

    private void Render1D(IBarcode barcode, Stream outputStream, out double outputWidth)
    {
        var document = SvgDocument.Create();
        double height = _options.BarcodeHeight*MmPerPixel;
            
        document.Width = barcode.Bounds.X;
        outputWidth = barcode.Bounds.X;
        document.Height = height;
        document.Fill = "#FFFFFF";
        document.Stroke = "#000000";
        document.StrokeWidth = 1;
        document.StrokeLineCap = SvgStrokeLineCap.Butt;

        var prevBar = false;
        for (var x = 0; x < barcode.Bounds.X; x++)
        {
            if (!barcode.At(x, 0))
            {
                prevBar = false;
                continue;
            }

            SvgLine line;
            double lineHeight = height;

            if (prevBar)
            {
                line = document.AddLine();
                line.StrokeWidth = 1.5;
                line.X1 = line.X2 = x - 0.25;
                line.Y1 = 0;
                line.Y2 = lineHeight;
            }
            else
            {
                line = document.AddLine();
                line.X1 = line.X2 = x;
                line.Y1 = 0;
                line.Y2 = lineHeight;
            }

            prevBar = true;
        }

        document.Save(outputStream);
    }
}