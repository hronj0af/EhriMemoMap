using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public class PacovNarrativeMap
{
    public long Id { get; set; }

    public long Type { get; set; }

    public string LabelCs { get; set; }

    public string LabelEn { get; set; }

    public string DescritpionCs { get; set; }

    public string DescritpionEn { get; set; }

    public virtual ICollection<PacovEntitiesXNarrativeMap> PacovEntitiesXNarrativeMaps { get; set; } = [];


    public virtual ICollection<PacovNarrativeMapXNarrativeMapStop> PacovNarrativeMapsXNarrativeMapStops { get; set; } = [];

    public virtual ICollection<PacovNarrativeMapXNarrativeMapStop> PacovNarrativeMapXNarrativeMapStops { get; set; } = [];


    public virtual PacovListItem TypeNavigation { get; set; } = null!;
}