using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class RicanyNarrativeMapStop
{
    public long Id { get; set; }

    public int? StopOrder { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DateCs { get; set; }

    public string? DateEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public virtual ICollection<RicanyDocumentsXNarrativeMapStop> RicanyDocumentsXNarrativeMapStops { get; set; } = [];

    public virtual ICollection<RicanyNarrativeMapStopsXPlace> RicanyNarrativeMapStopsXPlaces { get; set; } = [];
    public virtual ICollection<RicanyNarrativeMapXNarrativeMapStop> RicanyNarrativeMapXNarrativeMapStops { get; set; } = [];


}