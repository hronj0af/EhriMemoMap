using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Memogis;

public partial class EntitiesNoTimeline
{
    public int Id { get; set; }

    public int? EntityId { get; set; }

    public string? Label { get; set; }

    public string? DetailsCs { get; set; }

    public string? DetailsEn { get; set; }

    public string? AddressCs { get; set; }

    public string? AddressEn { get; set; }

    public int? PlaceId { get; set; }

    public virtual AddressesStat? Place { get; set; }
}
