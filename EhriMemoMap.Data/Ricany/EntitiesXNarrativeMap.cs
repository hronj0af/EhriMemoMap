using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class EntitiesXNarrativeMap
{
    public long Id { get; set; }

    public long EntityId { get; set; }

    public long NarrativeMapId { get; set; }

    public virtual Entity Entity { get; set; } = null!;

    public virtual NarrativeMap NarrativeMap { get; set; } = null!;
}
