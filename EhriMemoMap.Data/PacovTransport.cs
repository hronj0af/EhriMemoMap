using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class PacovTransport
{
    public long Id { get; set; }

    public string? TransportCode { get; set; }

    public DateTime? Date { get; set; }

    public long? PlaceFrom { get; set; }

    public long? PlaceTo { get; set; }

    public virtual ICollection<PacovEntitiesXTransport> PacovEntitiesXTransports { get; set; } = new List<PacovEntitiesXTransport>();

    public virtual PacovPlace? PlaceFromNavigation { get; set; }

    public virtual PacovPlace? PlaceToNavigation { get; set; }
}
