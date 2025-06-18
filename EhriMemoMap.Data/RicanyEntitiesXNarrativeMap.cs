using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class RicanyEntitiesXNarrativeMap
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long NarrativeMapId { get; set; }

    public virtual RicanyEntity Entity { get; set; } = null!;

    public virtual RicanyNarrativeMap NarrativeMap { get; set; } = null!;
}

