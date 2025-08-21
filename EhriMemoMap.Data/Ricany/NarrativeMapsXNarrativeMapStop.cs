using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class NarrativeMapsXNarrativeMapStop
{
    public long Id { get; set; }

    public long NarrativeMapId { get; set; }

    public long NarrativeMapStopId { get; set; }

    public virtual NarrativeMap NarrativeMap { get; set; } = null!;

    public virtual NarrativeMapStop NarrativeMapStop { get; set; } = null!;
}
