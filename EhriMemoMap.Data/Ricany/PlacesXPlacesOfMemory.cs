using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class PlacesXPlacesOfMemory
{
    public long Id { get; set; }

    public long PlaceId { get; set; }

    public long RelationshipType { get; set; }

    public long PlaceOfMemoryId { get; set; }

    public virtual Place Place { get; set; } = null!;

    public virtual PlacesOfMemory PlaceOfMemory { get; set; } = null!;

    public virtual RelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
