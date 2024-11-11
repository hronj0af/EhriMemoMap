namespace EhriMemoMap.Shared
{
    public class VictimShortInfoModel
    {
        public long Id { get; set; }
        public string? Label { get; set; }
        public string? Name { get; set; }
        public string? Birthdate { get; set; }
        public string? DetailsCs { get; set; }
        public string? DetailsEn { get; set; }
        public string? Photo { get; set; }
        public DateTime? TransportDate { get; set; }
        //public DateTime? RelationshipToAddressDateFrom { get; set; }
        //public DateTime? RelationshipToAddressDateTo { get; set; }
        //public string? RelationshipToAddressTypeCs { get; set; }
        //public string? RelationshipToAddressTypeEn { get; set; }
        public long? RelationshipToAddressType { get; set; }
        public long? RelationshipToPersonType { get; set; }
        public string? RelationshipToPersonCs { get; set; }
        public string? RelationshipToPersonEn { get; set; }
        public bool LongInfo { get; set; }
        public string GetLabelForPicture()
            => !string.IsNullOrEmpty(Label)
                ? (Label?.Insert(0, "<b>").Replace(" (*", "</b><br/>*").Replace(")", "") ?? "")
                : (Name + (Birthdate != null ? " (*" + Birthdate + ")" : ""));

        public string GetShortLabelForPicture()
            => Label?.Insert(0, "<b>").Replace(" (*", "</b><br/>*").Replace(")", "") ?? "";

        //public bool IsAddressPeriod()
        //    => RelationshipToAddressDateFrom != null || RelationshipToAddressDateTo != null;
        
        //public string GetAddressPeriod()
        //{
        //    if (RelationshipToAddressDateFrom == null && RelationshipToAddressDateTo == null)
        //        return "";
        //    if (RelationshipToAddressDateFrom == null)
        //        return RelationshipToAddressDateTo?.ToString("d. M. yyyy") ?? "";
        //    if (RelationshipToAddressDateTo == null)
        //        return RelationshipToAddressDateFrom?.ToString("d. M. yyyy") ?? "";
        //    return $"{RelationshipToAddressDateFrom?.ToString("d. M. yyyy")} - {RelationshipToAddressDateTo?.ToString("d. M. yyyy")}";

        //}

        public string GetBirthdateTitle()
            => Birthdate != null ? "* " + Birthdate : "";
    }
}
