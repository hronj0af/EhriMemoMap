namespace EhriMemoMap.Shared
{
    public class PlaceIncident
    {
        public long Id { get; set; }
        public string? LabelCs { get; set; }
        public string? LabelEn { get; set; }
        public string? TypeCs { get; set; }
        public string? TypeEn { get; set; }
        public string? SpecificationCs { get; set; }
        public string? SpecificationEn { get; set; }
        public string? AddressCs { get; set; }
        public string? AddressEn { get; set; }
        public string? DescriptionCs { get; set; }
        public string? DescriptionEn { get; set; }
        public string? DateCs { get; set; }
        public string? DateEn { get; set; }
        public DateTime? Date { get; set; }
        public Document[]? Documents { get; set; }

    }
}
