using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Avalonia.Svg.Skia;
using Barcoder.Code128;

namespace Etiquetadora.Models;

public class SvgTools
{
    private const double MmPerPixel = 3.7795;
    private static readonly XNamespace ns = "http://www.w3.org/2000/svg";
    private static double _barcodeWidth;

    public static string RenderTag(string barcodeValue, string productPrice, string productName, PresetAttributes preset)
    {
        //renderizacion y elemento de barcode
        var renderedBarcode = XDocument.Parse(CreateSvgBarcode(barcodeValue));
        var barcodeRoot = renderedBarcode.Root;
        var bars = barcodeRoot!.Elements(ns + "line").ToList();
        var barcodeX = (preset.TagWidth*MmPerPixel-_barcodeWidth)/2;
        var barcodeY = (preset.TagHeight - preset.BarcodeHeight - preset.BottomMargin)*MmPerPixel;
        Debug.WriteLine($"SVGTOOLS -> renderedBarcode.Root:\n{barcodeRoot}\n");
        var barcode = new XElement(ns + "g",
            new XAttribute("id", "barcode"),
            new XAttribute("transform",
                $"translate({barcodeX.ToString(CultureInfo.InvariantCulture)}," +
                $"{barcodeY.ToString(CultureInfo.InvariantCulture)})"),
            new XAttribute("fill", "#FFFFFF"),
            new XAttribute("stroke", "#000000"),
            new XAttribute("stroke-width", "1"),
            new XAttribute("stroke-linecap", "butt"),
            bars);

        // ---- fondo etiqueta
        var background = new XElement(ns + "g",
            new XAttribute("id", "barcode_background"),
            new XElement(ns + "rect",
                new XAttribute("x", "0"),
                new XAttribute("y", "0"),
                new XAttribute("width", "100%"),
                new XAttribute("height", "100%"),
                new XAttribute("fill", "#FFFFFF")));

        // ---- precio
        var price = new XElement(ns + "g",
            new XAttribute("id", "price"),
            new XElement(ns + "text",
                productPrice,
                new XAttribute("x", "50%"),
                new XAttribute("y", "80"),
                new XAttribute("fill", "#000000"),
                new XAttribute("font-size", 8),
                new XAttribute("text-anchor", "middle")));
        
        // ---- nombre del producto
        var name = new XElement(ns+"g",
            new XAttribute("id", "name"),
            new XElement(ns + "text",
                productName,
                new XAttribute("x", "50%"),
                new XAttribute("y", "30"),
                new XAttribute("fill", "#000000"),
                new XAttribute("font-size", 12),
                new XAttribute("text-anchor", "middle")));

        // svg final
        var newRoot = new XElement(ns + "svg",
            new XAttribute("xmlns", ns.NamespaceName),
            new XAttribute("width", preset.TagWidth.ToString(CultureInfo.InvariantCulture)+"mm"),
            new XAttribute("height", preset.TagHeight.ToString(CultureInfo.InvariantCulture)+"mm"),
            background,
            price,
            barcode,
            name);

        var svg = new XDocument(newRoot).ToString();
        Debug.WriteLine($"SVGTOOLS -> finalsvg:\n{svg}");
        return svg;
    }

    private static string CreateSvgBarcode(string barcode)
    {
        try
        {
            var barcoded = Code128Encoder.Encode(barcode);
            using (var stream = new MemoryStream())
            {
                var renderer = new BarcodeRenderer();
                renderer.Render(barcoded, stream, out _barcodeWidth);
                
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var svg = reader.ReadToEnd();
                    Debug.WriteLine($"SVGTOOLS -> barcode svg:\n{svg}");
                    return svg;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine($"SVGTOOLS -> ex:\n{exception}");
            return null!;
        }
    }

    public static string CreateDefaultPresetView(PresetAttributes preset)
    {
        //fondo blanco etiqueta
        var background = new XElement(ns + "g",
            new XAttribute("id", "background"),
            new XElement(ns +"rect",
                new XAttribute("width", "100%"),
                new XAttribute("height", "100%"),
                new XAttribute("fill", "#FFFFFF")));
        
        //nombre del producto
        var productName = "NOMBRE\nPRODUCTO";
        var name = new XElement(ns+"g",
            new XAttribute("id", "name"),
            new XElement(ns + "text",
                productName,
                new XAttribute("x", "50%"),
                new XAttribute("y", "30"),
                new XAttribute("fill", "#000000"),
                new XAttribute("font-size", 12),
                new XAttribute("text-anchor", "middle")));

        //mostrar precio
        var productPrice = "$99.999";
        var price = new XElement(ns + "g",
            new XAttribute("id", "price"),
            new XElement(ns + "text",
                productPrice,
                new XAttribute("x", "50%"),
                new XAttribute("y", "80"),
                new XAttribute("fill", "#000000"),
                new XAttribute("font-size", 8),
                new XAttribute("text-anchor", "middle")));
            
        //SKU barcode   for preview  
        var barcodeWidth = preset.TagWidth - 2*preset.SideMargin;
        var barcodeHeight = preset.BarcodeHeight;
        var barcodeX = preset.SideMargin;
        var barcodeY = preset.TagHeight - barcodeHeight - preset.BottomMargin;
        var barcode = new XElement(ns + "g",
            new XAttribute("id", "barcode"),
            new XElement(ns + "rect",
                new XAttribute("x", barcodeX.ToString(CultureInfo.InvariantCulture)+"mm"),
                new XAttribute("y", barcodeY.ToString(CultureInfo.InvariantCulture)+"mm"),
                new XAttribute("width", barcodeWidth.ToString(CultureInfo.InvariantCulture)+"mm"),
                new XAttribute("height", barcodeHeight.ToString(CultureInfo.InvariantCulture)+"mm"),
                new XAttribute("fill", "#000000")));

        var finalRoot = new XElement(ns + "svg",
            new XAttribute("xmlns", ns.NamespaceName),
            new XAttribute("width", preset.TagWidth.ToString(CultureInfo.InvariantCulture)+"mm"),
            new XAttribute("height", preset.TagHeight.ToString(CultureInfo.InvariantCulture)+"mm"),
            background,
            barcode,
            name,
            price);
        
        Debug.WriteLine($"SVGTOOLS -> finalpreviewedsvg:\n{finalRoot}");
        
        var svg = new XDocument(finalRoot).ToString();
        return svg;
    }
}