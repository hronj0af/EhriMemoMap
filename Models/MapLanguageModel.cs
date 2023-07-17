using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Globalization;

namespace EhriMemoMap.Models
{
    /// <summary>
    /// Informace o mapě
    /// </summary>
    public class MapLanguageModel
    {
        public string? MapName { get; set; }
        public string? LanguageCode { get; set; }
        public string? MapParameter { get; set; }
        public List<LayerModel>? Layers { get; set; }
        public List<CollectionModel>? Collections { get; set; }
    }


}
