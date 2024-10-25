using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data;

public partial class PacovPoi
{
    public long Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public long PlaceId { get; set; }

    public virtual ICollection<PacovDocumentsXPoi> PacovDocumentsXPois { get; set; } = new List<PacovDocumentsXPoi>();

    public virtual PacovPlace Place { get; set; } = null!;
}
