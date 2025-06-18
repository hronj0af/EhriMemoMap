using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyEntitiesXTransport
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long TransportId { get; set; }

    public int TransportOrder { get; set; }

    public int? NrInTransport { get; set; }

    public virtual RicanyEntity Entity { get; set; } = null!;

    public virtual RicanyTransport Transport { get; set; } = null!;
}
