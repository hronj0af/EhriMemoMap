using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class Poi
{
    public long Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public long PlaceId { get; set; }

    public virtual ICollection<DocumentsXPoi> DocumentsXPois { get; set; } = new List<DocumentsXPoi>();

    public virtual ICollection<NarrativeMapsXPoi> NarrativeMapsXPois { get; set; } = new List<NarrativeMapsXPoi>();

    public virtual Place Place { get; set; } = null!;
}
