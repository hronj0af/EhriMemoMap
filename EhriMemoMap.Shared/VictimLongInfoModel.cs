using EhriMemoMap.Data;

namespace EhriMemoMap.Shared
{
    public class VictimLongInfoModel
    {
        public long Id { get; set; }
        public string? Label { get; set; }
        public string? DetailsCs { get; set; }
        public string? DetailsEn { get; set; }
        public string? Photo { get; set; }
        public AddressInfo[]? Places { get; set; } 
        public VictimShortInfoModel[]? RelatedPersons { get; set; }
        public Document[]? Documents { get; set; }

        public DateTime? TransportDate { get; set; }
        public string GetLabelForPicture()
        => Label?.Insert(0, "<b>").Replace(" (*", "</b><br/>*").Replace(")", "") ?? "";

    }
}
