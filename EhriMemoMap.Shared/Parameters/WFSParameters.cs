namespace EhriMemoMap.Shared
{
    public class WFSParameters
    {
        public List<WFSLayerInfo>? WFSLayers { get; set; }
    }

    public class WFSLayerInfo
    {
        public string? Url { get; set; }
        public string? PlaceType { get; set; }

        public string? Name { get; set; }
    }
}
