using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class PacovNarrativeMapStopsXPlace
{
    public long Id { get; set; }

    public long NarrativeMapStopId { get; set; }

    public long PlaceId { get; set; }

    public long RelationshipType { get; set; }

    public virtual PacovNarrativeMapStop NarrativeMapStop { get; set; }
    public virtual PacovPlace Place { get; set; }
    public virtual PacovRelationshipType RelationshipTypeNavigation { get; set; }
}