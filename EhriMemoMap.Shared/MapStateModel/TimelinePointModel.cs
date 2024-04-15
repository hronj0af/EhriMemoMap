
namespace EhriMemoMap.Shared
{
    /// <summary>
    /// Informace o kolekci; kolekce = soubor vrstev; v aplikace je jedna vrstva jedním bodem na časové ose
    /// </summary>
    public class TimelinePointModel
    {
        public string? Name { get; set; }
        public string? Title { get; set; }
        public bool Selected { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public List<LayerModel>? AdditionalLayers { get; set; }

    }

}
