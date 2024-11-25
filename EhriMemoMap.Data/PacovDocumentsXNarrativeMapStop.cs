using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class PacovDocumentsXNarrativeMapStop
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long NarrativeMapStopId { get; set; }

    public virtual PacovDocument Document { get; set; } = null!;

    public virtual PacovNarrativeMapStop NarrativeMapStop { get; set; } = null!;
}

