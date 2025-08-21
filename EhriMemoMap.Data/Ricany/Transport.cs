using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data.Ricany;

public partial class Transport
{
    public long Id { get; set; }

    public string? TransportCode { get; set; }

    public DateTime? Date { get; set; }

    public long? PlaceFrom { get; set; }

    public long? PlaceTo { get; set; }

    public virtual ICollection<EntitiesXTransport> EntitiesXTransports { get; set; } = new List<EntitiesXTransport>();

    public virtual Place? PlaceFromNavigation { get; set; }

    public virtual Place? PlaceToNavigation { get; set; }
}
