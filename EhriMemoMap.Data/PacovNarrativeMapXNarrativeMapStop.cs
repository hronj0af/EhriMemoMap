using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class PacovNarrativeMapXNarrativeMapStop
{
    public long Id { get; set; }

    public long NarrativeMapId { get; set; }

    public long NarrativeMapStopId { get; set; }

    public virtual PacovNarrativeMap NarrativeMap { get; set; } = null!;

    public virtual PacovNarrativeMapStop NarrativeMapStop { get; set; } = null!;
}