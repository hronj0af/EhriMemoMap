using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class PlacesOfMemoryXPlacesOfMemory
{
    public long Id { get; set; }

    public long PlaceOfMemory1Id { get; set; }

    public long RelationshipType { get; set; }

    public long PlaceOfMemory2Id { get; set; }

    public virtual PlacesOfMemory PlaceOfMemory1 { get; set; } = null!;

    public virtual PlacesOfMemory PlaceOfMemory2 { get; set; } = null!;

    public virtual RelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
