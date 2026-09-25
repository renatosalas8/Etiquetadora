namespace Etiquetadora.Models
{
    public sealed class SvgRendererOptions
    {
        public bool IncludeEanContentAsText { get; set; } = true;

        public int? CustomMargin { get; set; } = null;

        public int BarHeightFor1DBarcode { get; set; } = 50;

        public string EanFontFamily { get; set; } = "arial";
    }
}