using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class PacovEntitiesXPlace
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long PlaceId { get; set; }

    public long RelationshipType { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public virtual PacovEntity Entity { get; set; } = null!;

    public virtual PacovPlace Place { get; set; } = null!;

    public virtual PacovRelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
