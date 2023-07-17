namespace EhriMemoMap.Models
{
    public class Place
    {
        public string? LabelCs { get; set; }
        public string? LabelEn { get; set; }

        public string? PlaceCs { get; set; }
        public string? PlaceEn { get; set; }
        public string? MapLocation { get; set; }
        public string? MapObject { get; set; }
        public string? DropDownInfoCs => LabelCs + (!string.IsNullOrEmpty(PlaceCs) ? " | " + PlaceCs : "");
        public string? DropDownInfoEn => LabelEn + (!string.IsNullOrEmpty(PlaceEn) ? " | " + PlaceEn : (!string.IsNullOrEmpty(PlaceCs) ? " | " + PlaceCs : ""));

    }
}
