using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Globalization;

namespace EhriMemoMap.Models
{
    /// <summary>
    /// Informace o vrstvě na mapě
    /// </summary>
    public class LayerModel
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public LayerType? Type { get; set; }
        public PlaceType? PlaceType { get; set; }
        public string? Attribution { get; set; }
        public string? Opacities { get; set; }
        public string? ZIndex { get; set; }
        public string? MapParameter { get; set; }
        public string? LayersParameter { get; set; }
        public bool Selected { get; set; }
        public int Order { get; set; }
        public bool QuickAccess { get; set; }
        public string? BackgroundColor { get; set; }
        public string? FontColor { get; set; }

        /// <summary>
        /// Od jakého minimálního stupně přiblížení se bude vrstva zobrazovat 
        /// (když bude zoom menší a mapa méně podrobná, vrstva se zobrazovat nebude)
        /// </summary>
        public int? MinZoom { get; set; }

        /// <summary>
        /// Do jakého maximálního stupně oddálení mapy se bude vrstva zobrazovat
        /// (když bude zoom větší a mapa podrobnější, vrstva se zobrazovat nebude)
        /// </summary>
        public int? MaxZoom { get; set; }


    }

    public enum LayerType
    {
        Images, Objects, WMS, Base, Polygons
    }
}
