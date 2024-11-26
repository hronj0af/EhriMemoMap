using EhriMemoMap.Data;

namespace EhriMemoMap.Shared
{
    public class VictimLongInfoModel
    {
        public long Id { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? DeathDate { get; set; }
        public string? Label { get; set; }
        public string? FateCs { get; set; }
        public string? FateEn { get; set; }
        public string? DetailsCs { get; set; }
        public string? DetailsEn { get; set; }
        public string? Photo { get; set; }
        public AddressInfo[]? Places { get; set; } 
        public VictimShortInfoModel[]? RelatedPersons { get; set; }
        public Document[]? Documents { get; set; }
        public Transport[]? Transports { get; set; }
        public DateTime? TransportDate { get; set; }
        public long? NarrativeMapId { get; set; }
        public string GetLabelForPicture()
        => Label?.Insert(0, "<b>").Replace(" (*", "</b><br/>*").Replace(")", "") ?? "";

    }
}
