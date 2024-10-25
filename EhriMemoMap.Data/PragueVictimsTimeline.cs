using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class PragueVictimsTimeline
{
    public int Id { get; set; }

    public int? EntityId { get; set; }

    public string? Label { get; set; }

    public string? DetailsCs { get; set; }

    public string? DetailsEn { get; set; }

    public string? AddressCs { get; set; }

    public string? AddressEn { get; set; }

    public int? PlaceId { get; set; }

    public DateTime? TransportDate { get; set; }

    public string? Photo { get; set; }

    public string? GetLabelForPicture()
    => Label?.Insert(0, "<b>").Replace(" (*", "</b><br/>*").Replace(")", "");

}
