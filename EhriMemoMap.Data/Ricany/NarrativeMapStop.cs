using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class NarrativeMapStop
{
    public long Id { get; set; }

    public int? StopOrder { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DateCs { get; set; }

    public string? DateEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public virtual ICollection<DocumentsXNarrativeMapStop> DocumentsXNarrativeMapStops { get; set; } = new List<DocumentsXNarrativeMapStop>();

    public virtual ICollection<NarrativeMapStopsXPlace> NarrativeMapStopsXPlaces { get; set; } = new List<NarrativeMapStopsXPlace>();

    public virtual ICollection<NarrativeMapsXNarrativeMapStop> NarrativeMapsXNarrativeMapStops { get; set; } = new List<NarrativeMapsXNarrativeMapStop>();
}
