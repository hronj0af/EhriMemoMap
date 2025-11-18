using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class RicanyTransport
{
    public long Id { get; set; }

    public string? TransportCode { get; set; }

    public DateTime? Date { get; set; }

    public long? PlaceFrom { get; set; }

    public long? PlaceTo { get; set; }

    public virtual ICollection<RicanyEntitiesXTransport> RicanyEntitiesXTransports { get; set; } = new List<RicanyEntitiesXTransport>();

    public virtual RicanyPlace? PlaceFromNavigation { get; set; }

    public virtual RicanyPlace? PlaceToNavigation { get; set; }
}
