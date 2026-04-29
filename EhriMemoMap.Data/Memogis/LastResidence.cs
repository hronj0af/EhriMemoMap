using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data.Memogis;

public partial class LastResidence
{
    public int Id { get; set; }

    public long VictimId { get; set; }

    public int AddressId { get; set; }

    public virtual Entity Victim { get; set; } = null!;

    public virtual AddressesStatsTimeline Address { get; set; } = null!;
}
