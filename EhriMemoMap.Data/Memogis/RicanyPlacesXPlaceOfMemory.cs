using System;

namespace EhriMemoMap.Data;

public class RicanyPlacesXPlaceOfMemory
{
    public long Id { get; set; }

    public long PlaceId { get; set; }

    public long RelationshipType { get; set; }

    public long PlaceOfMemoryId { get; set; }

    public virtual RicanyPlace Place { get; set; } = null!;

    public virtual RicanyPlaceOfMemory PlaceOfMemory { get; set; } = null!;

    public virtual RicanyRelationshipType RelationshipTypeNavigation { get; set; } = null!;
}