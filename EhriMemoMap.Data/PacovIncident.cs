using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class PacovIncident
{
    public long Id { get; set; }

    public string? LabelCs { get; set; }

    public string? LabelEn { get; set; }

    public string? DescriptionCs { get; set; }

    public string? DescriptionEn { get; set; }

    public string? DateCs { get; set; }
    public string? DateEn { get; set; }

    public long PlaceId { get; set; }

    public virtual PacovPlace Place { get; set; } = null!;
}
