using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Globalization;

namespace EhriMemoMap.Models
{
    /// <summary>
    /// Informace o vrstvě na mapě pro leaflet
    /// </summary>
    public class LayerForLeafletModel
    {
        public string? Name { get; set; }
        public string? Url { get; set; }
        public string? Type { get; set; }
        public bool? Selected { get; set; }
        public string? Attribution { get; set; }
        public string? MapParameter { get; set; }
        public string? LayersParameter { get; set; }
        public int? ZIndex { get; set; }

    }
}
