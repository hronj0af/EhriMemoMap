using System;

namespace EhriMemoMap.Data;

public class RicanyPlacesOfMemoryXPlaceOfMemory
{
    public long Id { get; set; }

    public long PlaceOfMemory1Id { get; set; }

    public long RelationshipType { get; set; }

    public long PlaceOfMemory2Id { get; set; }

    public virtual RicanyPlaceOfMemory PlaceOfMemory1 { get; set; } = null!;

    public virtual RicanyPlaceOfMemory PlaceOfMemory2 { get; set; } = null!;

    public virtual RicanyRelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
