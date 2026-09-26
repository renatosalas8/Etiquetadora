using System.IO;
using Barcoder;

namespace Etiquetadora.Models;

public interface IBarcodeRenderer
{
    void Render(IBarcode barcode, Stream outputStream, out double width);
}