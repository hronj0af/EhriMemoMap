using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class NarrativeMapStopsXPlace
{
    public long Id { get; set; }

    public long NarrativeMapStopId { get; set; }

    public long PlaceId { get; set; }

    public long RelationshipType { get; set; }

    public virtual NarrativeMapStop NarrativeMapStop { get; set; } = null!;

    public virtual Place Place { get; set; } = null!;

    public virtual RelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
