namespace EhriMemoMap.Models
{
    public class WMSPlaceInfo
    {
        public List<KeyValuePair<string, string>> Properties { get; set; }
        public WMSFeatureType FeatureType { get; set; }
    }
}
