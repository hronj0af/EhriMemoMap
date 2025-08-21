using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class EventsXPlace
{
    public long Id { get; set; }

    public long EventId { get; set; }

    public long RelationshipType { get; set; }

    public long PlaceId { get; set; }

    public virtual Event Event { get; set; } = null!;

    public virtual Place Place { get; set; } = null!;

    public virtual RelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
