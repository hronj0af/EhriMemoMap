using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data.Memogis;

public partial class QuartersStat
{
    public long Id { get; set; }

    public string Type { get; set; } = null!;

    public string QuarterCs { get; set; } = null!;

    public string QuarterEn { get; set; } = null!;

    public decimal Count { get; set; }

    public Geometry? Geography { get; set; }

    public Geometry? GeographyPolygon { get; set; }
}
