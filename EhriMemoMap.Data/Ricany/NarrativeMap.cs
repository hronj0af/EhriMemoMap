using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.Ricany;

public partial class NarrativeMap
{
    public long Id { get; set; }

    public long Type { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public virtual ICollection<EntitiesXNarrativeMap> EntitiesXNarrativeMaps { get; set; } = new List<EntitiesXNarrativeMap>();

    public virtual ICollection<NarrativeMapsXNarrativeMapStop> NarrativeMapsXNarrativeMapStops { get; set; } = new List<NarrativeMapsXNarrativeMapStop>();

    public virtual ICollection<NarrativeMapsXPoi> NarrativeMapsXPois { get; set; } = new List<NarrativeMapsXPoi>();

    public virtual ListItem TypeNavigation { get; set; } = null!;
}
