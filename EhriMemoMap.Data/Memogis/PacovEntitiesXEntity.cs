using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovEntitiesXEntity
{
    public long Id { get; set; }

    public long Entity1Id { get; set; }

    public long RelationshipType { get; set; }

    public long Entity2Id { get; set; }

    public virtual PacovEntity Entity1 { get; set; } = null!;

    public virtual PacovEntity Entity2 { get; set; } = null!;

    public virtual PacovRelationshipType RelationshipTypeNavigation { get; set; } = null!;
}
