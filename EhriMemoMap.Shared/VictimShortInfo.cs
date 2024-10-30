namespace EhriMemoMap.Shared
{
    public class VictimShortInfo
    {
        public long Id { get; set; }
        public string? Label { get; set; }
        public string? DetailsCs { get; set; }
        public string? DetailsEn { get; set; }
        public string? Photo { get; set; }
        public DateTime? TransportDate { get; set; }
        public bool LongInfo { get; set; }
        public string GetLabelForPicture()
        => Label?.Insert(0, "<b>").Replace(" (*", "</b><br/>*").Replace(")", "") ?? "";

    }
}
