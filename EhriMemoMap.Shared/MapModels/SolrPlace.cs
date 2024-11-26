namespace EhriMemoMap.Shared
{
    public class SolrPlace
    {
        public string Id { get; set; }
        public DateTime? Date { get; set; }
        public string? Type { get; set; }
        public string? LabelCs { get; set; }
        public string? LabelEn { get; set; }

        public string? PlaceCs { get; set; }
        public string? PlaceEn { get; set; }
        public string? PlaceDe { get; set; }
        public string? PlaceCurrentCs { get; set; }
        public string? PlaceCurrentEn { get; set; }
        public string? PlaceCurrentDe { get; set; }
        public string? MapLocation { get; set; }
        public string? MapObject { get; set; }
        public string? DropDownInfoCs => LabelCs + (!string.IsNullOrEmpty(PlaceCs) ? " | " + PlaceCs : "");
        public string? DropDownInfoEn => LabelEn + (!string.IsNullOrEmpty(PlaceEn) ? " | " + PlaceEn : (!string.IsNullOrEmpty(PlaceCs) ? " | " + PlaceCs : ""));

    }
}
