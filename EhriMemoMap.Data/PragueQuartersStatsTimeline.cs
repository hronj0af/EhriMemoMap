using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

public partial class PragueQuartersStatsTimeline
{
    public long Id { get; set; }

    public string? Type { get; set; }

    public string? QuarterCs { get; set; }

    public string? QuarterEn { get; set; }

    public decimal? Count { get; set; }

    public Geometry? Geography { get; set; }

    public Geometry? GeographyPolygon { get; set; }

    public decimal? _19420101 { get; set; }

    public decimal? _19420401 { get; set; }

    public decimal? _19420701 { get; set; }

    public decimal? _19421001 { get; set; }

    public decimal? _19430101 { get; set; }

    public decimal? _19430401 { get; set; }

    public decimal? _19430701 { get; set; }

    public decimal? _19431001 { get; set; }

    public decimal? _19440101 { get; set; }

    public decimal? _19440401 { get; set; }

    public decimal? _19440701 { get; set; }

    public decimal? _19441001 { get; set; }

    public decimal? _19450101 { get; set; }

    public decimal? _19450401 { get; set; }

    public decimal? _19450508 { get; set; }
}
