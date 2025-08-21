using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class EntitiesXTransport
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long TransportId { get; set; }

    public int TransportOrder { get; set; }

    public int? NrInTransport { get; set; }

    public virtual Entity Entity { get; set; } = null!;

    public virtual Transport Transport { get; set; } = null!;
}
