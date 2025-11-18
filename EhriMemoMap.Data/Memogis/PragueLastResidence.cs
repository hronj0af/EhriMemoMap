using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class PragueLastResidence
{
    public int Id { get; set; }

    public int VictimId { get; set; }

    public int AddressId { get; set; }

    public virtual PragueVictimsTimeline Victim { get; set; } = null!;

    public virtual PragueAddressesStatsTimeline Address { get; set; } = null!;
}
