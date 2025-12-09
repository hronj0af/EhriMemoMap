using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data.MemoMap;

public partial class EntitiesXPlace
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long PlaceId { get; set; }

    public long RelationshipType { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public virtual Entity Entity { get; set; } = null!;

    public virtual Place Place { get; set; } = null!;

    public virtual RelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
