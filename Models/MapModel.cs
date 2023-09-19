using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Globalization;

namespace EhriMemoMap.Models
{
    /// <summary>
    /// Informace o mapě
    /// </summary>
    public class MapModel
    {
        public List<LayerModel>? Layers { get; set; }
        public List<TimelinePointModel>? Timeline { get; set; }
    }


}
