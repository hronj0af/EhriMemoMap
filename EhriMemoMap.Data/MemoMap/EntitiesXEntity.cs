using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class EntitiesXEntity
{
    public long Id { get; set; }

    public long Entity1Id { get; set; }

    public long RelationshipType { get; set; }

    public long Entity2Id { get; set; }

    public virtual Entity Entity1 { get; set; } = null!;

    public virtual Entity Entity2 { get; set; } = null!;

    public virtual RelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
