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
        public string? Attribution { get; set; }
        public List<LayerModel>? Layers { get; set; }
    }


}
