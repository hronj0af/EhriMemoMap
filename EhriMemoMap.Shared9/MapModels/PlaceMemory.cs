using NetTopologySuite.Geometries;

namespace EhriMemoMap.Shared
{
    public class PlaceMemory
    {
        public string? Type { get; set; }
        public string? City { get; set; }
        public string? LabelCs { get; set; }
        public string? LabelEn { get; set; }
        public string? AddressCs { get; set; }
        public string? AddressEn { get; set; }
        public string? DescriptionCs { get; set; }
        public string? DescriptionEn { get; set; }
        public string? InscriptionCs { get; set; }
        public string? InscriptionEn { get; set; }
        public DateTime? CreationDate { get; set; }
        public PlaceMemoryItem[]? Items { get; set; }
        public Document[]? Documents { get; set; }

    }

    public class PlaceMemoryItem
    {
        public long Id { get; set; }
        public string? LabelCs { get; set; }
        public string? LabelEn { get; set; }
        public string? InscriptionCs { get; set; }
        public string? InscriptionEn { get; set; }
        public DateTime? CreationDate { get; set; }
        public string? LinkStolpersteineCs { get; set; }
        public string? LinkStolpersteineEn { get; set; }
        public string? LinkHolocaustCs { get; set; }
        public string? LinkHolocaustEn { get; set; }
    }
}