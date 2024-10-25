using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovEntitiesXTransport
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long TransportId { get; set; }

    public int TransportOrder { get; set; }

    public int? NrInTransport { get; set; }

    public virtual PacovEntity Entity { get; set; } = null!;

    public virtual PacovTransport Transport { get; set; } = null!;
}
