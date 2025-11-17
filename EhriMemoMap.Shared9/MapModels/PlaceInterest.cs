namespace EhriMemoMap.Shared
{
    public class PlaceInterest
    {
        public string? LabelCs { get; set; }
        public string? LabelEn { get; set; }

        public string? AddressCs { get; set; }
        public string? AddressEn { get; set; }
        public string? DescriptionCs { get; set; }
        public string? DescriptionEn { get; set; }
        public long? NarrativeMapId { get; set; }
        public Document[]? Documents { get; set; }

    }
}
