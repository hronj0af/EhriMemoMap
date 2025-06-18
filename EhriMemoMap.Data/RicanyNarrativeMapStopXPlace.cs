using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class RicanyNarrativeMapStopsXPlace
{
    public long Id { get; set; }

    public long NarrativeMapStopId { get; set; }

    public long PlaceId { get; set; }

    public long RelationshipType { get; set; }

    public virtual RicanyNarrativeMapStop NarrativeMapStop { get; set; }
    public virtual RicanyPlace Place { get; set; }
    public virtual RicanyRelationshipType RelationshipTypeNavigation { get; set; }
}