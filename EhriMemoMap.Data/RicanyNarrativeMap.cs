using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class RicanyNarrativeMap
{
    public long Id { get; set; }

    public long Type { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public virtual ICollection<RicanyEntitiesXNarrativeMap> RicanyEntitiesXNarrativeMaps { get; set; } = [];

    public virtual ICollection<RicanyNarrativeMapXNarrativeMapStop> RicanyNarrativeMapsXNarrativeMapStops { get; set; } = [];

    public virtual RicanyListItem TypeNavigation { get; set; } = null!;
}