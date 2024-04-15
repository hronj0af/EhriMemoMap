using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace EhriMemoMap.Data;

[Table("prague_quarters_stats_timeline")]
public partial class PragueQuartersStatsTimeline
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("type")]
    public string? Type { get; set; }

    [Column("quarter_cs")]
    public string? QuarterCs { get; set; }

    [Column("quarter_en")]
    public string? QuarterEn { get; set; }

    [Column("count")]
    public decimal? Count { get; set; }

    [Column("geography", TypeName = "geography")]
    public Geometry? Geography { get; set; }

    [Column("geography_polygon")]
    public Geometry? GeographyPolygon { get; set; }

    [Column("1942-01-01")]
    public decimal? _19420101 { get; set; }

    [Column("1942-04-01")]
    public decimal? _19420401 { get; set; }

    [Column("1942-07-01")]
    public decimal? _19420701 { get; set; }

    [Column("1942-10-01")]
    public decimal? _19421001 { get; set; }

    [Column("1943-01-01")]
    public decimal? _19430101 { get; set; }

    [Column("1943-04-01")]
    public decimal? _19430401 { get; set; }

    [Column("1943-07-01")]
    public decimal? _19430701 { get; set; }

    [Column("1943-10-01")]
    public decimal? _19431001 { get; set; }

    [Column("1944-01-01")]
    public decimal? _19440101 { get; set; }

    [Column("1944-04-01")]
    public decimal? _19440401 { get; set; }

    [Column("1944-07-01")]
    public decimal? _19440701 { get; set; }

    [Column("1944-10-01")]
    public decimal? _19441001 { get; set; }

    [Column("1945-01-01")]
    public decimal? _19450101 { get; set; }

    [Column("1945-04-01")]
    public decimal? _19450401 { get; set; }

    [Column("1945-05-08")]
    public decimal? _19450508 { get; set; }
}
