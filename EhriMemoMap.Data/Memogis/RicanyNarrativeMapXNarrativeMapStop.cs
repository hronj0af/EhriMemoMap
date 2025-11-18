using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class RicanyNarrativeMapXNarrativeMapStop
{
    public long Id { get; set; }

    public long NarrativeMapId { get; set; }

    public long NarrativeMapStopId { get; set; }

    public virtual RicanyNarrativeMap? NarrativeMap { get; set; } = null!;

    public virtual RicanyNarrativeMapStop? NarrativeMapStop { get; set; } = null!;
}