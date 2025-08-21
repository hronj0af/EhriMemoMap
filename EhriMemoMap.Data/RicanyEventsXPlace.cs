using System;

namespace EhriMemoMap.Data;

public class RicanyEventsXPlace
{
    public long Id { get; set; }

    public long EventId { get; set; }

    public long RelationshipType { get; set; }

    public long PlaceId { get; set; }

    public virtual RicanyEvent Event { get; set; } = null!;

    public virtual RicanyPlace Place { get; set; } = null!;

    public virtual RicanyRelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
