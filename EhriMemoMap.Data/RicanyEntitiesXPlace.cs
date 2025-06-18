using System;
using System.Collections.Generic;
using NodaTime;

namespace EhriMemoMap.Data;

public partial class RicanyEntitiesXPlace
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long PlaceId { get; set; }

    public long RelationshipType { get; set; }

    public DateTime? DateFrom { get; set; }

    public DateTime? DateTo { get; set; }

    public virtual RicanyEntity Entity { get; set; } = null!;

    public virtual RicanyPlace Place { get; set; } = null!;

    public virtual RicanyRelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
