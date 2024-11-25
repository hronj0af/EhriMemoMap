using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class PacovEntitiesXNarrativeMap
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long NarrativeMapId { get; set; }

    public virtual PacovEntity Entity { get; set; } = null!;

    public virtual PacovNarrativeMap NarrativeMap { get; set; } = null!;
}

