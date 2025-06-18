using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class RicanyEntitiesXEntity
{
    public long Id { get; set; }

    public long Entity1Id { get; set; }

    public long RelationshipType { get; set; }

    public long Entity2Id { get; set; }

    public virtual RicanyEntity Entity1 { get; set; } = null!;

    public virtual RicanyEntity Entity2 { get; set; } = null!;

    public virtual RicanyRelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
