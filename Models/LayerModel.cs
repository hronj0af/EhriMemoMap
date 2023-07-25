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
        public string? Title { get; set; }
        public string? Abstract { get; set; }
        /// <summary>
        /// Vlastnost, která se používá při změně tématu (tedy bodu na časové ose)
        /// </summary>
        public string? Code { get; set; }
        public string? MapName { get; set; }
        public bool Selected { get; set; }
        public bool Hidden { get; set; }

        public bool IsNotQueryable { get; set; }

    }
}
