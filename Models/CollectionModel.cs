using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Radzen;
using System.Globalization;

namespace EhriMemoMap.Models
{
    /// <summary>
    /// Informace o kolekci; kolekce = soubor vrstev; v aplikace je jedna vrstva jedním bodem na časové ose
    /// </summary>
    public class CollectionModel
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? MapName { get; set; }

        public List<LayerModel>? Layers { get; set; }
        public bool Selected { get; set; }
    }

}
