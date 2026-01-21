namespace EhriMemoMap.Shared
{
    public class PlaceMemorial
    {
        public long Id { get; set; }
        public string? LabelCs { get; set; }
        public string? LabelEn { get; set; }
        public string? LinkCs { get; set; }
        public string? LinkEn { get; set; }
        public string? AddressCs { get; set; }
        public string? AddressEn { get; set; }
        public string? DescriptionCs { get; set; }
        public string? DescriptionEn { get; set; }
        public DateTime? Date { get; set; }
        public Document[]? Documents { get; set; }

    }
}
