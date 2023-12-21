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

        public InitialVariables? InitialVariables { get; set; }
        public List<LayerModel>? Layers { get; set; }
        public List<TimelinePointModel>? Timeline { get; set; }
    }

    public class InitialVariables
    {
        public int Zoom { get; set; }
        public int ZoomMobile { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
    }

}
