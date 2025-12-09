using System;
using System.Collections.Generic;

namespace EhriMemoMap.Data.MemoMap;

public partial class Incident
{
    public long Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public long PlaceId { get; set; }

    public string? DateCs { get; set; }

    public string? DateEn { get; set; }

    public virtual Place Place { get; set; } = null!;
}
