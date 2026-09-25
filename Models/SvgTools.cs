using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Avalonia.Svg.Skia;
using Barcoder.Code128;

namespace Etiquetadora.Models;

public class SvgTools
{
    public SvgSource RenderTag(string barcodeValue, string productPrice, string productName, PresetAttributes settings)
    {
        XNamespace ns = "http://www.w3.org/2000/svg";
        var original = XDocument.Parse(CreateSvgBarcode(barcodeValue));
        var originalRoot = original.Root!;
        
        // Extraer el viewBox original para saber las dimensiones "naturales" del barcode
        var vb = originalRoot.Attribute("viewBox")!.Value.Split(' ');
        var origWidth = double.Parse(vb[2], CultureInfo.InvariantCulture);
        var origHeight = double.Parse(vb[3], CultureInfo.InvariantCulture);

        var barcodeWidth = settings.TagWidth - 2 * settings.SideMargin;
        var barcodeHeight = 9; //TODO verificar que se pueda cambiar la altura del codigo de barras
        // Si no se especifica tamaño del barcode, usa su tamaño natural
        double targetBarcodeWidth = barcodeWidth;
        double targetBarcodeHeight = barcodeHeight;

        var scaleX = targetBarcodeWidth / origWidth;
        double scaleY = targetBarcodeHeight / origHeight;

        // Tomamos todos los <line> del original
        var lines = originalRoot.Elements(ns + "line").ToList();

        // Grupo 'barcode': mismas líneas, con transform de escala + posición
        var barcodeX = (settings.TagWidth - barcodeWidth)/2;
        var barcodeY = settings.TagHeight - barcodeHeight - settings.BottomMargin;
        var barcodeGroup = new XElement(ns + "g",
            new XAttribute("id", "barcode"),
            new XAttribute("transform",
                $"translate({barcodeX.ToString(CultureInfo.InvariantCulture)}," +
                $"{barcodeY.ToString(CultureInfo.InvariantCulture)}) " +
                $"scale({scaleX.ToString(CultureInfo.InvariantCulture)}," +
                $"{scaleY.ToString(CultureInfo.InvariantCulture)})"),
            new XAttribute("fill", originalRoot.Attribute("fill")?.Value ?? "#FFFFFF"),
            new XAttribute("stroke", originalRoot.Attribute("stroke")?.Value ?? "#000000"),
            new XAttribute("stroke-width", originalRoot.Attribute("stroke-width")?.Value ?? "1"),
            new XAttribute("stroke-linecap", originalRoot.Attribute("stroke-linecap")?.Value ?? "butt"),
            lines);
        
        // ---- todo barcode logic
        var barcode = new XElement(ns + "g",
            new XAttribute("id", "barcode"),
            new XElement(ns + "rect",
                new XAttribute("x", barcodeX.ToString(CultureInfo.InvariantCulture)+"mm"),
                new XAttribute("y", barcodeY.ToString(CultureInfo.InvariantCulture)+"mm"),
                new XAttribute("width", barcodeWidth.ToString(CultureInfo.InvariantCulture)+"mm"),
                new XAttribute("height", barcodeHeight.ToString(CultureInfo.InvariantCulture)+"mm"),
                new XAttribute("fill", "#000000")));

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

        // Documento final: background primero (queda atrás), barcode encima
        var newRoot = new XElement(ns + "svg",
            new XAttribute("xmlns", ns.NamespaceName),
            new XAttribute("width", settings.TagWidth.ToString(CultureInfo.InvariantCulture)+"mm"),
            new XAttribute("height", settings.TagHeight.ToString(CultureInfo.InvariantCulture)+"mm"),
            // new XAttribute("viewBox",
            //     $"0 0 {settings.TagWidth.ToString(CultureInfo.InvariantCulture)} " +
            //     $"{settings.TagHeight.ToString(CultureInfo.InvariantCulture)}"),
            // backgroundGroup,
            background,
            price,
            barcode,
            name);

        var svg = new XDocument(newRoot).ToString();
        Debug.WriteLine(svg);
        return SvgSource.LoadFromSvg(svg);
    }

    private string CreateSvgBarcode(string barcode)
    {
        try
        {
            var barcoded = Code128Encoder.Encode(barcode);
            using (var stream = new MemoryStream())
            {
                var renderer = new BarcodeRenderer();
                renderer.Render(barcoded, stream);
                
                stream.Position = 0;

                using (var reader = new StreamReader(stream))
                {
                    var svg = reader.ReadToEnd();
                    Debug.WriteLine($"BARCODE SVG:\n{svg}");
                    return svg;
                }
            }
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
            return null!;
        }
    }

    public static SvgSource CreateDefaultPresetView(PresetAttributes settings)
    {
        XNamespace ns = "http://www.w3.org/2000/svg";

        //fondo blanco etiqueta
        var background = new XElement(ns + "g",
            new XAttribute("id", "background"),
            new XElement(ns +"rect",
                new XAttribute("width", "100%"),
                new XAttribute("height", "100%"),
                new XAttribute("fill", "#FFFFFF")));
        
        //nombre del producto
        var productName = "NOMBRE\nPRODUCTO"; //todo traducir mensaje o alguna wea
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
            
        //SKU barcode    
        var barcodeWidth = settings.TagWidth - 2*settings.SideMargin;
        var barcodeHeight = 9; //TODO corregir altura del codigo de barras
        var barcodeX = settings.SideMargin;
        var barcodeY = settings.TagHeight - barcodeHeight - settings.BottomMargin;
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
            new XAttribute("width", settings.TagWidth.ToString(CultureInfo.InvariantCulture)+"mm"),
            new XAttribute("height", settings.TagHeight.ToString(CultureInfo.InvariantCulture)+"mm"),
            background,
            barcode,
            name,
            price);
        
        Debug.WriteLine($"FINAL SVG:\n{finalRoot}");
        
        var svg = new XDocument(finalRoot).ToString();
        return SvgSource.LoadFromSvg(svg);
    }
}