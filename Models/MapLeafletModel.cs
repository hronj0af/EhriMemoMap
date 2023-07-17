using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Globalization;

namespace EhriMemoMap.Models
{
    /// <summary>
    /// Informace o mapě pro javascriptovou knihovnu LeafletJS
    /// </summary>
    public class MapLeafletModel
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? BaseUrl { get; set; }
        public string? Attribution { get; set; }
        public string? Opacities { get; set; }
        public string? ZIndex { get; set; }
        public string? TileSize { get; set; }
        public string? Type { get; set; }
        public string? MapParameter { get; set; }
        public List<LayerModel>? Layers { get; set; }
        public List<CollectionModel>? Collections { get; set; }

    }


}
