using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class RicanyDocumentsXNarrativeMapStop
{
    public long Id { get; set; }

    public long DocumentId { get; set; }

    public long NarrativeMapStopId { get; set; }

    public virtual RicanyDocument Document { get; set; } = null!;

    public virtual RicanyNarrativeMapStop NarrativeMapStop { get; set; } = null!;
}

